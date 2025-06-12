using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Enums;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Infrastructure.MessageInterfaces.TypeMapping;

namespace OcmPoc.Transceivers
{
	class Sender : IRunnable
	{
		static int messageCount = 0;

		readonly ISendingConnection connection;
		readonly IQueueListener<QueueItem> queueListener;
		readonly IMessageRepository messageRepository;
		readonly IEventLogger eventLogger;

		public Sender(ISendingConnection connection, IQueueListener<QueueItem> queueListener, IMessageRepository messageRepository, IEventLogger eventLogger)
		{
			this.connection = connection;
			this.queueListener = queueListener;
			this.messageRepository = messageRepository;
			this.eventLogger = eventLogger;
		}

		public async Task RunAsync(CancellationToken token)
		{
			var getMessage = new TransformBlock<QueueItem, (QueueItem, Message)>(async qi => await GetMessage(qi), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = token });
			var sendMessage = new ActionBlock<(QueueItem, Message)>(async x => await SendMessage(x), new ExecutionDataflowBlockOptions { CancellationToken = token });
			getMessage.LinkTo(sendMessage, new DataflowLinkOptions { PropagateCompletion = true });

			using (var sending = queueListener.QueueItems.Subscribe(getMessage.AsObserver()))
			{
				queueListener.Listen();

				await sendMessage.Completion;
			}
		}

		async Task<(QueueItem, Message)> GetMessage(QueueItem item)
		{
			var messageId = item.MessageId;

			var message = await messageRepository.RetrieveAsync(messageId);

			return (item, message);
		}

		async Task SendMessage((QueueItem, Message) itemAndMessage)
		{
			var (item, message) = itemAndMessage;
			var messageName = message.Name.Replace("<serialno>", (messageCount++).ToString());

			try
			{
				using (var stream = new MemoryStream(message.Body))
				{
					await UseOpenConnection(async () =>
					{
						await connection.SendAsync(messageName, stream);
					});
				}

				queueListener.Ack(item.Delivery);
				await eventLogger.LogEventAsync(item.ToMessageEvent(MessageEventType.Sent));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SendMessage failed: {ex}");
			}
		}

		readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
		Timer closeTimer;
		readonly TimeSpan maxIdleTime = TimeSpan.FromSeconds(15);

		async Task UseOpenConnection(Func<Task> action)
		{
			if (closeTimer == null) { closeTimer = new Timer(async _ => await CloseConnection()); }

			closeTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			await connection.ConnectAsync();
			await action.Invoke();

			closeTimer.Change(maxIdleTime, Timeout.InfiniteTimeSpan);
		}

		async Task CloseConnection()
		{
			await connection.DisconnectAsync();
		}
	}
}
