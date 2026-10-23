namespace sample.gateway.Tests;

using System.Net;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

[SupportedOSPlatform("windows")]
public class CommandAllocationEnvironmentPatchTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("""{"error":{"code":"NotFound","message":"No allocation document."}}""")]
    public void MissingDocumentCreatesRequestedCurrencyAllocation(string body)
    {
        using var fixture = new Fixture(HttpStatusCode.NotFound, body);

        Assert.Equal(0, fixture.Command.OnRun());

        Assert.Equal(2, fixture.Handler.Requests.Count);
        Assert.Equal(HttpMethod.Get, fixture.Handler.Requests[0].Method);
        var patch = fixture.Handler.Requests[1];
        Assert.Equal(HttpMethod.Patch, patch.Method);
        Assert.Contains("/licensing/environments/22222222-2222-2222-2222-222222222222/allocations?", patch.Url);
        var expected = JObject.Parse("""
            {
              "environmentId": "22222222-2222-2222-2222-222222222222",
              "currencyAllocations": [{ "currencyType": "MCSMessages", "allocated": 12 }]
            }
            """);
        Assert.True(JToken.DeepEquals(expected, JObject.Parse(patch.Body)), patch.Body);
    }

    [Theory]
    [InlineData("""{"currencyAllocations":[{"currencyType":"MCSMessages","allocated":1}]}""")]
    [InlineData("{}")]
    public void ExistingDocumentIsPatchedWhenCurrencyNeedsUpdating(string body)
    {
        using var fixture = new Fixture(HttpStatusCode.OK, body);

        Assert.Equal(0, fixture.Command.OnRun());
        Assert.Equal(HttpMethod.Patch, fixture.Handler.Requests[1].Method);
    }

    [Fact]
    public void MatchingAllocationDoesNotSendPatch()
    {
        using var fixture = new Fixture(HttpStatusCode.OK,
            """{"currencyAllocations":[{"currencyType":"MCSMessages","allocated":12}]}""");

        Assert.Equal(-1, fixture.Command.OnRun());
        Assert.Single(fixture.Handler.Requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, """{"error":{"code":"Unauthorized"}}""")]
    [InlineData(HttpStatusCode.InternalServerError, "")]
    [InlineData(HttpStatusCode.OK, "")]
    [InlineData(HttpStatusCode.OK, "null")]
    [InlineData(HttpStatusCode.OK, "not json")]
    [InlineData(null, "")]
    public void FailedOrInvalidGetDoesNotSendPatch(HttpStatusCode? statusCode, string body)
    {
        using var fixture = new Fixture(statusCode, body);

        Assert.Equal(-1, fixture.Command.OnRun());
        Assert.Single(fixture.Handler.Requests);
    }

    [Fact]
    public void FailedPatchReturnsFailure()
    {
        using var fixture = new Fixture(HttpStatusCode.NotFound, "", HttpStatusCode.InternalServerError);

        Assert.Equal(-1, fixture.Command.OnRun());
        Assert.Equal(2, fixture.Handler.Requests.Count);
    }

    [Fact]
    public void WhatIfWithMissingDocumentDoesNotSendPatch()
    {
        using var fixture = new Fixture(HttpStatusCode.NotFound, "");
        fixture.Command.Opts.WhatIf = true;

        Assert.Equal(0, fixture.Command.OnRun());
        Assert.Single(fixture.Handler.Requests);
    }

    [Fact]
    public void LegacySendStillReturnsEmptyForHttpErrors()
    {
        using var fixture = new Fixture(HttpStatusCode.NotFound, """{"error":{"code":"NotFound"}}""");

        var response = fixture.Command.OnSendAsync("https://gateway.example.test/", "tenant", "test-token", HttpMethod.Get);

        Assert.Equal(string.Empty, response);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly HttpClient client;
        private readonly ServiceProvider services;
        public RecordingHandler Handler { get; }
        public TestCommand Command { get; }

        public Fixture(HttpStatusCode? getStatus, string getBody, HttpStatusCode patchStatus = HttpStatusCode.NoContent)
        {
            Handler = new RecordingHandler(getStatus, getBody, patchStatus);
            client = new HttpClient(Handler);
            var factory = new Mock<IMsalHttpClientFactory>();
            factory.Setup(f => f.GetHttpClient()).Returns(client);
            var discovery = new Mock<INeptuneDiscovery>();
            discovery.Setup(d => d.GetGatewayEndpoint(It.IsAny<TenantId>(), It.IsAny<EnvironmentId>()))
                .Returns("gateway.example.test");
            discovery.Setup(d => d.GetTokenAudience()).Returns("https://gateway.example.test/");
            var config = new Mock<IOptionsMonitor<GatewayConfig>>();
            config.Setup(c => c.CurrentValue).Returns(new GatewayConfig());
            services = new ServiceCollection()
                .AddSingleton(config.Object)
                .AddSingleton(factory.Object)
                .AddSingleton(discovery.Object)
                .BuildServiceProvider();
            Command = new TestCommand(new CommandAllocationEnvironmentPatchOptions
            {
                TenantId = "11111111-1111-1111-1111-111111111111",
                EnvironmentId = "22222222-2222-2222-2222-222222222222",
                CurrencyType = ExternalCurrencyType.MCSMessages,
                Allocated = 12,
            }, services);
        }

        public void Dispose()
        {
            services.Dispose();
            client.Dispose();
        }
    }

    private sealed class TestCommand(CommandAllocationEnvironmentPatchOptions opts, IServiceProvider services)
        : CommandAllocationEnvironmentPatch(opts, new ConfigurationBuilder().Build(), NullLogger.Instance, services)
    {
        internal override Task<string> OnAcquireUserToken(
            string clientId, string resource, string tokenPrefix, string tokenResource,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("test-token");
        }
    }

    private sealed class RecordingHandler(HttpStatusCode? getStatus, string getBody, HttpStatusCode patchStatus) : HttpMessageHandler
    {
        public List<(HttpMethod Method, string Url, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.Method, request.RequestUri!.ToString(),
                request.Content == null ? "" : await request.Content.ReadAsStringAsync(cancellationToken)));
            if (request.Method == HttpMethod.Get && getStatus == null)
            {
                throw new HttpRequestException("Connection failed.");
            }

            return new HttpResponseMessage(request.Method == HttpMethod.Get ? getStatus!.Value : patchStatus)
            {
                Content = new StringContent(request.Method == HttpMethod.Get ? getBody : ""),
            };
        }
    }
}
