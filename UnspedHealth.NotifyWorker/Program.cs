using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Serilog;
using UnspedHealth.API.Persistence;
using UnspedHealth.Core.Interfaces;
using UnspedHealth.Core.Models;
using UnspedHealth.Core.Services;
using UnspedHealth.Worker;
using UnspedHealth.Worker.Config;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "UnspedHealthWatchdogService";
    });

    builder.Configuration
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .Build();

    AppConfiguration.ConfigureLogging(builder.Configuration);
    builder.Logging.ClearProviders();
    builder.Services.AddSerilog(Log.Logger);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Veritabanı bağlantı cümlesi (DefaultConnection) bulunamadı.");
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

    builder.Services.AddHttpClient();

    // servis kayıtları.
    builder.Services.AddScoped<IHealthService, HealthService>();
    builder.Services.AddScoped<IServerService, ServerService>();
    builder.Services.AddScoped<IRecoveryService, RecoveryService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    AgentSettings.DefaultPort = builder.Configuration.GetValue<int>("AgentSettings:DefaultPort");
    AgentSettings.ApiKey = builder.Configuration.GetValue<string>("AgentSettings:ApiKey") ?? string.Empty;

    // workeri arka plan görevi olarak ekliyoruz.
    builder.Services.AddHostedService<WatchdogWorker>();
    var host = builder.Build();

    Log.Information("Watchdog Service has been started successfully.");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A critical error occurred while starting the application.");
}
finally
{
    Log.Information("Watchdog Service is shutting down...");
    Log.CloseAndFlush();
}

