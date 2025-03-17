namespace EnronEmailSearch.Core.Models
{
    public class EmailRecipient
    {
        public int EmailId { get; set; }
        public int ContactId { get; set; }
        public RecipientType RecipientType { get; set; }

        // Navigation properties
        public EmailFile Email { get; set; } = null!;
        public Contact Contact { get; set; } = null!;
    }
}