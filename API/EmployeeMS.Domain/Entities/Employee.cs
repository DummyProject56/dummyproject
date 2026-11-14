using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMS.Domain.Entities;

public partial class Employee
{
    [Key]
    public int Id { get; set; }

    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [StringLength(100)]
    public string Department { get; set; } = null!;

    [StringLength(100)]
    public string Designation { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime JoiningDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [InverseProperty("Employee")]
    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();
}
