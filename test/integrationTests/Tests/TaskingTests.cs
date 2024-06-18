namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.IntegrationTests.Tests;

[Collection(nameof(TestSharedContext))]
public class TaskingTests : IClassFixture<TestSharedContext> {
    readonly TestSharedContext _context;

    public TaskingTests(TestSharedContext context) {
        _context = context;
    }

    [Fact]
    public async Task TaskingQueryAndResponse() {
        DateTime maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG);
        MessageFormats.HostServices.Sensor.TaskingResponse? response = null;

        var trackingId = Guid.NewGuid().ToString();
        var request = new MessageFormats.HostServices.Sensor.TaskingRequest() {
            RequestHeader = new() {
                TrackingId = trackingId,
            },
            SensorID = "TestSensorAlpha"
        };

        // Register a callback event to catch the response
        void TaskingResponseEventHandler(object? _, MessageFormats.HostServices.Sensor.TaskingResponse _response) {
            response = _response;
            MessageHandler<MessageFormats.HostServices.Sensor.TaskingResponse>.MessageReceivedEvent -= TaskingResponseEventHandler;
        }

        MessageHandler<MessageFormats.HostServices.Sensor.TaskingResponse>.MessageReceivedEvent += TaskingResponseEventHandler;


        await TestSharedContext.SPACEFX_CLIENT.DirectToApp(TestSharedContext.TARGET_SVC_APP_ID, request);


        while (response == null && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
        }

        if (response == null) throw new TimeoutException($"Failed to hear {nameof(response)} heartbeat after {TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG}.  Please check that {TestSharedContext.TARGET_SVC_APP_ID} is deployed");

        Assert.Equal(MessageFormats.Common.StatusCodes.Successful, response.ResponseHeader.Status);
    }
}