using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using UnspedHealth.API.Persistence;
using UnspedHealth.Core.Interfaces;
using UnspedHealth.Core.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
    .WriteTo.Debug()
    .WriteTo.File("Logs/unsped-health-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Unsped Health Check API Başlatılıyor...");

// 1. VERİTABANI YAPILANDIRMASI
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Bağlantı kopmalarına karşı otomatik yeniden deneme (Resiliency)
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

builder.Services.AddHttpClient();

// service registry.
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddScoped<IRestService, RestService>();


// 2. SERVİS KAYITLARI (Dependency Injection)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. SWAGGER YAPILANDIRMASI (Profesyonel Dokümantasyon)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Unsped Health Check API",
        Version = "v1",
        Description = "### UGM Merkezi Servis İzleme Sistemi\n" +
                      "Bu API, kurum içindeki tüm servislerin hayatta olup olmadığını (heartbeat) " +
                      "ve anlık durum raporlarını (reporting) takip etmek için kullanılır.",
        Contact = new OpenApiContact
        {
            Name = "Unsped IT Development Team"
        },
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.Run();