using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Components.Paralleliser
{
	internal class IndexedQueueItem : QueueItem
	{
		public long Index { get; set; }
		public string PartitionKey { get; set; }
	}
}