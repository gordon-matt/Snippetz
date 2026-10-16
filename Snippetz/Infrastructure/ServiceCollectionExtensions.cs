using Snippetz.Data.MySql;
using Snippetz.Data.Npgsql;
using Snippetz.Data.Sql;
using Snippetz.Data.Sqlite;

namespace Snippetz.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Reads <c>Database:Provider</c> and delegates to the matching provider project.
    /// </summary>
    public static IServiceCollection AddSnippetzDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string provider = DatabaseProviders.Resolve(
            configuration[DatabaseProviders.ConfigurationKey],
            DatabaseProviders.Sqlite,
            [.. DatabaseProviders.All]);

        return provider switch
        {
            DatabaseProviders.Sqlite => services.AddSnippetzSqlite(configuration),
            DatabaseProviders.SqlServer => services.AddSnippetzSqlServer(configuration),
            DatabaseProviders.Npgsql => services.AddSnippetzNpgsql(configuration),
            DatabaseProviders.MySql => services.AddSnippetzMySql(configuration),
            _ => throw new InvalidOperationException($"Unsupported {DatabaseProviders.ConfigurationKey} '{provider}'."),
        };
    }

    public static IServiceCollection AddSnippetzRepositories(this IServiceCollection services)
    {
        services.AddEntityFrameworkRepository();
        return services;
    }

    /// <summary>
    /// Wires up whichever authentication provider is configured and returns which one it was, so
    /// <c>Program.cs</c> can decide whether Identity's Razor Pages need mapping.
    /// </summary>
    public static AuthProvider AddSnippetzAuthentication(
        this IServiceCollection services,
        IConfiguration configuration) =>
        // Identity, Keycloak, and None — including IUserContextService — come from NetForge.
        // This host does not register an IUserInfoService.
        services.AddNetForgeAuthentication<ApplicationUser, ApplicationRole, ApplicationDbContextBase>(
            configuration);
}