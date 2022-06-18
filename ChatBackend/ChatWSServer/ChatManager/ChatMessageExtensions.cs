using Microsoft.Extensions.DependencyInjection;

namespace ChatWSServer
{
    public static class ChatMessageExtensions
    {
        public static IServiceCollection AddChatManager(this IServiceCollection services)
        {
            services.AddScoped<IChatManager, ChatManager>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            return services;
        }
    }
}