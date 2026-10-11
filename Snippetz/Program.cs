using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using LogVue;
using Snippetz.Components;

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
    .AddInteractiveServerComponents();

builder.Services.Configure<HttpConnectionDispatcherOptions>(options =>
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(20));

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddScoped<SidebarState>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton(TimeProvider.System);

string databaseProvider = SnippetzConfiguration.ResolveDatabaseProvider(builder.Configuration);

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
    Roles = Snippetz.Constants.Roles.Administrator
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
