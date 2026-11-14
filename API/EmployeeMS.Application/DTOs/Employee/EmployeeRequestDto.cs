using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMS.Application.DTOs.Employee
{
    public class EmployeeRequestDto
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = "";

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = "";

        [StringLength(30)]
        public string Phone { get; set; } = "";

        [Required, StringLength(100)]
        public string Department { get; set; } = "";

        [Required, StringLength(100)]
        public string Designation { get; set; } = "";

        [Required]
        public DateTime? JoiningDate { get; set; }

        [Required]
        [RegularExpression("(?i)^(active|inactive)$")]
        public string Status { get; set; } = "Active";
    }
}
