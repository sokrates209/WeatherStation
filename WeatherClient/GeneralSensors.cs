namespace WeatherClient
{
    public class GeneralSensors
    {
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