using ChatWSServer.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddChatManager();
            services.AddServerConfiguration();
            services.AddHostedService<BotMessagesConsumer>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();
            app.UseWebSocketServer();
            
            using var scope = app.ApplicationServices.CreateScope();
            using var context = scope.ServiceProvider.GetService<DbContext>();
            if (context != null)
                context.Database.Migrate();
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