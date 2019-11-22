using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Logging;
using SampleCourier.Contracts;

namespace SampleCourier.ProcessingService.Activities.PreProcess
{
	public class PreProcessActivity : Activity<PreProcessArguments,PreProcessLog>
	{
		static readonly ILog _logger = Logger.Get<PreProcessActivity>();

		public async Task<ExecutionResult> Execute(ExecuteContext<PreProcessArguments> context)
		{
			var sourceAddress = context.Arguments.Address;

			_logger.InfoFormat("PreProcess Content: {0}", sourceAddress);

			try
			{
                return context.Completed<PreProcessLog>(new Log("PreProcessParam"));
            }
            catch (Exception exception)
            {
                var preMessage = exception.GetType() == typeof(HttpRequestException) ? "Exception from HttpClient: " : "Exception: ";

                _logger.Error(preMessage, exception.InnerException ?? exception);

                return context.Faulted(exception);
            }
        }

		public Task<CompensationResult> Compensate(CompensateContext<PreProcessLog> context)
		{
			if (_logger.IsErrorEnabled) _logger.ErrorFormat("Hit here from fault in RetrieveActivity: {0}",context.Log.LocalFilePath);

			return Task.FromResult(context.Compensated());
		}

		class Log : PreProcessLog
		{
			public Log(string localFilePath) => LocalFilePath = localFilePath;

			public string LocalFilePath { get; private set; }
		}
	}
}