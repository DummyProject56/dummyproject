using EmployeeMS.Application.Interfaces.Employee;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeMS.API.Controllers
{
    [Route("api/employee/{employeed:int}/documents")]
    [ApiController]
    public class EmployeeDocumentController : ControllerBase
    {
        private readonly IEmployeeDocumentService _service;

        public EmployeeDocumentController(IEmployeeDocumentService service)
        {
            this._service = service;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Upload(int employeeId, List<IFormFile> files, CancellationToken ct)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "No files selected." });

            var streams = new List<Stream>();

            try
            {
                var uploads = new List<DocumentUploadFile>();

                foreach (var file in files)
                {
                    var stream = file.OpenReadStream();
                    streams.Add(stream);

                    uploads.Add(new DocumentUploadFile
                    {
                        FileName = file.FileName,
                        Content = stream,
                        Length = file.Length
                    });
                }

                var result = await _service.UploadAsync(employeeId, uploads, ct);

                return Ok(new { message = "Documents uploaded successfully.", documents = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                foreach (var stream in streams)
                    await stream.DisposeAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int employeeId, CancellationToken ct)
        {
            var documents = await _service.GetAllAsync(employeeId, ct);

            return Ok(documents);
        }

        [HttpGet("{documentId:int}")]
        public async Task<IActionResult> Download(int employeeId, int documentId, CancellationToken ct)
        {
            var file = await _service.DownloadAsync(employeeId,documentId, ct);

            if(file == null) return NotFound();

            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpPost("download-zip")]
        public async Task<IActionResult> DownloadZip(int employeeId, [FromBody] List<int> documentIds, CancellationToken ct)
        {
            try
            {
                var file = await _service.DownloadZipAsync(employeeId, documentIds, ct);

                if (file == null) return NotFound(new { message = "One or more documents not found." });

                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{documentId:int}")]
        public async Task<IActionResult> Delete(int employeeId, int documentId, CancellationToken ct)
        {
            var deleted = await _service.DeleteAsync(employeeId, documentId, ct);

            return deleted ? NoContent() : NotFound();
        }
    }
}
