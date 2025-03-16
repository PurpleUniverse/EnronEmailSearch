namespace EnronEmailSearch.Core.Models
{
    public class Word
    {
        public int WordId { get; set; }
        public string Text { get; set; } = string.Empty;
        
        // Navigation property
        public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();
    }
}