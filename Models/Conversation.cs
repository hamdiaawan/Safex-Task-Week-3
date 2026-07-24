namespace SafeXChat.Models
{
    // one conversation = one company user talking to one intern user
    // (kept 1-to-1 for now, group chat not in scope for this task)
    public class Conversation
    {
        public int ConversationId { get; set; }

        public string CompanyUserId { get; set; } = string.Empty;
        public string InternUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Message> Messages { get; set; } = new();
    }
}
