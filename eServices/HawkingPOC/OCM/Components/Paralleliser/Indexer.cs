using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using ParDepComponent;

namespace OcmPoc.Components.Paralleliser
{
	class Indexer : IDisposable
	{
		readonly IQueueClient queueClient;
		readonly IMapper mapper;

		readonly ConcurrentDictionary<(string, long), QueueItemDelivery> pendingAck;

		IQueueListener<QueueItem> listener;
		IQueueWriter writer;

		string runMessage;

		IObservable<IndexedQueueItem> indexedSequence;

		public Indexer(IQueueClient queueClient, IMapper mapper)
		{
			this.queueClient = queueClient;
			this.mapper = mapper;

			pendingAck = new ConcurrentDictionary<(string,long), QueueItemDelivery>();
		}

		public int BufferSize => pendingAck.Count;

		public Indexer IndexInputQueue(string queueName)
		{
			listener = queueClient.GetQueueListener<QueueItem>(queueName);
			runMessage = $"Indexing {queueName}";
			return this;
		}

		public Indexer PartitionedBy(string partitionHeaders)
		{
			indexedSequence = listener
				.QueueItems
				.SubscribeOn(ThreadPoolScheduler.Instance)
				.ObserveOn(ThreadPoolScheduler.Instance)
				.GroupBy(item => item.GetPartitionKey(partitionHeaders.Split(':', StringSplitOptions.RemoveEmptyEntries)))
				.SelectMany(grp => grp.Select((item, idx) => mapper.Map(item, new IndexedQueueItem { Index = idx, PartitionKey = grp.Key })));
			runMessage += $" on {partitionHeaders}";
			return this;
		}

		public Indexer ToOutputQueue(string queueName)
		{
			writer = queueClient.GetQueueWriter(queueName);
			runMessage += $" to {queueName}";
			return this;
		}

		public async Task RunAsync(CancellationToken token)
		{
			Display.WriteLine(ConsoleColor.DarkGray, runMessage);

			var indexing = indexedSequence
				.Do(item =>
				{
					Display.WriteLine($"Indexed queue item {item.MessageId} as {item.PartitionKey}##{item.Index}");
					writer.Publish(item);
					pendingAck[(item.PartitionKey, item.Index)] = item.Delivery;
				})
				.Catch<IndexedQueueItem, Exception>(ex =>
				{
					Display.WriteLine(ex);
					return Observable.Empty<IndexedQueueItem>();
				})
				.ForEachAsync(item => Display.WriteLine($"Partition {item.PartitionKey} : Index {item.Index}  ({pendingAck.Count})"), token);

			listener.Listen();
			await indexing;

			Display.WriteLine("Finished indexing");
		}

		public async Task Acknowledge(IObservable<IndexedQueueItem> items, CancellationToken token)
		{
			Display.WriteLine("Acknowledger wired up");
			await items.ForEachAsync(item =>
			{
				if (pendingAck.Remove((item.PartitionKey, item.Index), out QueueItemDelivery delivery))
				{
					listener.Ack(delivery);
					Display.WriteLine($"Acknowledged {item.PartitionKey}##{item.Index}  ({pendingAck.Count})");
				}
			});
			Display.WriteLine("Acknowledger complete");
		}

		public async Task Reject(IObservable<IndexedQueueItem> items, CancellationToken token)
		{
			Display.WriteLine("Rejecter wired up");
			await items.ForEachAsync(item =>
			{
				if (pendingAck.Remove((item.PartitionKey, item.Index), out QueueItemDelivery delivery))
				{
					listener.Nack(delivery);
					Display.WriteLine($"Rejected {item.PartitionKey}##{item.Index}  ({pendingAck.Count})");
				}
			});
			Display.WriteLine("Rejecter complete");
		}

		#region IDisposable Support
		bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					listener?.Stop();

					listener?.Dispose();
					writer?.Dispose();

					Display.WriteLine(ConsoleColor.DarkGray, $"Stopped indexing");
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
