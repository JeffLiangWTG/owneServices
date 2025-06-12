using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Integration
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
			this.PK = Guid.Empty;
			this.SenderPK = Guid.Empty;
			this.MessageLength = -1;
			this.OutboxDateTime = DateTime.MinValue;
			this.SequenceNumber = -1;
			this.LastDeliveredSN = -1;
		}

		public OutboxMessageDetails(Guid pk, Guid senderPK, long messageLength, DateTime outboxDateTime, long sequenceNumber, long lastDeliveredSN)
		{
			this.PK = pk;
			this.SenderPK = senderPK;
			this.MessageLength = messageLength;
			this.OutboxDateTime = outboxDateTime;
			this.SequenceNumber = sequenceNumber;
			this.LastDeliveredSN = lastDeliveredSN;
		}
	}
}
