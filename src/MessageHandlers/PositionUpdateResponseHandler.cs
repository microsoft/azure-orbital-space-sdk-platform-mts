using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> {
    private void PositionUpdateResponseHandler(MessageFormats.HostServices.Position.PositionUpdateResponse? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            _logger.LogDebug("Passing message '{messageType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);


            MessageFormats.HostServices.Position.PositionUpdateResponse? pluginResult =
                         _pluginLoader.CallPlugins<MessageFormats.HostServices.Position.PositionUpdateResponse?, Plugins.PluginBase>(
                             orig_request: message,
                             pluginDelegate: _pluginDelegates.PositionUpdateResponse);

            _logger.LogDebug("Plugins finished processing '{messageType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            if (pluginResult == null) {
                _logger.LogInformation("Plugins nullified '{messageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
                return;
            }

            _logger.LogInformation("Sending message '{messageType}' to local event handler  (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
            _messageRouter.CallEventHandlers<MessageFormats.HostServices.Position.PositionUpdateResponse>(pluginResult, fullMessage.SourceAppId, _messageRouter.PositionUpdateResponseReceivedEvent);
        };
    }
}

