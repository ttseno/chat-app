using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.RenderTree;
using Newtonsoft.Json;

namespace ChatWSServer
{
    public record WebSocketSession(string ClientId, string roomId, string Username);

    public interface IWebSocketServerConnectionManager
    {
        string AddSocket(WebSocket socket, string roomId, string username);
        Task CloseSocketAsync(WebSocket socket, WebSocketReceiveResult result);
        Task SendMessageAsync(WebSocket socket, string roomId, string message, string username, DateTimeOffset? timestamp = null);
        Task BroadcastAsync(string message, string roomId, string username);
    }
    
    public class WebSocketServerConnectionManager : IWebSocketServerConnectionManager
    {
        private ConcurrentDictionary<WebSocketSession, WebSocket> _sockets = new ConcurrentDictionary<WebSocketSession, WebSocket>();

        public string AddSocket(WebSocket socket, string roomId, string username)
        {
            var connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(new WebSocketSession(connectionId, roomId, username) , socket);
            Console.WriteLine("WebSocketServerConnectionManager-> AddSocket: WebSocket added with ID: " + connectionId);
            return connectionId;
        }

        public ConcurrentDictionary<WebSocketSession, WebSocket> GetAllSockets()
        {
            return _sockets;
        }

        public async Task SendMessageAsync(WebSocket socket, string roomId, string  message, string username, DateTimeOffset? timestamp = null)
        {
            timestamp ??= DateTimeOffset.Now;
            var response = JsonConvert.SerializeObject(new { message, username, roomId, timestamp });
            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(Encoding.UTF8.GetBytes(response), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        
        public async Task BroadcastAsync(string message, string roomId, string username)
        {
            var roomSockets = GetAllSockets().Where(x => x.Key.roomId == roomId);
            foreach (var sock in roomSockets)
            {
                await SendMessageAsync(sock.Value, roomId, message, username);
            }
        }

        public async Task CloseSocketAsync(WebSocket socket, WebSocketReceiveResult result)
        {
            var session = _sockets.FirstOrDefault(s => s.Value == socket).Key;
            _sockets.TryRemove(session, out WebSocket removedSocket);
            await removedSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}