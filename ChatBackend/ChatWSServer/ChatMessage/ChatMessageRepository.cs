using Microsoft.EntityFrameworkCore;

namespace ChatWSServer
{
    public interface IChatMessageRepository
    {
        
    }
    
    public class ChatMessageRepository
    {
        private readonly DbSet<ChatMessage> _chatMessage;

        public ChatMessageRepository(DbContext db)
        {
            _chatMessage = db.Set<ChatMessage>();
        }
    }
}