using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.ExampleRabbitClient
{
    public interface IServiceBus
    {
        MessageDispatcher Dispatcher { get; }

        TransportManager TransportManager { get; }

        /// <summary>
        /// Sends a messages directly to an exchange with no routing arguments.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendAsync(string destination, object message);

        /// <summary>
        /// Publish a message to an exchange with a routing key.
        /// </summary>
        /// <param name="destination">The name of the exchange.</param>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        Task PublishAsync(string destination, object message, string routingKey);

        /// <summary>
        /// Publish a message to an exchange with arguments.
        /// </summary>
        /// <param name="destiantion"></param>
        /// <param name="message"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        Task PublishAsync(string destiantion, object message, IDictionary<string, object> arguments);

        /// <summary>
        /// Publish a message to the local queue.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishLocalAsync(object message);
    }
}