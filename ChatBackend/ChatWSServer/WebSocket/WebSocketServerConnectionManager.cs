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
    public record WebSocketSession(string ClientId, string Username);

    public interface IWebSocketServerConnectionManager
    {
        string AddSocket(WebSocket socket, string username);
        Task CloseSocket(WebSocket socket, WebSocketReceiveResult result);
        Task SendMessage(WebSocket socket, string message, string username, DateTime? timestamp = null);
        Task Broadcast(string message, string username);
    }
    
    public class WebSocketServerConnectionManager : IWebSocketServerConnectionManager
    {
        private ConcurrentDictionary<WebSocketSession, WebSocket> _sockets = new ConcurrentDictionary<WebSocketSession, WebSocket>();

        public string AddSocket(WebSocket socket, string username)
        {
            var connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(new WebSocketSession(connectionId, username) , socket);
            Console.WriteLine("WebSocketServerConnectionManager-> AddSocket: WebSocket added with ID: " + connectionId);
            return connectionId;
        }

        public ConcurrentDictionary<WebSocketSession, WebSocket> GetAllSockets()
        {
            return _sockets;
        }

        public async Task SendMessage(WebSocket socket, string  message, string username, DateTime? timestamp = null)
        {
            timestamp ??= DateTime.Now;
            var response = JsonConvert.SerializeObject(new { message, username, timestamp });
            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(Encoding.UTF8.GetBytes(response), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        
        public async Task Broadcast(string message, string username)
        {
            foreach (var sock in GetAllSockets())
            {
                await SendMessage(sock.Value, message, username);
            }
        }

        public async Task CloseSocket(WebSocket socket, WebSocketReceiveResult result)
        {
            var session = _sockets.FirstOrDefault(s => s.Value == socket).Key;
            _sockets.TryRemove(session, out WebSocket removedSocket);
            await removedSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}