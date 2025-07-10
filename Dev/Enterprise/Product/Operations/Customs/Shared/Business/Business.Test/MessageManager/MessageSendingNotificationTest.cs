using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	abstract class MessageSendingNotificationTest : TestCaseWithFactory
	{
		public abstract void TestMessagePrefix();
	}
}
