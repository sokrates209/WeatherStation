namespace WeatherClient
{
    public class AzureSettings
    {
        public IotHubSettings IotHubSettings { get; set; }
    }

    public class IotHubSettings
    {
        public string ConnectionString { get; set; }
        public int DelayTime { get; set; }
    }
}