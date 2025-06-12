using System;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Extensions
{
	public class OceanInsighsSubscriptionValidationException : Exception
	{
		public OceanInsighsSubscriptionValidationException(string message)
			: base(message)
		{ }
	}
}