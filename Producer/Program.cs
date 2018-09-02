using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ExampleRabbitClient;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureLogging(builder => { builder.AddConsole(); })
                .ConfigureServices(collection => { collection.AddRabbitMq(); })
                .Build();

            host.Start();

            var client = host.Services.GetService<IServiceBus>();
            var logger = host.Services.GetService<ILogger<Program>>();

            var messages = new List<Message>
            {
                new Message {Type = "a", Body = "Hello World"},
                new Message {Type = "a", Body = "Hello World"},
                new Message {Type = "b", Body = "Hello World"},
                new Message {Type = "b", Body = "Hello World"},
                new Message {Type = "b", Body = "Hello World"}
            };

            logger.LogInformation("Sending messages...");

            const string testTopic1 = "test-topic-1";
            const string testTopic2 = "test-topic-2";

            logger.LogInformation("Testing routing keys. Sending {0} messages...", messages.Count);
            foreach (var message in messages)
            {
                var routingKey = $"test.topic.{message.Type}";
                client.PublishAsync(testTopic1, message, routingKey).GetAwaiter().GetResult();
            }

            logger.LogInformation("Testing SendLocal. Sending message...");
            client.PublishLocalAsync(new { Message = "Message to self." });

            logger.LogInformation("Testing direct to Consumer1");
            client.SendAsync("Consumer1", new TestMessageA {Message = "Hello direct message!"});

            logger.LogInformation("Testing topic 2");
            client.PublishAsync(testTopic2, new {Message = "Message from topic 2"}, "test.topic2.message");
        }

        public class Message
        {
            public string Type { get; set; }    

            public string Body { get; set; }
        }
    }
}
