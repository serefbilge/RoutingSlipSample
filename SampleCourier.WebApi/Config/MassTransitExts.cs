﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleCourier.Common.MassTransit;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SampleCourier.Models;
using SampleCourier.WebApi.Controllers;

namespace SampleCourier.WebApi.Config
{
	public static class MassTransitExts
	{
		public static void AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration appConfig)
		{
			if (services == null)
				throw new ArgumentNullException("services");

			if (appConfig == null)
				throw new ArgumentNullException("appConfig");
            			
			var actSection = appConfig.GetSection("Activities");

			if (!actSection.Exists())
				throw new InvalidOperationException("Appsettings: 'Activities' section was not found");

			services.Configure<MqActivityOptions>(actSection);

			services.AddSingleton(svcProv =>
			{
				var config = svcProv.GetService<IConfiguration>();
				var conStr = config.GetConnectionString("EfCoreRoutingSlip");
				return new RoutingSlipDbContextFactory(conStr);
			});

			services.AddScoped<RoutingSlipPublisher>();

			services.AddMassTransit();

			services.AddSingleton(svcProv =>
			{
				return Bus.Factory.CreateUsingRabbitMq(cfg =>
				{
					var host = cfg.CreateHost(appConfig);

					cfg.UseSerilog();
				});
			});
		}
	}
}
