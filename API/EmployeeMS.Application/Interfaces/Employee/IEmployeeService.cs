using EmployeeMS.Application.DTOs;
using EmployeeMS.Application.DTOs.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.Interfaces.Employee
{
    public interface IEmployeeService
    {
        Task<PageResultDto<EmployeeDto>> GetAllEmployeeAsync(EmployeeFilterDto filter, CancellationToken ct = default);
        Task<EmployeeDto?> GetEmployeeAsync(int id, CancellationToken ct = default);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeRequestDto request, CancellationToken ct = default);
        Task<bool> UpdateEmployeeAsync(int id, EmployeeRequestDto request, CancellationToken ct = default);
    }
}
