using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using StocksBot.Configuration;

namespace StocksBot
{
    public interface IMessagesSender
    {
        Task SendMessage(string message);
    }
    
    public class MessagesSender : IMessagesSender
    {
        private readonly RabbitMQConfig _config;
        
        public MessagesSender(RabbitMQConfig config)
        {
            _config = config;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(_config.ConnectionString)
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: _config.ResponseQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                channel.BasicPublish(exchange: "",
                    routingKey: _config.ResponseQueue,
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(message));

                Console.WriteLine($"Sent response on queue {_config.ResponseQueue}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
            }
        }
    }
}