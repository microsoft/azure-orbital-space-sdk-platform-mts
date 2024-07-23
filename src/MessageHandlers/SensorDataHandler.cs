using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> {
    private void SensorDataHandler(MessageFormats.HostServices.Sensor.SensorData? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);

            _logger.LogDebug("Passing message '{messageType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);

            MessageFormats.HostServices.Sensor.SensorData? pluginResult =
                         _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.SensorData?, Plugins.PluginBase>(
                             orig_request: message,
                             pluginDelegate: _pluginDelegates.SensorData);

            _logger.LogDebug("Plugins finished processing '{messageType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);

            if (pluginResult == null) {
                _logger.LogInformation("Plugins nullified '{messageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);
                return;
            }

            _logger.LogInformation("Routing message type '{messageType}' to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, $"hostsvc-{nameof(MessageFormats.Common.HostServices.Sensor)}", message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);
            _client.DirectToApp(appId: $"hostsvc-{nameof(MessageFormats.Common.HostServices.Sensor)}", message: pluginResult);
        };
    }
}
