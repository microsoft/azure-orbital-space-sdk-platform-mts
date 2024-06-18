namespace Microsoft.Azure.SpaceFx.PlatformServices.MessageTranslationService.IntegrationTests.Tests;

[Collection(nameof(TestSharedContext))]
public class LinkTests : IClassFixture<TestSharedContext> {
    readonly TestSharedContext _context;

    public LinkTests(TestSharedContext context) {
        _context = context;
    }

    [Fact]

    public async Task LinkResponseTest() {
        DateTime maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG);
        MessageFormats.HostServices.Link.LinkResponse? response = null;

        var (inbox, outbox, root) = await TestSharedContext.SPACEFX_CLIENT.GetXFerDirectories();
        System.IO.File.Copy("/workspaces/platform-mts/test/sampleData/astronaut.jpg", string.Format($"{outbox}/astronaut.jpg"), overwrite: true);

        MessageFormats.HostServices.Link.LinkRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
            FileName = "astronaut.jpg",
            DestinationAppId = "platform-mts",
            LinkType = MessageFormats.HostServices.Link.LinkRequest.Types.LinkType.App2App,
            Overwrite = true
        };


        string appId = await TestSharedContext.SPACEFX_CLIENT.GetAppID();


        // Register a callback event to catch the response
        void LinkResponseEventHandler(object? _, MessageFormats.HostServices.Link.LinkResponse _response) {
            if (_response.ResponseHeader.CorrelationId == request.RequestHeader.CorrelationId) {
                response = _response;
                Console.WriteLine($"LinkResponse received: {response.ResponseHeader.Status}");
                MessageHandler<MessageFormats.HostServices.Link.LinkResponse>.MessageReceivedEvent -= LinkResponseEventHandler;
            }
        }

        MessageHandler<MessageFormats.HostServices.Link.LinkResponse>.MessageReceivedEvent += LinkResponseEventHandler;


        await TestSharedContext.SPACEFX_CLIENT.DirectToApp("hostsvc-link", request);

        while (response == null && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
        }


        if (response == null) throw new TimeoutException($"Failed to hear {nameof(response)} heartbeat after {TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG}.  Please check that {TestSharedContext.TARGET_SVC_APP_ID} is deployed");

        Assert.Equal(MessageFormats.Common.StatusCodes.Pending, response.ResponseHeader.Status);

        maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG * 2);
        bool fileExists = File.Exists(Path.Combine(inbox, "astronaut.jpg"));

        while (!(fileExists) && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
            fileExists = File.Exists(Path.Combine(inbox, "astronaut.jpg"));
        }


        Assert.False(File.Exists(Path.Combine(inbox, "astronaut.jpg")));
    }

    [Fact]
    public async Task LinkResponseTest_PayloadApp() {
        DateTime maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG);
        MessageFormats.HostServices.Link.LinkResponse? response = null;

        var (inbox, outbox, root) = await TestSharedContext.SPACEFX_CLIENT.GetXFerDirectories();
        System.IO.File.Copy("/workspaces/platform-mts/test/sampleData/astronaut.jpg", string.Format($"{outbox}/astronaut.jpg"), overwrite: true);

        MessageFormats.HostServices.Link.LinkRequest request = new() {
            RequestHeader = new() {
                TrackingId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
            },
            FileName = "astronaut.jpg",
            DestinationAppId = "platform-mts",
            LinkType = MessageFormats.HostServices.Link.LinkRequest.Types.LinkType.App2App,
            Overwrite = true
        };


        string appId = await TestSharedContext.SPACEFX_CLIENT.GetAppID();

        request.RequestHeader.Metadata.Add("SOURCE_PAYLOAD_APP_ID", appId);


        // Register a callback event to catch the response
        void LinkResponseEventHandler(object? _, MessageFormats.HostServices.Link.LinkResponse _response) {
            if (_response.ResponseHeader.CorrelationId == request.RequestHeader.CorrelationId) {
                response = _response;
                Console.WriteLine($"LinkResponse received: {response.ResponseHeader.Status}");
                MessageHandler<MessageFormats.HostServices.Link.LinkResponse>.MessageReceivedEvent -= LinkResponseEventHandler;
            }
        }

        MessageHandler<MessageFormats.HostServices.Link.LinkResponse>.MessageReceivedEvent += LinkResponseEventHandler;


        await TestSharedContext.SPACEFX_CLIENT.DirectToApp("hostsvc-link", request);

        while (response == null && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
        }


        if (response == null) throw new TimeoutException($"Failed to hear {nameof(response)} heartbeat after {TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG}.  Please check that {TestSharedContext.TARGET_SVC_APP_ID} is deployed");

        Assert.Equal(MessageFormats.Common.StatusCodes.Pending, response.ResponseHeader.Status);

        maxTimeToWait = DateTime.Now.Add(TestSharedContext.MAX_TIMESPAN_TO_WAIT_FOR_MSG);
        bool fileExists = File.Exists(Path.Combine(inbox, "astronaut.jpg"));

        while (!(fileExists) && DateTime.Now <= maxTimeToWait) {
            Thread.Sleep(100);
            fileExists = File.Exists(Path.Combine(inbox, "astronaut.jpg"));
        }


        Assert.True(File.Exists(Path.Combine(inbox, "astronaut.jpg")));
        //Clean_up
        System.IO.File.Delete(string.Format($"{inbox}/astronaut.jpg"));
    }


}