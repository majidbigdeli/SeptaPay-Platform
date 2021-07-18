using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Diagnostics;
using System.IO;

namespace Pos
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static void Main(string[] args)
        {
            const string loggerTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}]<{ThreadId}> [{SourceContext:l}] {Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.FromLogContext()
                .WriteTo.Console(LogEventLevel.Information, loggerTemplate, theme: AnsiConsoleTheme.Literate)
                .WriteTo.Seq("http://192.168.42.76:5341")
                .CreateLogger();

            try
            {
                Log.Information("====================================================================");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application terminated unexpectedly");
            }
            finally
            {
                Log.Information("====================================================================\r\n");
                Log.CloseAndFlush();
            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseContentRoot(GetBasePath())
            .ConfigureAppConfiguration(cfgBuilder =>
            {
#if DEBUG
                Configuration = cfgBuilder.AddJsonFile("Configs/appsettings.json", true, true).AddEnvironmentVariables().Build(); // Add the filename.json configuration file stored in the /config directory
#elif RELEASEDEBUG
                Configuration = cfgBuilder.AddJsonFile("Configs/appsettings.releasedebug.json", true, true).AddEnvironmentVariables().Build(); // Add the filename.json configuration file stored in the /config directory
#else
                Configuration = cfgBuilder.AddJsonFile("Configs/appsettings.releasedebug.json", true, true).AddEnvironmentVariables().Build(); // Add the filename.json configuration file stored in the /config directory
#endif            

            })
             // .UseWindowsService()
             .UseSystemd()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                .UseKestrel(opts =>
                {
                    //  opts.Listen(IPAddress.Loopback, port: 5002);
                    opts.ListenLocalhost(4003);
                    opts.ListenAnyIP(4002);
                });

            })
            .UseSerilog();
        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }
    }
}