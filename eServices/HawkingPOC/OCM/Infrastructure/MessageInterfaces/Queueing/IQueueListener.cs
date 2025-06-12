using System;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IQueueListener<TQueueItem> : IDisposable
		where TQueueItem : QueueItem
	{
		IObservable<TQueueItem> QueueItems { get; }

		void Listen();
		void Stop();
		void Ack(QueueItemDelivery delivery);
		void Nack(QueueItemDelivery delivery, bool requeue = false);
	}
}
