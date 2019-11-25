// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GreenPipes;
using MassTransit;
using MassTransit.Courier;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;

namespace SampleCourier.Common.MassTransit
{
	public static class MassTransitExts
	{
		public static IRabbitMqHost CreateHost(this IRabbitMqBusFactoryConfigurator cfg,RabbitMqHostOptions opts)
		{
			string host = opts.Host ?? "[::1]";
			ushort port = (ushort)(opts.Port ?? 5672);
			var vhost = string.IsNullOrWhiteSpace(opts.VirtualHost) ? "/" : opts.VirtualHost.Trim('/');

			return cfg.Host(host,port,vhost,null,h =>
			{
				h.Username(opts.Username ?? "guest");
				h.Password(opts.Password ?? "guest");
				h.Heartbeat(opts.Heartbeat ?? 0);

				if (!string.IsNullOrEmpty(opts.ClusterNodeHostname))
				{
					h.UseCluster(cc =>
					{
						cc.Node(opts.ClusterNodeHostname);
						cc.ClusterMembers = opts.ClusterMembers?.Split(',') ?? new string[0];
					});
				}
			});
		}

        public static string UnderscorizePascalCamelCase(this Type type, string namespaceToOmit = null)
        {
            var typeNs = type.Namespace;
            var typeName = type.Name;

            if (namespaceToOmit != null)
                if (typeNs.StartsWith(namespaceToOmit))
                    typeNs = typeNs.Substring(namespaceToOmit.Length).Trim('.');
                else
                    throw new Exception();

            if (type.IsInterface && typeName.StartsWith('I'))
                typeName = typeName.Substring(1);

            string Underscorize(string s) => Regex.Replace(s, "(.)([A-Z])", "$1_$2");

            var nameSegment = Underscorize(typeName);
            var nsSegments = typeNs.Split(new[] { '.' }).Select(Underscorize);

            return string.Concat(string.Join('.', nsSegments), ".", typeName).ToLowerInvariant();
        }

        public static ICorrelationContext ToCorrelationContext(this ConsumeContext ctx)
        {
            return new CorrelationContext(ctx.MessageId.GetValueOrDefault(), ctx.ConversationId.GetValueOrDefault(), ctx.CorrelationId.GetValueOrDefault());
        }

        public static string GetActivityExchangeName(this Type activityType)
        {
            string exchangeName;
            const string usualCommandsNamespace = "";
            if (activityType.FullName.StartsWith(usualCommandsNamespace))
                exchangeName = activityType.UnderscorizePascalCamelCase(usualCommandsNamespace);
            else
                throw new Exception($"The activity '{activityType.FullName}' resides in unsupported namespace");

            return exchangeName;
        }

        public static string GetCompensatingActivityExchangeName(this Type activityType) => GetActivityExchangeName(activityType) + ".compensations";

        public static Uri AppendActivityExchangeName(this Uri busUri, Type activityType)
        {
            return new Uri(busUri, GetActivityExchangeName(activityType));
        }

        public static Uri AppendCompensatingActivityExchangeName(this Uri busUri, Type activityType)
        {
            return new Uri(busUri, GetCompensatingActivityExchangeName(activityType));
        }

        //public static string GenerateRoutingSlipFailureQueueName(this Assembly assembly)
        //{
        //    return assembly.GetName().Name + "_RoutingSlipFailures";
        //}

        public static IRabbitMqBusFactoryConfigurator ConfigureCompensatingActivity<TCompensateActivity, TCompensateActivityParameters, TCompensateActivityLog>(this IRabbitMqBusFactoryConfigurator cfg, IRabbitMqHost host, IServiceProvider serviceProvider, string busUriStr)
        where TCompensateActivity : class, Activity<TCompensateActivityParameters, TCompensateActivityLog>
        where TCompensateActivityParameters : class
        where TCompensateActivityLog : class
        {
            var queueName = typeof(TCompensateActivity).GetCompensatingActivityExchangeName();
            cfg.ReceiveEndpoint(host, queueName, cc =>
            {
                cc.PrefetchCount = 100;
                cc.UseMessageRetry(rCfg => rCfg.Interval(5, TimeSpan.FromSeconds(30)));
                cc.CompensateActivityHost<TCompensateActivity, TCompensateActivityLog>(serviceProvider);
            });

            return cfg;
        }

        public static IRabbitMqBusFactoryConfigurator ConfigureExecuteActivity<TExecuteActivity, TExecuteActivityParameters>(this IRabbitMqBusFactoryConfigurator cfg, IRabbitMqHost host, IServiceProvider serviceProvider, string busUriStr)
            where TExecuteActivity : class, ExecuteActivity<TExecuteActivityParameters>
            where TExecuteActivityParameters : class
        {
            var queueName = typeof(TExecuteActivity).GetActivityExchangeName();
            cfg.ReceiveEndpoint(host, queueName, cc =>
            {
                cc.PrefetchCount = 100;
                cc.UseMessageRetry(rCfg => rCfg.Interval(5, TimeSpan.FromSeconds(30)));

                if (typeof(ICompensateActivity).IsAssignableFrom(typeof(TExecuteActivity)))
                {
                    cc.ExecuteActivityHost<TExecuteActivity, TExecuteActivityParameters>(new Uri(busUriStr + typeof(TExecuteActivity).GetCompensatingActivityExchangeName()), serviceProvider);
                }
                else
                    cc.ExecuteActivityHost<TExecuteActivity, TExecuteActivityParameters>(serviceProvider);
            });

            return cfg;
        }

        private static Assembly EntryAssembly => Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Could not obtain entry assembly");

        //private static IEnumerable<(Type CommandHandlerType, Type CommandType, Type ResultType)> DefinedCommandHandlers
        //{
        //    get
        //    {
        //        foreach (var commandHandlerType in EntryAssembly.DefinedTypes
        //            .Where(t => t.HasInterface(typeof(ICommandHandler<,>))))
        //        {
        //            var genericArguments = commandHandlerType.GetInterface(typeof(ICommandHandler<,>))
        //                .GetGenericArguments();

        //            var commandType = genericArguments[0];
        //            if (!commandType.IsInterface)
        //                continue;

        //            var resultType = genericArguments[1];

        //            yield return (commandHandlerType, commandType, resultType);
        //        }
        //    }
        //}

        //private static IEnumerable<(Type EventHandlerType, Type EventType)> DefinedEventHandlers
        //{
        //    get
        //    {
        //        foreach (var eventHandlerType in EntryAssembly.DefinedTypes
        //            .Where(t => t.HasInterface(typeof(IEventHandler<>))))
        //        {
        //            var genericArguments = eventHandlerType.GetInterface(typeof(IEventHandler<>)).GetGenericArguments();

        //            var eventType = genericArguments[0];
        //            if (!eventType.IsInterface)
        //                continue;

        //            yield return (eventHandlerType, eventType);
        //        }
        //    }
        //}

        private static IEnumerable<Type> DefinedActivities =>
            EntryAssembly.DefinedTypes.Where(t =>
                !t.IsAbstract && t.IsClass && (typeof(IActivity).IsAssignableFrom(t) ||
                                               typeof(IExecuteActivity).IsAssignableFrom(t)));

        public static IRabbitMqBusFactoryConfigurator ConfigureActivities(this IRabbitMqBusFactoryConfigurator cfg, IRabbitMqHost host, IServiceProvider serviceProvider, IConfiguration svcCfg)
        {
            var busUriStr = new Uri(new Uri("rabbitmq://" + svcCfg.GetConnectionString("RabbitMQHost") + "/"), svcCfg.GetConnectionString("RabbitMQVhost")).ToString().TrimEnd('/') + '/';
            var invocationParameters = new object[] { cfg, host, serviceProvider, busUriStr };

            foreach (var activityType in DefinedActivities)
            {
                //var a = activityType.FullName;
                var interfaces = activityType.GetInterfaces();
                var argumentsType = interfaces.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ExecuteActivity<>)).GenericTypeArguments.Single();

                if (typeof(ICompensateActivity).IsAssignableFrom(activityType))
                {
                    var logType = interfaces.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(CompensateActivity<>)).GenericTypeArguments.Single();

                    typeof(MassTransitExts)
                        .GetMethod(nameof(ConfigureCompensatingActivity))
                        .MakeGenericMethod(activityType, argumentsType, logType)
                        .Invoke(null, invocationParameters);                    
                }

                if (typeof(IExecuteActivity).IsAssignableFrom(activityType))
                {
                    typeof(MassTransitExts)
                        .GetMethod(nameof(ConfigureExecuteActivity))
                        .MakeGenericMethod(activityType, argumentsType)
                        .Invoke(null, invocationParameters);
                }
            }

            return cfg;
        }
    }
}
