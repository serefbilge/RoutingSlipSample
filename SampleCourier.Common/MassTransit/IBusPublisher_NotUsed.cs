using SampleCourier.Common.OperationResults;
using SampleCourier.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleCourier.Common.MassTransit
{
    public interface IBusPublisher_NotUsed
    {
        Task<Guid> Send<TCommand>(TCommand command) where TCommand : class, ICommand;
        Task<IOperationResult> SendRequest<TCommand>(TCommand command) where TCommand : class, ICommand;
        Task<TResult> SendRequest<TCommand, TResult>(TCommand command) where TCommand : class, ICommand where TResult : class, IOperationResult;
        Task Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}
