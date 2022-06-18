using System.Threading.Tasks;

namespace ChatWSServer
{
    public interface IChatManager
    {
        Task HandleMessage(ChatMessage message);
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
                _botManager.SendMessage(message.MessageContent);
            }
            else
            {
                _messageRepository.Add(message);
            }
        }
    }
}