using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USRIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessorProcessesMessage()
		{
			var processor = new USRIncomingMessageProcessor();

			DeclarationTestHelper.SetupForSendMessage();
			var message = CreateTestMessage("102");
			Factory.Save();
			processor.ExecuteBatch();
			var message2 = new BusinessObjectFactory().Load<MQEDIMessage>(message.PK);
			AssertEquals(MQEDIMessage.Status.Failed, message2.EM_Status);
		}

		public void TestMessageInEDIMessageQueueState_NotProcesses()
		{
			var processor = new USRIncomingMessageProcessor();

			DeclarationTestHelper.SetupForSendMessage();
			var message = CreateTestMessage("102");
			Factory.Save();
			using (var cmd = Db.Connection.Command(@"INSERT EDIMessageQueueState (EQS_PK, EQS_ApplicationCode, EQS_EM, EQS_Keys, EQS_Status, EQS_ChainID, EQS_ParentMessageNumber, EQS_ParentSystemCreateTimeUtc, EQS_SystemCreateTimeUtc, EQS_SystemCreateUser, EQS_SystemLastEditTimeUtc, EQS_SystemLastEditUser)
VALUES (NEWID(), @ApplicationCode, @MessagePK, 'DEFAULT', 'PKE', '00000000-0000-0000-0000-000000000000', @MessageNum, @ParentSystemCreateTimeUtc, GETUTCDATE(), 'T', GETUTCDATE(), 'T')"))
			{
				cmd.AddParameter("@ApplicationCode", SqlDbType.VarChar, message.EM_ApplicationCode.ToString());
				cmd.AddParameter("@MessagePK", SqlDbType.UniqueIdentifier, message.PK.ToGuid());
				cmd.AddParameter("@MessageNum", SqlDbType.VarChar, message.EM_MessageNum.ToString());
				cmd.AddParameter("@ParentSystemCreateTimeUtc", SqlDbType.DateTime, message.EM_SystemCreateTimeUtc.ToDateTime());
				cmd.ExecuteNonQuery();
			}
			processor.ExecuteBatch();
			var message2 = new BusinessObjectFactory().Load<MQEDIMessage>(message.PK);
			AssertEquals(MQEDIMessage.Status.Queued, message2.EM_Status);
		}

		public void TestGetProcessableMessageOrderByTimeThenMessageNum()
		{
			TestCaseHelper.ClearTable("EdiMessage");
			var testMessage1 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1);

			var testMessage2 = CreateTestMessage("200");
			var testMessage3 = CreateTestMessage("300");
			var testMessage4 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1);

			var testMessage5 = CreateTestMessage("400");
			Factory.Save();
			System.Threading.Thread.Sleep(1);

			var testMessage6 = CreateTestMessage("500");
			Factory.Save();

			var testMessage7 = CreateTestMessage("600");
			Factory.Save();

			var testMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("There should be 7 test messages in the file", 7, testMessages.Length);

			var filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);

			var applicationCodeFilter = new ZQuery();
			applicationCodeFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, EDIMessage.ApplicationCodes.USCustomsImport);

			filter.AddToFilter(applicationCodeFilter);
			filter.MaximumRows = 5;
			var processor = new USRMessageProcessorForTest();
			filter.OrderBy = processor.sortorder;
			AssertEquals("message order by", "EM_SystemCreateTimeUtc, EM_MessageNum", filter.OrderBy);

			testMessages = Factory.Load<EDIMessage>(filter);
			AssertEquals("filter should have returned only earliest 5 messages", 5, testMessages.Length);
			Assert("Messages with same message number should be ordered by creation time", testMessages[0].EM_SystemCreateTimeUtc < testMessages[1].EM_SystemCreateTimeUtc);
			AssertEquals("first returned message", "100", testMessages[0].EM_MessageNum);
			AssertEquals("last returned message", "400", testMessages[4].EM_MessageNum);
		}

		MQEDIMessage CreateTestMessage(string messageNum)
		{
			var testMessage = Factory.New<MQEDIMessage>();
			testMessage.EM_Status = MQEDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			testMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			testMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			testMessage.EM_MessageNum = messageNum;

			return testMessage;
		}

		sealed class USRMessageProcessorForTest : USRIncomingMessageProcessor
		{
			public string sortorder => GetProcessableMessagesOrder();
		}
	}
}
