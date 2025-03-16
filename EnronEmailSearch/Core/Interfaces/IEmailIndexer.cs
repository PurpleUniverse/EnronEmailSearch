namespace EnronEmailSearch.Core.Interfaces
{
public interface IEmailIndexer
{
    /// <summary>
    /// Index a single email file
    /// </summary>
    /// <param name="filePath">Path to the cleaned email file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if indexing successful</returns>
    Task<bool> IndexFileAsync(string filePath, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Index a directory of cleaned email files
    /// </summary>
    /// <param name="directoryPath">Directory containing cleaned email files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of files indexed</returns>
    Task<int> IndexDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Search for emails containing specific terms
    /// </summary>
    /// <param name="searchTerms">Terms to search for</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    Task<IEnumerable<SearchResult>> SearchAsync(string searchTerms, int limit = 100, CancellationToken cancellationToken = default);
}
    
// Search result model
public class SearchResult
{
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int Relevance { get; set; }
}
}