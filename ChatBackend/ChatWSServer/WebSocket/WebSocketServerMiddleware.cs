using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatWSServer.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ChatWSServer 
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IWebSocketServerConnectionManager _socketManager;
        private readonly IChatManager _chatManager;
        
        public WebSocketServerMiddleware(
            RequestDelegate next, 
            IWebSocketServerConnectionManager socketManager,
            IChatManager chatManager)
        {
            _next = next;
            _socketManager = socketManager;
            _chatManager = chatManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("WebSocket Connected");

                context.Request.Query.TryGetValue("username", out var username);
                var connectionId = _socketManager.AddSocket(webSocket, username);
                context.Request.Query.TryGetValue("roomId", out var roomId);

                await Receive(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Receive message from client: " + connectionId);
                        
                        var chatMessage = new ChatMessage()
                            {
                                ConnectionId = connectionId, Username = username, RoomId = roomId,
                                MessageContent = message
                            };
                        
                        _chatManager.HandleMessage(chatMessage);

                        await _socketManager.Broadcast(message, username);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _socketManager.CloseSocket(webSocket, result);
                    }
                });
            }
            else
            {
                await _next(context);
            }
        }
        

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
        
    }
}