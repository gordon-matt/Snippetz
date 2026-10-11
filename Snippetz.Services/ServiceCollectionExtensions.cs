using Microsoft.Extensions.DependencyInjection;
using Snippetz.Services.Snippets;

namespace Snippetz.Services;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the domain services. Auth-provider-specific registrations are left to the host,
        /// which is the only place that knows which provider is configured.
        /// </summary>
        public IServiceCollection AddSnippetzServices()
        {
            services.AddScoped<ISnippetService, SnippetService>();
            return services;
        }
    }
}
