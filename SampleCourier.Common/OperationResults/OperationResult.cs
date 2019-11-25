using System;
using Newtonsoft.Json;

namespace SampleCourier.Common.OperationResults
{
    public class OperationResult : IOperationResult
    {
        public bool Successful { get; }
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

        public OperationResult(string errorMessage)
        {
            Successful = false;
            ErrorMessage = errorMessage;
        }

        [JsonConstructor]
        public OperationResult(bool successful, string errorMessage, DateTimeOffset created)
        {
            Successful = successful;
            ErrorMessage = errorMessage;
            Created = created;
        }
    }
}
