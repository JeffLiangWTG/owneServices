using System;
using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public class QueueItem
	{
		public QueueItem() { }

		public QueueItem(QueueItem item)
		{
			MessageFlowId = item.MessageFlowId;
			Sender = item.Sender;
			Recipient = item.Recipient;
			MessageId = item.MessageId;
			Metadata = new Dictionary<string, object>(item.Metadata);
		}

		public int MessageFlowId { get; set; }

		public string Sender { get; set; }

		public string Recipient { get; set; }

		public Guid MessageId { get; set; }

		public string FileName { get; set; }

		public string CorrelationId { get; set; }

		public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

		public QueueItemDelivery Delivery { get; set; }
	}
}
