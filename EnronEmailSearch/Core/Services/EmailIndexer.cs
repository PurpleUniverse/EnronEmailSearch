using System.Text.RegularExpressions;
using EnronEmailSearch.Core.Interfaces;
using EnronEmailSearch.Core.Models;
using EnronEmailSearch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnronEmailSearch.Core.Services
{
    public class EmailIndexer : IEmailIndexer
    {
        private readonly EnronDbContext _dbContext;
        private readonly ILogger<EmailIndexer> _logger;
        private static readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "and", "or", "but", "is", "are", "was", "were",
            "to", "of", "in", "for", "with", "on", "at", "from", "by", "about",
            "as", "into", "like", "through", "after", "over", "between", "out",
            "against", "during", "without", "before", "under", "around", "among"
        };
        
        // Regular expression to match valid words (alphanumeric)
        private static readonly Regex _wordRegex = new(@"\b[a-zA-Z0-9]+\b", RegexOptions.Compiled);
        
        public EmailIndexer(EnronDbContext dbContext, ILogger<EmailIndexer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        public async Task<bool> IndexFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                // Read file content
                string content = await File.ReadAllTextAsync(filePath, cancellationToken);
                byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                
                // Get or create file record
                string relativePath = Path.GetFileName(filePath);
                
                var fileEntity = await _dbContext.Files
                    .FirstOrDefaultAsync(f => f.FileName == relativePath, cancellationToken);
                
                if (fileEntity == null)
                {
                    fileEntity = new EmailFile
                    {
                        FileName = relativePath,
                        Content = contentBytes
                    };
                    
                    _dbContext.Files.Add(fileEntity);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                
                // Tokenize content and count word frequencies
                var wordCounts = TokenizeContent(content);
                
                // Add words and occurrences
                foreach (var wordCount in wordCounts)
                {
                    // Get or create word
                    var word = await _dbContext.Words
                        .FirstOrDefaultAsync(w => w.Text == wordCount.Key, cancellationToken);
                    
                    if (word == null)
                    {
                        word = new Word { Text = wordCount.Key };
                        _dbContext.Words.Add(word);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                    
                    // Create or update occurrence
                    var occurrence = await _dbContext.Occurrences
                        .FirstOrDefaultAsync(o => o.WordId == word.WordId && o.FileId == fileEntity.FileId, cancellationToken);
                    
                    if (occurrence == null)
                    {
                        occurrence = new Occurrence
                        {
                            WordId = word.WordId,
                            FileId = fileEntity.FileId,
                            Count = wordCount.Value
                        };
                        _dbContext.Occurrences.Add(occurrence);
                    }
                    else
                    {
                        occurrence.Count = wordCount.Value;
                    }
                }
                
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing file: {FilePath}", filePath);
                return false;
            }
        }
        
        public async Task<int> IndexDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            _logger.LogInformation("Found {Count} files to index", files.Length);
            
            int successCount = 0;
            int errorCount = 0;
            
            // Process in batches to avoid memory issues
            const int batchSize = 100;
            
            for (int i = 0; i < files.Length; i += batchSize)
            {
                var batch = files.Skip(i).Take(batchSize).ToArray();
                
                // Create a list to store word occurrences for batch processing
                var batchWordOccurrences = new List<(string FilePath, Dictionary<string, int> WordCounts)>();
                
                // Process each file in the batch
                foreach (var file in batch)
                {
                    try
                    {
                        string content = await File.ReadAllTextAsync(file, cancellationToken);
                        var wordCounts = TokenizeContent(content);
                        batchWordOccurrences.Add((file, wordCounts));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file: {FilePath}", file);
                        errorCount++;
                    }
                }
                
                // Process the batch in a transaction
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    // Process each file in the batch
                    foreach (var (filePath, wordCounts) in batchWordOccurrences)
                    {
                        string relativePath = Path.GetFileName(filePath);
                        byte[] contentBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                        
                        // Get or create file record
                        var fileEntity = await _dbContext.Files
                            .FirstOrDefaultAsync(f => f.FileName == relativePath, cancellationToken);
                        
                        if (fileEntity == null)
                        {
                            fileEntity = new EmailFile
                            {
                                FileName = relativePath,
                                Content = contentBytes
                            };
                            
                            _dbContext.Files.Add(fileEntity);
                            await _dbContext.SaveChangesAsync(cancellationToken);
                        }
                        
                        // Process words and occurrences
                        foreach (var wordCount in wordCounts)
                        {
                            // Get or create word
                            var word = await _dbContext.Words
                                .FirstOrDefaultAsync(w => w.Text == wordCount.Key, cancellationToken);
                            
                            if (word == null)
                            {
                                word = new Word { Text = wordCount.Key };
                                _dbContext.Words.Add(word);
                                await _dbContext.SaveChangesAsync(cancellationToken);
                            }
                            
                            // Create or update occurrence
                            var occurrence = await _dbContext.Occurrences
                                .FirstOrDefaultAsync(o => o.WordId == word.WordId && o.FileId == fileEntity.FileId, cancellationToken);
                            
                            if (occurrence == null)
                            {
                                occurrence = new Occurrence
                                {
                                    WordId = word.WordId,
                                    FileId = fileEntity.FileId,
                                    Count = wordCount.Value
                                };
                                _dbContext.Occurrences.Add(occurrence);
                            }
                            else
                            {
                                occurrence.Count = wordCount.Value;
                            }
                        }
                        
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        successCount++;
                    }
                    
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error processing batch of files");
                    errorCount += batchWordOccurrences.Count;
                }
                
                _logger.LogInformation("Indexed {Count}/{Total} files", i + Math.Min(batchSize, files.Length - i), files.Length);
            }
            
            _logger.LogInformation("Completed indexing with {SuccessCount} successes and {ErrorCount} errors", 
                successCount, errorCount);
            
            return successCount;
        }
        
        public async Task<IEnumerable<SearchResult>> SearchAsync(string searchTerms, int limit = 100, CancellationToken cancellationToken = default)
        {
            // Tokenize search terms
            var terms = TokenizeContent(searchTerms).Keys.ToList();
            
            if (terms.Count == 0)
            {
                return Array.Empty<SearchResult>();
            }
            
            // Find files containing these terms
            var results = await _dbContext.Occurrences
                .Where(o => terms.Contains(o.Word.Text))
                .GroupBy(o => new { o.FileId, o.File.FileName })
                .Select(g => new SearchResult
                {
                    FileId = g.Key.FileId,
                    FileName = g.Key.FileName,
                    Relevance = g.Sum(o => o.Count)
                })
                .OrderByDescending(r => r.Relevance)
                .Take(limit)
                .ToListAsync(cancellationToken);
            
            return results;
        }
        
        private Dictionary<string, int> TokenizeContent(string content)
        {
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            // Find all words in the content
            var matches = _wordRegex.Matches(content.ToLowerInvariant());
            
            foreach (Match match in matches)
            {
                string word = match.Value;
                
                // Skip stop words and single characters
                if (_stopWords.Contains(word) || word.Length <= 1)
                {
                    continue;
                }
                
                if (wordCounts.ContainsKey(word))
                {
                    wordCounts[word]++;
                }
                else
                {
                    wordCounts[word] = 1;
                }
            }
            
            return wordCounts;
        }
    }
}