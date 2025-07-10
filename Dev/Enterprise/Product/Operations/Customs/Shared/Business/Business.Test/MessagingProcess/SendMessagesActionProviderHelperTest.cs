using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class SendMessagesActionProviderHelperTest : TestCaseWithFactory
	{
		public void TestDeleteMessages()
		{
			var msg1 = Factory.New<EDIMessage>();
			msg1.EM_MessageText = "Hello";
			var msg2 = Factory.New<EDIMessage>();
			msg2.EM_MessageText = "World";

			var prev = new ActionResult(true);
			prev.EDIMessages = new List<EDIMessage> { msg1, msg2 };

			CombineAssertions(() =>
			{
				AssertEquals("Pre-req Msg count", 2, prev.EDIMessages.Count);
				foreach (var msg in prev.EDIMessages)
				{
					AssertEquals("Pre-req - Msg not deleted", false, msg.IsDeleted);
				}

				var result = SendMessagesActionProviderHelper.DeleteMessages(prev);

				AssertEquals("Msg count", 2, result.EDIMessages.Count);
				foreach (var msg in result.EDIMessages)
				{
					AssertEquals("Msg deleted", true, msg.IsDeleted);
				}
			});
		}
	}
}
