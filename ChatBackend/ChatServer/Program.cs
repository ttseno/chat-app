using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ChatServer
{
    class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs message)
        {
            Console.WriteLine("Message received: " + message.Data);
            Send(message.Data);
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var serverUrl = "ws://localhost:5050";
            var server = new WebSocketServer(serverUrl);
            
            server.AddWebSocketService<Echo>("/Echo");
            
            server.Start();
            Console.WriteLine($"Starting server at {serverUrl}");
            Console.ReadKey();
            Console.WriteLine($"Stopping server at {serverUrl}");
        }
    }
}