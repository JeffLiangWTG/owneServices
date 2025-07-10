using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	class DTEMessageProcessorTest : DeclarationMessageProcessorBaseTest<DTEMessageProcessor>
	{
		protected override DTEMessageProcessor Processor => new DTEMessageProcessor(logger);

		protected override ZString DefaultMessageType => TRMessageTypes.Codes.DTE;

		public void TestProcessMessage()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DTE001");
			var incomingMessage = messageObjects.incomingMessage;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			var createdPollingTransaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery().AddToFilter(CusPollingTransactionSchema.CPT_ParentID, incomingMessage.PK));

			var actualMessage = incomingMessage.EM_MessageInterpretation;

			AssertNotNull("Should have created a CusPollingTransaction linked to the incoming message.", createdPollingTransaction);
			CombineAssertions("Created PollingTransaction created.", () =>
			{
				AssertEquals("CPT_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, createdPollingTransaction.CPT_ApplicationCode);
				AssertEquals("CPT_Type", DefaultMessageType, createdPollingTransaction.CPT_Type);
				AssertEquals("CPT_TransactionID", "6f65c6e4-23c0-4c2e-bf2d-651d71e514bb", createdPollingTransaction.CPT_TransactionID);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
				AssertNotNullOrEmpty(actualMessage);
				Assert("EM_MessageInterpretation should contains 'Query GUID'", actualMessage.Contains("<td>Query GUID:</td><td>6f65c6e4-23c0-4c2e-bf2d-651d71e514bb</td>"));
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", incomingMessage.EM_SystemCreateTimeUtc.AddMinutes(TRMessageConstants.DT1PollingDelay), createdPollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.RMA, EDIMessage.Status.ProcessedOK);
		}

		public void TestGetCorrectBranchPK()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DTE001");

			var incomingMessage = messageObjects.incomingMessage;
			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("GetCorrectBranchPK should have returned the correct BranchPK", messageObjects.messageAttachee.JE_GB, incomingMessage.EM_GB);
		}

		public void TestProcessMessageEmptyMessage()
		{
			var messageObjects = CreateMessageWithOrigins(string.Empty, "DT3001", TRMessageTypes.Codes.DTE);
			var emptyMessage = messageObjects.incomingMessage;

			AssertNoExceptionThrown("Processing empty DTE message should not throw exception.",
				() =>
				{
					Processor.PreProcessMessage(emptyMessage);
					Processor.ProcessMessage(emptyMessage);
				}
			);

			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, emptyMessage.EM_Status);
			AssertNull("Should not create CusPollingTransaction.", Factory.LoadTop1<CusPollingTransaction>(new ZQuery().AddToFilter(CusPollingTransactionSchema.CPT_ParentID, emptyMessage.PK)));
		}

		[TestDate(2022, 10, 05)]
		public void TestGeneratedDT2Message()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin.zhang@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetFileText("DTEResultError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming.");

			var messageObjects1 = CreateMessageWithOrigins(messageText, "DTO003", TRMessageTypes.Codes.DTE);
			var incomingMessage1 = messageObjects1.incomingMessage;
			incomingMessage1.EM_Status = EDIMessage.Status.PreProcessedOK;
			incomingMessage1.EM_SystemCreateTimeUtc = new ZDateTime(2022, 01, 01, 0, 0, 0);
			incomingMessage1.EM_ApplicationReference = "XXXXX";
			var originalMessage1 = messageObjects1.originalMessage;
			originalMessage1.EM_SystemCreateUser = staff.GS_Code;
			var cusEntryHeader1 = originalMessage1?.EM_LinkedObject as CusEntryHeader;

			var messageObjects2 = CreateMessageWithOrigins(messageText, "DTO004", TRMessageTypes.Codes.DTE);
			var incomingMessage2 = messageObjects2.incomingMessage;
			incomingMessage2.EM_Status = EDIMessage.Status.PreProcessedOK;
			incomingMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2022, 01, 02, 0, 0, 0);
			incomingMessage2.EM_ApplicationReference = "YYY";
			var originalMessage2 = messageObjects2.originalMessage;
			originalMessage2.EM_SystemCreateUser = staff.GS_Code;
			var cusEntryHeader2 = originalMessage2?.EM_LinkedObject as CusEntryHeader;

			var messageObjects3 = CreateMessageWithOrigins(messageText, "DTO005", TRMessageTypes.Codes.DTE);
			var incomingMessage3 = messageObjects3.incomingMessage;
			incomingMessage3.EM_Status = EDIMessage.Status.PreProcessedOK;
			incomingMessage3.EM_SystemCreateTimeUtc = new ZDateTime(2022, 01, 03, 0, 0, 0);
			incomingMessage3.EM_ApplicationReference = ZString.Empty;
			var originalMessage3 = messageObjects3.originalMessage;
			originalMessage3.EM_SystemCreateUser = staff.GS_Code;
			var cusEntryHeader3 = originalMessage3?.EM_LinkedObject as CusEntryHeader;

			var messageDT1 = originalMessage3 = TRMessageTestHelper.CreateMessage<TRImportExportMessage>(Factory, TRMessageTypes.Codes.DTE, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, "DKOTRX001", string.Empty);

			Processor.PreProcessMessage(incomingMessage3);
			Processor.ProcessMessage(incomingMessage3);

			CombineAssertions(() =>
			{
				var messageDT2 = cusEntryHeader3.Messages[2];
				AssertEquals("EM_IsActive", true, messageDT2.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", messageDT2.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "DT2", messageDT2.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", messageDT2.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", messageDT2.EM_Status);
				var messageInterpretation = messageDT2.EM_MessageInterpretation;
				Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretation.Contains("Declaration Message Type DT2 sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Job Number'", messageInterpretation.Contains("<tr><td>Job Number:</td><td>B00001002</td></tr>"));
				Assert("EM_MessageInterpretation should contains 'Query Date'", messageInterpretation.Contains("<tr><td>Query Date:</td><td>2022-10-05</td></tr>"));
				AssertCollectionNotContains("Logger", "\tUnable to find the original outgoing message for the message, number: DTO001", logger.UserLogStrings);
			});
		}

		ZString GetDefaultMessageText() => TRMessageTestHelper.GetFileText("DTEResultSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming.");
	}
}
