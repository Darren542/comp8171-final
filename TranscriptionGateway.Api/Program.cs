using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore.Infrastructure;
using TranscriptionGateway.Api.Data;
using TranscriptionGateway.Api.Models;
using TranscriptionGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(
        connectionString,
        oracleOptions => oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion21)
    ));

builder.Services.Configure<GpuApiOptions>(
    builder.Configuration.GetSection("GpuApi"));

builder.Services.AddHttpClient("gpu");

// Identity API endpoints + bearer/cookie support
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddAuthorization();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Typed HttpClient for the GPU app
builder.Services.AddHttpClient<IGpuTranscriptionClient, GpuTranscriptionClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GpuApi:BaseUrl"]!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Built-in Identity auth endpoints
app.MapIdentityApi<ApplicationUser>();

app.MapGet("/ping", () => "pong");

app.MapControllers();

app.Run();