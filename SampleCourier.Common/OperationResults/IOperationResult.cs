using System;

namespace SampleCourier.Common.OperationResults
{
    public interface IOperationResult
    {
        bool Successful { get; }
        string ErrorCode { get; }
        string ErrorMessage { get; }
        DateTimeOffset Created { get; }
    }
}
