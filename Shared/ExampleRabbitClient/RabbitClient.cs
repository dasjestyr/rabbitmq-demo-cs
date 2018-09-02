using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Shared.ExampleRabbitClient
{
    /*
     *  The plan here is to create a fanout exchange for the service but also
     *  provide a way to create and consume topics which aren't necessarily
     *  "owned" by any particular service
     */

    public class RabbitClient : IServiceBus, IDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ILogger<RabbitClient> _logger;
        private IConnection _connection;
        private IModel _channel;

        public MessageDispatcher Dispatcher { get; private set; }

        public TransportManager TransportManager { get; private set; }

        public RabbitClient(
            string connectionString, // host=<host>;user=<user>;pass=<pass>
            string queueName,
            ILogger<RabbitClient> logger)
        {
            _connectionString = connectionString;
            _queueName = queueName;
            _logger = logger;
        }

        public void Init(IServiceProvider serviceProvider)
        {
            var properties = ParseConnectionString(_connectionString);
            var factory = new ConnectionFactory
            {
                HostName = properties["host"],
                UserName = properties["user"],
                Password = properties["pass"],
                //DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("Installing Queues and Exchanges...");
            TransportManager = new TransportManager(_queueName, _channel);
            TransportManager.Install();
            Dispatcher = new MessageDispatcher(_queueName, _channel, serviceProvider, _logger);
        }

        public void Start()
        {
            Dispatcher.Start();
        }

        public Task SendAsync(string destination, object message)
        {
            return PublishAsync(destination, message, string.Empty);
        }

        public Task PublishAsync(string destination, object message, string routingKey)
        {
            var properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object>();
            properties.Headers.Add("MessageType", message.GetType().Name);
            return Task.Run(() => _channel.BasicPublish(
                exchange: destination,
                routingKey: routingKey,
                basicProperties: properties,
                body: SerializeMessage(message)));
        }

        public Task PublishAsync(string destination, object message, IDictionary<string, object> arguments)
        {
            arguments.Add("MessageType", message.GetType().Name);
            var properties = new BasicProperties {Headers = arguments};

            return Task.Run(() => _channel.BasicPublish(
                exchange: destination,
                routingKey: string.Empty,
                basicProperties: properties,
                body: SerializeMessage(message)));
        }

        public Task PublishLocalAsync(object message)
        {
            return PublishAsync(_queueName, message, string.Empty);
        }
        
        private static byte[] SerializeMessage(object message) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        private static IDictionary<string, string> ParseConnectionString(string connectionString)
        {
            var kvps = connectionString.Split(';');

            return kvps
                .Select(pair => pair.Split('='))
                .ToDictionary(values => values[0], values => values[1]);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(!disposing)
                return;

            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}
