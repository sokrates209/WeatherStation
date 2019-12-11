using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace WeatherClient
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder().Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var env = context.HostingEnvironment;

                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                    builder.AddApplicationInsights(instrumentationKey: "930a2a44-41bf-43a3-be29-4e87663afc6f");
                    builder.AddConsole();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAzureHub, AzureHub>();
            services.AddJobScheduler();
            
            //configuration
            services.AddSingleton(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var azureSettings = configuration.GetSection("AzureSettings").Get<AzureSettings>();

                return azureSettings;
            });

            var provider = services.BuildServiceProvider();
            var azure = provider.GetService<IAzureHub>();
            
            azure.Initialize().Wait();
            provider.ScheduleJobs().Wait();
        }
    }
}