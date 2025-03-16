namespace EnronEmailSearch.Core.Models
{
    public class Occurrence
    {
        public int WordId { get; set; }
        public int FileId { get; set; }
        public int Count { get; set; }
        
        // Navigation properties
        public Word Word { get; set; } = null!;
        public EmailFile File { get; set; } = null!;
    }
}