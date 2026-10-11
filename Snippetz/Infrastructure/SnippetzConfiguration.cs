namespace Snippetz.Infrastructure;

/// <summary>
/// Configuration lookups more than one part of the host has to agree on.
/// </summary>
public static class SnippetzConfiguration
{
    public static string ResolveDatabaseProvider(IConfiguration configuration) =>
        configuration["Database:Provider"] is { Length: > 0 } provider
            ? provider.Trim()
            : Constants.DatabaseProviders.Sqlite;
}
