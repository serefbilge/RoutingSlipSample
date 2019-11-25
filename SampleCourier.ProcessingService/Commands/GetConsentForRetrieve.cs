using SampleCourier.Contracts.Commands.ConsentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.ProcessingService.Commands
{
    public class GetConsentForRetrieve: IGetConsentForRetrieve
    {
        public GetConsentForRetrieve(Uri adress)
        {
            Address = adress;
        }
        
        public Uri Address { get; }
    }
}
