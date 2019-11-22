using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCourier.Contracts.Commands.ConsentService
{
    public interface IGetConsentOfRetrieve : ICommand
    {
        string ResponseNote { get; }
        bool Affirmative { get; }
    }
}
