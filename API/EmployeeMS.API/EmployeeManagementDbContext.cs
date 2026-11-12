using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMS.Domain.Entities;

public partial class EmployeeManagementDbContext : DbContext
{
    public EmployeeManagementDbContext()
    {
    }

    public EmployeeManagementDbContext(DbContextOptions<EmployeeManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS01; Initial Catalog=EmployeeManagementDB; Integrated Security=SSPI; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeDocuments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeDocument_Employee");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
