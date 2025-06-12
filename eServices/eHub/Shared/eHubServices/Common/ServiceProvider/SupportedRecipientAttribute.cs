using System;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class SupportedRecipientAttribute : Attribute
	{
		public SupportedRecipientAttribute(string recipient)
		{
			Recipient = recipient;
		}

		public string Recipient { get; private set; }
	}
}