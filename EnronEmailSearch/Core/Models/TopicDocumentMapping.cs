namespace EnronEmailSearch.Core.Models
{
    public class TopicDocumentMapping
    {
        public int TopicId { get; set; }
        public int FileId { get; set; }
        public double RelevanceScore { get; set; }

        // Navigation properties
        public Topic Topic { get; set; } = null!;
        public EmailFile File { get; set; } = null!;
    }
}