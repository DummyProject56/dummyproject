using EmployeeMS.Application.DTOs;
using EmployeeMS.Application.DTOs.Employee;
using EmployeeMS.Application.Interfaces.Employee;
using EmployeeMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMS.API.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;
        public EmployeeController(IEmployeeService service)
        {
            this._service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployee([FromQuery] EmployeeFilterDto filter, CancellationToken ct)
        {
            if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate > filter.ToDate)
            {
                return BadRequest(new { message = "FromDate cannot exceed ToDate." });
            }

            var result = await _service.GetAllEmployeeAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployee(int id, CancellationToken ct)
        {
            var employee = await _service.GetEmployeeAsync(id, ct);
            return employee is null ? NotFound(new { message = "Employee not found." }) : Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeRequestDto request, CancellationToken ct)
        {
            try
            {
                var employees = await _service.CreateEmployeeAsync(request, ct);
                return Ok(new { message = "Employee created successfully.", data = employees });
            }
            catch (ArgumentException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeRequestDto request, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateEmployeeAsync(id, request, ct);
                return updated ? NoContent() : NotFound(new { message = "Employee not found." });
            }
            catch (ArgumentException ex)
            {
                return Conflict(new { message = ex.Message });    
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Employee update failed. Check unique values." });
            }
        }
    }
}
