using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using EmployeeMS.Application.DTOs;
using EmployeeMS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EmployeeMS.API.Controllers
{
    [Route("api/employee/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly EmployeeManagementDbContext _db;

        public ImportController(EmployeeManagementDbContext db)
        {
            this._db = db;
        }

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> ImportDocument(IFormFile file)
        {
            if (file == null || file.Length == 0) 
                return BadRequest("File is Required.");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("Maximum file size is 10 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (ext != ".csv" && ext != ".xlsx")
                return BadRequest("Only CSV and xlsx are supported.");

            List<string[]> rows;

            try
            {
                using var stream = file.OpenReadStream();
                rows = ext == ".csv"
                    ? ReadCsv(stream)
                : ReadExcel(stream);
            }
            catch (Exception ex) when (
                ex is IOException ||
                    ex is FormatException ||
                    ex is InvalidDataException)
            {
                    return BadRequest(new {
                        message = "Could not parse the uploaded file."
                    });
            }

            var result = new ImportResultDto();
            var exisingEmails = (await _db.Employees.AsNoTracking().Select(e => e.Email).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var pending = new List<Employee>();

            for (int i = 0; i < rows.Count; i++) 
            {
                var rowNumber = i + 2;
                var row = rows[i];
                result.TotalRecords++;

                var errorBefore = result.Errors.Count;

                if(row.Length != 6)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "Row",
                        Reason = "Expected exactly & columns"
                    });
                    continue;
                }

                var name = row[0].Trim();
                var email = row[1].Trim();
                var phone = row[2].Trim();
                var department = row[3].Trim();
                var designation = row[4].Trim();
                var dateText = row[5].Trim();

                void Error(string field, string reason) => result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = rowNumber,
                    Field = field,
                    Reason = reason,
                });

                if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
                     Error("Name", "Required, maximum 150 characters");

                if (!new EmailAddressAttribute().IsValid(email))
                    Error("Email", "Invalid email address");
                else if (!exisingEmails.Add(email))
                    Error("Email", "Duplicate email");

                if (string.IsNullOrWhiteSpace(phone) || phone.Length > 20)
                    Error("Phone", "Required, maximum 20 characters");

                if (string.IsNullOrWhiteSpace(department) || department.Length > 100)
                    Error("Department", "Required, maximum 100 characters");

                if (string.IsNullOrWhiteSpace(designation) || designation.Length > 100)
                    Error("Designation", "Required, maximum 100 characters");

                var validDate = DateTime.TryParseExact(
                    dateText, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var joiningDate
                    );

                if (!validDate)
                    Error("JoiningDate", "Use yyyy-MM-dd format");

                if (result.Errors.Count != errorBefore)
                    continue;

                pending.Add(new Employee
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Department = department,
                    Designation = designation,
                    JoiningDate = joiningDate,
                    Status = "Active"
                });
            }

            _db.Employees.AddRange(pending);
            await _db.SaveChangesAsync();

            result.SuccessfulRecords = pending.Count;
            result.FailedRecords = result.TotalRecords - result.SuccessfulRecords;

            return Ok(result);
        }

        private static List<string[]> ReadCsv(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, 
                new CsvConfiguration(CultureInfo.InvariantCulture) { 
                    HasHeaderRecord = true 
                });

            var rows = new List<string[]>();

            csv.Read();
            csv.ReadHeader();

            var expected = new[]
            {
                "Name", "Email", "Phone",
                "Department", "Designation", "JoiningDate"
            };

            if (csv.HeaderRecord == null || !csv.HeaderRecord.SequenceEqual(expected))
                throw new InvalidDataException("Invalid CSV headers");

            while (csv.Read())
            {
                rows.Add(Enumerable.Range(0, 6).Select(i => csv.GetField(i) ?? "").ToArray());
            }
            ;
            return rows;
        }

        private static List<string[]> ReadExcel(Stream stream) 
        {
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);
            var rows = sheet.RowsUsed().ToList();

            var expected = new[]
            {
                "Name", "Email", "Phone",
                "Department", "Designation", "JoiningDate"
            };

            if (!rows.Any() || !rows[0].Cells(1, 6).Select(c => c.GetString().Trim()).SequenceEqual(expected))
                throw new InvalidDataException("Invalid Excel headers");

            return rows.Skip(1).Select(r => 
            Enumerable.Range(1, 6).Select(i => r.Cell(i).GetFormattedString()).ToArray()).ToList();
        }
    }
}
