namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusProvider()
		{
			var messagingProvider = new MessagingProvider();
			AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
		}
	}
}
