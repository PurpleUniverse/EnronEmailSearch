using EnronEmailSearch.Core.Interfaces;
using EnronEmailSearch.Core.Services;
using EnronEmailSearch.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnronEmailSearch.Infrastructure.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnronServices(this IServiceCollection services)
        {
            // Register Core services
            services.AddTransient<IEmailCleaner, EmailCleaner>();
            services.AddTransient<EmailIndexer>();
            services.AddTransient<IEmailIndexer, ResilientEmailIndexer>();
            
            // Register Infrastructure services
            services.AddResilienceServices();
            
            return services;
        }
    }
}