using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherStation.Scheduler;
using WeatherStation.Services;
using WeatherStation.Settings;

namespace WeatherStation
{
    public class Program
    {
        static async Task Main()
        {
            await CreateHostBuilder().Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddEnvironmentVariables(prefix:"ASPNETCORE_");
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var env = context.HostingEnvironment;

                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    builder.AddSentry("https://09bb7f71e0f04571a388684ffdec8f85@sentry.io/1497882");
                    builder.AddConsole();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //services
            services.AddMediatR(typeof(Program));
            services.AddSingleton<IAzureIotHub, AzureHub>();
            services.AddSingleton<IAirQualityProvider, AirQualityProvider>();
            services.AddSingleton<IPressureProvider, PressureProvider>();
            services.AddSingleton<IUvProvider, UvProvider>();
            services.AddSingleton<ITempProvider, TempProvider>();
            services.AddJobScheduler();
            
            //configuration
            services.AddSingleton(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var azureSettings = configuration.GetSection("AzureSettings").Get<AzureSettings>();

                return azureSettings;
            });

            services.AddSingleton(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var airQualitySettings = configuration.GetSection("AirQuality").Get<AirQualitySettings>();

                return airQualitySettings;
            });

            //initialize
            var provider = services.BuildServiceProvider();
            var azureIot = provider.GetService<IAzureIotHub>();
            var airQuality = provider.GetService<IAirQualityProvider>();

            provider.ScheduleJobs().Wait();
            azureIot.Initialize().Wait();
            airQuality.Initialize().Wait();

            //testers
            //TestUvSensor(provider).Wait();
            //TestPressureSensor(provider).Wait();
            //TestTempProvider(provider).Wait();
        }

        private static async Task TestTempProvider(ServiceProvider provider)
        {
            var sensor = provider.GetService<ITempProvider>();

            while (true)
            {
                sensor.GetTemp();
                sensor.GetHumidity();

                await Task.Delay(500);
            }
        }

        private static async Task TestPressureSensor(ServiceProvider provider)
        {
            var pressure = provider.GetService<IPressureProvider>();
            while (true)
            {
                await pressure.GetPressure();
                await Task.Delay(500);
            }
        }

        private static async Task TestUvSensor(ServiceProvider provider)
        {
            var uvProvider = provider.GetService<IUvProvider>();
            while (true)
            {
                Console.WriteLine(uvProvider.ReadUV());

                await Task.Delay(500);
            }
        }
    }
}