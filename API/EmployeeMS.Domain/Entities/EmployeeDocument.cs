using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMS.Domain.Entities;

public partial class EmployeeDocument
{
    [Key]
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [StringLength(500)]
    public string FilePath { get; set; } = null!;

    [StringLength(100)]
    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UploadedAt { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("EmployeeDocuments")]
    public virtual Employee Employee { get; set; } = null!;
}
