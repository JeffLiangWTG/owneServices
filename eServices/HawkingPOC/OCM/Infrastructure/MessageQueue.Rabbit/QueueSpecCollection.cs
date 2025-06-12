using System.Collections;
using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueSpecCollection : IQueueSpecCollection
	{
		readonly List<IQueueSpec> specs = new List<IQueueSpec>();

		public IQueueSpecCollection Add(IQueueSpec queueSpec)
		{
			if (queueSpec.DeadLetterQueue != null)
			{
				specs.Add(queueSpec.DeadLetterQueue);
			}

			specs.Add(queueSpec);

			return this;
		}

		public IEnumerator<IQueueSpec> GetEnumerator()
		{
			return specs.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)specs).GetEnumerator();
		}
	}
}
