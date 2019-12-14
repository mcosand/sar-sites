using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.IO;

namespace Kcsara.Database.Api
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
      var builder = WebHost.CreateDefaultBuilder(args);

      return builder.UseStartup<Startup>()
        .ConfigureAppConfiguration((context, config) =>
        {
          config.AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddJsonFile("appsettings.local.json", true, true)
                .AddEnvironmentVariables();
        })
        .ConfigureLogging(logging =>
        {
          Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.RollingFile(Path.Combine("", "log-{Date}.txt"), restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

          logging.AddSerilog();
        });
    }
  }
}