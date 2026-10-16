using Extenso.Data.Entity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Snippetz.Data.Sqlite;

/// <summary>
/// Hands out short-lived contexts to code with no request scope of its own, without going
/// through the scoped DI registration.
/// </summary>
public class ApplicationDbContextFactory(IConfiguration configuration) : IDbContextFactory
{
    private DbContextOptions<ApplicationDbContext> Options
        => field ??= BuildOptions(configuration.GetConnectionString("DefaultConnection"));

    public DbContext GetContext() => new ApplicationDbContext(Options);

    public DbContext GetContext(string connectionString) =>
        new ApplicationDbContext(BuildOptions(connectionString));

    private static DbContextOptions<ApplicationDbContext> BuildOptions(string? connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseInMemoryDatabase("SnippetzDb");
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString, sqlite =>
                sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            // SQLite has no schemas; the shared HasDefaultSchema("app") only affects table naming.
            optionsBuilder.ConfigureWarnings(w => w.Ignore(SqliteEventId.SchemaConfiguredWarning));
        }

        return optionsBuilder.Options;
    }
}