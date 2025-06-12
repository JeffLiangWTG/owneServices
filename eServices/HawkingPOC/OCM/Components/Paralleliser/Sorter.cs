using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Utils.Config;

namespace OcmPoc.Components.Paralleliser
{
	class Sorter : IDisposable
	{
		readonly IQueueClient queueClient;
		readonly IMapper mapper;

		IQueueListener<IndexedQueueItem> inputListener;
		IQueueListener<IndexedQueueItem> deadLetterListener;
		IQueueWriter writer;

		IObservable<(IndexedQueueItem Item, bool DeadLetter)> unorderedItems;
		string runMessage;

		public Sorter(IQueueClient queueClient, IMapper mapper)
		{
			this.queueClient = queueClient;
			this.mapper = mapper;
		}

		public IObservable<IndexedQueueItem> SuccessfulItems { get; private set; }
		public IObservable<IndexedQueueItem> FailedItems { get; private set; }

		public Sorter SortQueues(string inputQueue, string deadLetterQueue)
		{
			inputListener = queueClient.GetQueueListener<IndexedQueueItem>(inputQueue);
			deadLetterListener = queueClient.GetQueueListener<IndexedQueueItem>(deadLetterQueue);

			unorderedItems = Observable.Merge(
				inputListener.QueueItems.Select(item => (Item: item, DeadLetter: false)),
				deadLetterListener.QueueItems.Select(item => (Item: item, DeadLetter: true)));

			runMessage = $"Sorting {inputQueue} and {deadLetterQueue}";
			return this;
		}

		public Sorter ToOutput(BindingConfig binding)
		{
			var exchangeName = binding.Exchange;
			var queueName = binding.Queue;

			writer = exchangeName == null ? queueClient.GetQueueWriter(queueName) : queueClient.GetExchangeWriter(exchangeName);

			runMessage += $" to {exchangeName}:{queueName}";
			return this;
		}

		public async Task RunAsync(CancellationToken token)
		{
			var orderedItems = unorderedItems
				.SubscribeOn(ThreadPoolScheduler.Instance)
				.ObserveOn(ThreadPoolScheduler.Instance)
				.GroupBy(x => x.Item.PartitionKey)
				.SelectMany(grp => grp.SortContiguousBy(x => x.Item.Index));

			SuccessfulItems = orderedItems.Where(x => !x.DeadLetter).Select(x => x.Item);
			FailedItems = orderedItems.Where(x => x.DeadLetter).Select(x => x.Item);

			Display.WriteLine(ConsoleColor.DarkGray, runMessage);

			var sorting = Task.WhenAll(
				SuccessfulItems.ForEachAsync(Publish, token),
				FailedItems.ForEachAsync(Discard, token)
			);

			inputListener.Listen();
			deadLetterListener.Listen();

			await sorting;

			Display.WriteLine("Done sorting");
		}

		void Discard(IndexedQueueItem item)
		{
			deadLetterListener.Ack(item.Delivery);
		}

		void Publish(IndexedQueueItem item)
		{
			writer.Publish(mapper.Map<QueueItem>(item));
			inputListener.Ack(item.Delivery);
			Display.WriteLine(ConsoleColor.Green, $"                                                                                      Delivered msg {item.PartitionKey}:{item.Index}");
		}

		#region IDisposable Support
		bool disposedValue = false; 

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					inputListener?.Dispose();
					deadLetterListener?.Dispose();
					writer?.Dispose();
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
