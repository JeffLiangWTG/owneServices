using System;

namespace Hawking.eHub.Model.DataAccess.Integration
{
    public class OutboxMessageDetails
    {
        public Guid PK { get; private set; }
        public Guid SenderPK { get; private set; }
        public long MessageLength { get; private set; }
        public DateTime OutboxDateTime { get; private set; }
        public long SequenceNumber { get; private set; }
        public long LastDeliveredSN { get; private set; }

        public OutboxMessageDetails()
        {
            PK = Guid.Empty;
            SenderPK = Guid.Empty;
            MessageLength = -1;
            OutboxDateTime = DateTime.MinValue;
            SequenceNumber = -1;
            LastDeliveredSN = -1;
        }

        public OutboxMessageDetails(Guid pk, Guid senderPK, long messageLength, DateTime outboxDateTime, long sequenceNumber, long lastDeliveredSN)
        {
            PK = pk;
            SenderPK = senderPK;
            MessageLength = messageLength;
            OutboxDateTime = outboxDateTime;
            SequenceNumber = sequenceNumber;
            LastDeliveredSN = lastDeliveredSN;
        }
    }
}
