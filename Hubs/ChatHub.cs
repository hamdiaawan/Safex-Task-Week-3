using Microsoft.AspNetCore.SignalR;
using SafeXChat.Data;
using SafeXChat.Models;

namespace SafeXChat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        // NOTE: in a real deployment userId would come from Context.User (auth claims)
        // for now client sends its own userId on connect via query string, that's the
        // simplest way to get this working for the demo without wiring full auth yet
        private string? GetUserId()
        {
            var httpContext = Context.GetHttpContext();
            return httpContext?.Request.Query["userId"];
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var existing = _db.UserConnections.FirstOrDefault(u => u.UserId == userId);
                if (existing == null)
                {
                    _db.UserConnections.Add(new UserConnection
                    {
                        UserId = userId,
                        ConnectionId = Context.ConnectionId,
                        IsOnline = true,
                        LastSeen = DateTime.UtcNow
                    });
                }
                else
                {
                    existing.ConnectionId = Context.ConnectionId;
                    existing.IsOnline = true;
                    existing.LastSeen = DateTime.UtcNow;
                }
                await _db.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                await Clients.Others.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var conn = _db.UserConnections.FirstOrDefault(u => u.UserId == userId);
                if (conn != null)
                {
                    conn.IsOnline = false;
                    conn.LastSeen = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                await Clients.Others.SendAsync("UserOffline", userId, DateTime.UtcNow);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // plain text message
        public async Task SendMessage(int conversationId, string senderId, string receiverId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            // send to receiver's group (covers multi-tab / reconnect w/ new connection id)
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", message);
            await Clients.Caller.SendAsync("MessageDelivered", message);

            var isReceiverOnline = _db.UserConnections.Any(u => u.UserId == receiverId && u.IsOnline);
            if (!isReceiverOnline)
            {
                // receiver offline -> queue for email notification (handled outside the hub, see NotificationService)
                await Clients.Group(receiverId).SendAsync("PendingNotification", message.MessageId);
            }
        }

        // message with a file attached, file itself already uploaded via REST endpoint,
        // this just creates the message record pointing at it
        public async Task SendFileMessage(int conversationId, string senderId, string receiverId, string fileUrl, string fileName, long fileSize)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                FileUrl = fileUrl,
                FileName = fileName,
                FileSizeBytes = fileSize,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await Clients.Group(receiverId).SendAsync("ReceiveMessage", message);
            await Clients.Caller.SendAsync("MessageDelivered", message);
        }

        public async Task MarkAsRead(int messageId, string readerId)
        {
            var message = await _db.Messages.FindAsync(messageId);
            if (message == null || message.IsRead) return;

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // tell the original sender their message got read
            await Clients.Group(message.SenderId).SendAsync("MessageRead", messageId, message.ReadAt);
        }

        // called by the client right after reconnecting, so it can catch up
        // on whatever it missed while the socket was down
        public async Task GetMissedMessages(int conversationId, DateTime lastSeenAt)
        {
            var missed = _db.Messages
                .Where(m => m.ConversationId == conversationId && m.SentAt > lastSeenAt)
                .OrderBy(m => m.SentAt)
                .ToList();

            await Clients.Caller.SendAsync("MissedMessages", missed);
        }

        public async Task NotifyTyping(string receiverId, string senderId)
        {
            await Clients.Group(receiverId).SendAsync("UserTyping", senderId);
        }
    }
}
