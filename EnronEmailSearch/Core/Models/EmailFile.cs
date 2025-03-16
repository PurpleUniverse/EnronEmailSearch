namespace EnronEmailSearch.Core.Models
{
    public class EmailFile
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        
        // Navigation property
        public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();
    }
}