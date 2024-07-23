using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> {
    private void TaskingResponseHandler(MessageFormats.HostServices.Sensor.TaskingResponse? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {

            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);

            _logger.LogDebug("Passing message '{messageType}' and '{responseType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);

            MessageFormats.HostServices.Sensor.TaskingResponse? pluginResult =
                         _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.TaskingResponse?, Plugins.PluginBase>(
                             orig_request: message,
                             pluginDelegate: _pluginDelegates.TaskingResponse);

            _logger.LogDebug("Plugins finished processing '{messageType}' and '{responseType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);

            if (pluginResult == null) {
                _logger.LogInformation("Plugins nullified '{messageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);
                return;
            } else {
                _logger.LogInformation("Sending message '{messageType}' to '{appId}'  (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", message.GetType().Name, MessageFormats.Common.HostServices.Sensor, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId, message.ResponseHeader.Status);
                _client.DirectToApp(appId: $"hostsvc-{MessageFormats.Common.HostServices.Sensor}", message: pluginResult);
            }
        };
    }
}
