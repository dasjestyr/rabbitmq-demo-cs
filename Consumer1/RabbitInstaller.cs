﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ExampleRabbitClient;

namespace Consumer1
{
    public static class RabbitInstaller
    {
        public static void AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IServiceBus>(p =>
            {
                var client = new RabbitClient("host=localhost;user=user;pass=bitnami", "Consumer1", p.GetService<ILogger<RabbitClient>>());
                client.Init(p);
                client.Dispatcher.RegisterHandler<TestMessageA, TypeAHandler>();
                client.Start();

                // install our subscriptions
                client.TransportManager.SubscribeTopic("test-topic-1", "#.a"); // only things ending in .a
                client.TransportManager.SubscribeTopic("test-topic-2", "#"); // everything

                return client;
            });
        }
    }

    

    public class TypeAHandler : IHandleMessages<TestMessageA>
    {
        public Task Handle(TestMessageA message)
        {
            Console.WriteLine($"Message: {message.Message}");
            return Task.CompletedTask;
        }
    }
}
