using EnronEmailSearch.Core.Interfaces;
using EnronEmailSearch.Core.Services;
using EnronEmailSearch.Infrastructure.Data;
using EnronEmailSearch.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/webapp-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add database context
builder.Services.AddDbContext<EnronDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=enron_index.db");
});

// Add services
builder.Services.AddTransient<IEmailCleaner, EmailCleaner>();
builder.Services.AddTransient<EmailIndexer>();
builder.Services.AddTransient<IEmailIndexer, ResilientEmailIndexer>();

// Add resilience services
builder.Services.AddResilienceServices();

// Add controllers and views
builder.Services.AddControllersWithViews();

// Add health checks
builder.Services.AddHealthChecks()
    // Custom DB check instead of AddDbContextCheck due to NuGet package not available
    .AddCheck("Database", () => 
    {
        try 
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EnronDbContext>();
            dbContext.Database.ExecuteSqlRaw("SELECT 1");
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message);
        }
    })
    .ForwardToPrometheus();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Add Prometheus metrics middleware
app.UseMetricServer();
app.UseHttpMetrics();

// Map health checks
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}");

// Ensure database is created on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnronDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Log database statistics
    var fileCount = dbContext.Files.Count();
    var wordCount = dbContext.Words.Count();
    var occurrenceCount = dbContext.Occurrences.Count();
    
    Log.Information("Database statistics: {FileCount} files, {WordCount} words, {OccurrenceCount} occurrences",
        fileCount, wordCount, occurrenceCount);
}

app.Run();