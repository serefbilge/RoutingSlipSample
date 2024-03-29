﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleCourier.Common.MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleCourier.ProcessingService.Config;

namespace SampleCourier.ProcessingService
{
	public class Startup
	{
		public Startup(IServiceProvider serviceProvider, IConfiguration configuration)
		{
            ServiceProvider = serviceProvider;
            Configuration = configuration;
		}

        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMassTransitWithRabbitMq(ServiceProvider, Configuration);
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);			
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app,IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMassTransit();
			app.UseMvc();			
		}
	}
}
