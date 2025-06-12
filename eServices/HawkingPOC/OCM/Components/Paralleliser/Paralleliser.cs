using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Components.Paralleliser
{
	class Paralleliser
	{
		readonly IQueueClient queueClient;
		readonly IMapper mapper;

		public Paralleliser(IQueueClient queueClient, IMapper mapper)
		{
			this.queueClient = queueClient;
			this.mapper = mapper;
		}

		public async Task RunAsync(Configuration config, IQueueSpecCollection queues, CancellationToken token)
		{ 
			try
			{
				using (queueClient.CreateTemporaryQueues(queues))
				using (var sorter = new Sorter(queueClient, mapper))
				using (var indexer = new Indexer(queueClient, mapper))
				//using (new ProcessManager(indexer))
				{
					indexer.IndexInputQueue(config.RabbitMq.Queue.Input)
						   .PartitionedBy(config.PartitionHeaders ?? String.Empty)
						   .ToOutputQueue(config.RabbitMq.Queue.InternalInput);

					sorter.SortQueues(config.RabbitMq.Queue.InternalOutput, config.RabbitMq.Queue.InternalDeadLetter)
						  .ToOutput(config.RabbitMq.Output);

					await Task.WhenAll(
						indexer.RunAsync(token),
						sorter.RunAsync(token),
						indexer.Acknowledge(sorter.SuccessfulItems, token),
						indexer.Reject(sorter.FailedItems, token)
					);

					Console.WriteLine("Parallelisation complete");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
