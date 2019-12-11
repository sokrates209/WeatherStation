using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

namespace WeatherClient
{
    [DisallowConcurrentExecution]
    public class WeatherStationClient : IJob
    {
        private readonly IAzureHub _azureHub;
        private readonly ILogger<WeatherStationClient> _logger;

        public WeatherStationClient(IAzureHub azureHub, ILogger<WeatherStationClient> logger)
        {
            _azureHub = azureHub;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var airQuality = JsonConvert.DeserializeObject<AirQualitySensors>(await _azureHub.GetProperty("AirQualitySensors"));
            var general = JsonConvert.DeserializeObject<GeneralSensors>(await _azureHub.GetProperty("GeneralSensors"));
            
            _logger.LogInformation($"Timestamp: {DateTime.Now}: {airQuality} {general}");
        }
    }
}