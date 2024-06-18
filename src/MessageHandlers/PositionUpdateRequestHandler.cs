using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> {
    private void PositionUpdateRequestHandler(MessageFormats.HostServices.Position.PositionUpdateRequest? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            DateTime maxTimeToWait = DateTime.Now.Add(TimeSpan.FromMilliseconds(_appConfig.MESSAGE_RESPONSE_TIMEOUT_MS));

            MessageFormats.HostServices.Position.PositionUpdateResponse? response = null;
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            // Register a callback event to catch the response
            void PositionUpdateResponseCallback(object? sender, MessageFormats.HostServices.Position.PositionUpdateResponse _response) {
                if (_response.ResponseHeader.TrackingId == message.RequestHeader.TrackingId) {
                    _logger.LogDebug("Received '{messageType}' from '{senderId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, sender?.ToString(), message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
                    response = _response;
                    _messageRouter.PositionUpdateResponseReceivedEvent -= PositionUpdateResponseCallback;
                }
            }


            _messageRouter.PositionUpdateResponseReceivedEvent += PositionUpdateResponseCallback;

            _logger.LogInformation("Routing message type '{messageType}' to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, $"hostsvc-{MessageFormats.Common.HostServices.Position}", message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
            _client.DirectToApp(appId: $"hostsvc-{MessageFormats.Common.HostServices.Position}", message: message);


            while (response == null && DateTime.Now <= maxTimeToWait) {
                Task.Delay(100).Wait();
            }

            // If response is null then update to show a timeout message
            response = response ?? new() {
                ResponseHeader = new() {
                    TrackingId = message.RequestHeader.TrackingId,
                    CorrelationId = message.RequestHeader.CorrelationId,
                    Status = MessageFormats.Common.StatusCodes.Timeout,
                    Message = "Didn't not receive a response from 'hostsvc-position' in time."
                }
            };

            _logger.LogDebug("Passing message '{messageType}' and '{responseType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, response.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            (MessageFormats.HostServices.Position.PositionUpdateRequest? output_request, MessageFormats.HostServices.Position.PositionUpdateResponse? output_response) =
                                            _pluginLoader.CallPlugins<MessageFormats.HostServices.Position.PositionUpdateRequest?, Plugins.PluginBase, MessageFormats.HostServices.Position.PositionUpdateResponse>(
                                                orig_request: message, orig_response: response,
                                                pluginDelegate: _pluginDelegates.PositionUpdateRequest);

            _logger.LogDebug("Plugins finished processing '{messageType}' and '{responseType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, response.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            if (output_response == null || output_request == null) {
                _logger.LogInformation("Plugins nullified '{messageType}' or '{output_requestMessageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}')", response.GetType().Name, message.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
                return;
            }

            response = output_response;
            message = output_request;

            _logger.LogInformation("Routing message type '{messageType}' to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.RequestHeader.AppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
            _client.DirectToApp(appId: fullMessage.SourceAppId, message: response);
        };
    }

}