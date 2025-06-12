using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using OcmPoc.Core.Services;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Components.Correlator
{
	class Correlator
	{
		readonly IQueueListener<QueueItem> listener;
		readonly IQueueWriter writer;
		readonly IMessageFlowCorrelator correlator;

		public Correlator(IQueueListener<QueueItem> listener, IQueueWriter writer, IMessageFlowCorrelator correlator)
		{
			this.listener = listener;
			this.writer = writer;
			this.correlator = correlator;
		}

		public async Task RunAsync(CancellationToken token)
		{
			var correlateMessage = CreateCorrelationBlock(token);
			var sendMessage = CreateSendBlock(token);
			correlateMessage.LinkTo(sendMessage, new DataflowLinkOptions { PropagateCompletion = true });

			using (var subscription = listener.QueueItems.Subscribe(correlateMessage.AsObserver()))//.ForEachAsync(async qi => await Correlate(qi), token);
			{
				listener.Listen();
				await sendMessage.Completion;
			}
		}

		TransformBlock<QueueItem, QueueItem> CreateCorrelationBlock(CancellationToken token)
		{
			return new TransformBlock<QueueItem, QueueItem>(async qi =>
				{
					await correlator.CorrelateFlowAsync(qi.MessageFlowId, qi.Sender, qi.Recipient, qi.CorrelationId);
					return qi;
				},
				new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = token });
		}

		ActionBlock<QueueItem> CreateSendBlock(CancellationToken token)
		{
			return new ActionBlock<QueueItem>(qi =>
				{
					using (var tx = writer.NewTransaction())
					{
						writer.Publish(qi);
						listener.Ack(qi.Delivery);
						tx.Commit();
					}
				},
				new ExecutionDataflowBlockOptions { CancellationToken = token });
		}
	}
}
