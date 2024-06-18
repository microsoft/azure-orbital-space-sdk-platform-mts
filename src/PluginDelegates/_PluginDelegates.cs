namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.Utils;

public partial class PluginDelegates {
    private readonly ILogger<PluginDelegates> _logger;
    private readonly List<Core.Models.PLUG_IN> _plugins;
    public PluginDelegates(ILogger<PluginDelegates> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _plugins = serviceProvider.GetService<List<Core.Models.PLUG_IN>>() ?? new List<Core.Models.PLUG_IN>();
    }

    internal MessageFormats.HostServices.Sensor.SensorData? SensorData((MessageFormats.HostServices.Sensor.SensorData? input_request, Plugins.PluginBase plugin) input) {
        const string methodName = nameof(input.plugin.SensorData);

        if (input.input_request is null || input.input_request is default(MessageFormats.HostServices.Sensor.SensorData)) {
            _logger.LogDebug("Plugin {pluginName} / {pluginMethod}: Received empty input.  Returning empty results", input.plugin.ToString(), methodName);
            return input.input_request;
        }
        _logger.LogDebug("Plugin {pluginMethod}: START", methodName);

        try {
            Task<MessageFormats.HostServices.Sensor.SensorData?> pluginTask = input.plugin.SensorData(input_request: input.input_request);
            pluginTask.Wait();
            input.input_request = pluginTask.Result;
        } catch (Exception ex) {
            _logger.LogError("Plugin {pluginName} / {pluginMethod}: Error: {errorMessage}", input.plugin.ToString(), methodName, ex.Message);
        }

        _logger.LogDebug("Plugin {pluginName} / {pluginMethod}: END", input.plugin.ToString(), methodName);
        return input.input_request;
    }
}
