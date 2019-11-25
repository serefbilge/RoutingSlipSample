using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using Microsoft.Extensions.Options;
using SampleCourier.Common.MassTransit;
using SampleCourier.Contracts;
using SampleCourier.Contracts.Events;
using SampleCourier.WebApi.Config;

namespace SampleCourier.WebApi.Controllers
{
	public class RoutingSlipPublisher
	{
		public RoutingSlipPublisher(IBusControl bus, IBusControl busControl, IOptions<MqActivityOptions> actCfg)
		{
			_bus = bus;
            _busControl = busControl;
            _activityConfig = actCfg.Value;
		}

		private readonly IBus _bus;
        private readonly IBusControl _busControl;
        private readonly MqActivityOptions _activityConfig;

		public async Task<Guid> Publish(Uri reqUri)
		{
			var builder = new RoutingSlipBuilder(NewId.NextGuid());

            //builder.AddActivity(typeof(ValidateActivity).FullName, _busControl.Address.AppendActivityExchangeName(typeof(TActivity)));
            //new Uri(busUri, GetActivityExchangeName(activityType))

            var baseUri = _busControl.Address;
            var fullNamePrefix = "SampleCourier.ProcessingService.Activities";            
            var exchageNamePrefix = "sample_courier.processing_service.activities";

            builder.AddActivity($"{fullNamePrefix}.Validate.ValidateActivity", new Uri(baseUri, $"{exchageNamePrefix}.validate.validateactivity"));

            builder.AddActivity($"{fullNamePrefix}.PreProcess.PostProcessActivity", new Uri(baseUri, $"{exchageNamePrefix}.preprocess.preprocessactivity"));
            //builder.AddActivity($"{fullNamePrefix}.PreProcess.PostProcessActivity", new Uri(baseUri, $"{exchageNamePrefix}.preprocess.preprocessactivity.compensations"));

            builder.AddActivity($"{fullNamePrefix}.Retrieve.RetrieveActivity", new Uri(baseUri, $"{exchageNamePrefix}.retrieve.retrieveactivity"));
            //builder.AddActivity($"{fullNamePrefix}.Retrieve.RetrieveActivity", new Uri(baseUri, $"{exchageNamePrefix}.retrieve.retrieveactivity.compensations"));

            builder.AddActivity($"{fullNamePrefix}.PostProcess.PostProcessActivity", new Uri(baseUri, $"{exchageNamePrefix}.postprocess.postprocessactivity"));
            //builder.AddActivity($"{fullNamePrefix}.PostProcess.PostProcessActivity", new Uri(baseUri, $"{exchageNamePrefix}.postprocess.postprocessactivity.compensations"));

            //builder.AddActivity("Validate",new Uri(_activityConfig.ValidateAddress));
            //builder.AddActivity("PreProcess", new Uri(_activityConfig.PreProcessAddress));
            //builder.AddActivity("Retrieve",new Uri(_activityConfig.RetrieveAddress));
            //builder.AddActivity("PostProcess", new Uri(_activityConfig.PostProcessAddress));

            builder.SetVariables(new
			{
				RequestId = NewId.NextGuid(),
				Address = reqUri,
			});

			var routingSlip = builder.Build();

			await _bus.Publish<RoutingSlipCreated>(new
			{
				routingSlip.TrackingNumber,
				Timestamp = routingSlip.CreateTimestamp,
			});

			await _bus.Execute(routingSlip);

			return routingSlip.TrackingNumber;
		}
	}
}
