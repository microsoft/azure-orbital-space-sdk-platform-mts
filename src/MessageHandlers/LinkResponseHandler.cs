using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public partial class MessageHandler<T> {

    private void InboxToOutbox(MessageFormats.HostServices.Link.LinkRequest? linkRequest) {
        string sourceFilePath = _client.GetXFerDirectories().Result.inbox_directory;
        string destFilePath = _client.GetXFerDirectories().Result.outbox_directory;

        if (!string.IsNullOrWhiteSpace(linkRequest.Subdirectory)) {
            sourceFilePath = Path.Combine(sourceFilePath, linkRequest.Subdirectory);
            destFilePath = Path.Combine(destFilePath, linkRequest.Subdirectory);
            System.IO.Directory.CreateDirectory(destFilePath); // This'll create the subdirectory in outbox if needed
        }

        sourceFilePath = Path.Combine(sourceFilePath, linkRequest.FileName);
        destFilePath = Path.Combine(destFilePath, linkRequest.FileName);

        System.IO.File.Move(sourceFilePath, destFilePath, overwrite: true);
    }

    private void SendFileToApp(MessageFormats.HostServices.Link.LinkResponse? message) {

        _logger.LogInformation("Processing {messageType} for potential additional file transfer.(trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

        string? sourcePayloadAppId = message.LinkRequest.RequestHeader.Metadata.FirstOrDefault((_item) => _item.Key == "SOURCE_PAYLOAD_APP_ID").Value;

        // If the LinkRequest was sent by platform-mts itself,  does not contain the required SOURCE_PAYLOAD_APP_ID metadata field, or was successful in it's file transfer? drop the LinkResponse
        if (message.LinkRequest.RequestHeader.AppId == $"platform-{MessageFormats.Common.PlatformServices.Mts}".ToLower() || string.IsNullOrWhiteSpace(sourcePayloadAppId) || message.ResponseHeader.Status != MessageFormats.Common.StatusCodes.Successful) return;

        _logger.LogInformation("Detected '{messageType}' associated with LinkRequest for payload-app file.  (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

        InboxToOutbox(message.LinkRequest);

        var filePath = Path.Combine(Core.GetXFerDirectories().Result.inbox_directory, message.LinkRequest.FileName);

        if (!string.IsNullOrWhiteSpace(message.LinkRequest.Subdirectory)) filePath = Path.Combine(Core.GetXFerDirectories().Result.inbox_directory, message.LinkRequest.Subdirectory, message.LinkRequest.FileName);

        _logger.LogInformation("{methodRequest}: Found SOURCE_PAYLOAD_APP_ID metadata {sourcePayloadAppID} and found file at '{filePath}'.  Sending LinkRequest to '{destinationAppId}'. (TrackingId: {trackingId}, CorrelationId: {correlationId})",
            nameof(SendFileToApp), sourcePayloadAppId, filePath, sourcePayloadAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);


        // Modify the original LinkRequest to transfer the file to SOURCE_PAYLOAD_APP_ID
        message.LinkRequest.DestinationAppId = sourcePayloadAppId;
        message.LinkRequest.RequestHeader.TrackingId = Guid.NewGuid().ToString();
        message.LinkRequest.RequestHeader.AppId = $"platform-{MessageFormats.Common.PlatformServices.Mts}".ToLower();

        Core.DirectToApp(appId: $"hostsvc-{nameof(MessageFormats.Common.HostServices.Link)}", message: message.LinkRequest);
    }

    private void LinkResponseHandler(MessageFormats.HostServices.Link.LinkResponse? message, MessageFormats.Common.DirectToApp fullMessage) {

        if (message == null) return;

        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            // Deployment Scenario: No Plugins
            // No plugins are loaded.  Process LinkResponse to see if the file requires an additional transfer
            if (_client.GetPlugins().Result.Count == 0 &&
                System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != "Development" &&
                _client.ServicesOnline().Any(app => string.Equals(app.AppId, "vth", StringComparison.InvariantCultureIgnoreCase))) {
                SendFileToApp(message);
                return;
            }

            var loadedPlugins = _client.GetPlugins().Result;

            // Integration Test Scenario:
            // 1 Plugin is loaded and it is the IntegrationTestPlugin.  Process LinkResponse to see if the file requires an additional transfer
            if (loadedPlugins.Count == 1 && loadedPlugins[0].PLUGINNAME == "integrationTestPlugin" &&
                _client.ServicesOnline().Any(app => string.Equals(app.AppId, "platform-mts-client", StringComparison.InvariantCultureIgnoreCase))) {
                SendFileToApp(message);
                return;
            }

            // Deployment Plugin Scenario:
            _logger.LogDebug("Passing message '{messageType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);


            MessageFormats.HostServices.Link.LinkResponse? pluginResult =
                        _pluginLoader.CallPlugins<MessageFormats.HostServices.Link.LinkResponse?, Plugins.PluginBase>(
                            orig_request: message,
                            pluginDelegate: _pluginDelegates.LinkResponse);

            _logger.LogDebug("Plugins finished processing '{messageType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
        };
    }
}
