using SampleCourier.Contracts;
using System.Threading.Tasks;

namespace SampleCourier.Common
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, ICorrelationContext correlationContext);
    }
}
