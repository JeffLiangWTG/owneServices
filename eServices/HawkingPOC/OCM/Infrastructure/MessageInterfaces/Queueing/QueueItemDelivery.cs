using System;
using System.Collections.Generic;
using System.Text;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
    public class QueueItemDelivery
    {
		public QueueItemDelivery(ulong deliveryTag, string exchange, string routingKey, bool redelivered)
		{
			DeliveryTag = deliveryTag;
			Exchange = exchange;
			Redelivered = redelivered;
			RoutingKey = routingKey;
		}

		public ulong DeliveryTag { get; }
		public string Exchange { get; }
		public bool Redelivered { get; }
		public string RoutingKey { get; }
	}
}
