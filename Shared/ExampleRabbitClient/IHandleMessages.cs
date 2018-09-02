using System.Threading.Tasks;

namespace Shared.ExampleRabbitClient
{
    public interface IHandleMessages<in TMessageType> : IMessageHandler
    {
        Task Handle(TMessageType message);
    }
}