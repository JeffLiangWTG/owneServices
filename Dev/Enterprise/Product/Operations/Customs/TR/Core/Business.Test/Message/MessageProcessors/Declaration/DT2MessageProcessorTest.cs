using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DT2MessageProcessorTest : DeclarationMessageProcessorBaseTest<DT2MessageProcessor>
	{
		[TestDate(2022, 4, 2, 17, 5, 12)]
		public void TestCreateCusPollingTransactionRecord()
		{
			var messageObjects = CreateMessageWithOrigins(GetMessageText(), "DK1001", TRMessageTypes.Codes.DT2);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			var temporaryQueryGUID = "344d5a52-938e-4fc4-b38b-c58d2f03319d";
			incomingMessage.EM_ApplicationReference = temporaryQueryGUID;

			var header = Factory.New<JobDeclaration>();
			header.JE_DeclarationReference = "2019/00002345";

			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var query = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, incomingMessage.PK);
			var pollingTransactions = Factory.Load<CusPollingTransaction>(query);
			AssertEquals("There should be one CusPollingTransaction record created", 1, pollingTransactions.Length);

			var pollingTransaction = pollingTransactions[0];
			CombineAssertions("Cus Polling Transaction Record", () =>
			{
				AssertEquals("CPT_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, pollingTransaction.CPT_ApplicationCode);
				AssertEquals("CPT_Type", TRMessageTypes.Codes.DT2, pollingTransaction.CPT_Type);
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_NumberOfAttempts", 5, pollingTransaction.CPT_NumberOfAttempts.ToZInt());
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", incomingMessage.EM_SystemCreateTimeUtc.AddMinutes(1), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_TransactionID", temporaryQueryGUID, pollingTransaction.CPT_TransactionID);
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.QUA, EDIMessage.Status.ProcessedOK);
		}

		public void TestProcessMessageByEmpty()
		{
			var messageObjects = CreateMessageWithOrigins(TRMessageTestHelper.GetFileText("DT2Empty.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), "DT2001", TRMessageTypes.Codes.DT2);
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			var messageText = incomingMessage.EM_MessageText;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var actualMessage = incomingMessage.EM_MessageInterpretation;
			Assert("EM_MessageInterpretation should contains 'Empty'", actualMessage.Contains(@"The message text sent back from Customs is empty."));
		}

		protected override DT2MessageProcessor Processor => new DT2MessageProcessor(logger);

		protected override ZString DefaultMessageType => TRMessageTypes.Codes.DT2;

		ZString GetMessageText() => TRMessageTestHelper.GetFileText("Common.IslemSorgula3Response.xml");
	}
}
