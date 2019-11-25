using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using SampleCourier.Common.OperationResults;
using SampleCourier.Contracts;

namespace SampleCourier.Common.MassTransit
{
    public class CommandHandlerWrapper<TCommand, TResult> : IConsumer<TCommand>
        where TCommand : class, ICommand
        where TResult : class, IOperationResult
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandHandlerWrapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
                    var result = await handler.HandleAsync(context.Message, context.ToCorrelationContext());
                    await context.RespondAsync(result);
                }
            }
            catch (Exception e) when (typeof(TResult) == typeof(IOperationResult))
            {
                await context.RespondAsync(new OperationResult(e.Message));
            }
            catch (Exception e) when (typeof(TResult) == typeof(IIdentifierResult))
            {
                await context.RespondAsync(new IdentifierResult(e.Message));
            }
        }
    }
}