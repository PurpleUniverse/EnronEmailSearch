using CommandLine;
using EnronEmailSearch.Core.Interfaces;
using EnronEmailSearch.Core.Services;
using EnronEmailSearch.Infrastructure.Data;
using EnronEmailSearch.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EnronEmailSearch.Indexer
{
    class Program
    {
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "Input directory with raw email files")]
            public string InputDirectory { get; set; } = string.Empty;

            [Option('o', "output", Required = false, HelpText = "Output directory for cleaned files")]
            public string OutputDirectory { get; set; } = string.Empty;

            [Option('d', "db", Required = false, HelpText = "Database file path")]
            public string DatabasePath { get; set; } = "enron_index.db";

            [Option('c', "clean-only", Required = false, HelpText = "Only clean emails, don't index")]
            public bool CleanOnly { get; set; }

            [Option('x', "index-only", Required = false, HelpText = "Only index cleaned emails, don't clean")]
            public bool IndexOnly { get; set; }
        }

        static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/indexer-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                await Parser.Default.ParseArguments<Options>(args)
                    .WithParsedAsync(RunIndexerAsync);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        static async Task RunIndexerAsync(Options opts)
        {
            Log.Information("Starting Enron Email Indexer");

            // Set default output directory if not specified
            if (string.IsNullOrEmpty(opts.OutputDirectory))
            {
                opts.OutputDirectory = Path.Combine(Path.GetDirectoryName(opts.InputDirectory) ?? ".", "cleaned_emails");
            }

            // Configure services
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // Add database
            services.AddDbContext<EnronDbContext>(options =>
            {
                options.UseSqlite($"Data Source={opts.DatabasePath}");
            });

            // Add services
            services.AddTransient<IEmailCleaner, EmailCleaner>();
            services.AddTransient<EmailIndexer>();
            services.AddTransient<IEmailIndexer, ResilientEmailIndexer>();
            
            // Add resilience services
            services.AddResilienceServices();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Get services
            var dbContext = serviceProvider.GetRequiredService<EnronDbContext>();
            var cleaner = serviceProvider.GetRequiredService<IEmailCleaner>();
            var indexer = serviceProvider.GetRequiredService<IEmailIndexer>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Clean emails if needed
            if (!opts.IndexOnly)
            {
                logger.LogInformation("Starting email cleaning process");
                logger.LogInformation("Input directory: {InputDir}", opts.InputDirectory);
                logger.LogInformation("Output directory: {OutputDir}", opts.OutputDirectory);

                var cleanStopwatch = System.Diagnostics.Stopwatch.StartNew();
                int cleanedCount = await cleaner.CleanDirectoryAsync(opts.InputDirectory, opts.OutputDirectory);
                cleanStopwatch.Stop();

                logger.LogInformation("Cleaned {Count} emails in {Time}ms", 
                    cleanedCount, cleanStopwatch.ElapsedMilliseconds);
            }

            // Index emails if needed
            if (!opts.CleanOnly)
            {
                logger.LogInformation("Starting email indexing process");
                logger.LogInformation("Using cleaned emails from: {CleanedDir}", opts.OutputDirectory);

                var indexStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var indexedCount = await indexer.IndexDirectoryAsync(opts.OutputDirectory);
                indexStopwatch.Stop();

                logger.LogInformation("Indexed {Count} emails in {Time}ms", 
                    indexedCount, indexStopwatch.ElapsedMilliseconds);
            }

            stopwatch.Stop();
            logger.LogInformation("Total processing time: {Time}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}