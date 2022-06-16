using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace StocksBot
{
    public class MessagesConsumer : BackgroundService
    {
        private const string ConnectionString = "amqp://guest:guest@localhost:5672";
        private const string Queue = "stocks-queue";
        private const int ExecutionDelay = 10;
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Waiting for stocks prices requests...");

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(ConnectionString)
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: Queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += MessageHandler;
            channel.BasicConsume(queue: Queue,
                autoAck: true,
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(ExecutionDelay, stoppingToken);
            }
        }

        private void MessageHandler(
            object sender, BasicDeliverEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"New message received: {message}");
        }
    }
}