using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using EmployeeMS.Application.DTOs;
using EmployeeMS.Application.Interfaces.Employee;
using EmployeeMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace EmployeeMS.Infrastructure.Services.Employee
{
    public class EmployeeImportService : IEmployeeImportService
    {
        private readonly EmployeeManagementDbContext _db;

        private static readonly string[] Headers =
        {
            "Name", "Email", "Phone", "Department",
            "Designation", "JoiningDate", "Status"
        };

        public EmployeeImportService(EmployeeManagementDbContext db)
        {
            this._db = db;
        }
        public async Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            List<(int Rowm, Dictionary<string, string> Data)> rows;

            if (extension == ".csv")
                rows = ReadCsv(fileStream);
            else if (extension == ".xlsx")
                rows = ReadExcel(fileStream);
            else
                throw new ArgumentException("Only CSV and XLSX are supported.");

            if (rows.Count > 10000)
                throw new ArgumentException("Maximum 10,000 rows allowed.");

            var result = new ImportResultDto
            {
                TotalRecords = rows.Count,
            };

            var existingEmails = (await _db.Employees.AsNoTracking().Select(x => x.Email).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var validEmployees = new List<Domain.Entities.Employee>();

            foreach (var (rowNumber, data) in rows)
            {
                var errorBefore = result.Errors.Count;

                string Get(string name) => data.TryGetValue(name, out var value) ? value.Trim() : "";

                var name = Get("Name");
                var email = Get("Email").ToLowerInvariant();
                var phone = Get("Phone");
                var department = Get("Department");
                var designation = Get("Designation");
                var status = Get("status");
                var dataText = Get("JoiningDate");

                void Error(string field, string reason)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = field,
                        Reason = reason,
                    });
                }

                if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
                    Error("Name", "Name is required (max 150 characters).");

                if (string.IsNullOrWhiteSpace(email) || email.Length > 254 || !new EmailAddressAttribute().IsValid(email))
                    Error("Email", "Valid email is required.");

                if (phone.Length > 30)
                    Error("Phone", "Maximum 30 characters.");

                if (string.IsNullOrWhiteSpace(department) || department.Length > 100)
                    Error("Department", "Department is required (max 100).");

                if (string.IsNullOrWhiteSpace(designation) || designation.Length > 100)
                    Error("Designation", "Designation is required (max 100).");

                if (!DateTime.TryParseExact(dataText, new[] { "yyyy-MM-dd", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var joiningDate))
                {
                    Error("JoiningDate", "Use yyy-MM-dd or dd/MM/yyyy.");
                }

                if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) && !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    Error("Status", "Status must be Active or Inactive.");

                if(!string.IsNullOrWhiteSpace(email))
                {
                    if (!seenEmails.Add(email))
                        Error("Email", "Duplicate email in import file.");

                    if (existingEmails.Contains(email))
                        Error("Email", "Email already exists in database.");
                }

                if (result.Errors.Count != errorBefore)
                    continue;

                validEmployees.Add(new Domain.Entities.Employee
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Department = department,
                    Designation = designation,
                    JoiningDate = joiningDate.Date,
                    Status = status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Active" : "Inactive"
                });
            }

            if(validEmployees.Count > 0)
            {
                _db.Employees.AddRange(validEmployees);
                await _db.SaveChangesAsync();
            }

            result.SuccessfulRecords = validEmployees.Count;
            result.FailedRecords = result.TotalRecords - result.SuccessfulRecords;

            return result;
        }

        private static List<(int Row, Dictionary<string, string> Data)> ReadCsv(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null,
            });

            if (!csv.Read() || !csv.ReadHeader())
                throw new ArgumentException("CSV header is missing.");

            var headers = csv.HeaderRecord ?? Array.Empty<string>();

            foreach (var required in headers)
            {
                if (!headers.Contains(required, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException($"Missing Column: {required}");
            }

            var rows = new List<(int, Dictionary<string, string>)>();

            while(csv.Read())
            {
                var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var header in headers)
                    data[header] = csv.GetField(header) ?? "";

                rows.Add((csv.Context.Parser.Row, data));

                if (rows.Count > 10000)
                    throw new ArgumentException("Maximum 10,000 rows allowed.");
            }

            return rows;
        }

        private static List<(int Row, Dictionary<string, string> Data)> ReadExcel(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault() ?? throw new ArgumentException("Workbook is empty.");

            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
            var lastColumn = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;

            if (lastRow < 1)
                throw new ArgumentException("Excel sheet is empty.");

            if (lastRow > 10001)
                throw new ArgumentException("Maximum 10,000 rows allowed.");

            var headerPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int col = 1; col <= lastColumn; col++)
            {
                var header = sheet.Cell(1, col).GetString().Trim();
                
                if(!string.IsNullOrEmpty(header))
                    headerPositions[header] = col;
            }

            foreach (var required in Headers)
            {
                if (!headerPositions.ContainsKey(required))
                    throw new ArgumentException($"Missing Column: {required}");
            }

            var rows = new List<(int, Dictionary<string, string>)>();

            for (var row = 2; row <= lastRow; row++)
            {
                var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var header in Headers)
                {
                    var cell = sheet.Cell(row, headerPositions[header]);

                    if (header == "JoiningDate" && cell.TryGetValue<DateTime>(out var date))
                        data[header] = date.ToString("yyyy-MM-dd");
                    else
                        data[header] = cell.GetFormattedString().Trim();
                }

                if (data.Values.All(string.IsNullOrWhiteSpace))
                    continue;

                rows.Add((row, data));
            }

            return rows;
        }
    }
}
