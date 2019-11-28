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
using SampleCourier.ConsentService.Activities.Consent;
using MassTransit.RabbitMqTransport;
using MassTransit.Courier;
using SampleCourier.Common;

namespace SampleCourier.ConsentService.Config
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

                    exchangeNames.AddRange(cfg.ConfigureActivity(typeof(ConsentActivity), host, serviceProvider, appConfig));

                    // Serilog
                    cfg.UseSerilog();
                });
            });
        } 
    }
}
