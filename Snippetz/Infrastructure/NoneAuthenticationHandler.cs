using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Snippetz.Infrastructure;

/// <summary>
/// Authenticates every request as a single synthetic administrator. For a box that lives on one
/// machine and is never exposed. Do not select this mode for an instance anyone else can reach.
/// </summary>
public sealed class NoneAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "None";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Constants.DefaultUserId),
                new Claim(ClaimTypes.Name, Constants.DefaultUserName),
                new Claim(ClaimTypes.Role, Constants.Roles.Administrator),
                new Claim(ClaimTypes.Role, Constants.Roles.User),
            ],
            authenticationType: SchemeName,
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
