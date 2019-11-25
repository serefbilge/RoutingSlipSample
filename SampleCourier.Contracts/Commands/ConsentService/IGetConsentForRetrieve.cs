using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCourier.Contracts.Commands.ConsentService
{
    public interface IGetConsentForRetrieve : ICommand
    {
        Uri Address { get; }
    }
}
