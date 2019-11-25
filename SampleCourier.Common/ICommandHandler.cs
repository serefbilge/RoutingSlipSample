using System.Threading.Tasks;

namespace SampleCourier.Common
{
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command, ICorrelationContext correlationContext);
    }
}
