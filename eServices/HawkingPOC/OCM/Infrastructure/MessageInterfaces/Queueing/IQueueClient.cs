using System;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IQueueClient : IDisposable
	{
		IDisposable CreateTemporaryQueues(IQueueSpecCollection specs);
		void CreateDurableQueues(IQueueSpecCollection specs);
		void CreateExchange(IExchangeSpec spec);
		void CreateExchanges(IExchangeSpecCollection specs);
		void BindQueues(IQueueSpecCollection specs);
		IQueueListener<TQueueItem> GetQueueListener<TQueueItem>(string queueName = null)
			where TQueueItem : QueueItem, new();
		IQueueReader<TQueueItem> GetQueueReader<TQueueItem>(string queueName = null)
			where TQueueItem : QueueItem, new();
		IQueueWriter GetQueueWriter(string queueName = null);
		IExchangeWriter GetExchangeWriter(string exchangeName);
	}
}
