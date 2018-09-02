using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ExampleRabbitClient;

namespace Consumer2
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

            host.RunAsync().GetAwaiter().GetResult();

            var services = (IServiceBus)host.Services.GetService(typeof(IServiceBus));
            var testMessage = new { Type = "TestMessage", Message = "Hello world!" };
        }
    }
}
