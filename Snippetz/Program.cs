using LogVue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Heroicsoft.NetForge.Components.Shell;
using Snippetz.Components;
using Snippetz.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogVue(options =>
    builder.Configuration.GetSection(LogVueOptions.SectionName).Bind(options));

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.LogVue(services));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
        // The circuit default is 32 KB. Code, notes and description travel in one message, so a
        // longer snippet was dropped and the editor validated as if the code box were empty.
        // The columns themselves are unbounded text; this is only the transport ceiling.
        options.MaximumReceiveMessageSize = 32 * 1024 * 1024);

builder.Services.Configure<HttpConnectionDispatcherOptions>(options =>
{
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(20);

    // Must be at least MaximumReceiveMessageSize, or a large message is still refused.
    options.ApplicationMaxBufferSize = 32 * 1024 * 1024;
    options.TransportMaxBufferSize = 32 * 1024 * 1024;
});

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddNetForgeComponents(ui =>
{
    // The key the app has always used, so a stored light/dark choice survives the switch.
    ui.ThemeStorageKey = Snippetz.Constants.ClientThemeLocalStorageKey;

    // Before the first choice is stored the app followed the operating system.
    ui.DefaultColorScheme = "system";
    ui.AccentColor = "#4f46e5";
    ui.AccentColorDark = "#818cf8";
});

builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Snippetz";
    shell.LogoUrl = "img/Logo.png";
    shell.LogoDarkUrl = "img/Logo-Dark.png";
    shell.MarkUrl = "img/Icon.png";
    shell.SearchPlaceholder = "card lang:csharp tags:ui";
});
builder.Services.AddScoped<IShellNavigation, SnippetNavigation>();

builder.Services.AddSingleton(TimeProvider.System);

string databaseProvider = DatabaseProviders.Resolve(
    builder.Configuration[DatabaseProviders.ConfigurationKey],
    DatabaseProviders.Sqlite,
    [.. DatabaseProviders.All]);

builder.Services.AddSnippetzDatabase(builder.Configuration);
builder.Services.AddSnippetzRepositories();

var authProvider = builder.Services.AddSnippetzAuthentication(builder.Configuration);

builder.Services.AddSnippetzServices();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeaders.KnownIPNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapLogVue().RequireAuthorization(new AuthorizeAttribute
{
    Roles = WellKnownRoles.Administrator
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .WithStaticAssets();

app.MapControllers();

if (authProvider == AuthProvider.Identity)
{
    app.MapRazorPages().WithStaticAssets();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider, authProvider, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}

Log.Information(
    "Snippetz {Version} started with the {DatabaseProvider} database provider and {AuthProvider} authentication",
    AppVersion.Current,
    databaseProvider,
    authProvider);

app.Run();