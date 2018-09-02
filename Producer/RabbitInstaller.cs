using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ExampleRabbitClient;

namespace Producer
{
    public static class RabbitInstaller
    {
        public static void AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IServiceBus>(p =>
            {
                var client = new RabbitClient("host=localhost;user=user;pass=bitnami", "Producer", p.GetService<ILogger<RabbitClient>>());
                client.Init(p);
                
                // declare any topics that we're going to be publishing to
                client.TransportManager.DeclareTopic("test-topic-1");
                client.TransportManager.DeclareTopic("test-topic-2");

                return client;
            });
        }
    }
}
