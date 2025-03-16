using EnronEmailSearch.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EnronEmailSearch.Infrastructure.Data
{
    public class EnronDbContext : DbContext
    {
        public EnronDbContext(DbContextOptions<EnronDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Word> Words { get; set; } = null!;
        public DbSet<EmailFile> Files { get; set; } = null!;
        public DbSet<Occurrence> Occurrences { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Word entity
            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasKey(e => e.WordId);
                entity.Property(e => e.WordId).ValueGeneratedOnAdd();
                entity.Property(e => e.Text).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Text).IsUnique();
            });
            
            // Configure EmailFile entity
            modelBuilder.Entity<EmailFile>(entity =>
            {
                entity.HasKey(e => e.FileId);
                entity.Property(e => e.FileId).ValueGeneratedOnAdd();
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.FileName);
            });
            
            // Configure Occurrence entity (junction table)
            modelBuilder.Entity<Occurrence>(entity =>
            {
                entity.HasKey(e => new { e.WordId, e.FileId });
                
                entity.HasOne(e => e.Word)
                      .WithMany(w => w.Occurrences)
                      .HasForeignKey(e => e.WordId);
                      
                entity.HasOne(e => e.File)
                      .WithMany(f => f.Occurrences)
                      .HasForeignKey(e => e.FileId);
            });
        }
    }
}