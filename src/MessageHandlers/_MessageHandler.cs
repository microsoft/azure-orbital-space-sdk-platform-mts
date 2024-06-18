namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> : Microsoft.Azure.SpaceFx.Core.IMessageHandler<T> where T : notnull {
    private readonly ILogger<MessageHandler<T>> _logger;
    private readonly Utils.PluginDelegates _pluginDelegates;
    private readonly Microsoft.Azure.SpaceFx.Core.Services.PluginLoader _pluginLoader;
    private readonly IServiceProvider _serviceProvider;
    private readonly Core.Client _client;
    private readonly Models.APP_CONFIG _appConfig;
    private readonly Utils.MessageRouter _messageRouter;

    public MessageHandler(ILogger<MessageHandler<T>> logger, Utils.PluginDelegates pluginDelegates, Microsoft.Azure.SpaceFx.Core.Services.PluginLoader pluginLoader, IServiceProvider serviceProvider, Core.Client client, Utils.MessageRouter messageRouter) {
        _logger = logger;
        _pluginDelegates = pluginDelegates;
        _pluginLoader = pluginLoader;
        _serviceProvider = serviceProvider;
        _appConfig = new Models.APP_CONFIG();
        _client = client;
        _messageRouter = messageRouter;
    }

    public void MessageReceived(T message, MessageFormats.Common.DirectToApp fullMessage) => Task.Run(() => {
        using (var scope = _serviceProvider.CreateScope()) {

            if (message == null || EqualityComparer<T>.Default.Equals(message, default)) {
                _logger.LogInformation("Received empty message '{messageType}' from '{appId}'.  Discarding message.", typeof(T).Name, fullMessage.SourceAppId);
                return;
            }

            switch (typeof(T).Name) {
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.SensorData).Name, StringComparison.CurrentCultureIgnoreCase):
                    SensorDataHandler(message: message as MessageFormats.HostServices.Sensor.SensorData, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.SensorsAvailableRequest).Name, StringComparison.CurrentCultureIgnoreCase):
                    SensorsAvailableRequestHandler(message: message as MessageFormats.HostServices.Sensor.SensorsAvailableRequest, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.SensorsAvailableResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    SensorsAvailableResponseHandler(message: message as MessageFormats.HostServices.Sensor.SensorsAvailableResponse, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.TaskingPreCheckRequest).Name, StringComparison.CurrentCultureIgnoreCase):
                    TaskingPreCheckRequestHandler(message: message as MessageFormats.HostServices.Sensor.TaskingPreCheckRequest, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.TaskingPreCheckResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    TaskingPreCheckResponseHandler(message: message as MessageFormats.HostServices.Sensor.TaskingPreCheckResponse, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.TaskingRequest).Name, StringComparison.CurrentCultureIgnoreCase):
                    TaskingRequestHandler(message: message as MessageFormats.HostServices.Sensor.TaskingRequest, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Sensor.TaskingResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    TaskingResponseHandler(message: message as MessageFormats.HostServices.Sensor.TaskingResponse, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Link.LinkResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    LinkResponseHandler(message: message as MessageFormats.HostServices.Link.LinkResponse, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Position.PositionUpdateRequest).Name, StringComparison.CurrentCultureIgnoreCase):
                    PositionUpdateRequestHandler(message: message as MessageFormats.HostServices.Position.PositionUpdateRequest, fullMessage: fullMessage);
                    break;
                case string messageType when messageType.Equals(typeof(MessageFormats.HostServices.Position.PositionUpdateResponse).Name, StringComparison.CurrentCultureIgnoreCase):
                    PositionUpdateResponseHandler(message: message as MessageFormats.HostServices.Position.PositionUpdateResponse, fullMessage: fullMessage);
                    break;
            }
        }
    });
}
