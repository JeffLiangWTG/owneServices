using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IExchangeSpecCollection : IEnumerable<IExchangeSpec>
	{
		IExchangeSpecCollection Add(IExchangeSpec queueSpec, IQueueSpecCollection queueSpecs);
	}
}
