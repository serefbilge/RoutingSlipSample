using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreenPipes;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Factories;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;

namespace SampleCourier.Common.MassTransit
{
	public static class MassTransitExts
	{
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

        public static IRabbitMqBusFactoryConfigurator ConfigureActivityWithCompensation<TCompensateActivity, TCompensateActivityParameters, TCompensateActivityLog>(this IRabbitMqBusFactoryConfigurator cfg, IRabbitMqHost host, IServiceProvider serviceProvider, string busUriStr)
        where TCompensateActivity : class, Activity<TCompensateActivityParameters, TCompensateActivityLog>, new()
        where TCompensateActivityParameters : class
        where TCompensateActivityLog : class
        {
            var exchangeName = typeof(TCompensateActivity).GetActivityExchangeName();
            var compExchangeName = typeof(TCompensateActivity).GetCompensatingActivityExchangeName();

            cfg.ReceiveEndpoint(host, exchangeName, e =>
            {
                var compAddress = new Uri(host.Address, compExchangeName);

                e.PrefetchCount = 100;
                e.ExecuteActivityHost<TCompensateActivity, TCompensateActivityParameters>(compAddress, c => c.UseRetry(r => r.Immediate(5)));
            });

            cfg.ReceiveEndpoint(host, compExchangeName, e => e.CompensateActivityHost<TCompensateActivity, TCompensateActivityLog>(c => c.UseRetry(r => r.Immediate(5))));

            return cfg;
        }

        public static IRabbitMqBusFactoryConfigurator ConfigureActivityHasNoCompensation<TExecuteActivity, TExecuteActivityParameters>(this IRabbitMqBusFactoryConfigurator cfg, IRabbitMqHost host, IServiceProvider serviceProvider, string busUriStr)
        where TExecuteActivity : class, ExecuteActivity<TExecuteActivityParameters>, new()
        where TExecuteActivityParameters : class
        {
            var exchangeName = typeof(TExecuteActivity).GetActivityExchangeName();
            cfg.ReceiveEndpoint(host, exchangeName, e =>
            {
                e.PrefetchCount = 100;
                e.ExecuteActivityHost(
                    DefaultConstructorExecuteActivityFactory<TExecuteActivity, TExecuteActivityParameters>.ExecuteFactory, c => c.UseRetry(r => r.Immediate(5))
                );
            });
                                   
            return cfg;
        }

        public static List<string> ConfigureActivity(this IRabbitMqBusFactoryConfigurator cfg, Type activityType, IRabbitMqHost host, IServiceProvider serviceProvider, IConfiguration svcCfg)
        {
            var extClassType = typeof(Common.MassTransit.MassTransitExts);
            var interfaces = activityType.GetInterfaces();
            var argumentsType = interfaces.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ExecuteActivity<>)).GenericTypeArguments.Single();

            var busUriStr = new Uri(new Uri("rabbitmq://" + svcCfg.GetConnectionString("RabbitMQHost") + "/"), svcCfg.GetConnectionString("RabbitMQVhost")).ToString().TrimEnd('/') + '/';
            var invocationParameters = new object[] { cfg, host, serviceProvider, busUriStr };
            var exchangeNames = new List<string>();

            if (!typeof(IExecuteActivity).IsAssignableFrom(activityType))
                return exchangeNames;

            exchangeNames.Add(activityType.GetActivityExchangeName());

            var hasNoCompensation = !typeof(ICompensateActivity).IsAssignableFrom(activityType);
            if (hasNoCompensation)
            {
                extClassType
                    .GetMethod(nameof(ConfigureActivityHasNoCompensation))
                    .MakeGenericMethod(activityType, argumentsType)
                    .Invoke(null, invocationParameters);
            }
            else
            {
                var logType = interfaces.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(CompensateActivity<>)).GenericTypeArguments.Single();
                exchangeNames.Add(activityType.GetCompensatingActivityExchangeName());

                extClassType
                    .GetMethod(nameof(ConfigureActivityWithCompensation))
                    .MakeGenericMethod(activityType, argumentsType, logType)
                    .Invoke(null, invocationParameters);
            }

            return exchangeNames;
        }
        
        public static IRabbitMqHost CreateHost(this IRabbitMqBusFactoryConfigurator cfg, IConfiguration appConfig)
        {
            var hostAddress = appConfig.GetConnectionString("RabbitMQHost");
            var vhost = string.IsNullOrWhiteSpace(appConfig.GetConnectionString("RabbitMQVhost")) ? "/" : appConfig.GetConnectionString("RabbitMQVhost").Trim('/');
            var username = appConfig.GetConnectionString("RabbitMQUser");
            var password = appConfig.GetConnectionString("RabbitMQPassword");

            var host = cfg.Host(hostAddress, vhost, hostCfg =>
            {
                hostCfg.Username(username);
                hostCfg.Password(password);
            });

            return host;
        }
    }
}
