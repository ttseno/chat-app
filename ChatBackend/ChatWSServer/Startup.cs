using System;
using ChatWSServer.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatWSServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddChatBotCommands();
            services.AddWebSocketServerConnectionManager();
            services.AddServerConfiguration();
            services.AddHostedService<BotMessagesConsumer>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();
            app.UseWebSocketServer();
        }
    }

    public static class StartupExtensions
    {
        public static IServiceCollection AddServerConfiguration(this IServiceCollection services)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            
            services.AddSingleton(config.GetRequiredSection("RabbitMQConfig").Get<RabbitMQConfig>());
            services.AddDbContext<DbContext, DatabaseContext>(
                x => x.UseNpgsql(config["ConnectionString"]));

            return services;
        }
    }

}