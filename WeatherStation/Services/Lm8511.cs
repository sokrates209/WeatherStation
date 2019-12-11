using System.Device.I2c;
using System.Device.I2c.Drivers;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;

namespace WeatherStation.Services
{
    public class Lm8511
    {
        private readonly I2cConnectionSettings _adcSetting;
        private readonly ILogger<UvProvider> _logger;

        public double UV
        {
            get => GetUv();
        }

        public Lm8511(I2cConnectionSettings adcSetting, ILogger<UvProvider> logger)
        {
            _adcSetting = adcSetting;
            _logger = logger;
        }

        private double GetUv()
        {
            var pinOut = new UnixI2cDevice(_adcSetting);
            //var pin3v3 = new UnixI2cDevice(_adcSetting);

            short uvLevel = 0, refLevel = 0;
            using (var adcOut = new Iot.Device.Ads1115.Ads1115(pinOut, InputMultiplexer.AIN0))
            {
                uvLevel = adcOut.ReadRaw();
                _logger.LogDebug($"Output reading: {uvLevel}; Voltage: {adcOut.RawToVoltage(uvLevel)}");
            }

//            using (var adc3v3 = new Iot.Device.Ads1115.Ads1115(pinOut, InputMultiplexer.AIN3))
//            {
//                refLevel = adc3v3.ReadRaw();
//                _logger.LogDebug($"Referance reading: {refLevel}; Voltage: {adc3v3.RawToVoltage(refLevel)}");
//            }

            var outputVoltage = (3.3 / 26205) * uvLevel;

            return Map(outputVoltage, 0.99, 2.8, 0.0, 15.0);
        }

        private static double Map(double x, double inMin, double inMax, double outMin, double outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}