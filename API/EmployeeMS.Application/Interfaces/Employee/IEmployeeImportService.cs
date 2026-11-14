using EmployeeMS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.Interfaces.Employee
{
    public interface IEmployeeImportService
    {
        Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    }
}
