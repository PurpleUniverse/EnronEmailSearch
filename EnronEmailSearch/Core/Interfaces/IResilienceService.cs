namespace EnronEmailSearch.Core.Interfaces
{
    /// <summary>
    /// Interface for resilience service that implements fault tolerance patterns
    /// </summary>
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
}