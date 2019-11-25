using MassTransit;
using MassTransit.Courier;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GreenPipes;
using GreenPipes.Internals.Extensions;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Hosting;

namespace SampleCourier.Common.MassTransit
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommandHandlers(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null)
                throw new ArgumentNullException(nameof(serviceCollection));

            foreach (var handler in Assembly.GetEntryAssembly().DefinedTypes.Where(t =>
                t.GetInterfaces().Any(tt =>
                    tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))))
            {
                var serviceType = handler.GetInterfaces().Single(tt => tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
                var impl = handler;

                serviceCollection.AddTransient(serviceType, handler);
            }
        }
                
        public static void AddEventHandlers(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null)
                throw new ArgumentNullException(nameof(serviceCollection));

            foreach (var handler in Assembly.GetEntryAssembly().DefinedTypes.Where(t => t.GetInterfaces().Any(tt => tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(IEventHandler<>))))
                serviceCollection.AddScoped(handler.GetInterfaces().Single(tt => tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(IEventHandler<>)), handler);
        }

        public static void AddMassTransitConf(this IServiceCollection serviceCollection, Action<IRabbitMqHost, IRabbitMqBusFactoryConfigurator, IServiceProvider> configurator = null, params Type[] activitiesToAdd)
        {
            foreach (var definedCommandHandler in DefinedCommandHandlers)
                serviceCollection.AddScoped(typeof(ICommandHandler<,>).MakeGenericType(definedCommandHandler.CommandType, definedCommandHandler.ResultType),
                    definedCommandHandler.CommandHandlerType);

            foreach (var definedEventHandler in DefinedEventHandlers)
                serviceCollection.AddScoped(typeof(IEventHandler<>).MakeGenericType(definedEventHandler.EventType),
                    definedEventHandler.EventHandlerType);

            foreach (var definedActivityType in DefinedActivities)
                serviceCollection.AddScoped(definedActivityType);

            //if (DefinedActivities.Any())
            //    serviceCollection.AddSingleton<RoutingSlipFailureConsumer>();

            serviceCollection.AddSingleton<IHostedService, BusService>();
            serviceCollection.AddSingleton<IBusPublisher, BusPublisher>();
            serviceCollection.AddSingleton(typeof(CommandHandlerWrapper<,>), typeof(CommandHandlerWrapper<,>));
            serviceCollection.AddSingleton(typeof(EventHandlerWrapper<>), typeof(EventHandlerWrapper<>));

            serviceCollection.AddMassTransit(x => x.AddBus(sp => BusFactory(sp, configurator)));
        }

        private static IBusControl BusFactory(IServiceProvider serviceProvider, Action<IRabbitMqHost, IRabbitMqBusFactoryConfigurator, IServiceProvider> configurator)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();

            var hostname = configuration.GetConnectionString("RabbitMQHost");
            var username = configuration.GetConnectionString("RabbitMQUser");
            var password = configuration.GetConnectionString("RabbitMQPassword");
            var vhost = string.IsNullOrWhiteSpace(configuration.GetConnectionString("RabbitMQVhost")) ? "/" : configuration.GetConnectionString("RabbitMQVhost").Trim('/');

            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(hostname, vhost, hostCfg =>
                {
                    hostCfg.Username(username);
                    hostCfg.Password(password);
                });

                //if (DefinedActivities.Any())
                //{
                //    cfg.ReceiveEndpoint(host, Assembly.GetEntryAssembly().GenerateRoutingSlipFailureQueueName(), x =>
                //    {
                //        x.PrefetchCount = 10;
                //        x.UseMessageRetry(rCfg => rCfg.Interval(5, TimeSpan.FromSeconds(30)));
                //        x.Consumer(typeof(RoutingSlipFailureConsumer), serviceProvider.GetRequiredService);
                //    });
                //}

                foreach (var definedCommandHandler in DefinedCommandHandlers)
                {
                    var (_, commandType, resultType) = definedCommandHandler;

                    string queueName;
                    const string usualCommandsNamespace = "SampleCourier.Contracts.Commands.";
                    if (commandType.FullName.StartsWith(usualCommandsNamespace))
                        queueName = commandType.UnderscorizePascalCamelCase(usualCommandsNamespace);
                    else
                        throw new Exception($"The command '{commandType.FullName}' is not assigned to any message namespace");

                    void Configure(IRabbitMqReceiveEndpointConfigurator epCfg)
                    {
                        epCfg.PrefetchCount = 1;
                        epCfg.UseMessageRetry(rCfg => rCfg.Interval(5, TimeSpan.FromSeconds(30)));
                        epCfg.Consumer(typeof(CommandHandlerWrapper<,>).MakeGenericType(commandType, resultType), serviceProvider.GetRequiredService);
                    }

                    cfg.ReceiveEndpoint(
                        host,
                        queueName,
                        Configure);

                    System.Diagnostics.Debug.WriteLine($"Configured endpoint for qn: \"{queueName}\"");
                }

                foreach (var definedEventHandler in DefinedEventHandlers)
                {
                    var (_, eventType) = definedEventHandler;

                    string queueName;
                    const string usualEventsNamespace = "SampleCourier.Contracts.Events.";
                    if (eventType.FullName.StartsWith(usualEventsNamespace))
                        queueName = eventType.UnderscorizePascalCamelCase(usualEventsNamespace);
                    else
                        throw new Exception($"The event '{eventType.FullName}' is not assigned to any message namespace");

                    void Configure(IRabbitMqReceiveEndpointConfigurator epCfg)
                    {
                        epCfg.PrefetchCount = 1;
                        epCfg.UseMessageRetry(rCfg => rCfg.Interval(5, TimeSpan.FromSeconds(30)));
                        epCfg.Consumer(typeof(EventHandlerWrapper<>).MakeGenericType(eventType), serviceProvider.GetRequiredService);
                    }

                    cfg.ReceiveEndpoint(
                        host,
                        queueName + "_" + EntryAssembly.GetName().Name,
                        Configure
                    );
                }

                configurator?.Invoke(host, cfg, serviceProvider);
            });
        }

        private static Assembly EntryAssembly => Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Could not obtain entry assembly");

        private static IEnumerable<(Type CommandHandlerType, Type CommandType, Type ResultType)> DefinedCommandHandlers
        {
            get
            {
                foreach (var commandHandlerType in EntryAssembly.DefinedTypes
                    .Where(t => t.HasInterface(typeof(ICommandHandler<,>))))
                {
                    var genericArguments = commandHandlerType.GetInterface(typeof(ICommandHandler<,>))
                        .GetGenericArguments();

                    var commandType = genericArguments[0];
                    if (!commandType.IsInterface)
                        continue;

                    var resultType = genericArguments[1];

                    yield return (commandHandlerType, commandType, resultType);
                }
            }
        }

        private static IEnumerable<(Type EventHandlerType, Type EventType)> DefinedEventHandlers
        {
            get
            {
                foreach (var eventHandlerType in EntryAssembly.DefinedTypes
                    .Where(t => t.HasInterface(typeof(IEventHandler<>))))
                {
                    var genericArguments = eventHandlerType.GetInterface(typeof(IEventHandler<>)).GetGenericArguments();

                    var eventType = genericArguments[0];
                    if (!eventType.IsInterface)
                        continue;

                    yield return (eventHandlerType, eventType);
                }
            }
        }

        private static IEnumerable<Type> DefinedActivities =>
            EntryAssembly.DefinedTypes.Where(t =>
                !t.IsAbstract && t.IsClass && (typeof(IActivity).IsAssignableFrom(t) ||
                                               typeof(IExecuteActivity).IsAssignableFrom(t)));
    }
}
