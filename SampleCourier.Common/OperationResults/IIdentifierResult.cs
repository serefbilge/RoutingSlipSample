using System;

namespace SampleCourier.Common.OperationResults
{
    public interface IIdentifierResult : IOperationResult
    {
        Guid? Id { get; }
    }
}
