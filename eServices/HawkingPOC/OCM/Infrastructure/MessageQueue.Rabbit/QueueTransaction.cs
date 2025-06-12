using System.Threading;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueTransaction : IQueueTransaction
	{
		static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		IModel channel;
		bool committed;
		bool disposed;

		
		public QueueTransaction(IModel channel)
		{
			this.channel = channel;
			semaphore.Wait();
			channel.TxSelect();
		}

		public void Commit()
		{
			channel.TxCommit();
			committed = true;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				if (!committed)
				{
					channel.TxRollback();
				}
				semaphore.Release();
				disposed = true;
			}
		}
	}
}
