using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ChatWSServer
{
    public static class WebSocketServerMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketServerMiddleware>();
        }
        
        public static IServiceCollection AddWebSocketServerConnectionManager(this IServiceCollection services)
        {   
            services.AddSingleton<IWebSocketServerConnectionManager, WebSocketServerConnectionManager>();
            return services;
        }
    }
    
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IWebSocketServerConnectionManager _manager;

        public WebSocketServerMiddleware(RequestDelegate next, IWebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("WebSocket Connected");

                context.Request.Query.TryGetValue("username", out var username);
                var connectionId = _manager.AddSocket(webSocket, username);
                

                await Receive(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Receive message from client: " + connectionId);
                        Console.WriteLine($"Message: {message}");
                        await Broadcast(message, username);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _manager.CloseSocket(webSocket, result);
                    }
                });
            }
            else
            {
                Console.WriteLine("Request made from");
                await _next(context);
            }
        }

        private async Task Broadcast(string message, string username)
        {
            Console.WriteLine("Broadcast");
            foreach (var sock in _manager.GetAllSockets())
            {
                var response = JsonConvert.SerializeObject(new { message, username });
                if (sock.Value.State == WebSocketState.Open)
                    await sock.Value.SendAsync(Encoding.UTF8.GetBytes(response), WebSocketMessageType.Text, true, CancellationToken.None);
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