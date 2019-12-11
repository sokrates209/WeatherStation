using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Quartz;
using WeatherStation.Events;
using WeatherStation.Model;
using WeatherStation.Services;

namespace WeatherStation.Scheduler
{
    [DisallowConcurrentExecution]
    public class SensorReadingsProcessor : IJob
    {
        private readonly IUvProvider _uvProvider;
        private readonly IPressureProvider _pressureProvider;
        private readonly ITempProvider _tempProvider;
        private readonly ILogger<SensorReadingsProcessor> _logger;
        private readonly IMediator _mediator;

        public SensorReadingsProcessor(
            IUvProvider uvProvider,
            IPressureProvider pressureProvider,
            ITempProvider tempProvider,
            ILogger<SensorReadingsProcessor> logger,
            IMediator mediator)
        {
            _uvProvider = uvProvider;
            _pressureProvider = pressureProvider;
            _tempProvider = tempProvider;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Getting sensory readings. TimeStamp: {DateTime.UtcNow}");

            var readings = new GeneralSensors()
            {
                Humidity = _tempProvider.GetHumidity(),
                Pressure = await _pressureProvider.GetPressure(),
                Temperature = _tempProvider.GetTemp(),
                LightLevel = _uvProvider.ReadUV()
            };
            
            _logger.LogInformation($"Readings: {readings}");

            await _mediator.Publish(new ReadingsUpdated(new TwinCollection()
            {
                [nameof(GeneralSensors)] = readings
            }));
        }
    }
}