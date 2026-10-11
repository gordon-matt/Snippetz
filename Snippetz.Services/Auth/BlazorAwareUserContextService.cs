using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace Snippetz.Services.Auth;

/// <summary>
/// Resolves the caller from <c>HttpContext</c> when there is one, and from the Blazor
/// authentication state otherwise. Interactive Blazor Server components run on a SignalR
/// circuit where <c>HttpContext</c> is null.
/// </summary>
public sealed class BlazorAwareUserContextService(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authenticationStateProvider) : IUserContextService
{
    public string? GetCurrentUserId() => ResolvePrincipal().FindUserId();

    public string? GetCurrentUserName() => ResolvePrincipal()?.Identity?.Name;

    public bool IsAuthenticated() => ResolvePrincipal()?.Identity?.IsAuthenticated ?? false;

    public bool IsAdministrator()
    {
        var user = ResolvePrincipal();
        if (user is null)
        {
            return false;
        }

        return user.IsInRole(Constants.Roles.Administrator)
            || GetRoleNames().Contains(Constants.Roles.Administrator, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> GetRoleNames()
    {
        var user = ResolvePrincipal();
        if (user is null)
        {
            return [];
        }

        var types = new HashSet<string>(StringComparer.Ordinal)
        {
            ClaimTypes.Role,
            "role",
            "roles",
        };

        if (user.Identity is ClaimsIdentity identity)
        {
            types.Add(identity.RoleClaimType);
        }

        return user.Claims
            .Where(c => types.Contains(c.Type))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private ClaimsPrincipal? ResolvePrincipal()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User;
        }

        try
        {
            var user = Task.Run(async () =>
                    (await authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false)).User)
                .GetAwaiter()
                .GetResult();

            return user.Identity?.IsAuthenticated == true ? user : httpContext?.User;
        }
        catch
        {
            return httpContext?.User;
        }
    }
}
