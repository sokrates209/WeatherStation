using System.Device.I2c;
using Microsoft.Extensions.Logging;

namespace WeatherStation.Services
{
    public interface IUvProvider
    {
        double ReadUV();
    }

    public class UvProvider : IUvProvider
    {
        private readonly ILogger<UvProvider> _logger;
        private readonly Lm8511 _sensor;

        public UvProvider(ILogger<UvProvider> logger)
        {
            _logger = logger;
            
            var adsSettings = new I2cConnectionSettings(1, (byte)Iot.Device.Ads1115.I2cAddress.GND);
            _sensor = new Lm8511(adsSettings, _logger);
        }

        public double ReadUV()
        {
            var reading = _sensor.UV;
            _logger.LogDebug($"UV value: {reading}");
            
            return reading;
        }
    }
}