using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeatherStation.Device;

namespace WeatherStation.Services
{
    public interface IPressureProvider
    {
        Task<double> GetPressure();
    }

    public class PressureProvider : IPressureProvider
    {
        private readonly ILogger<PressureProvider> _logger;
        private readonly UnixI2cDevice _bmp280;

        public PressureProvider(ILogger<PressureProvider> logger)
        {
            _logger = logger;
            var bmpSettings = new I2cConnectionSettings(1, 0x76);
            _bmp280 = new UnixI2cDevice(bmpSettings);
        }

        public async Task<double> GetPressure()
        {
            using (var sensor = new Bme280(_bmp280))
            {
                sensor.SetPowerMode(PowerMode.Forced);
                double pressure = await sensor.ReadPressureAsync();
                var temp = await sensor.ReadTemperatureAsync();
                var alt = await sensor.ReadAltitudeAsync(pressure);
                double humid = await sensor.ReadHumidityAsync();
                _logger.LogDebug(
                    $"Pressure value: {pressure}; PowerMode: {sensor.ReadPowerMode()}; Temperature: {temp.Celsius}; Altitude: {alt}; Humidity: {humid}");

                return pressure;
            }
        }
    }
}