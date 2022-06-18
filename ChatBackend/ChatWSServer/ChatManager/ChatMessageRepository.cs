using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatWSServer
{
    public interface IChatMessageRepository
    {
        Task Add(ChatMessage message);
        Task<List<ChatMessage>> GetMessages(string roomId, int take = 50, int skip = 0);
    }
    
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly DbSet<ChatMessage> _chatMessage;

        public ChatMessageRepository(DbContext db)
        {
            _chatMessage = db.Set<ChatMessage>();
        }

        public async Task Add(ChatMessage message)
        {
            _chatMessage.Add(message);
        }

        public async Task<List<ChatMessage>> GetMessages(string roomId, int take = 50, int skip = 0)
        {
            return _chatMessage
                .Where(m => m.RoomId == roomId)
                .Take(take)
                .Skip(skip)
                .ToList();
        }
    }
}