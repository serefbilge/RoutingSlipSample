using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Logging;
using SampleCourier.Common.MassTransit;
using SampleCourier.Common.OperationResults;
using SampleCourier.Contracts;
using SampleCourier.Contracts.Commands.ConsentService;
using SampleCourier.ProcessingService.Commands;

namespace SampleCourier.ProcessingService.Activities.PostProcess
{
    public class PostProcessActivity : Activity<PostProcessArguments, PostProcessLog>
    {
        static readonly ILog _logger = Logger.Get<PostProcessActivity>();
        private readonly IBusPublisher _busPublisher;

        public PostProcessActivity(IBusPublisher busPublisher)
        {
            _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<PostProcessArguments> context)
        {
            var sourceAddress = context.Arguments.Address;

            _logger.InfoFormat("PostProcess Content: {0}", sourceAddress);

            try
            {
                //throw new Exception("There is a fault in PostProcess Activity!");

                //var command = new GetConsentForRetrieve(context.Arguments.Address);
                //var result = await _busPublisher.SendRequest<IGetConsentForRetrieve, IOperationResult>(command);

                //if (!result.Successful)
                //    return context.Faulted(new Exception(result.ErrorMessage));

                return context.Completed<PostProcessLog>(new Log("PostProcessParam"));
            }
            catch (Exception exception)
            {
                var preMessage = exception.GetType() == typeof(HttpRequestException) ? "Exception from HttpClient: " : "Exception: ";

                _logger.Error(preMessage, exception.InnerException ?? exception);

                return context.Faulted(exception);
            }
        }

        public Task<CompensationResult> Compensate(CompensateContext<PostProcessLog> context)
        {
            if (_logger.IsErrorEnabled) _logger.ErrorFormat("Can not be hit here, there is no further activity!: {0}", context.Log.LocalFilePath);

            return Task.FromResult(context.Compensated());
        }

        class Log : PostProcessLog
        {
            public Log(string localFilePath) => LocalFilePath = localFilePath;

            public string LocalFilePath { get; private set; }
        }
    }
}