using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatWSServer
{
    public class ChatMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; init; }
        public string ConnectionId { get; init; }
        public string ChatRoom { get; init; }
        public string Username { get; init; }
        public string MessageContent { get; init; }
    }
}