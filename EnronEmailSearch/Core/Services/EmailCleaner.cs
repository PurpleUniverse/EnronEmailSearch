using System.Text.RegularExpressions;
using EnronEmailSearch.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnronEmailSearch.Core.Services
{
    public class EmailCleaner : IEmailCleaner
    {
        private readonly ILogger<EmailCleaner> _logger;
        
        public EmailCleaner(ILogger<EmailCleaner> logger)
        {
            _logger = logger;
        }
        
        public async Task<string> CleanEmailAsync(string filePath, string? outputPath = null)
        {
            try
            {
                // Read the email file
                string content = await File.ReadAllTextAsync(filePath);
                
                // Find the boundary between headers and body (blank line)
                // Headers and body are separated by a blank line (two consecutive newlines)
                string cleanedContent;
                
                // Look for double newlines which separate headers from body
                var match = Regex.Match(content, @"\r?\n\r?\n");
                if (match.Success)
                {
                    // Extract everything after the match (the body)
                    cleanedContent = content[(match.Index + match.Length)..].Trim();
                }
                else
                {
                    // If no clear separation found, keep the original content
                    _logger.LogWarning("No clear header separation found in file: {FilePath}", filePath);
                    cleanedContent = content;
                }
                
                // If output path is provided, save the cleaned content
                if (!string.IsNullOrEmpty(outputPath))
                {
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    await File.WriteAllTextAsync(outputPath, cleanedContent);
                }
                
                return cleanedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning email file: {FilePath}", filePath);
                throw;
            }
        }
        
        public async Task<int> CleanDirectoryAsync(string inputDirectory, string outputDirectory, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create output directory if it doesn't exist
                Directory.CreateDirectory(outputDirectory);
                
                // Get all files in the input directory and subdirectories
                var files = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
                
                _logger.LogInformation("Found {Count} files to process", files.Length);
                
                int processedCount = 0;
                int errorCount = 0;
                
                // Process files in parallel
                await Parallel.ForEachAsync(
                    files, 
                    new ParallelOptions 
                    { 
                        MaxDegreeOfParallelism = Environment.ProcessorCount, 
                        CancellationToken = cancellationToken 
                    },
                    async (file, token) =>
                    {
                        try
                        {
                            // Get relative path to maintain directory structure
                            string relativePath = Path.GetRelativePath(inputDirectory, file);
                            string outputPath = Path.Combine(outputDirectory, relativePath);
                            
                            await CleanEmailAsync(file, outputPath);
                            Interlocked.Increment(ref processedCount);
                            
                            // Log progress periodically
                            if (processedCount % 1000 == 0)
                            {
                                _logger.LogInformation("Processed {Count} files so far", processedCount);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing file: {FilePath}", file);
                            Interlocked.Increment(ref errorCount);
                        }
                    });
                
                _logger.LogInformation("Completed cleaning {ProcessedCount} files with {ErrorCount} errors", 
                    processedCount, errorCount);
                
                return processedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing directory: {InputDirectory}", inputDirectory);
                throw;
            }
        }
    }
}