using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
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
        string provider = SnippetzConfiguration.ResolveDatabaseProvider(configuration);

        return provider switch
        {
            Constants.DatabaseProviders.Sqlite => services.AddSnippetzSqlite(configuration),
            Constants.DatabaseProviders.SqlServer => services.AddSnippetzSqlServer(configuration),
            Constants.DatabaseProviders.Npgsql => services.AddSnippetzNpgsql(configuration),
            Constants.DatabaseProviders.MySql => services.AddSnippetzMySql(configuration),
            _ => throw new InvalidOperationException(
                $"Unknown Database:Provider '{provider}'. Valid values: Sqlite, SqlServer, Npgsql, MySql."),
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
        IConfiguration configuration)
    {
        string provider = configuration["Authentication:Provider"] is { Length: > 0 } configured
            ? configured
            : Constants.AuthProviders.Identity;

        services.AddScoped<IUserContextService, BlazorAwareUserContextService>();

        return provider switch
        {
            Constants.AuthProviders.Identity => services.AddIdentityAuthentication(),
            Constants.AuthProviders.Keycloak => services.AddKeycloakAuthentication(configuration),
            Constants.AuthProviders.None => services.AddNoAuthentication(),
            _ => throw new InvalidOperationException(
                $"Unknown Authentication:Provider '{provider}'. Valid values: Identity, Keycloak, None."),
        };
    }

    private static AuthProvider AddIdentityAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Identity, null));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContextBase>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        return AuthProvider.Identity;
    }

    private static AuthProvider AddNoAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.None, null));

        services.AddAuthentication(NoneAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, NoneAuthenticationHandler>(
                NoneAuthenticationHandler.SchemeName,
                _ => { });

        services.RemoveAll<IUserContextService>();
        services.AddSingleton<IUserContextService, NoneUserContextService>();

        return AuthProvider.None;
    }

    private static AuthProvider AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? authority = configuration["Authentication:Keycloak:Authority"];

        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException(
                "Authentication:Provider is 'Keycloak' but Authentication:Keycloak:Authority is not set.");
        }

        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Keycloak, authority));

        string? publicOrigin = configuration["Authentication:Keycloak:PublicOrigin"];
        string? backchannelBase = configuration["Authentication:Keycloak:BackchannelBaseUrl"];

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/auth/login";
                options.AccessDeniedPath = "/access-denied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authority;

                if (!string.IsNullOrWhiteSpace(backchannelBase))
                {
                    options.BackchannelHttpHandler = new KeycloakBackchannelHandler(authority, backchannelBase);
                }

                options.ClientId = configuration["Authentication:Keycloak:ClientId"];
                options.ClientSecret = configuration["Authentication:Keycloak:ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                };

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.NonceCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(publicOrigin))
                        {
                            context.ProtocolMessage.RedirectUri = RewriteOrigin(
                                context.ProtocolMessage.RedirectUri,
                                publicOrigin);
                        }

                        return Task.CompletedTask;
                    },

                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(publicOrigin))
                        {
                            context.ProtocolMessage.PostLogoutRedirectUri = RewriteOrigin(
                                context.ProtocolMessage.PostLogoutRedirectUri,
                                publicOrigin);
                        }

                        return Task.CompletedTask;
                    },

                    OnTokenValidated = context =>
                    {
                        MapKeycloakRoles(context.Principal);
                        EnsureNameIdentifier(context.Principal);
                        return Task.CompletedTask;
                    },

                    OnUserInformationReceived = context =>
                    {
                        MapKeycloakRoles(context.Principal);
                        EnsureNameIdentifier(context.Principal);
                        return Task.CompletedTask;
                    },

                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/access-denied");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                };
            });

        return AuthProvider.Keycloak;
    }

    private static void MapKeycloakRoles(ClaimsPrincipal? principal)
    {
        if (principal?.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var roleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var claim in identity.FindAll("realm_access").Concat(identity.FindAll("resource_access")))
        {
            try
            {
                using var document = JsonDocument.Parse(claim.Value);
                CollectRoles(document.RootElement, roleNames);
            }
            catch (JsonException)
            {
            }
        }

        foreach (string role in roleNames)
        {
            if (identity.HasClaim(ClaimTypes.Role, role) || identity.HasClaim(identity.RoleClaimType, role))
            {
                continue;
            }

            identity.AddClaim(new Claim(identity.RoleClaimType, role));

            if (identity.RoleClaimType != ClaimTypes.Role)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }

    private static void EnsureNameIdentifier(ClaimsPrincipal? principal)
    {
        if (principal?.Identity is not ClaimsIdentity identity
            || identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
        {
            return;
        }

        if (identity.FindFirst("sub")?.Value is { Length: > 0 } sub)
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
        }
    }

    private static void CollectRoles(JsonElement element, HashSet<string> roleNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (element.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in roles.EnumerateArray())
            {
                if (role.GetString() is { Length: > 0 } name)
                {
                    roleNames.Add(name);
                }
            }
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                CollectRoles(property.Value, roleNames);
            }
        }
    }

    private static string? RewriteOrigin(string? url, string publicOrigin)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        return Uri.TryCreate(publicOrigin, UriKind.Absolute, out var origin)
            ? new UriBuilder(uri) { Scheme = origin.Scheme, Host = origin.Host, Port = origin.Port }.Uri.ToString()
            : url;
    }
}
