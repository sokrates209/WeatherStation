using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace WeatherClient
{
    public interface IAzureHub
    {
        Task<string> GetProperty(string propName);
        Task Initialize();
    }

    public class AzureHub : IAzureHub
    {
        private static RegistryManager _registryManager;
        private static ServiceClient _serviceClient;
        private const string DeviceId = "WeatherStation";

        public AzureHub(AzureSettings settings)
        {
            _registryManager = RegistryManager.CreateFromConnectionString(settings.IotHubSettings.ConnectionString);
            _serviceClient = ServiceClient.CreateFromConnectionString(settings.IotHubSettings.ConnectionString);
        }

        public async Task Initialize()
        {
            await _registryManager.OpenAsync();
            await _serviceClient.OpenAsync();
        }

        public async Task<string> GetProperty(string name)
        {
            try
            {
                var twin = await _registryManager.GetTwinAsync(DeviceId);

                var propValue = twin.Properties.Reported[name];

                return propValue.ToJson();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}