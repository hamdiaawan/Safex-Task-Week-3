using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeXChat.Data;
using SafeXChat.Models;

namespace SafeXChat.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ChatController(ApplicationDbContext db)
        {
            _db = db;
        }

        // get or create the 1-1 conversation between a company and an intern
        [HttpGet("conversation")]
        public async Task<IActionResult> GetOrCreateConversation(string companyUserId, string internUserId)
        {
            var convo = await _db.Conversations
                .FirstOrDefaultAsync(c => c.CompanyUserId == companyUserId && c.InternUserId == internUserId);

            if (convo == null)
            {
                convo = new Conversation
                {
                    CompanyUserId = companyUserId,
                    InternUserId = internUserId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Conversations.Add(convo);
                await _db.SaveChangesAsync();
            }

            return Ok(convo);
        }

        // message history, paged, oldest first for chat display
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId, int page = 1, int pageSize = 30)
        {
            var messages = await _db.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.SentAt) // re-sort ascending for display after paging
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("{conversationId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int conversationId, string userId)
        {
            var count = await _db.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .CountAsync();

            return Ok(new { unreadCount = count });
        }
    }
}
