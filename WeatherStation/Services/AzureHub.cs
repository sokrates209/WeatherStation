using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using UnitsNet.Extensions.NumberToDuration;
using WeatherStation.Settings;

namespace WeatherStation.Services
{
    public class AzureHub : IAzureIotHub
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AzureHub> _logger;
        private readonly AzureSettings _settings;
        private DeviceClient _deviceClient;

        public AzureHub(IMediator mediator, ILogger<AzureHub> logger, AzureSettings settings)
        {
            _mediator = mediator;
            _logger = logger;
            _settings = settings;
        }

        public async Task Initialize()
        {
            while (true)
            {
                try
                {
                    _logger.LogInformation("Azure IOT Hub is initializing");

                    _deviceClient =
                        DeviceClient.CreateFromConnectionString(_settings.IotHubSettings.ConnectionString, TransportType.Mqtt);
            
                    _deviceClient.SetConnectionStatusChangesHandler(async (a, r) => await ConnectionStatusChanged(a, r));

                    await _deviceClient.OpenAsync();
            
                    await RegisterTwinUpdateAsync();

                    var props = await GetDesiredProperties();

                    await UpdateDeviceTwin(props);
            
                    _logger.LogDebug("Azure IOT Hub initialized.");
                    
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Azure IOT Hub connection exception: {e}");
                }
                
                _logger.LogError($"Azure IOT Hub initialize will delay re-try for {_settings.IotHubSettings.DelayTime} seconds.");
                await Task.Delay(_settings.IotHubSettings.DelayTime.Seconds().ToTimeSpan());
            }
        }
        
        private async Task DesiredPropertiesUpdated(TwinCollection desiredProperties, object userContext)
        {
            await UpdateDeviceTwin(desiredProperties);
        }
        
        private async Task RegisterTwinUpdateAsync()
        {
            _logger.LogDebug("Registering Device Twin update callback");
            
            await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertiesUpdated, null);
        }
        
        public async Task UpdateReportedProperties(TwinCollection reportedProperties)
        {
            _logger.LogInformation("Updating reported properties...");

            try
            {
                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured on UpdateReportedProperties:", ex);
            }
        }
        
        private async Task<TwinCollection> GetDesiredProperties()
        {
            _logger.LogInformation("Getting desired device twin properties");

            try
            {
                var twin = await _deviceClient.GetTwinAsync();
                return twin.Properties.Desired;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured on GetDesiredProperties:", ex);
            }

            return new TwinCollection();
        }

        private async Task UpdateDeviceTwin(TwinCollection desiredProperties)
        {
            _logger.LogInformation("Desired properties updating...");

            try
            {
                var reportedProperties = new TwinCollection();
                // TODO: Fill properties with values from sensors
                
                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured on UpdateDeviceTwin:", ex);
            }
        }

        private async Task ConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            // https://github.com/Azure/azure-iot-sdk-csharp/pull/741
            //Log.Information("Azure IoT Hub connection status Changed Status: {status} Reason: {reason}", status, reason);

            //if (status == ConnectionStatus.Connected && reason == ConnectionStatusChangeReason.Connection_Ok)
            //{
            //    Log.Information("Client connected (initially and after a successful retry).");
            //}

            if (status == ConnectionStatus.Disabled && reason == ConnectionStatusChangeReason.Client_Close)
            {
                _logger?.LogInformation("Application disposed the client.");
                await TryClose();
                await Initialize();
            }

            if (status == ConnectionStatus.Disconnected && reason == ConnectionStatusChangeReason.Communication_Error)
            {
                _logger?.LogInformation("If no callback subscriptions exist, the client will not automatically connect. A future operation will attempt to reconnect the client.");
                await TryClose();
                await Initialize();
            }

            if (status == ConnectionStatus.Disconnected_Retrying && reason == ConnectionStatusChangeReason.Communication_Error)
            {
                _logger?.LogInformation("If any callback subscriptions exist (methods, twin, events) and connectivity is lost, the client will try to reconnect.");
            }

            if (status == ConnectionStatus.Disconnected && reason == ConnectionStatusChangeReason.Retry_Expired)
            {
                _logger?.LogInformation("Retry timeout. The RetryHandler will attempt to recover links for a duration of OperationTimeoutInMilliseconds (default 4 minutes).");
            }

            if (status == ConnectionStatus.Disconnected && reason == ConnectionStatusChangeReason.Bad_Credential)
            {
                _logger?.LogInformation("UnauthorizedException during Retry.");
                await TryClose();
                await Initialize();
            }

            if (status == ConnectionStatus.Disconnected && reason == ConnectionStatusChangeReason.Device_Disabled)
            {
                _logger?.LogInformation("DeviceDisabledException during Retry.");
                await TryClose();
                await Initialize();
            }
        }

        private async Task TryClose()
        {
            try
            {
                _logger.LogInformation("Azure IoT Hub trying to close");
                
                _deviceClient.SetConnectionStatusChangesHandler(null);
                await _deviceClient.CloseAsync();
                _deviceClient = null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Azure IoT Hub Exception on TryClose. {ex}");
            }
        }
    }

    public interface IAzureIotHub
    {
        Task Initialize();
        Task UpdateReportedProperties(TwinCollection reportedProperties);
    }
}