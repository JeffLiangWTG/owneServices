using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusProvider()
		{
			var messagingProvider = new MessagingProvider();
			AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
		}
	}
}
