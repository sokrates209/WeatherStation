using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WeatherStation.Services;

namespace WeatherStation.Events
{
    public class ReadingsUpdatedEventHandler : INotificationHandler<ReadingsUpdated>
    {
        private readonly IAzureIotHub _azureIotHub;

        public ReadingsUpdatedEventHandler(IAzureIotHub azureIotHub)
        {
            _azureIotHub = azureIotHub;
        }

        public async Task Handle(ReadingsUpdated notification, CancellationToken cancellationToken)
        {
            await _azureIotHub.UpdateReportedProperties(notification.TwinCollection);
        }
    }
}