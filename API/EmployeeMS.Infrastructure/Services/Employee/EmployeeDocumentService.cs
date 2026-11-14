using EmployeeMS.Application.DTOs.Employee;
using EmployeeMS.Application.Interfaces.Employee;
using EmployeeMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;

namespace EmployeeMS.Infrastructure.Services.Employee
{
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly EmployeeManagementDbContext _db;
        private readonly string _storageRoot;

        private const long MaxFileSize = 10 * 1024 * 1024;
        private const long MaxTotalSize = 50 * 1024 * 1024;
        private const int MaxFileCount = 20;

        private static readonly Dictionary<string, string> AllowedTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".jpeg"] = "image/jpeg",
                [".jpg"] = "image.jpeg",
                [".png"] = "image/png",
                [".webp"] = "image/webp"
            };

        public EmployeeDocumentService(EmployeeManagementDbContext db, IConfiguration configuration)
        {
            this._db = db;
            _storageRoot = configuration["DocumentStorage:RootPath"] ?? throw new InvalidOperationException("DocumentStorage:RootPath is missing.");

            if (!Path.IsPathFullyQualified(_storageRoot))
                throw new InvalidOperationException("Document storage path must be absolute.");

            Directory.CreateDirectory(_storageRoot);

        }
        public async Task<bool> DeleteAsync(int employeeId, int documentId, CancellationToken ct = default)
        {
            var document = await _db.EmployeeDocuments.FirstOrDefaultAsync(x => x.Id == documentId && x.EmployeeId == employeeId, ct);

            if (document == null) return false;

            var path = GetDocumentPath(document);

            _db.EmployeeDocuments.Remove(document);
            await _db.SaveChangesAsync();

            try
            {
                if (!File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return true;
        }

        public async Task<DocumentDownloadDto?> DownloadAsync(int employeeId, int documentId, CancellationToken ct = default)
        {
            var document = await _db.EmployeeDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == documentId && x.EmployeeId == employeeId, ct);

            if(document == null)
                return null;

            var path = GetDocumentPath(document);

            if (!File.Exists(path))
                return null;

            return new DocumentDownloadDto
            {
                Content = await File.ReadAllBytesAsync(path, ct),
                ContentType = document.ContentType,
                FileName = document.FileName,
            };
        }

        public async Task<DocumentDownloadDto?> DownloadZipAsync(int employeeId, IReadOnlyList<int> documentIds, CancellationToken ct = default)
        {
            var ids = documentIds.Distinct().ToArray();

            if (ids.Length == 0 || ids.Length > 100)
                throw new ArgumentException("Select between 1 and 100 documents.");

            var documents = await _db.EmployeeDocuments.AsNoTracking().Where(x => x.EmployeeId == employeeId && ids.Contains(x.Id)).ToListAsync(ct);

            if (documents.Count != ids.Length)
                return null;

            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var document in documents)
                {
                    ct.ThrowIfCancellationRequested();

                    var path = GetDocumentPath(document);

                    if(!File.Exists(path)) return null;

                    var entryName = $"{document.Id}_{Path.GetFileName(document.FileName)}";
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);

                    await using var entryStream = entry.Open();
                    await using var source = File.OpenRead(path);

                    await source.CopyToAsync(entryStream, ct);
                }
            }

            return new DocumentDownloadDto
            {
                Content = output.ToArray(),
                ContentType = "application/zip",
                FileName = $"Employee_{employeeId}_Documents.zip"
            };
        }

        public async Task<List<EmployeeDocumentDto>> GetAllAsync(int employeeId, CancellationToken ct = default)
        {
            return await _db.EmployeeDocuments.AsNoTracking().Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.UploadedAt).Select(x => new EmployeeDocumentDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                FileName = x.FileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                UploadedAt = x.UploadedAt,
            }).ToListAsync();
        }

        public async Task<IReadOnlyList<EmployeeDocumentDto>> UploadAsync(int employeeId, IReadOnlyList<DocumentUploadFile> files, CancellationToken ct = default)
        {
            if (!await _db.Employees.AnyAsync(x => x.Id == employeeId, ct))
                throw new KeyNotFoundException("Employee not found.");

            if (files.Count == 0 || files.Count > MaxFileCount)
                throw new ArgumentException($"Select between 1 and {MaxFileCount} files.");

            if (files.Any(x => x.Length <= 0 || x.Length > MaxFileSize))
                throw new ArgumentException("Each file must be between 1 byte and 10 MB.");

            if (files.Sum(x => x.Length) > MaxTotalSize)
                throw new ArgumentException("Combined file size cannot exceed 50 MB.");

            var employeeFolder = GetEmployeeFolder(employeeId);
            Directory.CreateDirectory(employeeFolder);

            var documents = new List<EmployeeDocument>();
            var writtenPaths = new List<string>();

            try
            {
                foreach (var file in files)
                {
                    ct.ThrowIfCancellationRequested();

                    var originalName = Path.GetFileName(file.FileName.Replace('\\', '/'));

                    if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255)
                        throw new ArgumentException("Invalid document filename.");

                    var extension = Path.GetExtension(originalName).ToLowerInvariant();

                    if (!AllowedTypes.ContainsKey(extension))
                        throw new ArgumentException($"Unsupported file type: {extension}");

                    byte[] originalBytes;

                    using (var buffer = new MemoryStream())
                    {
                        await file.Content.CopyToAsync(buffer, ct);

                        if (buffer.Length > MaxFileSize)
                            throw new ArgumentException("A file exceeds the 10 MB limit.");

                        originalBytes = buffer.ToArray();
                    }

                    var actualType = DetectFileType(originalBytes);

                    if (actualType != extension && !(actualType == ".jpg" && extension == ".jpeg"))
                        throw new ArgumentException($"File content does not match {originalName}.");

                    var outputBytes = originalBytes;
                    var outputExtension = extension;

                    var storedName = $"{Guid.NewGuid():N}{outputExtension}";
                    var fullPath = Path.Combine(employeeFolder, storedName);

                    await File.WriteAllBytesAsync(fullPath, outputBytes, ct);

                    writtenPaths.Add(fullPath);

                    documents.Add(new EmployeeDocument
                    {
                        EmployeeId = employeeId,
                        FileName = storedName,
                        ContentType = AllowedTypes[extension],
                        FileSize = outputBytes.LongLength,
                        UploadedAt = DateTime.UtcNow
                    });
                }

                _db.EmployeeDocuments.AddRange(documents);
                await _db.SaveChangesAsync(ct);

                return documents.Select(Map).ToList();
            }
            catch
            {
                foreach(var path in writtenPaths)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }

                _db.ChangeTracker.Clear();
                throw;
            }
        }

        private string GetEmployeeFolder(int employeeId)
        {
            return Path.Combine(_storageRoot, employeeId.ToString());
        }

        private string GetDocumentPath(EmployeeDocument document)
        {
            return Path.Combine(GetEmployeeFolder(document.EmployeeId), Path.GetFileName(document.FileName));
        }

        private static EmployeeDocumentDto Map(EmployeeDocument x) => new()
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            FileName = x.FileName,
            ContentType = x.ContentType,
            FileSize = x.FileSize,
            UploadedAt = x.UploadedAt
        };

        private static string DetectFileType(byte[] bytes)
        {
            if (bytes.Length >= 5 && bytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8))
                return ".pdf";

            if (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                return ".png";

            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return ".jpg";

            if (bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
                return ".webp";

            if(bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B)
            {
                using var stream = new MemoryStream(bytes);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

                if (archive.Entries.Any(x => x.FullName.Equals("word/document.xml", StringComparison.OrdinalIgnoreCase)))
                    return ".docx";
            }

            return "";
        }
    }
}
