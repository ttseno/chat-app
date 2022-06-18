using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatWSServer
{
    public interface IChatMessageRepository
    {
        Task AddAsync(ChatMessage message);
        IEnumerable<ChatMessage> GetMessages(string roomId, int take = 50, int skip = 0);
    }
    
    public class ChatMessageRepository : IChatMessageRepository
    {
        private DbContext _db;
        private readonly DbSet<ChatMessage> _chatMessage;

        public ChatMessageRepository(DbContext db)
        {
            _db = db;
            _chatMessage = db.Set<ChatMessage>();
        }

        public async Task AddAsync(ChatMessage message)
        {
            try
            {
                await _chatMessage.AddAsync(message);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public IEnumerable<ChatMessage> GetMessages(string roomId, int take, int skip)
        {
            return _chatMessage
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.TimeStamp)
                .Take(take)
                .Skip(skip);
        }
    }
}