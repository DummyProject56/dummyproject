using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.DTOs
{
    public class EmployeeDto
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Designation { get; set; } = string.Empty;

        [Required]
        public DateTime? JoiningDate { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}
