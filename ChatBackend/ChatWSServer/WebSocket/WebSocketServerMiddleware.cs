using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ChatWSServer 
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IWebSocketServerConnectionManager _socketManager;
        
        public WebSocketServerMiddleware(RequestDelegate next, IWebSocketServerConnectionManager socketManager)
        {
            _next = next;
            _socketManager = socketManager;
        }

        public async Task InvokeAsync(HttpContext context, IChatManager chatManager)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("WebSocket Connected");

                    context.Request.Query.TryGetValue("username", out var username);
                    context.Request.Query.TryGetValue("roomId", out var roomId);
                    var connectionId = _socketManager.AddSocket(webSocket, roomId, username);

                    await SendMessages(webSocket, roomId, chatManager.GetRoomHistory(roomId));

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

                            await chatManager.HandleMessage(chatMessage);

                            await _socketManager.Broadcast(message, roomId, username);
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _socketManager.CloseSocket(webSocket, result);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error while handling message. Exception: {ex.Message}");
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task SendMessages(WebSocket socket, string roomId, IEnumerable<ChatMessage> chatMessages)
        {
            foreach (var message in chatMessages)
            {
                await _socketManager.SendMessage(socket, roomId, message.MessageContent, message.Username, message.TimeStamp);
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