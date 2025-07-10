namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageSendingWarningTest : MessageSendingNotificationTest
	{
		public override void TestMessagePrefix()
		{
			AssertEquals("Warning", new MessageSendingWarning("message").MessagePrefix);
		}
	}
}
