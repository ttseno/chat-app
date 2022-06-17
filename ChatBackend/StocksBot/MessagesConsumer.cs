using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StocksBot.Configuration;

namespace StocksBot
{
    public class MessagesConsumer : BackgroundService
    {
        private readonly IStooqClient _client;
        private readonly IMessagesSender _messagesSender;
        private readonly RabbitMQConfig _rabbitMqConfig;
        
        public MessagesConsumer(IStooqClient client, RabbitMQConfig rabbitMqConfig, IMessagesSender messagesSender)
        {
            _client = client;
            _rabbitMqConfig = rabbitMqConfig;
            _messagesSender = messagesSender;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for stocks prices requests...");

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_rabbitMqConfig.ConnectionString),
                DispatchConsumersAsync = true
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _rabbitMqConfig.Queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += MessageHandler;
            channel.BasicConsume(queue: _rabbitMqConfig.Queue,
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
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"New message received: {message}");
            var response = await _client.GetStockQuotation(message);
            _messagesSender.SendMessage(response.ToString());
        }
    }
}