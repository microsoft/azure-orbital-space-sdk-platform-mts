namespace PayloadApp.DebugClient;

public class MessageSender : BackgroundService {
    private readonly ILogger<MessageSender> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Microsoft.Azure.SpaceFx.Core.Client _client;
    private readonly string _appId;
    private readonly string _hostSvcAppId;

    public MessageSender(ILogger<MessageSender> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _client = _serviceProvider.GetService<Microsoft.Azure.SpaceFx.Core.Client>() ?? throw new NullReferenceException($"{nameof(Microsoft.Azure.SpaceFx.Core.Client)} is null");
        _appId = _client.GetAppID().Result;
        _hostSvcAppId = _appId.Replace("-client", "");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("MessageSender running at: {time}", DateTimeOffset.Now);

            await QuerySensorsAvailable();
            await SensorTaskingPreCheck();
            await SensorTasking();
            await UpdatePosition();
            await SendFileRootDirectory();
        }
    }

    private void ListHeardServices() {
        System.Threading.Thread.Sleep(3000);
        _logger.LogInformation("Apps Online:");
        _client.ServicesOnline().ForEach((pulse) => Console.WriteLine($"...{pulse.AppId}..."));
    }


    private async Task QuerySensorsAvailable() {
        _logger.LogInformation("Querying Available Sensors:");
        Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor.SensorsAvailableRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString()
            }
        };

        await _client.DirectToApp(appId: _hostSvcAppId, message: request);
    }

    private async Task SensorTaskingPreCheck() {
        Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor.TaskingPreCheckRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString()
            },
            SensorID = "DemoTemperatureSensor"
        };

        await _client.DirectToApp(appId: _hostSvcAppId, message: request);
    }

    private async Task SensorTasking() {

        Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor.TaskingRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString()
            },
            SensorID = "DemoTemperatureSensor"
        };


        await _client.DirectToApp(appId: _hostSvcAppId, message: request);
    }

    private async Task PluginConfigurationRequest() {
        PluginConfigurationRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString()
            }
        };

        _logger.LogInformation("Sending Plugin Configuration Request");

        VerifyTopicHasSensorId(request, "Testing");

        await _client.DirectToApp(appId: _hostSvcAppId, message: request);
    }

    private void VerifyTopicHasSensorId(IMessage message, string topic) {
        string sensorId = string.Empty;
        _logger.LogInformation(message.Descriptor.FullName);
    }

    public async Task UpdatePosition() {
        _logger.LogInformation("Sending Position Request:");
        Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Position.PositionUpdateRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString()
            },
            Position = new Position() {
                PositionTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Point = new Position.Types.Point() {
                    X = 1,
                    Y = 2,
                    Z = 3,
                },
                Attitude = new Position.Types.Attitude() {
                    X = 1,
                    Y = 2,
                    Z = 3,
                    K = 4
                }
            }
        };

        await _client.DirectToApp(_hostSvcAppId, request);
    }

    private async Task SendFileRootDirectory() {
        var (inbox, outbox, root) = _client.GetXFerDirectories().Result;
        System.IO.File.Copy("/workspaces/platform-mts/test/sampleData/astronaut.jpg", string.Format($"{outbox}/astronaut.jpg"), overwrite: true);

        Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Link.LinkRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
            FileName = "astronaut.jpg",
            DestinationAppId = "platform-mts",
            LinkType = LinkRequest.Types.LinkType.Downlink
        };

        request.RequestHeader.Metadata.Add("SOURCE_PAYLOAD_APP_ID", Core.GetAppID().Result);

        await _client.DirectToApp(appId: "hostsvc-link", message: request);
    }
}
