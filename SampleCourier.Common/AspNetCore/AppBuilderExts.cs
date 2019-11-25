using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleCourier.Common.MassTransit;

// Copyright (c) DMO Consulting LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SampleCourier.Common.AspNetCore
{
	public static class AppBuilderExts
	{
        public static IServiceCollection AddCommonServices(this IServiceCollection serviceCollection, IConfiguration configuration, Action<IRabbitMqHost, IRabbitMqBusFactoryConfigurator, IServiceProvider> cfgAction = null)
        {
            serviceCollection.AddCommandHandlers();
            serviceCollection.AddEventHandlers();
            serviceCollection.AddMassTransitConf(cfgAction);
            return serviceCollection;
        }

        public static void UseMassTransit(this IApplicationBuilder app)
		{
			// start/stop the bus with the web application
			var appLifetime = (app ?? throw new ArgumentNullException(nameof(app)))
				.ApplicationServices.GetService<IApplicationLifetime>();

			var bus = app.ApplicationServices.GetService<IBusControl>();
			appLifetime.ApplicationStarted.Register(() => bus.Start());
			appLifetime.ApplicationStopped.Register(bus.Stop);
		}
	}
}
