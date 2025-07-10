using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class BIRDExportProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessages()
		{
			var externalBroker = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mode = externalBroker.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@ema.com";
			mode.EK_ServerAddressSubject = "EmailAsAttchMode";
			mode.EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_OH_ExternalBroker = externalBroker.PK;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var message = new BIRDEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage();
			AssertEquals("PreCondition", EDIMessage.Status.Pending, message.EM_Status);
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var notifications = new NotificationCollection();
			new BIRDExportProcessor().Execute(notifications);
			AssertEquals("Processed", EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("One email has been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("One text file attachment", 1, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			var birdMessage = UTF8Encoding.ASCII.GetString(Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].Data);
			AssertMultilineASCIIEquals("", message.EM_FormattedMessageText, birdMessage);
			AssertContains(BIRDApplicationCodeList.Codes.EntrySummary, Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].DisplayName);
		}
	}
}
