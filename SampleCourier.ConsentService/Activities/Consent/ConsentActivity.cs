using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Logging;
using SampleCourier.Contracts;

namespace SampleCourier.ConsentService.Activities.Consent
{
	public class ConsentActivity : Activity<ConsentArguments,ConsentLog>
	{
		static readonly ILog _logger = Logger.Get<ConsentActivity>();

        private MassTransit.Context.MessageConsumeContext<MassTransit.Courier.Contracts.RoutingSlip> GetRoutingSlip(ExecuteContext execContext)
        {
            //var execContext = (ExecuteContext)context;
            //var routingSlip = (MassTransit.Context.MessageConsumeContext<MassTransit.Courier.Contracts.RoutingSlip>)execContext.ConsumeContext;

            return (MassTransit.Context.MessageConsumeContext<MassTransit.Courier.Contracts.RoutingSlip>)execContext.ConsumeContext;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<ConsentArguments> context)
		{
            var sourceAddress = context.Arguments.Address;

            _logger.InfoFormat("Consent Content: {0}", sourceAddress);

            try
            {
                var routingSlip = GetRoutingSlip(context);
                var compensateLogs = routingSlip.Message.CompensateLogs;

                var a = compensateLogs[0].Data["localFilePath"];

                throw new Exception("There is a fault in Consent Activity!");

                return context.Completed<ConsentLog>(new Log("ConsentParam"));
            }            
            catch (Exception exception)
            {
                var preMessage = exception.GetType() == typeof(HttpRequestException) ? "Exception from HttpClient: " : "Exception: ";

                _logger.Error(preMessage, exception.InnerException ?? exception);

                return context.Faulted(exception);
            }
        }

		public Task<CompensationResult> Compensate(CompensateContext<ConsentLog> context)
		{
            if (_logger.IsErrorEnabled) _logger.ErrorFormat("Can not be hit here, there is no further activity!: {0}", context.Log.LocalFilePath);

            return Task.FromResult(context.Compensated());
		}

		class Log : ConsentLog
		{
			public Log(string localFilePath) => LocalFilePath = localFilePath;

			public string LocalFilePath { get; private set; }
		}
	}
}