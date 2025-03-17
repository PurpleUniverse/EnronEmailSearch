using System.Data.Common;
using EnronEmailSearch.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Polly;
using Microsoft.Extensions.DependencyInjection;

namespace EnronEmailSearch.Infrastructure.Services
{
    public class ResilienceService : IResilienceService
    {
        private readonly ILogger<ResilienceService> _logger;
        private readonly IAsyncPolicy _databasePolicy;
        private readonly IAsyncPolicy _filePolicy;
        private readonly IAsyncPolicy _searchPolicy;
        
        public ResilienceService(ILogger<ResilienceService> logger)
        {
            _logger = logger;
            
            // Configure database resilience policy
            var dbRetryPolicy = Policy
                .Handle<SqliteException>()
                .Or<DbException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timeSpan, retryCount, _) =>
                    {
                        _logger.LogWarning("Database operation failed. Retrying in {RetryTimeSpan}s. Retry attempt {RetryCount}", 
                            timeSpan.TotalSeconds, retryCount);
                    });
            
            var dbCircuitBreakerPolicy = Policy
                .Handle<SqliteException>()
                .Or<DbException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (ex, _) =>
                    {
                        _logger.LogError(ex, "Circuit breaker opened for database operations");
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset for database operations");
                    });
            
            // Simple timeout policy
            var dbTimeoutPolicy = Policy.TimeoutAsync(10);
            
            _databasePolicy = Policy.WrapAsync(dbRetryPolicy, dbCircuitBreakerPolicy, dbTimeoutPolicy);
            
            // Configure file resilience policy
            var fileRetryPolicy = Policy
                .Handle<IOException>()
                .Or<UnauthorizedAccessException>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timeSpan, retryCount, _) =>
                    {
                        _logger.LogWarning(ex, "File operation failed. Retrying in {RetryTimeSpan}s. Retry attempt {RetryCount}",
                            timeSpan.TotalSeconds, retryCount);
                    });
            
            // Simple timeout policy
            var fileTimeoutPolicy = Policy.TimeoutAsync(5);
            
            _filePolicy = Policy.WrapAsync(fileRetryPolicy, fileTimeoutPolicy);
            
            // Configure search resilience policy
            var searchRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    2,
                    attempt => TimeSpan.FromMilliseconds(50 * attempt),
                    (ex, timeSpan, retryCount, _) =>
                    {
                        _logger.LogWarning(ex, "Search operation failed. Retrying in {RetryTimeSpan}ms. Retry attempt {RetryCount}",
                            timeSpan.TotalMilliseconds, retryCount);
                    });
            
            // Simple timeout policy
            var searchTimeoutPolicy = Policy.TimeoutAsync(3);
            
            _searchPolicy = Policy.WrapAsync(searchRetryPolicy, searchTimeoutPolicy);
        }
        
        public async Task<T> ExecuteDatabaseOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return await _databasePolicy.ExecuteAsync(
                async (ctx) => await operation(cancellationToken), 
                new Context("DatabaseOperation"));
        }
        
        public async Task<T> ExecuteFileOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return await _filePolicy.ExecuteAsync(
                async (ctx) => await operation(cancellationToken), 
                new Context("FileOperation"));
        }
        
        public async Task<T> ExecuteSearchOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return await _searchPolicy.ExecuteAsync(
                async (ctx) => await operation(cancellationToken), 
                new Context("SearchOperation"));
        }
    }
    
    // Extension method to register resilience services
    public static class ResilienceServiceExtensions
    {
        public static IServiceCollection AddResilienceServices(this IServiceCollection services)
        {
            services.AddSingleton<IResilienceService, ResilienceService>();
            return services;
        }
    }
}