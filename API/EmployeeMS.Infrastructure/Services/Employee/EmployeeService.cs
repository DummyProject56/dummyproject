using EmployeeMS.Application.DTOs;
using EmployeeMS.Application.DTOs.Employee;
using EmployeeMS.Application.Interfaces.Employee;
using EmployeeMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMS.Application.Services.Employee
{
    public class EmployeeService : IEmployeeService
    {
        private readonly EmployeeManagementDbContext _db;

        public EmployeeService(EmployeeManagementDbContext db)
        {
            this._db = db;
        }
        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeRequestDto request, CancellationToken ct = default)
        {
           var email = request.Email.Trim().ToLowerInvariant();
            
            if(await _db.Employees.AnyAsync(x => x.Email == email, ct))
            {
                throw new ArgumentException("An employee with this email already exists.");
            }

            var employee = new Domain.Entities.Employee
            {
                Name = request.Name.Trim(),
                Email = email,
                Phone = request.Phone.Trim(),
                Department = request.Department.Trim(),
                Designation = request.Designation.Trim(),
                JoiningDate = request.JoiningDate!.Value.Date,
                Status = NormalizeStatus(request.Status),
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync(ct);

            return Map(employee);
        }

        public async Task<PageResultDto<EmployeeDto>> GetAllEmployeeAsync(EmployeeFilterDto filter, CancellationToken ct = default)
        {
            var query = _db.Employees.AsNoTracking().AsQueryable();

            if(!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
            }

            if(!string.IsNullOrWhiteSpace(filter.Department))
            {
                var department = filter.Department.Trim();
                query = query.Where(x => x.Department.Contains(department));
            }

            if (!string.IsNullOrWhiteSpace(filter.Designation))
            {
                var designation = filter.Designation.Trim();
                query = query.Where(x => x.Designation.Contains(designation));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var status = filter.Status.Trim();
                query = query.Where(x => x.Status == status);
            }

            if(filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;
                query = query.Where(x => x.JoiningDate >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDate = filter.ToDate.Value.Date;
                query = query.Where(x => x.JoiningDate < toDate);
            }

            var total = await query.CountAsync(ct);

            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var employee = await query.OrderBy(x => x.Id).Skip((page-1) * pageSize).Take(pageSize).Select(x => new EmployeeDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Department = x.Department,
                Designation = x.Designation,
                JoiningDate = x.JoiningDate,
                Status = x.Status,
            }).ToListAsync();

            return new PageResultDto<EmployeeDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Employees = employee,
            };
        }

        public async Task<EmployeeDto?> GetEmployeeAsync(int id, CancellationToken ct = default)
        {
            return await _db.Employees.AsNoTracking().Where(x => x.Id == id).Select(x => new EmployeeDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                Department = x.Department,
                Designation = x.Designation,
                JoiningDate = x.JoiningDate,
                Status = x.Status,
            }).FirstOrDefaultAsync(ct);
        }

        public async Task<bool> UpdateEmployeeAsync(int id, EmployeeRequestDto request, CancellationToken ct = default)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (employee == null) 
                return false;

            var email = request.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Employees.AnyAsync(x => x.Email == email && x.Id != id, ct);

            if (emailExists) 
                throw new ArgumentException("Another employee already use this email.");

            employee.Name = request.Name.Trim();
            employee.Email = email; 
            employee.Phone = request.Phone.Trim();
            employee.Department = request.Department.Trim();
            employee.Designation = request.Designation.Trim();
            employee.JoiningDate = request.JoiningDate!.Value.Date;
            employee.Status = NormalizeStatus(request.Status);

            await _db.SaveChangesAsync(ct);

            return true;
        }

        private static string NormalizeStatus(string status)
        {
            return status.Equals(
                "active", StringComparison.OrdinalIgnoreCase)
                ? "Active"
                : "Inactive";
        }

        private static EmployeeDto Map(Domain.Entities.Employee x) => new()
        {
            Id = x.Id,
            Name = x.Name,
            Email = x.Email,
            Phone = x.Phone,
            Department = x.Department,
            Designation = x.Designation,
            JoiningDate = x.JoiningDate,
            Status = x.Status
        };
    }
}
