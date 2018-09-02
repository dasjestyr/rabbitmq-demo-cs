using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ExampleRabbitClient;

namespace Consumer2
{
    public static class RabbitInstaller
    {
        public static void AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IServiceBus>(p =>
            {
                var client = new RabbitClient("host=localhost;user=user;pass=bitnami", "Consumer2", p.GetService<ILogger<RabbitClient>>());
                client.Init(p);
                
                // install our subscriptions
                client.TransportManager.SubscribeTopic("test-topic-1", "#.b");

                return client;
            });
        }
    }
}
