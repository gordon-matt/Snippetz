namespace Snippetz.Infrastructure;

/// <summary>
/// Rewrites the host of Keycloak back-channel calls from the browser-facing authority to an
/// internally reachable base URL. In Docker the browser and the app container do not share a host name.
/// </summary>
public sealed class KeycloakBackchannelHandler : HttpClientHandler
{
    private readonly Uri authority;
    private readonly Uri backchannelBase;

    public KeycloakBackchannelHandler(string authority, string backchannelBaseUrl)
    {
        this.authority = new Uri(authority, UriKind.Absolute);
        backchannelBase = new Uri(backchannelBaseUrl, UriKind.Absolute);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null
            && string.Equals(request.RequestUri.Host, authority.Host, StringComparison.OrdinalIgnoreCase))
        {
            request.RequestUri = new UriBuilder(request.RequestUri)
            {
                Scheme = backchannelBase.Scheme,
                Host = backchannelBase.Host,
                Port = backchannelBase.Port,
            }.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
