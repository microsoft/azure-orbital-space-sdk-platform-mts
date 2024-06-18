using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> {

    private void TaskingPreCheckRequestHandler(MessageFormats.HostServices.Sensor.TaskingPreCheckRequest? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            // No plugins are loaded.  Automatically route the message to VTH if VTH is online
            if (_client.GetPlugins().Result.Count == 0 &&
                System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != "Development" &&
                _client.ServicesOnline().Any(app => string.Equals(app.AppId, "vth", StringComparison.InvariantCultureIgnoreCase))) {
                _logger.LogInformation("No plugins detected.  Forwarding message type '{messageType}' to VTH (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
                _client.DirectToApp(appId: "VTH", message: message);
                return;
            }

            MessageFormats.HostServices.Sensor.TaskingPreCheckResponse returnResponse = new() {
                ResponseHeader = new() {
                    TrackingId = message.RequestHeader.TrackingId,
                    CorrelationId = message.RequestHeader.CorrelationId,
                    Status = MessageFormats.Common.StatusCodes.Successful
                }
            };

            _logger.LogDebug("Passing message '{messageType}' and '{responseType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);


            (MessageFormats.HostServices.Sensor.TaskingPreCheckRequest? output_request, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? output_response) =
                                            _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.TaskingPreCheckRequest?, Plugins.PluginBase, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>(
                                                orig_request: message, orig_response: returnResponse,
                                                pluginDelegate: _pluginDelegates.TaskingPreCheckRequest);

            _logger.LogDebug("Plugins finished processing '{messageType}' and '{responseType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            if (output_response == null || output_request == null) {
                _logger.LogInformation("Plugins nullified '{messageType}' or '{output_requestMessageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}')", returnResponse.GetType().Name, message.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
                return;
            }

            returnResponse = output_response;
            message = output_request;

            _logger.LogInformation("Routing message type '{messageType}' to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.RequestHeader.AppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
            _client.DirectToApp(appId: fullMessage.SourceAppId, message: returnResponse);
        };
    }

}
