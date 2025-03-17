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

        // Keep existing tables
        public DbSet<Word> Words { get; set; } = null!;
        public DbSet<EmailFile> Files { get; set; } = null!;
        public DbSet<Occurrence> Occurrences { get; set; } = null!;

        // Add new tables
        public DbSet<Contact> Contacts { get; set; } = null!;
        public DbSet<EmailRecipient> EmailRecipients { get; set; } = null!;
        public DbSet<Topic> Topics { get; set; } = null!;
        public DbSet<TopicDocumentMapping> TopicDocumentMappings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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

            // Configure EmailRecipient entity
            modelBuilder.Entity<EmailRecipient>(entity =>
            {
                entity.HasKey(e => new { e.EmailId, e.ContactId });

                entity.HasOne(e => e.Email)
                      .WithMany(f => f.Recipients)
                      .HasForeignKey(e => e.EmailId);

                entity.HasOne(e => e.Contact)
                      .WithMany(c => c.EmailsReceived)
                      .HasForeignKey(e => e.ContactId);

                entity.HasIndex(e => e.RecipientType);
            });

            // Configure Contact entity
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.ContactId);
                entity.Property(e => e.ContactId).ValueGeneratedOnAdd();
                entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.EmailAddress).IsUnique();
            });

            // Configure Topic entity
            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasKey(e => e.TopicId);
                entity.Property(e => e.TopicId).ValueGeneratedOnAdd();
                entity.Property(e => e.TopicName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.TopicName);
            });

            // Configure TopicDocumentMapping entity
            modelBuilder.Entity<TopicDocumentMapping>(entity =>
            {
                entity.HasKey(e => new { e.TopicId, e.FileId });

                entity.HasOne(e => e.Topic)
                      .WithMany(t => t.DocumentMappings)
                      .HasForeignKey(e => e.TopicId);

                entity.HasOne(e => e.File)
                      .WithMany(f => f.TopicMappings)
                      .HasForeignKey(e => e.FileId);

                entity.HasIndex(e => e.RelevanceScore);
            });
        }
    }
}