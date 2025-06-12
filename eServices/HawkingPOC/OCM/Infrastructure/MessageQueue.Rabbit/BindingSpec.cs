using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class BindingSpec : IBindingSpec
	{
		public BindingSpec(string exchangeName)
		{
			ExchangeName = exchangeName;
		}

		public string ExchangeName { get; }

		public virtual string RoutingKey => "";

		public virtual IDictionary<string, object> BindArguments => null;
	}
}
