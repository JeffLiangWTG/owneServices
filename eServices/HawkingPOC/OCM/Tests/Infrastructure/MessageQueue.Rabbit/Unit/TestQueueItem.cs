using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	class TestQueueItem : QueueItem
    {
        public string AdditionalHeader { get; set; }
	}
}
