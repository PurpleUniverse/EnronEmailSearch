namespace EnronEmailSearch.Core.Models
{
    public class Topic
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;

        // Navigation property
        public ICollection<TopicDocumentMapping> DocumentMappings { get; set; } = new List<TopicDocumentMapping>();
    }
}