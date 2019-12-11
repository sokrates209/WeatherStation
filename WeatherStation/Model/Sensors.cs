using Newtonsoft.Json;

namespace WeatherStation.Model
{
    public class AirQualitySensors
    {
        [JsonConstructor]
        public AirQualitySensors()
        {
        }

        public AirQualitySensors(float pm25, float pm10)
        {
            AirQuality_PM25 = pm25;
            AirQuality_PM10 = pm10;
        }

        public float AirQuality_PM25 { get; set; }
        public float AirQuality_PM10 { get; set; }
    }

    public class GeneralSensors
    {
        [JsonConstructor]
        public GeneralSensors()
        {
        }

        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double LightLevel { get; set; }

        public override string ToString()
        {
            return $"Temperature: {Temperature}; Humidity: {Humidity}; Pressure: {Pressure}; UvLevel: {LightLevel}";
        }
    }
}