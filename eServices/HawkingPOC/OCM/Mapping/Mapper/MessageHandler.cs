using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Infrastructure.MessageInterfaces.TypeMapping;

namespace OcmPoc.Mapping.Mapper
{
	class MessageHandler
	{
		readonly IQueueListener<QueueItem> listener;
		readonly IQueueWriter writer;
		readonly IMessageMapper messageMapper;
		readonly IEventLogger eventLogger;

		public MessageHandler(IQueueListener<QueueItem> listener, IQueueWriter writer, IMessageMapper messageMapper, IEventLogger eventLogger)
		{
			this.listener = listener;
			this.writer = writer;
			this.messageMapper = messageMapper;
			this.eventLogger = eventLogger;
		}

		public async Task RunAsync(CancellationToken token)
		{
			var task = listener.QueueItems.ForEachAsync(async item => await HandleMessageAsync(item), token);
			listener.Listen();
			await task;
		}

		async Task HandleMessageAsync(QueueItem item)
		{
			var messageFlowId = item.MessageFlowId;

			await eventLogger.LogEventAsync(item.ToMessageEvent(MessageEventType.MappingFrom));

			QueueItem mappedItem = null;

			try
			{
				mappedItem = await messageMapper.MapMessageAsync(item);
			}
			catch (Exception ex)
			{
				listener.Nack(item.Delivery);
				await eventLogger.LogEventAsync(item.ToMessageEvent(ex));
				Console.WriteLine($"Mapping failed: {ex}");
				return;
			}

			using (var tx = writer.NewTransaction())
			{
				await eventLogger.LogEventAsync(mappedItem.ToMessageEvent(MessageEventType.MappedTo));
				writer.Publish(mappedItem);
				listener.Ack(item.Delivery);
				tx.Commit();
			}

			Console.WriteLine($"Mapped msg {item.MessageId} to {mappedItem.MessageId} for flow {item.MessageFlowId}");
		}
	}
}
