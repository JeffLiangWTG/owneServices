using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class DrawbackSummaryMessageManagerTest : TestCaseWithFactory
	{
		public void TestPopulateMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var manager = new DrawbackSummaryMessageManager(new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add));
			manager.PopulateMessage(UpdateActionCode.Add);
			AssertEquals("1 message", 1, declaration.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.DrawbackSummaryOriginal, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal + " - " + DrawbackSummaryStatusList.Descriptions.AwaitingDrawbackSummaryOriginal, declaration.JE_MessageStatusDescription);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			manager = new DrawbackSummaryMessageManager(new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Delete));
			manager.PopulateMessage(UpdateActionCode.Delete);
			AssertEquals("1 message", 1, declaration.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.DrawbackSummaryDelete, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryDelete + " - " + DrawbackSummaryStatusList.Descriptions.AwaitingDrawbackSummaryDelete, declaration.JE_MessageStatusDescription);
		}

		public void TestCanSendThisMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.ABI;
			var manager = new DrawbackSummaryMessageManager(new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Delete));
			Assert("Has Changes", declaration.HasChanges);
			Assert("Has Changes", !manager.CanSendThisMessage(UpdateActionCode.Add, out var messageText));
			Assert("Has Changes", messageText.Contains("Drawback not yet saved, Please save before sending."));

			declaration.JE_EntryStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			Factory.Save();
			Assert("Already Exists", !manager.CanSendThisMessage(UpdateActionCode.Add, out messageText));
			Assert("Already Exists", messageText.Contains("the Drawback Summary has already been added."));
			declaration.JE_EntryStatus = "";
			declaration.HasChanges = false;
			Assert("OK to send Original", manager.CanSendThisMessage(UpdateActionCode.Add, out messageText));

			Assert("Not Exists", !manager.CanSendThisMessage(UpdateActionCode.Delete, out messageText));
			Assert("Not Exists", messageText.Contains("the Drawback Summary has not been added yet."));
			declaration.JE_EntryStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			declaration.HasChanges = false;
			Assert("OK to send Delete", manager.CanSendThisMessage(UpdateActionCode.Delete, out messageText));

			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.Manual;
			declaration.HasChanges = false;
			Factory.Save();
			Assert("Manual Filing", !manager.CanSendThisMessage(UpdateActionCode.Add, out messageText));
			AssertEquals(DrawbackSummaryMessageManager.NotFiledAsABI, messageText);
		}

		public void TestIsWaitingForResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var manager = new DrawbackSummaryMessageManager(new JobDeclarationDrawbackSupporter(declaration, UpdateActionCode.Add));
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryOriginal;
			Assert("IsWaitingForResponse", manager.IsWaitingForResponse);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.AwaitingDrawbackSummaryDelete;
			Assert("IsWaitingForResponse", manager.IsWaitingForResponse);
			declaration.JE_MessageStatus = "";
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryDelete;
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryOriginal;
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ErrorDrawbackSummaryDelete;
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
		}
	}
}
