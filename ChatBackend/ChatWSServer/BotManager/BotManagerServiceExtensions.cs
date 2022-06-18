using ChatWSServer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatWSServer
{
    public static class BotManagerServiceExtensions
    {
        public static IServiceCollection AddChatBotCommands(this IServiceCollection services)
        {
            services.AddSingleton<IBotManager, BotManager>();
            return services;
        }
    }
}