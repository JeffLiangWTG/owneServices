using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseMessageSendingObjectTest : TestCaseWithFactory
	{
		public void TestMessageCreated()
		{
			void PreviewMessage(MessageEventArgs args)
			{
				args.MessageText = args.MessageText.Replace("A", "B");
			}

			var sendingObject = new BaseMessageSendingObjectForTest(Factory);
			sendingObject.PreviewMessage += PreviewMessage;

			var messageText = sendingObject.MessageCreated("ABCDEFABCA");
			AssertEquals("expected message text", "BBCDEFBBCB", messageText);
		}
	}
}
