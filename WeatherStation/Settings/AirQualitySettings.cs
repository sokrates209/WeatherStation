using System.ComponentModel;

namespace WeatherStation.Settings
{
    public class AirQualitySettings
    {
        public string PortName { get; set; }
        public int UpdateIntervalInSeconds { get; set; }
    }
}