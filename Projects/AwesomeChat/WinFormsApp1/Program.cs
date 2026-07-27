using Serilog;
using System.Diagnostics.CodeAnalysis;
using WinFormsApp1.Forms;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AwesomeChat.Tests")]

namespace WinFormsApp1
{
    // ExcludeFromCodeCoverage: entry point + Serilog setup are not unit-testable
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "AwesomeChat", "logs", "app-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("AwesomeChat starting");

            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new LoginForm());
            }
            finally
            {
                Log.Information("AwesomeChat shutting down");
                Log.CloseAndFlush();
            }
        }
    }
}

