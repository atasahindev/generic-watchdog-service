using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace UnspedHealth.Worker.Config
{
    public static class AppConfiguration
    {
        public static void ConfigureLogging(IConfiguration configuration)
        {

            var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            var logFile = Path.Combine(logDirectory, "unsped-watchdog-.log");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .WriteTo.Debug()
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
        }
    }
}
