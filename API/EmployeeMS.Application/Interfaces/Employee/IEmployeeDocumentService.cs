using EmployeeMS.Application.DTOs.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.Interfaces.Employee
{
    public sealed class DocumentUploadFile
    {
        public string FileName { get; init; } = "";
        public Stream Content { get; init; } = Stream.Null;
        public long Length { get; init; }
    }
    public interface IEmployeeDocumentService
    {
        Task<IReadOnlyList<EmployeeDocumentDto>> UploadAsync(int employeeId, IReadOnlyList<DocumentUploadFile> files, CancellationToken ct = default);
        Task<List<EmployeeDocumentDto>> GetAllAsync(int employeeId, CancellationToken ct = default);
        Task<DocumentDownloadDto?> DownloadAsync(int employeeId, int documentId, CancellationToken ct = default);
        Task<DocumentDownloadDto?> DownloadZipAsync(int employeeId, IReadOnlyList<int> documentIds, CancellationToken ct = default);
        Task<bool> DeleteAsync(int employeeId, int documentId,  CancellationToken ct = default);
    }
}
