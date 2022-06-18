using Microsoft.Extensions.DependencyInjection;

namespace ChatWSServer
{
    public static class BotManagerServiceExtensions
    {
        public static IServiceCollection AddChatBotCommands(this IServiceCollection services)
        {
            services.AddScoped<IBotManager, BotManager>();
            return services;
        }
    }
}