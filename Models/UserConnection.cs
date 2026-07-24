namespace SafeXChat.Models
{
    // tracks which SignalR connection belongs to which user
    // needed because a user could open the app in 2 tabs, and because
    // we need LastSeen for the "load missed messages after reconnect" logic
    public class UserConnection
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}
