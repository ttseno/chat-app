using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatWSServer.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatWSServer
{
    public class BotMessagesConsumer : BackgroundService
    {
        private readonly RabbitMQConfig _rabbitMqConfig;
        private readonly IWebSocketServerConnectionManager _socketsManager;
        
        public BotMessagesConsumer(RabbitMQConfig rabbitMqConfig, IWebSocketServerConnectionManager socketsManager)
        {
            _rabbitMqConfig = rabbitMqConfig;
            _socketsManager = socketsManager;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for bot messages...");

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_rabbitMqConfig.ConnectionString),
                DispatchConsumersAsync = true
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _rabbitMqConfig.BotsResponseQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += MessageHandler;
            channel.BasicConsume(queue: _rabbitMqConfig.BotsResponseQueue,
                autoAck: true,
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_rabbitMqConfig.ExecutionDelay, stoppingToken);
            }
        }

        private async Task MessageHandler(
            object sender, BasicDeliverEventArgs e)
        {
            var body = Encoding.UTF8.GetString(e.Body.ToArray());
            var botMessage = JsonConvert.DeserializeObject<BotMessage>(body);
            await _socketsManager.Broadcast(botMessage.message, botMessage.roomId, botMessage.user);
        }
    }

    public record BotMessage(string user, string roomId, string message);
}