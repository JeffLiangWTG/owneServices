using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IBindingSpec
    {
		string ExchangeName { get; }
		string RoutingKey { get; }
		IDictionary<string, object> BindArguments { get; }
	}
}
