using System;

namespace eServices.eHubRoutingRuleEngine
{
	public class RoutingRuleException : Exception
	{
		public RoutingRuleException() : base() { }
		public RoutingRuleException(string message) : base(message) { }
		public RoutingRuleException(string message, Exception innerException) : base(message, innerException) { }
	}
}
