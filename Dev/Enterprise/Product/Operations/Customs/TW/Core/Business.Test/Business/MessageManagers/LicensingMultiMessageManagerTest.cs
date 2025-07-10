using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LicensingMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendMessages()
		{
			SendAllMessage();
			foreach (LicensingMessageSendingObject action in parent.SendingObjectsCollection)
			{
				var entry = action.Header;
				var message = entry.Messages.Cast<TWMessage>().FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertEquals("entry.Messages has 1", 1, entry.Messages.Count);
					AssertEquals("message.EM_ApplicationCode is TWC", EDIMessage.ApplicationCodes.TaiwanCustoms, message.EM_ApplicationCode);
					AssertEquals("message.EM_Status is QUE", Status.Queued, message.EM_Status);
					AssertEquals("message.EM_ReceiveTransmit is TRX", Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType is 101", MessageTypeList.Codes._101, message.EM_MessageType);
					AssertEquals("message.EM_LinkUniqueID is not null", entry.PK, message.EM_LinkUniqueID);
					AssertEquals("message.EM_LinkTable is CusTWControllingMessageHeader", CusTWControllingMessageHeaderSchema.Constants.TableName, message.EM_LinkTable);
					AssertEquals("message.EM_ApplicationReference is tt1234", "tt1234", message.EM_ApplicationReference);
					AssertEquals("message.EM_IsTestMessage is true", true, message.EM_IsTestMessage);
				});
			}
		}

		void SendAllMessage()
		{
			parent = new NX101LicensingMessageSendingObjectParent(declaration);
			AssertEquals("Count is 1", 1, parent.SendingObjectsCollection.Count);

			parent.SendingObjectsCollection.Cast<LicensingMessageSendingObject>().ForEach(x => x.ShouldSend = true);
			var manager = new LicensingMultiMessageManager(parent, controllingMessageHeader.TW1_ControllingMessageType);
			manager.SendMessagesWithoutSaving(new SendsMessagesToCustomsShutterUpperer());
		}

		public void TestSendMessages_DeclarationEvent()
		{
			AssertSendMessages_DeclarationEvent("NX101", "5", Events.DeclarationAmendmentSent.Code, "test menu caption, SendingObjectsCollectionAction='5'");
			AssertSendMessages_DeclarationEvent("NX101", "17", Events.DeclarationAmendmentSent.Code, "test menu caption, SendingObjectsCollectionAction='17'");
			AssertSendMessages_DeclarationEvent("NX101", "18", Events.DeclarationAmendmentSent.Code, "test menu caption, SendingObjectsCollectionAction='18'");
		}

		void AssertSendMessages_DeclarationEvent(string messageType, string actionCode, string eventCode, string expectReference)
		{
			var parent = new NX101LicensingMessageSendingObjectParent(declaration, "test menu caption");
			foreach (LicensingMessageSendingObject action in parent.SendingObjectsCollection)
			{
				action.Action = actionCode;
			}
			var manager = new LicensingMultiMessageManagerForTest(parent, messageType);
			manager.LogDeclarationEventExpose();

			Factory.Save();
			var log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
			AssertEquals("should always log a MSN event", expectReference, log.SL_Reference);

			log = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
			AssertEquals(expectReference, log.SL_Reference);
		}

		class LicensingMultiMessageManagerForTest : LicensingMultiMessageManager
		{
			public LicensingMultiMessageManagerForTest(LicensingMessageSendingObjectParent licensingWrapper, ZString messageType) : base(licensingWrapper, messageType)
			{
			}

			public void LogDeclarationEventExpose() => LogDeclarationEvent();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Taiwan;
			org.MainAddress.OA_Address1 = "Address 1";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "tt1234";
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_FunctionalReferenceId = "C";
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(controllingMessageHeader);
			Factory.Save();
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader controllingMessageHeader;
		LicensingMessageSendingObjectParent parent;
	}
}
