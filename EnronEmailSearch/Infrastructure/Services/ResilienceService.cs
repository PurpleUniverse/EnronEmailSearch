using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace EnronEmailSearch.Infrastructure.Services
{
    public interface IResilienceService
    {
        /// <summary>
        /// Execute a database operation with resilience policies applied
        /// </summary>
        Task<T> ExecuteDatabaseOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute a file operation with resilience policies applied
        /// </summary>
        Task<T> ExecuteFileOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Execute a search operation with resilience policies applied
        /// </summary>
        Task<T> ExecuteSearchOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
    }
    
    public class ResilienceService : IResilienceService
    {
        private readonly ILogger<ResilienceService> _logger;
        private readonly AsyncPolicyWrap _databasePolicy;
        private readonly AsyncPolicyWrap _filePolicy;
        private readonly AsyncPolicyWrap _searchPolicy;
        
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
                    (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(ex, 
                            "Database operation failed. Retrying in {RetryTimeSpan}s. Retry attempt {RetryCount}",
                            timeSpan.TotalSeconds, retryCount);
                    });
            
            var dbCircuitBreakerPolicy = Policy
                .Handle<SqliteException>()
                .Or<DbException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (ex, breakDuration) =>
                    {
                        _logger.LogError(ex, 
                            "Circuit breaker opened for database operations. Breaking for {BreakDuration}s",
                            breakDuration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset for database operations");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit breaker half-open for database operations");
                    });
            
            var dbTimeoutPolicy = Policy
                .TimeoutAsync(10, TimeoutStrategy.Pessimistic, (context, timespan, task) =>
                {
                    _logger.LogWarning("Database operation timed out after {Timespan}s", timespan.TotalSeconds);
                    return Task.CompletedTask;
                });
            
            _databasePolicy = Policy.WrapAsync(dbRetryPolicy, dbCircuitBreakerPolicy, dbTimeoutPolicy);
            
            // Configure file resilience policy
            var fileRetryPolicy = Policy
                .Handle<IOException>()
                .Or<UnauthorizedAccessException>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(ex, 
                            "File operation failed. Retrying in {RetryTimeSpan}s. Retry attempt {RetryCount}",
                            timeSpan.TotalSeconds, retryCount);
                    });
            
            var fileTimeoutPolicy = Policy
                .TimeoutAsync(5, TimeoutStrategy.Pessimistic);
            
            _filePolicy = Policy.WrapAsync(fileRetryPolicy, fileTimeoutPolicy);
            
            // Configure search resilience policy
            var searchRetryPolicy = Policy
                .Handle<Exception>()
                .OrResult<object>(result => result == null)
                .WaitAndRetryAsync(
                    2,
                    attempt => TimeSpan.FromMilliseconds(50 * attempt),
                    (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(ex?.Exception, 
                            "Search operation failed. Retrying in {RetryTimeSpan}ms. Retry attempt {RetryCount}",
                            timeSpan.TotalMilliseconds, retryCount);
                    });
            
            var searchTimeoutPolicy = Policy
                .TimeoutAsync(3, TimeoutStrategy.Pessimistic);
            
            var searchBulkheadPolicy = Policy
                .BulkheadAsync(10, 100, onBulkheadRejectedAsync: context =>
                {
                    _logger.LogWarning("Search request rejected due to bulkhead overflow");
                    return Task.CompletedTask;
                });
            
            _searchPolicy = Policy.WrapAsync(searchRetryPolicy, searchTimeoutPolicy, searchBulkheadPolicy);
        }
        
        public Task<T> ExecuteDatabaseOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return _databasePolicy.ExecuteAsync(
                async ct => await operation(ct),
                cancellationToken);
        }
        
        public Task<T> ExecuteFileOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return _filePolicy.ExecuteAsync(
                async ct => await operation(ct),
                cancellationToken);
        }
        
        public Task<T> ExecuteSearchOperationAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            return _searchPolicy.ExecuteAsync(
                async ct => await operation(ct),
                cancellationToken);
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