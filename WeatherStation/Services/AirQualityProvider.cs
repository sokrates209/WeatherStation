using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using WeatherStation.Events;
using WeatherStation.Model;
using WeatherStation.Settings;

namespace WeatherStation.Services
{
    public interface IAirQualityProvider : IDisposable
    {
        Task Initialize();
    }

    public class AirQualityProvider : IAirQualityProvider
    {
        private readonly ILogger<AirQualityProvider> _logger;
        private readonly IMediator _mediator;
        private readonly AirQualitySettings _settings;
        private SerialPortStream _serial;
        private DateTime _lastUpdate;

        const byte SDS_HEADER1 = 0xAA;
        const byte SDS_HEADER2 = 0xC0;
        const byte SDS_TAIL = 0xAB;

        public AirQualityProvider(ILogger<AirQualityProvider> logger, IMediator mediator, AirQualitySettings settings)
        {
            _logger = logger;
            _mediator = mediator;
            _settings = settings;
        }

        public async Task Initialize()
        {
            _logger.LogInformation("Initializing air quality provider.");
            var i = 0;

            while (true)
            {
                if (i == 5)
                {
                    var ports = SerialPortStream.GetPortNames();
                    _logger.LogWarning(
                        $"Failed to initialize sensor connection. Change port. Available ports {string.Join(',', ports)}");
                    return;
                }

                try
                {
                    _logger.LogInformation($"Connecting to: {_settings.PortName}");
                    
                    _serial = new SerialPortStream(_settings.PortName, 9600, 8, Parity.None, StopBits.One);
                    _serial.DataReceived += SerialOnDataReceived;
                    _serial.ErrorReceived += OnErrorReceived;
                    _serial.OpenDirect();
                    
                    if(!_serial.IsOpen)
                        throw new InvalidOperationException();
                    _serial.Handshake = Handshake.None;
                    _serial.ReadTimeout = 1000;
                    _serial.NewLine = "\r\n";

                    _lastUpdate = DateTime.Now;
                    _logger.LogInformation("Provider initialized.");
                    return;
                }
                catch (Exception e)
                {
                    i++;
                    _logger.LogWarning($"Failed to initialize. Re-trying {i} time.");
                }

                await Task.Delay(1000);
            }
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _logger.LogError($"{e.EventType}");
        }

        private void SerialOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(!(sender is SerialPortStream serial)) return;
            
            var rcvLength = serial.BytesToRead;
            if (rcvLength == 0) return;

            var data = new byte[rcvLength];
            serial.Read(data, 0, rcvLength);
            
           // _logger.LogDebug($"Received: {string.Join(',', data)}");
            
            var data_cc = new List<byte>();
            data_cc.AddRange(data);

            if (data_cc.IndexOf(SDS_TAIL) - data_cc.IndexOf(SDS_HEADER1) == 9)
            {
                Parse(data);
            }
        }

        private void Parse(byte[] data)
        {
            // packet format: AA C0 PM25_Low PM25_High PM10_Low PM10_High 0 0 CRC AB

            if (data.Length != 10) return;
            if (data[0] != SDS_HEADER1 || data[1] != SDS_HEADER2 || data[9] != SDS_TAIL) return;

            byte crc = 0;
            for (var i = 2; i < 8; i++)
            {
                crc += data[i];
            }

            if (crc != data[8]) return; // crc error

            var pm25 = (float) ((int) data[2] | (int) (data[3] << 8)) / 10;
            var pm10 = (float) ((int) data[4] | (int) (data[5] << 8)) / 10;

            TryPublish(pm25, pm10).Wait();
        }

        private async Task TryPublish(float pm25, float pm10)
        {
            var currentTime = DateTime.Now;
            if ((currentTime - _lastUpdate).TotalSeconds >= _settings.UpdateIntervalInSeconds)
            {
               
                _logger.LogInformation($"PM10: {pm10}, PM2.5: {pm25}"); await _mediator.Publish(new ReadingsUpdated(new TwinCollection()
                {
                    [nameof(AirQualitySensors)] = new AirQualitySensors(pm25, pm10)
                }));

                _lastUpdate = currentTime;
            }
        }

        public void Dispose()
        {
            _serial?.Dispose();
        }
    }
}