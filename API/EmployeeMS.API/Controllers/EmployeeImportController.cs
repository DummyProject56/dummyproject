using EmployeeMS.Application.Interfaces.Employee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeMS.API.Controllers
{
    [Route("api/employee/import")]
    [ApiController]
    public class EmployeeImportController : ControllerBase
    {
        private readonly IEmployeeImportService _service;

        public EmployeeImportController(IEmployeeImportService service)
        {
            this._service = service;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
        {
            if(file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Please select a file." });
            }

            if(file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "Maximum file size is 10 MB." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if(extension != ".csv" && extension != ".xlsx")
            {
                return BadRequest(new { message = "Only CSV and XLSX files are allowed." });
            }

            try
            {
                await using var stream = file.OpenReadStream();

                var result = await _service.ImportAsync(stream, file.FileName, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidDataException)
            {
                return BadRequest(new { message = "Invalid or corrupted import file." });
            }
        }
    }
}
