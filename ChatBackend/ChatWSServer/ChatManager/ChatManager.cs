using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatWSServer
{
    public interface IChatManager
    {
        Task HandleMessage(ChatMessage message);
        IEnumerable<ChatMessage> GetRoomHistory(string roomId, int take = 50, int skip = 0);
    }
    
    public class ChatManager : IChatManager
    {
        private readonly IBotManager _botManager;
        private readonly IChatMessageRepository _messageRepository;

        public ChatManager(IBotManager botManager, IChatMessageRepository messageRepository)
        {
            _botManager = botManager;
            _messageRepository = messageRepository;
        }

        public async Task HandleMessage(ChatMessage message)
        {
            if (_botManager.IsBotCommand(message.MessageContent))
            {
                _botManager.SendMessage(message.RoomId, message.MessageContent);
            }
            else
            {
                await _messageRepository.AddAsync(message);
            }
        }

        public IEnumerable<ChatMessage> GetRoomHistory(string roomId, int take = 50, int skip = 0)
        {
            var history = _messageRepository.GetMessages(roomId, take, skip);
            return history.OrderBy(h => h.TimeStamp);
        }
    }
}