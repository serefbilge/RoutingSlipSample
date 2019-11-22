using System;
using Newtonsoft.Json;

namespace SampleCourier.Common.OperationResults
{
    public class IdentifierResult : OperationResult, IIdentifierResult
    {
        public Guid? Id { get; }

        public IdentifierResult(Guid id) : base()
        {
            Id = id;
        }

        public IdentifierResult(string errorCode, string errorMessage) : base(errorCode, errorMessage)
        {
        }

        [JsonConstructor]
        public IdentifierResult(Guid? id, bool successful, string errorCode, string errorMessage, DateTimeOffset created) : base(successful, errorCode, errorMessage, created)
        {
            Id = id;
        }
    }
}
