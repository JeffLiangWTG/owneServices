using System.Collections;
using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class ExchangeSpecCollection : IExchangeSpecCollection
	{
		readonly List<IExchangeSpec> specs = new List<IExchangeSpec>();

		public IExchangeSpecCollection Add(IExchangeSpec exchangeSpec, IQueueSpecCollection queueSpecs)
		{
			if (exchangeSpec.DefaultExchange != null)
			{
				specs.Add(exchangeSpec.DefaultExchange);
				queueSpecs.Add(exchangeSpec.DefaultQueue);
			}

			specs.Add(exchangeSpec);

			return this;
		}

		public IEnumerator<IExchangeSpec> GetEnumerator()
		{
			return specs.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)specs).GetEnumerator();
		}
	}
}
