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

        public IdentifierResult(string errorMessage) : base(errorMessage)
        {
        }

        [JsonConstructor]
        public IdentifierResult(Guid? id, bool successful, string errorMessage, DateTimeOffset created) : base(successful, errorMessage, created)
        {
            Id = id;
        }
    }
}
