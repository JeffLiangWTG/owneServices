namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageSendingInformationTestCase : MessageSendingNotificationTest
	{
		public override void TestMessagePrefix()
		{
			AssertEquals("Information", new MessageSendingInformation("message").MessagePrefix);
		}
	}
}
