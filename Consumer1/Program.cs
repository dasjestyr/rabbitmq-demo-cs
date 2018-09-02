using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;
using Shared;
using Shared.ExampleRabbitClient;

namespace Consumer1
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

            var service = host.Services.GetService<IServiceBus>();

            host.RunAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }
    }
}
