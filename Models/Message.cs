namespace SafeXChat.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        public string SenderId { get; set; } = string.Empty;

        // text content - can be empty if message is just a file
        public string? Content { get; set; }

        // file stuff, null if it's a plain text message
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSizeBytes { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}
