using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	class ObservableConsumer<TQueueItem> : DefaultBasicConsumer
		where TQueueItem : QueueItem, new()
	{
		IObserver<TQueueItem> observer;

		public ObservableConsumer(IModel model) 
			: base(model)
		{
			var deliveries = Observable.Create<TQueueItem>(obs =>
			{
				observer = obs;
				return Disposable.Empty;
			}).Publish();
			deliveries.Connect();
			Deliveries = deliveries;
		}

		public IObservable<TQueueItem> Deliveries { get; }

		public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
		{
			//Console.WriteLine($"Received queue item from {exchange}:{routingKey} (thread {Thread.CurrentThread.ManagedThreadId})");
			try
			{
				var item = properties.MapTo<TQueueItem>();
				item.Delivery = new QueueItemDelivery(deliveryTag, exchange, routingKey, redelivered);

				//Console.WriteLine($"Received queue item {item.MessageId} from {exchange}:{routingKey}");
				try
				{
					observer.OnNext(item);
				}
				catch (Exception ex)
				{
					Model.BasicNack(deliveryTag, false, true);
					Console.WriteLine(ex);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Queue item mapping failed: {ex}");
			}
		}

		public override void HandleBasicCancel(string consumerTag)
		{
			observer.OnCompleted();
			base.HandleBasicCancel(consumerTag);
		}

		public override void HandleBasicCancelOk(string consumerTag)
		{
			observer.OnCompleted();
			base.HandleBasicCancelOk(consumerTag);
		}

		public override void HandleModelShutdown(object model, ShutdownEventArgs reason)
		{
			observer.OnCompleted();
			base.HandleModelShutdown(model, reason);
		}
	}
}
