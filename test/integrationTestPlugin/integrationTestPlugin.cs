using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Link;
using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Position;

namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.Plugins;
public class IntegrationTestPlugin : Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.Plugins.PluginBase {
    public override ILogger Logger { get; set; }

    public IntegrationTestPlugin() {
        LoggerFactory loggerFactory = new();
        Logger = loggerFactory.CreateLogger<IntegrationTestPlugin>();
    }

    public override Task BackgroundTask() => Task.CompletedTask;

    public override void ConfigureLogging(ILoggerFactory loggerFactory) => Logger = loggerFactory.CreateLogger<IntegrationTestPlugin>();

    public override Task<PluginHealthCheckResponse> PluginHealthCheckResponse() => Task.FromResult(new MessageFormats.Common.PluginHealthCheckResponse());

    public override Task<LinkResponse?> LinkResponse(LinkResponse? input_response) => Task.Run(() => {
        // This is not run within integration testing
        if (input_response == null) return input_response;
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        Core.DirectToApp(appId: input_response.ResponseHeader.AppId + "-client", message: input_response);
        return null;
    });

    public override Task<(PositionUpdateRequest?, PositionUpdateResponse?)> PositionUpdateRequest(PositionUpdateRequest? input_request, PositionUpdateResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        Core.DirectToApp(appId: input_request.RequestHeader.AppId, message: input_response);
        return (null, null);
    });

    public override Task<PositionUpdateResponse?> PositionUpdateResponse(PositionUpdateResponse? input_response) => (Task<PositionUpdateResponse?>) Task.Run(() => {
        if (input_response != null) Core.DirectToApp(appId: input_response.ResponseHeader.AppId + "-client", message: input_response);
        return null;
    });

    public override Task<SensorData?> SensorData(SensorData? input_request) => (Task<SensorData?>) Task.Run(() => {
        if (input_request != null) Core.DirectToApp(appId: input_request.ResponseHeader.AppId, message: input_request);
        return null;
    });

    public override Task<(SensorsAvailableRequest?, SensorsAvailableResponse?)> SensorsAvailableRequest(SensorsAvailableRequest? input_request, SensorsAvailableResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        Core.DirectToApp(appId: input_request.RequestHeader.AppId, message: input_response);
        return (null, null);
    });

    public override Task<SensorsAvailableResponse?> SensorsAvailableResponse(SensorsAvailableResponse? input_response) => (Task<SensorsAvailableResponse?>) Task.Run(() => {
        if (input_response != null) Core.DirectToApp(appId: input_response.ResponseHeader.AppId, message: input_response);
        return null;
    });

    public override Task<(TaskingPreCheckRequest?, TaskingPreCheckResponse?)> TaskingPreCheckRequest(TaskingPreCheckRequest? input_request, TaskingPreCheckResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        Core.DirectToApp(appId: input_request.RequestHeader.AppId, message: input_response);
        return (null, null);
    });

    public override Task<TaskingPreCheckResponse?> TaskingPreCheckResponse(TaskingPreCheckResponse? input_response) => (Task<TaskingPreCheckResponse?>) Task.Run(() => {
        if (input_response != null) Core.DirectToApp(appId: input_response.ResponseHeader.AppId + "-client", message: input_response);
        return null;
    });

    public override Task<(TaskingRequest?, TaskingResponse?)> TaskingRequest(TaskingRequest? input_request, TaskingResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        Core.DirectToApp(appId: input_request.RequestHeader.AppId, message: input_response);
        return (null, null);
    });

    public override Task<TaskingResponse?> TaskingResponse(TaskingResponse? input_response) => (Task<TaskingResponse?>) Task.Run(() => {
        if (input_response != null) Core.DirectToApp(appId: input_response.ResponseHeader.AppId + "-client", message: input_response);
        return null;
    });
}
