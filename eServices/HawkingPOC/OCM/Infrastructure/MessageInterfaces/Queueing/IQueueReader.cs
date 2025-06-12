using System;
using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IQueueReader<TQueueItem> : IDisposable
		where TQueueItem : QueueItem
	{
		IEnumerable<TQueueItem> GetMessages(int maxFetchSize);
		void Ack(QueueItemDelivery delivery);
		void Nack(QueueItemDelivery delivery, bool requeue = false);
	}
}
