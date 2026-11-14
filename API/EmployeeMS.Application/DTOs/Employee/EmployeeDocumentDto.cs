using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.DTOs.Employee
{
    public class EmployeeDocumentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
    public class DocumentDownloadDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = "";
    }
}
