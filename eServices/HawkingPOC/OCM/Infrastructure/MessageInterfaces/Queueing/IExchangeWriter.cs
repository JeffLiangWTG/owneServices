using System;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IExchangeWriter : IDisposable, IQueueWriter
	{
		void Publish(QueueItem item, string routingKey);
	}
}
