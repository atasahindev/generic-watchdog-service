using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using UnspedHealth.Core.Models;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration.GetValue<int>("AgentSettings:Port", 5005);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "Logs", "agent-log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "Unsped Watchdog Agent Service";
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var apiKey = app.Configuration.GetValue<string>("AgentSettings:ApiKey");
    if (!context.Request.Headers.TryGetValue("X-Agent-Key", out var extractedKey) || apiKey != extractedKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { Message = "Unauthorized Unsped Watchdog Agent Access." });
        return;
    }
    await next();
});

app.MapPost("/process/restart", async ([FromBody] RestartRequest request, ILogger<Program> logger) =>
{
    try
    {
        logger.LogWarning("[UNSPED_HEALTH] Restart talebi alýndý: {ProcessName}", request.ProcessName);

        var existingProcesses = Process.GetProcessesByName(request.ProcessName);
        foreach (var p in existingProcesses)
        {
            try
            {
                logger.LogInformation("[UNSPED_HEALTH] {ProcessName} (PID: {Pid}) durduruluyor...", request.ProcessName, p.Id);
                p.Kill(true);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await p.WaitForExitAsync(cts.Token);

                logger.LogInformation("[UNSPED_HEALTH] {ProcessName} baþarýyla durduruldu.", request.ProcessName);
            }

            catch (Exception ex)
            {
                logger.LogError("[UNSPED_HEALTH] Süreç (PID: {Pid}) durdurulamadý: {Message}", p.Id, ex.Message);
            }
        }

        if (!File.Exists(request.ExecutionPath))
        {
            logger.LogError("[UNSPED_HEALTH] Dosya bulunamadý: {Path}", request.ExecutionPath);
            return Results.NotFound(new AgentResponse(false, "Çalýþtýrýlabilir dosya yolu geçersiz.", DateTime.Now));
        }

        var workingDirectory = Path.GetDirectoryName(request.ExecutionPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutionPath,
            Arguments = request.Arguments ?? string.Empty,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        var startedProcess = Process.Start(startInfo);

        if (startedProcess == null)
        {
            throw new Exception("Süreç baþlatýldý ancak handle alýnamadý.");
        }

        logger.LogInformation("[UNSPED_HEALTH] {ProcessName} baþarýyla baþlatýldý. PID: {Pid}", request.ProcessName, startedProcess.Id);

        return Results.Ok(new AgentResponse(true, $"{request.ProcessName} baþarýyla baþlatýldý.", DateTime.Now));
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "[UNSPED_HEALTH] Restart iþlemi sýrasýnda hata meydana geldi.");
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Version = "1.0.0" }));

try
{
    Log.Information("[AGENT] UnspedHealth Agent Baþlatýlýyor...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "[AGENT] Agent beklenmedik bir þekilde durdu!");
}
finally
{
    Log.CloseAndFlush();
}