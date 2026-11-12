using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.DTOs
{
    public class ImportErrorDto
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = "";
        public string Reason { get; set; } = "";
    }
}
