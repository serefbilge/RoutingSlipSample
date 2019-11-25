using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCourier.Common
{
    public class CorrelationContext : ICorrelationContext
    {
        public CorrelationContext(Guid messageId, Guid conversationId, Guid correlationId)
        {
            MessageId = messageId;
            ConversationId = conversationId;
            CorrelationId = correlationId;
        }

        public Guid MessageId { get; }
        public Guid ConversationId { get; }
        public Guid CorrelationId { get; }
    }
}
