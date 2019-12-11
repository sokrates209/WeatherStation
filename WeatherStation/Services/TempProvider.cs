using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading.Tasks;
using Iot.Device.Sht3x;
using Microsoft.Extensions.Logging;

namespace WeatherStation.Services
{
    public interface ITempProvider
    {
        double GetTemp();
        double GetHumidity();
    }

    public class TempProvider : ITempProvider
    {
        private readonly ILogger<TempProvider> _logger;
        private UnixI2cDevice _sht3x;

        public TempProvider(ILogger<TempProvider> logger)
        {
            _logger = logger;
            var sht3xSettings = new I2cConnectionSettings(1, (byte)Iot.Device.Sht3x.I2cAddress.AddrHigh);
            
            _sht3x = new UnixI2cDevice(sht3xSettings);
        }

        public double GetTemp()
        {
            using(var sensor = new Sht3x(_sht3x))
            {
                var temp = sensor.Temperature.Celsius;
                
                _logger.LogDebug($"Temperature: {temp} C");

                return temp;
            }
        }
        
        public double GetHumidity()
        {
            using(var sensor = new Sht3x(_sht3x))
            {
                var humidity = sensor.Humidity;
                
                _logger.LogDebug($"Humidity: {humidity}");

                return humidity;
            }
        }
    }
}