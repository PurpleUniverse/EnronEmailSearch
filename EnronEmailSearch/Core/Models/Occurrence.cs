using System.ComponentModel.DataAnnotations;

namespace EnronEmailSearch.Core.Models
{
    public class Occurrence
    {
        [Key]
        public int WordId { get; set; }
        [Key]
        public int FileId { get; set; }
        public int Count { get; set; }
        
        // Navigation properties
        public Word Word { get; set; } = null!;
        public EmailFile File { get; set; } = null!;
    }
}