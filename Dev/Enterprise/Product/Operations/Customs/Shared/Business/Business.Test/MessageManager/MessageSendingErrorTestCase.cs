namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageSendingErrorTestCase : MessageSendingNotificationTest
	{
		public override void TestMessagePrefix()
		{
			AssertEquals("Error", new MessageSendingError("message").MessagePrefix);
		}
	}
}
