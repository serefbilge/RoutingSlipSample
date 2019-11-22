using System;
using Newtonsoft.Json;

namespace SampleCourier.Common.OperationResults
{
    public class OperationResult : IOperationResult
    {
        public bool Successful { get; }
        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow;

        public OperationResult()
        {
            Successful = true;
        }

        public static OperationResult Ok()
        {
            return new OperationResult();
        }

        public OperationResult(string errorCode, string errorMessage)
        {
            Successful = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        [JsonConstructor]
        public OperationResult(bool successful, string errorCode, string errorMessage, DateTimeOffset created)
        {
            Successful = successful;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Created = created;
        }
    }
}
