namespace OcmPoc.Tests.Scenario.Helpers
{
	class FlowSpec
    {
		public FlowSpec(string senderRecipient)
		{
			var names = senderRecipient.Split(':');
			Sender = names[0];
			Recipient = names[1];
		}

		public string Sender { get; }
		public string Recipient { get; }
	}
}
