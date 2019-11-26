using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleCourier.Common.MassTransit;
using GreenPipes;
using MassTransit;
using MassTransit.Courier.Factories;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SampleCourier.ProcessingService.Activities.PostProcess;
using SampleCourier.ProcessingService.Activities.PreProcess;
using SampleCourier.ProcessingService.Activities.Retrieve;
using SampleCourier.ProcessingService.Activities.Validate;
using MassTransit.RabbitMqTransport;
using MassTransit.Courier;
using SampleCourier.Common;

namespace SampleCourier.ProcessingService.Config
{
    public static class MassTransitExts
    {
        public static void AddMassTransitWithRabbitMq(this IServiceCollection services, IServiceProvider serviceProvider, IConfiguration appConfig)
        {
            if (services == null)
                throw new ArgumentNullException("services");

            if (appConfig == null)
                throw new ArgumentNullException("appConfig");

            services.AddMassTransit();

            services.AddSingleton(svcProv =>
            {
                return Bus.Factory.CreateUsingRabbitMq(cfg =>
                {                   
                    var host = cfg.CreateHost(appConfig);
                    var exchangeNames = new List<string>();

                    exchangeNames.AddRange(cfg.ConfigureActivity(typeof(ValidateActivity), host, serviceProvider, appConfig));
                    exchangeNames.AddRange(cfg.ConfigureActivity(typeof(PreProcessActivity), host, serviceProvider, appConfig));
                    exchangeNames.AddRange(cfg.ConfigureActivity(typeof(RetrieveActivity), host, serviceProvider, appConfig));
                    exchangeNames.AddRange(cfg.ConfigureActivity(typeof(PostProcessActivity), host, serviceProvider, appConfig));

                    // Serilog
                    cfg.UseSerilog();
                });
            });
        }

        //private static void ConfigureEndpoints(IRabbitMqHost host, IRabbitMqBusFactoryConfigurator cfg, ActivityConfig validateOpts, MqActivityOptions actOpts)
        //{
        //    // Validate Activity                    
        //    cfg.ReceiveEndpoint(host, validateOpts.QueueName, e =>
        //    {
        //        e.PrefetchCount = (ushort)validateOpts.PrefetchCount;
        //        e.ExecuteActivityHost(
        //            DefaultConstructorExecuteActivityFactory<ValidateActivity, ValidateArguments>.ExecuteFactory,
        //            c => c.UseRetry(r => r.Immediate(validateOpts.RetryLimit))
        //        );
        //    });


        //    // PreProcess Activity and Compensation
        //    var preProcessOpts = actOpts.PreProcessActivity;
        //    var compPreProcessOpts = actOpts.CompensatePreProcessActivity;

        //    cfg.ReceiveEndpoint(host, preProcessOpts.QueueName, e =>
        //    {
        //        var compAddress = new Uri(host.Address, compPreProcessOpts.QueueName);

        //        e.PrefetchCount = (ushort)preProcessOpts.PrefetchCount;
        //        e.ExecuteActivityHost<PreProcessActivity, PreProcessArguments>(compAddress,
        //            c => c.UseRetry(r => r.Immediate(preProcessOpts.RetryLimit)));
        //    });

        //    cfg.ReceiveEndpoint(host, compPreProcessOpts.QueueName,
        //        e => e.CompensateActivityHost<PreProcessActivity, PreProcessLog>(c =>
        //             c.UseRetry(r => r.Immediate(compPreProcessOpts.RetryLimit))));


        //    // Retrieve Activity and Compensation
        //    var retrieveOpts = actOpts.RetrieveActivity;
        //    var compRetrieveOpts = actOpts.CompensateRetrieveActivity;

        //    cfg.ReceiveEndpoint(host, retrieveOpts.QueueName, e =>
        //      {
        //          var compAddress = new Uri(host.Address, compRetrieveOpts.QueueName);

        //          e.PrefetchCount = (ushort)retrieveOpts.PrefetchCount;
        //          e.ExecuteActivityHost<RetrieveActivity, RetrieveArguments>(compAddress,
        //              c => c.UseRetry(r => r.Immediate(retrieveOpts.RetryLimit)));
        //      });

        //    cfg.ReceiveEndpoint(host, compRetrieveOpts.QueueName,
        //        e => e.CompensateActivityHost<RetrieveActivity, RetrieveLog>(c =>
        //             c.UseRetry(r => r.Immediate(compRetrieveOpts.RetryLimit))));


        //    // PostProcess Activity and Compensation
        //    var postProcessOpts = actOpts.PostProcessActivity;
        //    var compPostProcessOpts = actOpts.CompensatePostProcessActivity;

        //    cfg.ReceiveEndpoint(host, postProcessOpts.QueueName, e =>
        //    {
        //        var compAddress = new Uri(host.Address, compPostProcessOpts.QueueName);

        //        e.PrefetchCount = (ushort)postProcessOpts.PrefetchCount;
        //        e.ExecuteActivityHost<PostProcessActivity, PostProcessArguments>(compAddress,
        //            c => c.UseRetry(r => r.Immediate(postProcessOpts.RetryLimit)));
        //    });

        //    cfg.ReceiveEndpoint(host, compPostProcessOpts.QueueName,
        //        e => e.CompensateActivityHost<PostProcessActivity, PostProcessLog>(c =>
        //             c.UseRetry(r => r.Immediate(compPostProcessOpts.RetryLimit))));
        //}
    }
}
