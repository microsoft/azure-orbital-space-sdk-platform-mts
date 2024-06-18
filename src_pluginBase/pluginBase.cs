namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.Plugins;
public abstract class PluginBase : Microsoft.Azure.SpaceFx.Core.IPluginBase, IPluginBase {
    public abstract ILogger Logger { get; set; }
    public abstract Task BackgroundTask();
    public abstract void ConfigureLogging(ILoggerFactory loggerFactory);
    public abstract Task<MessageFormats.Common.PluginHealthCheckResponse> PluginHealthCheckResponse();

    // Link Service Stuff
    public abstract Task<MessageFormats.HostServices.Link.LinkResponse?> LinkResponse(MessageFormats.HostServices.Link.LinkResponse? input_response);

    // Position Service Stuff
    public abstract Task<(MessageFormats.HostServices.Position.PositionUpdateRequest?, MessageFormats.HostServices.Position.PositionUpdateResponse?)> PositionUpdateRequest(MessageFormats.HostServices.Position.PositionUpdateRequest? input_request, MessageFormats.HostServices.Position.PositionUpdateResponse? input_response);
    public abstract Task<MessageFormats.HostServices.Position.PositionUpdateResponse?> PositionUpdateResponse(MessageFormats.HostServices.Position.PositionUpdateResponse? input_response);

    // Sensor Service Stuff
    public abstract Task<(MessageFormats.HostServices.Sensor.SensorsAvailableRequest?, MessageFormats.HostServices.Sensor.SensorsAvailableResponse?)> SensorsAvailableRequest(MessageFormats.HostServices.Sensor.SensorsAvailableRequest? input_request, MessageFormats.HostServices.Sensor.SensorsAvailableResponse? input_response);
    public abstract Task<MessageFormats.HostServices.Sensor.SensorsAvailableResponse?> SensorsAvailableResponse(MessageFormats.HostServices.Sensor.SensorsAvailableResponse? input_response);
    public abstract Task<(MessageFormats.HostServices.Sensor.TaskingPreCheckRequest?, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse?)> TaskingPreCheckRequest(MessageFormats.HostServices.Sensor.TaskingPreCheckRequest? input_request, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? input_response);
    public abstract Task<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse?> TaskingPreCheckResponse(MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? input_response);
    public abstract Task<(MessageFormats.HostServices.Sensor.TaskingRequest?, MessageFormats.HostServices.Sensor.TaskingResponse?)> TaskingRequest(MessageFormats.HostServices.Sensor.TaskingRequest? input_request, MessageFormats.HostServices.Sensor.TaskingResponse? input_response);
    public abstract Task<MessageFormats.HostServices.Sensor.TaskingResponse?> TaskingResponse(MessageFormats.HostServices.Sensor.TaskingResponse? input_response);
    public abstract Task<MessageFormats.HostServices.Sensor.SensorData?> SensorData(MessageFormats.HostServices.Sensor.SensorData? input_request);

}

public interface IPluginBase {
    ILogger Logger { get; set; }
    Task<(MessageFormats.HostServices.Position.PositionUpdateRequest?, MessageFormats.HostServices.Position.PositionUpdateResponse?)> PositionUpdateRequest(MessageFormats.HostServices.Position.PositionUpdateRequest? input_request, MessageFormats.HostServices.Position.PositionUpdateResponse? input_response);
    Task<MessageFormats.HostServices.Position.PositionUpdateResponse?> PositionUpdateResponse(MessageFormats.HostServices.Position.PositionUpdateResponse? input_response);

    Task<(MessageFormats.HostServices.Sensor.SensorsAvailableRequest?, MessageFormats.HostServices.Sensor.SensorsAvailableResponse?)> SensorsAvailableRequest(MessageFormats.HostServices.Sensor.SensorsAvailableRequest? input_request, MessageFormats.HostServices.Sensor.SensorsAvailableResponse? input_response);
    Task<MessageFormats.HostServices.Sensor.SensorsAvailableResponse?> SensorsAvailableResponse(MessageFormats.HostServices.Sensor.SensorsAvailableResponse? input_response);
    Task<(MessageFormats.HostServices.Sensor.TaskingPreCheckRequest?, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse?)> TaskingPreCheckRequest(MessageFormats.HostServices.Sensor.TaskingPreCheckRequest? input_request, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? input_response);
    Task<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse?> TaskingPreCheckResponse(MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? input_response);
    Task<(MessageFormats.HostServices.Sensor.TaskingRequest?, MessageFormats.HostServices.Sensor.TaskingResponse?)> TaskingRequest(MessageFormats.HostServices.Sensor.TaskingRequest? input_request, MessageFormats.HostServices.Sensor.TaskingResponse? input_response);
    Task<MessageFormats.HostServices.Sensor.TaskingResponse?> TaskingResponse(MessageFormats.HostServices.Sensor.TaskingResponse? input_response);
    Task<MessageFormats.HostServices.Sensor.SensorData?> SensorData(MessageFormats.HostServices.Sensor.SensorData? input_request);

    Task<MessageFormats.HostServices.Link.LinkResponse?> LinkResponse(MessageFormats.HostServices.Link.LinkResponse? input_response);
}
