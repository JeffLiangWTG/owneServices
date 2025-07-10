using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusProvider()
		{
			var messagingProvider = new MessagingProvider();
			AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
		}
	}
}
