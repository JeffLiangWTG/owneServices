using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IQueueSpecCollection : IEnumerable<IQueueSpec>
	{
		IQueueSpecCollection Add(IQueueSpec queueSpec);
	}
}
