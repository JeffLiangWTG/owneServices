using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class ExchangeSpec : IExchangeSpec
	{
		public ExchangeSpec(string type, string name)
		{
			Type = type;
			Name = name;
			Configuration = new Dictionary<string, object>();
		}

		public string Type { get; }
		public string Name { get; }

		public IDictionary<string, object> Configuration { get; }

		public IExchangeSpec DefaultExchange { get; private set; }

		public IQueueSpec DefaultQueue { get; private set; }

		public IExchangeSpec WithDefaultRouting(string name)
		{
			Configuration[Headers.AlternateExchange] = name;

			DefaultQueue = new QueueSpec(name).WithBinding(new BindingSpec(name));
			DefaultExchange = new ExchangeSpec("fanout", name);

			return this;
		}

		public static ExchangeSpec Parse(string spec)
		{
			var elements = spec.Split(':');

			var exchangeSpec = new ExchangeSpec(elements[0], elements[1]);

			if (elements.Length > 2)
			{
				exchangeSpec.WithDefaultRouting(elements[2]);
			}

			return exchangeSpec;
		}
	}
}
