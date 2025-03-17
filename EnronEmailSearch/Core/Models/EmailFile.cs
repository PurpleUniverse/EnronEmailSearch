using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnronEmailSearch.Core.Models
{
    public class EmailFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();

        // Keep original navigation properties
        public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();

        // Add new navigation properties
        public ICollection<EmailRecipient> Recipients { get; set; } = new List<EmailRecipient>();
        public ICollection<TopicDocumentMapping> TopicMappings { get; set; } = new List<TopicDocumentMapping>();
    }
}