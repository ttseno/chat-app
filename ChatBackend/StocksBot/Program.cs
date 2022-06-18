using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StocksBot.Configuration;

namespace StocksBot
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                { 
                    IConfiguration config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables()
                        .Build();
                    AppConfiguration settings = config.GetRequiredSection("AppConfiguration").Get<AppConfiguration>();
                    services.AddSingleton(settings);
                    
                    services.AddSingleton(config.GetRequiredSection("RabbitMQConfig").Get<RabbitMQConfig>());
                    services.AddSingleton(x =>
                    {
                        return new HttpClient
                        {
                            BaseAddress = new Uri(settings.StooqApiBaseUrl)
                        };
                    });
                    services.AddScoped<IStooqClient, StooqClient>();
                    services.AddScoped<IMessagesSender, MessagesSender>();
                    services.AddHostedService<MessagesConsumer>();
                });
    }
}