using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using SampleCourier.Common.OperationResults;
using SampleCourier.Contracts;

namespace SampleCourier.Common.MassTransit
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IBus _bus;

        public BusPublisher(IBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        private readonly HashSet<Type> _mapped = new HashSet<Type>();

        public async Task<Guid> Send<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var correlationId = Guid.NewGuid();
            var commandType = typeof(TCommand);

            string queueName;
            const string usualCommandsNamespace = "CoreContracts.Commands.";
            if (!commandType.IsInterface)
                commandType = commandType.GetInterfaces().First(i => typeof(ICommand).IsAssignableFrom(i));

            if (commandType.FullName.StartsWith(usualCommandsNamespace))
                queueName = commandType.UnderscorizePascalCamelCase(usualCommandsNamespace);
            else
                throw new Exception($"The command '{commandType.FullName}' is not assigned to any message namespace");

            var addr = new Uri(_bus.Address, queueName);
            var ep = await _bus.GetSendEndpoint(addr);
            await ep.Send(command);
            return correlationId;
        }

        public async Task<IOperationResult> SendRequest<TCommand>(TCommand command) where TCommand : class, ICommand
            => await SendRequest<TCommand, IOperationResult>(command);

        public async Task<TResult> SendRequest<TCommand, TResult>(TCommand command) where TCommand : class, ICommand where TResult : class, IOperationResult
            => await SendRequestInternal<TCommand, TResult>(command);

        private static Type GetImplementedInterfaceImplementing(Type concreteInstance, Type parentInterfaceOfTarget)
        {
            if (concreteInstance == null) throw new ArgumentNullException(nameof(concreteInstance));
            if (parentInterfaceOfTarget == null) throw new ArgumentNullException(nameof(parentInterfaceOfTarget));

            if (!parentInterfaceOfTarget.IsInterface)
                throw new ArgumentException("Type must be an interface", nameof(parentInterfaceOfTarget));

            if (concreteInstance.IsInterface)
                if (concreteInstance.GetInterfaces().Contains(parentInterfaceOfTarget))
                    return concreteInstance;

            if (concreteInstance.IsClass)
            {
                var interfaces = concreteInstance.GetInterfaces();
                var applicableIfaces = interfaces.Where(i => i != parentInterfaceOfTarget && i.GetInterfaces().Contains(parentInterfaceOfTarget));

                if (applicableIfaces.Count() > 1)
                    applicableIfaces = applicableIfaces.Where(i => i.Name.Equals($"I{concreteInstance.Name}", StringComparison.OrdinalIgnoreCase));

                if (applicableIfaces.SingleOrDefault() is Type destIface)
                    return destIface;
            }

            throw new InvalidOperationException($"Could not obtain type implementing {parentInterfaceOfTarget} from {concreteInstance}");
        }

        protected async Task<TResult> SendRequestInternal<TCommand, TResult>(TCommand command) where TCommand : class, ICommand where TResult : class, IOperationResult
        {
            var commandInterface = GetImplementedInterfaceImplementing(typeof(TCommand), typeof(ICommand));
            var returnedTask = (Task<TResult>)GetType().GetMethod(nameof(ActuallySendRequest), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(commandInterface, typeof(TResult))
                .Invoke(this, new object[] { command });

            //var cmdStr = command.ToString();

            //if (cmdStr == "Operations.Commands.AccountingGroup.CreateAccountingGroupViaTransaction")
            //{
            //    return await returnedTask;
            //}

            return await returnedTask;
        }

        protected async Task<TResult> ActuallySendRequest<TCommand, TResult>(TCommand command) where TCommand : class, ICommand where TResult : class, IOperationResult
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var correlationId = Guid.NewGuid();
            var commandType = typeof(TCommand);

            string queueName;
            const string usualCommandsNamespace = "SampleCourier.Contracts.Commands.";
            if (!commandType.IsInterface)
                commandType = commandType.GetInterfaces().First(i => typeof(ICommand).IsAssignableFrom(i));

            if (commandType.FullName.StartsWith(usualCommandsNamespace))
                queueName = commandType.UnderscorizePascalCamelCase(usualCommandsNamespace);
            else
                throw new Exception($"The command '{commandType.FullName}' is not assigned to any message namespace");

            var serviceAddress = new Uri(_bus.Address, queueName);
            var client = _bus.CreateRequestClient<TCommand>(serviceAddress, TimeSpan.FromSeconds(60));
            using (var handle = client.Create(command))
            {
                var response = await handle.GetResponse<TResult>(true);
                return response.Message;
            }
        }

        public async Task Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var correlationId = Guid.NewGuid();
            var commandType = typeof(TEvent);

            string routingKey;
            const string usualCommandsNamespace = "SampleCourier.Contracts.Events.";
            if (!commandType.IsInterface)
                commandType = commandType.GetInterfaces().First(i => typeof(IEvent).IsAssignableFrom(i));

            if (commandType.FullName.StartsWith(usualCommandsNamespace))
                routingKey = commandType.UnderscorizePascalCamelCase(usualCommandsNamespace);
            else
                throw new Exception($"The event '{commandType.FullName}' is not assigned to any message namespace");

            await _bus.Publish(@event, context =>
            {
                context.SetRoutingKey(routingKey);
                context.CorrelationId = context.ConversationId = correlationId;
            });
        }
    }
}
