using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatWSServer.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ChatWSServer
{
    public record Bot(string name, string prefix, string queue);
    
    public interface IBotManager
    {
        bool IsBotCommand(string message);
        void SendMessage(string roomId, string message);
    }
    
    public class BotManager : IBotManager
    {
        private static string ConnectionString;

        public List<Bot> BotList = new List<Bot>()
        {
            new Bot("stocks-bot", "/stock=", "stocks-queue")
        };

        public BotManager(RabbitMQConfig config)
        {
            ConnectionString = config.ConnectionString;
        }

        public bool IsBotCommand(string message)
        {
            return BotList.Any(b => message.StartsWith(b.prefix));
        }

        public void SendMessage(string roomId, string message)
        {
            var bot = BotList.First(b => message.StartsWith(b.prefix));
            var requestContent = new
            {
                roomId,
                stockCode = message.Replace(bot.prefix, "")
            };

            SendMessage(JsonConvert.SerializeObject(requestContent), bot);
        }

        
        private void SendMessage(string message, Bot bot)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(ConnectionString)
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: bot.queue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.BasicPublish(exchange: "",
                    routingKey: bot.queue,
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(message));

                Console.WriteLine($"Sent command request to bot {bot.name}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
            }
        }
    }
}