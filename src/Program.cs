namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("/workspaces/platform-mts-config/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("/workspaces/platform-mts/src/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("/workspaces/platform-mts/src/appsettings.{env:DOTNET_ENVIRONMENT}.json", optional: true, reloadOnChange: true).Build();

        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(50051, o => o.Protocols = HttpProtocols.Http2))
        .ConfigureServices((services) => {
            services.AddAzureOrbitalFramework();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.SensorData>, MessageHandler<MessageFormats.HostServices.Sensor.SensorData>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableRequest>, MessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableResponse>, MessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableResponse>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckRequest>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingRequest>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingResponse>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingResponse>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Link.LinkResponse>, MessageHandler<MessageFormats.HostServices.Link.LinkResponse>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Position.PositionUpdateRequest>, MessageHandler<MessageFormats.HostServices.Position.PositionUpdateRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Position.PositionUpdateResponse>, MessageHandler<MessageFormats.HostServices.Position.PositionUpdateResponse>>();

            services.AddSingleton<Utils.PluginDelegates>();
            services.AddSingleton<Utils.MessageRouter>();


        }).ConfigureLogging((logging) => {
            logging.AddProvider(new Microsoft.Extensions.Logging.SpaceFX.Logger.HostSvcLoggerProvider());
            logging.AddConsole();
        });

        var app = builder.Build();

        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapGrpcService<Microsoft.Azure.SpaceFx.Core.Services.MessageReceiver>();
            endpoints.MapGet("/", async context => {
                await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            });
        });

        // Add a middleware to catch exceptions and stop the host gracefully
        app.Use(async (context, next) => {
            try {
                await next.Invoke();
            } catch (Exception ex) {
                Console.Error.WriteLine($"Triggering shutdown due to exception caught in global exception handler.  Error: {ex.Message}.  Stack Trace: {ex.StackTrace}");

                // Stop the host gracefully so it triggers the pod to error
                var lifetime = context.RequestServices.GetService<IHostApplicationLifetime>();
                lifetime?.StopApplication();
            }
        });

        app.Run();
    }
}