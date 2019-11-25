using System;
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
using SampleCourier.Common.AspNetCore;
using MassTransit.RabbitMqTransport;

namespace SampleCourier.ProcessingService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			//services.AddMassTransitWithRabbitMq(Configuration);
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //services.AddCommonServices(Configuration);
            services.AddCommonServices(Configuration, (host, configurator, sp) => configurator.ConfigureActivities(host, sp, Configuration));
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
