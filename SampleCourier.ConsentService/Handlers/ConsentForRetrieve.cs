using SampleCourier.Common;
using SampleCourier.Common.MassTransit;
using SampleCourier.Common.OperationResults;
using SampleCourier.Contracts.Commands.ConsentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.ConsentService.Handlers
{
    public class GetConsentForRetrieveHandler : ICommandHandler<IGetConsentForRetrieve, IOperationResult>
    {
        private readonly IBusPublisher _busPublisher;

        public GetConsentForRetrieveHandler(IBusPublisher busPublisher)
        {
            _busPublisher = busPublisher ?? throw new ArgumentNullException(nameof(busPublisher));
        }

        public async Task<IOperationResult> HandleAsync(IGetConsentForRetrieve command, ICorrelationContext context)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return OperationResult.Ok();

            //return new OperationResult("Consent is not affirmative!");
        }
    }
}
