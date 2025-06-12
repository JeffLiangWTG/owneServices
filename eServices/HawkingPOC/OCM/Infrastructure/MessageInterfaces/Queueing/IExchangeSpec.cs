using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IExchangeSpec
	{
		string Name { get; }
		string Type { get; }

		IDictionary<string, object> Configuration { get; }

		IExchangeSpec DefaultExchange { get; }
		IQueueSpec DefaultQueue { get; }
	}
}