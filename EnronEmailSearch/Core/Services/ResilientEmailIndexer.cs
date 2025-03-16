using EnronEmailSearch.Core.Interfaces;
using EnronEmailSearch.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace EnronEmailSearch.Core.Services
{
    /// <summary>
    /// A resilient decorator for the EmailIndexer that adds fault tolerance
    /// </summary>
    public class ResilientEmailIndexer : IEmailIndexer
    {
        private readonly EmailIndexer _innerIndexer;
        private readonly IResilienceService _resilienceService;
        private readonly ILogger<ResilientEmailIndexer> _logger;
        
        public ResilientEmailIndexer(
            EmailIndexer innerIndexer,
            IResilienceService resilienceService,
            ILogger<ResilientEmailIndexer> logger)
        {
            _innerIndexer = innerIndexer;
            _resilienceService = resilienceService;
            _logger = logger;
        }
        
        public Task<bool> IndexFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _resilienceService.ExecuteFileOperationAsync(
                ct => _innerIndexer.IndexFileAsync(filePath, ct),
                cancellationToken);
        }
        
        public Task<int> IndexDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
        {
            return _resilienceService.ExecuteFileOperationAsync(
                ct => _innerIndexer.IndexDirectoryAsync(directoryPath, ct),
                cancellationToken);
        }
        
        public Task<IEnumerable<SearchResult>> SearchAsync(string searchTerms, int limit = 100, CancellationToken cancellationToken = default)
        {
            return _resilienceService.ExecuteSearchOperationAsync(
                async ct =>
                {
                    var results = await _innerIndexer.SearchAsync(searchTerms, limit, ct);
                    return results;
                },
                cancellationToken);
        }
    }
    
    // Extension method to register resilient services
    public static class ResilientServicesExtensions
    {
        public static IServiceCollection AddResilientServices(this IServiceCollection services)
        {
            // Register both the normal and resilient versions
            services.AddTransient<EmailIndexer>();
            services.AddTransient<IEmailIndexer, ResilientEmailIndexer>();
            
            return services;
        }
    }
}