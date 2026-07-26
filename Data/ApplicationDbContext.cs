using Microsoft.EntityFrameworkCore;
using SafeXChat.Models;

namespace SafeXChat.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<UserConnection> UserConnections { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId);

            // speeds up "give me all messages in this conversation, newest first"
            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ConversationId, m.SentAt });

            modelBuilder.Entity<UserConnection>()
                .HasIndex(u => u.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
