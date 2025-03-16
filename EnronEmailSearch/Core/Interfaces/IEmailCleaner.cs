namespace EnronEmailSearch.Core.Interfaces
{
    public interface IEmailCleaner
    {
        /// <summary>
        /// Cleans email content by removing headers
        /// </summary>
        /// <param name="filePath">Path to the email file</param>
        /// <param name="outputPath">Path where cleaned file should be saved, if any</param>
        /// <returns>Cleaned email content</returns>
        Task<string> CleanEmailAsync(string filePath, string? outputPath = null);
        
        /// <summary>
        /// Process a directory of email files, removing headers from each
        /// </summary>
        /// <param name="inputDirectory">Directory containing email files</param>
        /// <param name="outputDirectory">Directory to save cleaned files</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of files processed</returns>
        Task<int> CleanDirectoryAsync(string inputDirectory, string outputDirectory, CancellationToken cancellationToken = default);
    }
}