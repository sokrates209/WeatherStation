using MediatR;
using Microsoft.Azure.Devices.Shared;

namespace WeatherStation.Events
{
    public class ReadingsUpdated :INotification
    {
        public TwinCollection TwinCollection { get; }

        public ReadingsUpdated(TwinCollection twinCollection)
        {
            TwinCollection = twinCollection;
        }
    }
}