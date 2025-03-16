using EnronEmailSearch.Core.Interfaces;
using EnronEmailSearch.Core.Models;
using EnronEmailSearch.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnronEmailSearch.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IEmailIndexer _emailIndexer;
        private readonly EnronDbContext _dbContext;
        private readonly ILogger<SearchController> _logger;
        
        public SearchController(
            IEmailIndexer emailIndexer,
            EnronDbContext dbContext,
            ILogger<SearchController> logger)
        {
            _emailIndexer = emailIndexer;
            _dbContext = dbContext;
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> Search(string q, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return View("Index");
            }
            
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Search for emails
                var results = await _emailIndexer.SearchAsync(q, limit: 1000);
                
                // Apply paging
                var pagedResults = results
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                var searchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                // Create view model
                var viewModel = new SearchViewModel
                {
                    Query = q,
                    Results = pagedResults,
                    TotalResults = results.Count(),
                    CurrentPage = page,
                    PageSize = pageSize,
                    SearchTimeMs = searchTime
                };
                
                _logger.LogInformation("Search for '{Query}' returned {Count} results in {Time}ms",
                    q, results.Count(), searchTime);
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for '{Query}'", q);
                ModelState.AddModelError("", "An error occurred while searching.");
                return View("Index");
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var file = await _dbContext.Files.FindAsync(id);
                
                if (file == null)
                {
                    return NotFound();
                }
                
                return File(file.Content, "text/plain", file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file with ID {FileId}", id);
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            try
            {
                var file = await _dbContext.Files.FindAsync(id);
                
                if (file == null)
                {
                    return NotFound();
                }
                
                // Convert content to string and limit to first 2000 characters
                string content = System.Text.Encoding.UTF8.GetString(file.Content);
                string preview = content.Length > 2000 
                    ? content.Substring(0, 2000) + "..." 
                    : content;
                
                var viewModel = new EmailPreviewViewModel
                {
                    FileId = file.FileId,
                    FileName = file.FileName,
                    Content = preview
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing file with ID {FileId}", id);
                return StatusCode(500, "An error occurred while previewing the file.");
            }
        }
    }
    
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public IEnumerable<SearchResult> Results { get; set; } = Array.Empty<SearchResult>();
        public int TotalResults { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public double SearchTimeMs { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    }
    
    public class EmailPreviewViewModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}