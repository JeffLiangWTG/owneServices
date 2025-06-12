using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;

namespace OcmPoc.Components.EventProcessor
{
	class HistoryBuilder
	{
		readonly IEventListener eventListener;
		readonly IMessageFlowRepository messageFlowRepository;

		public HistoryBuilder(IEventListener eventListener, IMessageFlowRepository messageFlowRepository)
		{
			this.eventListener = eventListener;
			this.messageFlowRepository = messageFlowRepository;
		}

		internal async Task RunAsync(Configuration configuration, CancellationToken token)
		{
			Console.WriteLine("Listening to Kafka stream");
			//eventListener.OnLog += (_, e) => Console.WriteLine(e);
			using (var semaphore = new SemaphoreSlim(1))
			{
				await eventListener.Events
					.ForEachAsync(async messageEvent =>
					{
						try
						{
							await semaphore.WaitAsync();
							await messageFlowRepository.AddEventAsync(messageEvent.MessageFlowId, messageEvent);
							//Console.WriteLine($"Stored MessageEvent {messageEvent.MessageFlowId}:{messageEvent.Id} with MessageId = {messageEvent.MessageId}");
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
						finally
						{
							semaphore.Release();
						}
					},
					token);
			}
			Console.WriteLine("Finished listening");
		}
	}
}
