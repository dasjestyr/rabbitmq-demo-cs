using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Shared.ExampleRabbitClient
{
    public class TransportManager
    {
        private readonly string _queueName;
        private readonly IModel _channel;

        public TransportManager(string queueName, IModel channel)
        {
            _queueName = queueName;
            _channel = channel;
        }

        public void Install()
        {
            CreateServiceExchange();
            CreateServiceQueue();
            BindQueue();
        }
        
        /// <summary>
        /// Subscribes this service's exchange to a topic exchange using.
        /// </summary>
        /// <param name="sourceExchangeName"></param>
        /// <param name="routingKey"></param>
        public void SubscribeTopic(string sourceExchangeName, string routingKey)
        {
            DeclareTopic(sourceExchangeName);
            _channel.ExchangeBind(
                destination: _queueName,
                source: sourceExchangeName,
                routingKey: routingKey,
                arguments: null);
        }

        /// <summary>
        /// Binds this service's exchange to a header exchange using the provided specification.
        /// </summary>
        /// <param name="sourceExchangeName"></param>
        /// <param name="matchRule"></param>
        /// <param name="specification"></param>
        public void SubscribeHeaders(
            string sourceExchangeName,
            string matchRule,
            IDictionary<string, object> specification)
        {
            DeclareHeadersExchange(sourceExchangeName);
            _channel.ExchangeBind(
                destination: _queueName,
                source: sourceExchangeName,
                routingKey: string.Empty,
                arguments: specification);
        }

        public void DeclareTopic(string topicName)
        {
            _channel.ExchangeDeclare(
                exchange: topicName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null);
        }

        public void DeclareHeadersExchange(string name)
        {
            _channel.ExchangeDeclare(
                exchange: name,
                type: ExchangeType.Headers,
                durable: true,
                autoDelete: false,
                arguments: null);
        }

        public bool ExchangeExists(string name)
        {
            try
            {
                _channel.ExchangeDeclarePassive(name);
                return true;
            }
            catch (OperationInterruptedException ex)
            {
                if (ex.ShutdownReason.ReplyCode == 404)
                    return false;

                throw;
            }
        }
        
        private void CreateServiceQueue()
        {
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private void CreateServiceExchange()
        {
            _channel.ExchangeDeclare(
                exchange: _queueName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);
        }

        private void BindQueue()
        {
            _channel.QueueBind(
                queue: _queueName,
                exchange: _queueName, 
                routingKey: "", 
                arguments: null);
        }
    }
}