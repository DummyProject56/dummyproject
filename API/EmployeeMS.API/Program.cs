using EmployeeMS.Domain.Entities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- no upload size limit ----
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = null);
builder.Services.Configure<IISServerOptions>(o => o.MaxRequestBodySize = null);
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = long.MaxValue);

builder.Services.AddControllers();

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

// ---------- Database ----------
builder.Services.AddDbContext<EmployeeManagementDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DbContext"),
        providerOptions => providerOptions.EnableRetryOnFailure()).EnableSensitiveDataLogging().EnableDetailedErrors(); ;

    //if (builder.Environment.IsDevelopment())
    //    options.EnableSensitiveDataLogging().EnableDetailedErrors();
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

//app.UseMiddleware<ExceptionMiddleware>();

app.MapOpenApi();
app.UseSwaggerUi(options =>
{
    options.DocumentPath = "openapi/v1.json";
});


app.UseRouting();

app.UseCors("GlobalCors");

app.UseAuthentication();   // must be before UseAuthorization

//app.UseAuthorization();

//app.UseRateLimiter();

app.MapControllers();

app.Run();