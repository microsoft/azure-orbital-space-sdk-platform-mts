
namespace PayloadApp.DebugClient;

public class MessageHandler<T> : Microsoft.Azure.SpaceFx.Core.IMessageHandler<T> where T : notnull {
    private readonly ILogger<MessageHandler<T>> _logger;
    private readonly Microsoft.Azure.SpaceFx.Core.Services.PluginLoader _pluginLoader;
    private readonly IServiceProvider _serviceProvider;
    public MessageHandler(ILogger<MessageHandler<T>> logger, Microsoft.Azure.SpaceFx.Core.Services.PluginLoader pluginLoader, IServiceProvider serviceProvider) {
        _logger = logger;
        _pluginLoader = pluginLoader;
        _serviceProvider = serviceProvider;
    }

    public void MessageReceived(T message, Microsoft.Azure.SpaceFx.MessageFormats.Common.DirectToApp fullMessage) {
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation($"Found {typeof(T).Name}");

            switch (typeof(T).Name) {
                case string messageType when messageType.Equals(typeof(SensorData).Name, StringComparison.CurrentCultureIgnoreCase):
                    SensorDataHandler(response: message as SensorData);
                    break;

                case string messageType when messageType.Equals(typeof(PluginConfigurationResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    PluginConfigurationResponseHandler(response: message as PluginConfigurationResponse);
                    break;

                case string messageType when messageType.Equals(typeof(SensorsAvailableResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    SensorsAvailableResponseHandler(response: message as SensorsAvailableResponse);
                    break;

                case string messageType when messageType.Equals(typeof(TaskingResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    TaskingResponseHandler(response: message as TaskingResponse);
                    break;

                case string messageType when messageType.Equals(typeof(TaskingPreCheckResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    TaskingPreCheckResponseHandler(response: message as TaskingPreCheckResponse);
                    break;

                case string messageType when messageType.Equals(typeof(PositionUpdateResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    PositionUpdateResponseHandler(response: message as PositionUpdateResponse);
                    break;

                case string messageType when messageType.Equals(typeof(LinkResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    LinkResponseHandler(response: message as LinkResponse);
                    break;
            }
        }
    }

    public void SensorDataHandler(SensorData response) {
        _logger.LogInformation($"SensorData received from Sensor ID '{response.SensorID}': {response.Data.ToByteString().ToString(System.Text.Encoding.UTF8)}");
        _logger.LogInformation($"SensorData: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}");

    }

    public void SensorsAvailableResponseHandler(SensorsAvailableResponse response) {
        _logger.LogInformation($"SensorsAvailableResponse Heard from {response.ResponseHeader.AppId}");
        _logger.LogInformation($"SensorsAvailableResponse: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}");
        _logger.LogInformation($"{response.GetType().Name} Sensors ({response.Sensors.Count}): ");
        response.Sensors.ToList().ForEach((sensor) => Console.WriteLine($"...Sensor '{sensor.SensorID}'..."));
    }

    public void TaskingResponseHandler(TaskingResponse response) {
        _logger.LogInformation($"TaskingResponse Heard from {response.ResponseHeader.AppId}");
        _logger.LogInformation($"TaskingResponse: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}");
    }
    public void TaskingPreCheckResponseHandler(TaskingPreCheckResponse response) {
        _logger.LogInformation($"TaskingPreCheckResponse Heard from {response.ResponseHeader.AppId}");
        _logger.LogInformation($"TaskingPreCheckResponse: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}");
    }

    public void PositionUpdateResponseHandler(PositionUpdateResponse response) {
        _logger.LogInformation($"PositionUpdateResponse Heard from {response.ResponseHeader.AppId}");
        _logger.LogInformation($"PositionUpdateResponse: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}");
    }

    public void PluginConfigurationResponseHandler(PluginConfigurationResponse response) {
        _logger.LogInformation($"PluginConfigurationResponse: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}");
    }

    public void LinkResponseHandler(LinkResponse response) {
        _logger.LogInformation($"LinkResponse Heard from {response.ResponseHeader.AppId}");
        _logger.LogInformation($"LinkResponse: {response.ResponseHeader.Status}. TrackingID: {response.ResponseHeader.TrackingId}, FileName {response.LinkRequest.FileName}");
    }
}