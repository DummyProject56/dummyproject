using EmployeeMS.Application.Interfaces.Employee;
using EmployeeMS.Application.Services.Employee;
using EmployeeMS.Domain.Entities;
using EmployeeMS.Infrastructure.Services.Employee;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Employee Management API",
        Version = "v1",
        Description = " Employee Management System API"
    });
});

// ---------- Database ----------
builder.Services.AddDbContext<EmployeeManagementDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbContext"),
        providerOptions => providerOptions.EnableRetryOnFailure()).EnableSensitiveDataLogging().EnableDetailedErrors();
});

// ---------- Service ----------
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeImportService, EmployeeImportService>();
builder.Services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("GlobalCors", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("GlobalCors");

app.UseAuthorization();

app.MapControllers();

app.Run();