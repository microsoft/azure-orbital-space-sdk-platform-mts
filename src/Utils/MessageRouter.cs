namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.Utils;

/// <summary>
/// Allows async events to be stored and called from one spot
/// </summary>
public class MessageRouter {
    private readonly ILogger<MessageRouter> _logger;
    public EventHandler<MessageFormats.HostServices.Position.PositionUpdateResponse>? PositionUpdateResponseReceivedEvent;
    private readonly IServiceProvider _serviceProvider;
    public MessageRouter(ILogger<MessageRouter> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _logger.LogInformation("Services.{serviceName} Initialized.", nameof(MessageRouter));

    }

    /// <summary>
    /// Notify all the services that something happened
    /// </summary>
    /// <returns>void</returns>
    internal void CallEventHandlers<T>(T message, string sourceAppId, EventHandler<T>? EventHandler) {
        if (EventHandler == null || message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {

            foreach (Delegate handler in EventHandler.GetInvocationList()) {
                Task.Factory.StartNew(
                    () => handler.DynamicInvoke(sourceAppId, message));
            }
        }
    }
}
