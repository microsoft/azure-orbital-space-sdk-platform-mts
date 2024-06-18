namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.IntegrationTests.Tests;

[Collection(nameof(TestSharedContext))]
public class TaskingPreCheckTests : IClassFixture<TestSharedContext> {
    readonly TestSharedContext _context;

    public TaskingPreCheckTests(TestSharedContext context) {
        _context = context;
    }

    [Fact]
    public async Task TaskingQueryAndResponse() {
        DateTime maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG);
        MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? response = null;

        var trackingId = Guid.NewGuid().ToString();
        var request = new MessageFormats.HostServices.Sensor.TaskingPreCheckRequest() {
            RequestHeader = new() {
                TrackingId = trackingId,
            },
            SensorID = "TestSensorAlpha"
        };

        // Register a callback event to catch the response
        void TaskingPreCheckResponseEventHandler(object? _, MessageFormats.HostServices.Sensor.TaskingPreCheckResponse _response) {
            response = _response;
            MessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>.MessageReceivedEvent -= TaskingPreCheckResponseEventHandler;
        }

        MessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>.MessageReceivedEvent += TaskingPreCheckResponseEventHandler;


        await TestSharedContext.SPACEFX_CLIENT.DirectToApp(TestSharedContext.TARGET_SVC_APP_ID, request);


        while (response == null && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
        }

        if (response == null) throw new TimeoutException($"Failed to hear {nameof(response)} heartbeat after {TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG}.  Please check that {TestSharedContext.TARGET_SVC_APP_ID} is deployed");

        Assert.Equal(MessageFormats.Common.StatusCodes.Successful, response.ResponseHeader.Status);
    }
}