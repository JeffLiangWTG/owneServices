using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SecureContainerReleaseMessageEventsProcessor : IMessageEventsProcessor
	{
		#region IMessageEventsProcessor

		public void OnMessageSent()
		{ }

		public void OnMessageWithdrawalSent()
		{ }

		public void OnResetToOriginal()
		{ }

		#endregion
	}
}
