namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.IntegrationTests.Tests;

[Collection(nameof(TestSharedContext))]
public class SensorDataTests : IClassFixture<TestSharedContext> {
    readonly TestSharedContext _context;

    public SensorDataTests(TestSharedContext context) {
        _context = context;
    }

    [Fact]
    public async Task SensorDataReceived() {
        const string testName = nameof(SensorDataReceived);
        DateTime maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG);

        // Preset the requestId so we don't cross wires with any other tests
        string requestId = Guid.NewGuid().ToString();

        Console.WriteLine($"[{testName}] - START");

        // Set this to null to make it easier for asynchronously tracking
        Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor.SensorData? dataResponse = null;

        MessageFormats.HostServices.Sensor.SensorData request = new() {
            ResponseHeader = new() {
                TrackingId = requestId,
                CorrelationId = requestId,
                Status = MessageFormats.Common.StatusCodes.Successful
            },
            SensorID = "N/A"
        };

        MessageHandler<MessageFormats.HostServices.Sensor.SensorData>.MessageReceivedEvent += (object? _, MessageFormats.HostServices.Sensor.SensorData _dataResponse) => {
            if (_dataResponse.ResponseHeader.TrackingId == request.ResponseHeader.TrackingId) dataResponse = _dataResponse;
        };


        Console.WriteLine($"Sending '{request.GetType().Name}' (TrackingId: '{request.ResponseHeader.TrackingId}')");
        await TestSharedContext.SPACEFX_CLIENT.DirectToApp(TestSharedContext.TARGET_SVC_APP_ID, request);


        Console.WriteLine($"Waiting for response (TrackingId: '{request.ResponseHeader.TrackingId}')");

        while (dataResponse == null && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
        }

        if (dataResponse == null) throw new TimeoutException($"Failed to hear {nameof(dataResponse)} after {TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG}.  Please check that {TestSharedContext.TARGET_SVC_APP_ID} is deployed");


        Assert.Equal(MessageFormats.Common.StatusCodes.Successful, dataResponse.ResponseHeader.Status);
    }
}