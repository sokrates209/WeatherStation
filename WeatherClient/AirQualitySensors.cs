namespace WeatherClient
{
    public class AirQualitySensors
    {
        public float AirQuality_PM10 { get; set; }
        public float AirQuality_PM25 { get; set; }

        public override string ToString()
        {
            return $"Pm10: {AirQuality_PM10}; Pm2.5: {AirQuality_PM25}.";
        }
    }
}