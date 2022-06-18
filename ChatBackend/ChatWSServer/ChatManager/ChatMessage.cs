using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatWSServer
{
    public class ChatMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; init; }
        public string ConnectionId { get; init; }
        public string RoomId { get; init; }
        public string Username { get; init; }
        public string MessageContent { get; init; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string TimeStamp { get; init; } 
    }
}