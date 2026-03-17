namespace sample.gateway;

using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using sample.gateway.Discovery;
using sample.gateway.Models;
using sample.gateway.Tokens;

public abstract class BaseCommand<T> where T : ICommandOptions
{
    public IConfiguration Configuration { get; }
    public virtual T Opts { get; }
    public ILogger TraceLogger { get; }

    internal readonly GatewayConfig _gatewayConfig;
    internal readonly INeptuneDiscovery _neptuneDiscovery;
    internal readonly IMsalHttpClientFactory _msalFactory;

    /// <summary>
    /// Microsoft Powershell
    /// </summary>
    internal Guid PowershellClientId { get; } = Guid.Parse("1950a258-227b-4e31-a9cf-717495945fc2");

    /// <summary>
    /// The EntraId application (client) id
    /// </summary>
    internal virtual string _clientId { get; set; }
    /// <summary>
    /// The authority typically login.microsoftonline.com/common or login.microsoft.com/<tenantid>
    /// </summary>
    internal virtual Uri _authority { get; set; }
    /// <summary>
    /// The MSAL client to aquire tokens.
    /// </summary>
    internal virtual IPublicClientApplication _clientApplication { get; set; }

    protected BaseCommand(
        T opts,
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        Opts = opts ?? throw new ArgumentNullException(nameof(opts), "CommonOptions object is required.");
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "IAppSettings is required.");
        LoggerAvailable = false;
        TraceLogger = logger;

        _gatewayConfig = serviceProvider.GetRequiredService<IOptionsMonitor<GatewayConfig>>().CurrentValue;
        _msalFactory = serviceProvider.GetRequiredService<IMsalHttpClientFactory>();
        _neptuneDiscovery = serviceProvider.GetRequiredService<INeptuneDiscovery>();
    }

    public int Run()
    {
        int result = -1;
        OnInit();

        try
        {
            result = OnRun();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Cmdlet Exception {0}", ex.Message);
            LogError(new Exception("Failed to run commandline switch."), $"OpenError");
        }
        finally
        {
            OnEnd();
        }

        return result;
    }

    public virtual void OnInit()
    {
        _clientId = PowershellClientId.ToString();

        _authority = new Uri(_gatewayConfig.AuthenticationEndpoint.GetScopeEnsureResourceTrailingSlash(Opts.TenantId));

        // Will will use a Public Client to obtain tokens interactively
        _clientApplication = PublicClientApplicationBuilder
          .Create(_clientId)
          .WithAuthority(_authority.ToString(), validateAuthority: true)
          .WithDefaultRedirectUri()
          .WithInstanceDiscovery(enableInstanceDiscovery: true)
          .Build();
    }

    public abstract int OnRun();

    public virtual void OnEnd() { }

    /// <summary>
    /// the logger is available
    /// </summary>
    internal bool LoggerAvailable { get; private set; }

    protected virtual void WriteConsole(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    /// <summary>
    /// Log: ERROR
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    protected virtual void LogError(Exception ex, string message, params object[] args)
    {
        if (LoggerAvailable)
        {
            TraceLogger.LogError(ex, message, args);
        }

        if (string.IsNullOrEmpty(message))
        {
            System.Diagnostics.Trace.TraceError("Exception: {0}", ex.Message);
        }
        else
        {
            System.Diagnostics.Trace.TraceError(message, args);
        }
    }

    /// <summary>
    /// Log: DEBUG
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    protected virtual void LogDebugging(string message, params object[] args)
    {
        if (LoggerAvailable)
        {
            TraceLogger.LogDebug(message, args);
        }
        System.Diagnostics.Trace.TraceInformation(message, args);
    }

    /// <summary>
    /// Writes a warning message to the cmdlet and logs to directory
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    protected virtual void LogWarning(string message, params object[] args)
    {
        if (LoggerAvailable)
        {
            TraceLogger.LogWarning(message, args);
        }
        System.Diagnostics.Trace.TraceWarning(message, args);
    }

    /// <summary>
    /// Log: VERBOSE
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    protected virtual void LogVerbose(string message, params object[] args)
    {
        if (LoggerAvailable)
        {
            TraceLogger.LogInformation(message, args);
        }
        System.Diagnostics.Trace.TraceInformation(message, args);
    }

    /// <summary>
    /// Sends an HTTP GET request
    /// </summary>
    /// <param name="url">The gateway endpoint to be requested.</param>
    /// <param name="tenantId">The tenant id should be from your EntraId.  This value should match with your token TID.</param>
    /// <param name="accessToken">The access token claimed in previous steps.</param>
    /// <param name="httpMethod">The HTTP method to be used for the request.</param>
    /// <param name="requestBody">The request body to be sent with the request. Leave empty if this is a GET.</param>
    /// <param name="correlationId">The correlation id associated with the requests.  This assists in tracing.</param>
    /// <param name="cancellationToken">The cancellation token to handle the thread requests.</param>
    /// <returns></returns>
    internal virtual string OnSendAsync(
        string url,
        string tenantId,
        string accessToken,
        HttpMethod httpMethod,
        string requestBody = "",
        Guid? correlationId = default,
        CancellationToken cancellationToken = default)
    {
        correlationId ??= Guid.NewGuid();

        const int maxRetries = 3;
        const int throttleBackoffSeconds = 60;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            HttpRequestMessage request = new(httpMethod, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("x-ms-client-tenant-id", tenantId);
            request.Headers.Add("x-ms-correlation-id", correlationId.ToString());

            try
            {
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = _msalFactory.GetHttpClient().SendAsync(request, cancellationToken).Result;

                TraceLogger.LogInformation("Request Status Code = {StatusCode}:  CorrelationId = {CorrelationId}", response.StatusCode, correlationId);

                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = response.Content.ReadAsStringAsync(cancellationToken).Result;
                    TraceLogger.LogInformation("Request: {url}", url);
                    TraceLogger.LogInformation("Response: {ErrorResponse}", errorResponse);

                    if (IsBurstThrottled(errorResponse) && attempt < maxRetries)
                    {
                        TraceLogger.LogWarning(
                            "BAP request throttled (BurstRequestsThrottled). Backing off for {Seconds}s before retry {Attempt}/{MaxRetries}.",
                            throttleBackoffSeconds, attempt, maxRetries);
                        Task.Delay(TimeSpan.FromSeconds(throttleBackoffSeconds), cancellationToken).Wait(cancellationToken);
                        continue;
                    }
                }
                else
                {
                    return response.Content.ReadAsStringAsync(cancellationToken).Result;
                }
            }
            catch (HttpRequestException httpEx)
            {
                TraceLogger.LogError(httpEx, "Exception => Request failed: {Message}", httpEx.Message);
            }
            catch (Exception ex)
            {
                TraceLogger.LogError(ex, "Exception => Request failed: {Message}", ex.Message);
            }

            break;
        }

        return string.Empty;
    }

    private static bool IsBurstThrottled(string errorResponse)
    {
        try
        {
            var throttle = JsonConvert.DeserializeObject<BapThrottleResponse>(errorResponse);
            return throttle?.Error?.Code == "BurstRequestsThrottled";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// This method acquires a user token interactively using MSAL.NET's PublicClientApplication.
    /// Please note that this method is only supported on Windows platforms due to storing the token in the user roaming store.
    /// </summary>
    /// <param name="clientId">The application registered in the EntraID for the <tenantid> specified in the authority.</param>
    /// <param name="resource">The 'scope' or 'audience' for which the token will be claimed.</param>
    /// <param name="tokenPrefix">Used to differentiate token storage.</param>
    /// <param name="tokenResource"></param>
    /// <paramref>The cancellation token to handle the thread requests.</paramref>
    /// <returns>A cached access token.</returns>
    [SupportedOSPlatform("windows")]
    internal virtual async Task<string> OnAcquireUserToken(
        string clientId,
        string resource,
        string tokenPrefix,
        string tokenResource,
        CancellationToken cancellationToken = default)
    {
        string[] scopes = new[] { resource.GetScopeEnsureResourceTrailingSlash() };
        string resourceScoped = $"{tokenPrefix}-{tokenResource}".ToLowerInvariant();

        // The consent URL is not used in this method, but it can be useful for debugging or manual consent
        // We need the user to provide consent once
        TraceLogger.LogInformation($"Consent URL: {_authority}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read");

        TokenInfo token = TokenStorage.LoadToken(resourceScoped);

        if (token == null || token.ExpirationUtc <= DateTime.UtcNow)
        {
            // Token missing or expired: Get new token from server!
            AuthenticationResult authenticationResult = await _clientApplication
                .AcquireTokenInteractive(scopes)
                .ExecuteAsync();
            string accessToken = authenticationResult.AccessToken;

            TokenStorage.SaveToken(new TokenInfo
            {
                AccessToken = accessToken,
                ExpirationUtc = authenticationResult.ExpiresOn
            },
            resourceScoped);

            return accessToken;
        }
        else
        {
            TraceLogger.LogInformation("Using cached authentication result.");
            return token.AccessToken;
        }
    }

    /// <summary>
    /// Example SSRF prevention: Make sure nextLink is on expected host and scheme
    /// </summary>
    /// <param name="baseUri">the root domain for the initial request.</param>
    /// <param name="nextLink">the nextLink from paged results, to validate.</param>
    /// <returns></returns>
    internal static bool IsSafeNextLink(Uri baseUri, string nextLink)
    {
        if (Uri.TryCreate(nextLink, UriKind.Absolute, out Uri uri))
        {
            // Only allow host & scheme you trust
            if (uri.Host == baseUri.Host && uri.Scheme == baseUri.Scheme)
            {
                // (Optional) additional path checks
                return true;
            }
        }
        return false;
    }
}