namespace Snippetz.Services.Auth;

/// <summary>
/// The single synthetic user of <c>None</c> authentication mode. Always an administrator, because
/// with nobody to authenticate there is nobody to withhold anything from.
/// </summary>
public sealed class NoneUserContextService : IUserContextService
{
    private static readonly IReadOnlyList<string> Roles =
        [Constants.Roles.Administrator, Constants.Roles.User];

    public string? GetCurrentUserId() => Constants.DefaultUserId;

    public string? GetCurrentUserName() => Constants.DefaultUserName;

    public bool IsAuthenticated() => true;

    public bool IsAdministrator() => true;

    public IReadOnlyList<string> GetRoleNames() => Roles;
}
