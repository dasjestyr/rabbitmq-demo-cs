using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.ExampleRabbitClient
{
    public class MessageDispatcher
    {
        private readonly string _queueName;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly EventingBasicConsumer _consumer;
        private readonly Dictionary<string, HandlerDescriptor> _handlers;

        public MessageDispatcher(string queueName, IModel channel, IServiceProvider serviceProvider, ILogger logger)
        {
            _queueName = queueName;
            _channel = channel;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _consumer = new EventingBasicConsumer(channel);
            _handlers = new Dictionary<string, HandlerDescriptor>();
        }

        public void Start()
        {
            _channel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: _consumer);
            SetListener();
        }

        private void SetListener()
        {
            _consumer.Received += (sender, args) =>
            {
                try
                {
                    string messageType = null;
                    if (args.BasicProperties.Headers?["MessageType"] is byte[] messageTypeBytes)
                    messageType = Encoding.UTF8.GetString(messageTypeBytes);
                
                    if (messageType == null || !_handlers.ContainsKey(messageType))
                    {
                        _logger.LogWarning($"No handler found for type '{messageType}'");
                        return;
                    }

                    var descriptor = _handlers[messageType];
                    var message = JsonConvert.DeserializeObject(
                        Encoding.UTF8.GetString(args.Body),
                        descriptor.ObjectType);

                    var handler = ActivatorUtilities.CreateInstance(_serviceProvider, descriptor.HandlerType);
                
                    handler
                        .GetType()
                        .GetMethod("Handle")
                        .Invoke(handler, new[] { message });

                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Fatal error while executing handler.");
                    _channel.BasicNack(args.DeliveryTag, false, true); // TODO: how to deal with max noack? e.g. move to error queue after x number of failures
                    throw;
                }
            };
        }

        public void RegisterHandler<TMessageType, THandlerType>()
            where THandlerType : IHandleMessages<TMessageType>
        {
            var descriptor = new HandlerDescriptor(typeof(TMessageType), typeof(THandlerType));
            _handlers.Add(typeof(TMessageType).Name, descriptor);
        }

        private class HandlerDescriptor
        {
            public readonly Type ObjectType;
            public readonly Type HandlerType;

            public HandlerDescriptor(Type objectType, Type handlerType)
            {
                ObjectType = objectType;
                HandlerType = handlerType;
            }
        }
    }
}