using EmployeeMS.Application.DTOs;
using EmployeeMS.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMS.API.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeManagementDbContext _db;

        public EmployeeController(EmployeeManagementDbContext db)
        {
            this._db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployee([FromQuery] string? search, [FromQuery] string? department, [FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.Employees.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Name.Contains(search) || e.Email.Contains(search));

            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(e => e.Department == department);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            if(fromDate.HasValue)
                query = query.Where(e => e.JoiningDate >=  fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(e => e.JoiningDate < toDate.Value.Date);

            var total = await query.CountAsync();

            var employees = await query.OrderBy(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();    

            return Ok(new {total, page, pageSize, employees});
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return employee is null ? NotFound() : Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(EmployeeDto dto)
        {
            var email = dto.Email.Trim();

            if (await _db.Employees.AnyAsync(e => e.Email == email))
                return Conflict(new { message = "Email already exists" });

            var employee = new Employee
            {
                Name = dto.Name.Trim(),
                Email = email,
                Phone = dto.Phone.Trim(),
                Department = dto.Department.Trim(),
                Designation = dto.Designation.Trim(),
                JoiningDate = dto.JoiningDate!.Value.Date,
                Status = dto.Status.Trim(),
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto dto)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee is null) return NotFound();

            var email = dto.Email.Trim();

            if (await _db.Employees.AnyAsync(e => e.Email == email && e.Id != id))
                return Conflict(new { message = "Email already exists" });

            employee.Name = dto.Name.Trim();
            employee.Email = email;
            employee.Phone = dto.Phone.Trim();
            employee.Department = dto.Department.Trim();
            employee.Designation = dto.Designation.Trim();
            employee.JoiningDate = dto.JoiningDate!.Value.Date;
            employee.Status = dto.Status.Trim();
            employee.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
