namespace EnronEmailSearch.Core.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Position { get; set; }

        // Navigation property
        public ICollection<EmailRecipient> EmailsReceived { get; set; } = new List<EmailRecipient>();
    }
}