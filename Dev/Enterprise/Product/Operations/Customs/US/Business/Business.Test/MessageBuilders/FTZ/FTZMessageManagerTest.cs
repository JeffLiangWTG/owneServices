using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FTZMessageManagerTest : TestCaseWithFactory
	{
		public void TestPopulateMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var manager = new FTZMessageManager(ftzMessageSendingObject);
			manager.PopulateMessage(ftzMessageSendingObject.ActionCode);
			AssertEquals("1 message", 1, declaration.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.FTZAdmissionAdd, declaration.Messages[0].EM_MessageSubType);

			ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Delete);
			manager = new FTZMessageManager(ftzMessageSendingObject);
			manager.PopulateMessage(ftzMessageSendingObject.ActionCode);
			AssertEquals("2 message", 2, declaration.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.FTZAdmissionDelete, declaration.Messages[1].EM_MessageSubType);

			ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Replace);
			manager = new FTZMessageManager(ftzMessageSendingObject);
			manager.PopulateMessage(ftzMessageSendingObject.ActionCode);
			AssertEquals("3 message", 3, declaration.Messages.Count);
			AssertEquals("EM_MessageSubType", EM_MessageSubTypeList.Codes.FTZAdmissionReplace, declaration.Messages[2].EM_MessageSubType);
		}

		public void TestCanSendThisMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var manager = new FTZMessageManager(ftzMessageSendingObject);
			Assert("Can send Original - no transactions exist", manager.CanSendThisMessage(ftzMessageSendingObject.ActionCode, out var _, out var _));

			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			Factory.Save();
			Assert("Cannot send Original second time - Original Accepted", !manager.CanSendThisMessage(UpdateActionCode.Add, out var messageText, out var _));
			Assert("Error text should contains 'Already Added' notification", messageText.Contains(FTZMessageManager.AlreadyAdded));
			Assert("Can send Delete - Original Accepted", manager.CanSendThisMessage(UpdateActionCode.Delete, out var _, out var _));

			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionDelete;
			Assert("Cannot send Delete Message - Original not added", !manager.CanSendThisMessage(UpdateActionCode.Delete, out messageText, out var _));
			Assert("Error text should contains 'Not Added' notification", messageText.Contains(FTZMessageManager.NotAdded));
		}

		public void TestIsWaitingForResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var manager = new FTZMessageManager(ftzMessageSendingObject);
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;
			Assert("IsWaitingForResponse", manager.IsWaitingForResponse);
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionDelete;
			Assert("IsWaitingForResponse", manager.IsWaitingForResponse);
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend;
			Assert("IsWaitingForResponse", manager.IsWaitingForResponse);

			declaration.AdmissionStatus = "";
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);

			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend;
			Assert("IsWaitingForResponse", !manager.IsWaitingForResponse);
		}
	}
}
