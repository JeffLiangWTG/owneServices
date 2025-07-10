using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ABIIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestWhenNewBranchIsAdded()
		{
			var testMessage1 = CreateTestMessage("100");
			testMessage1.EM_MessageText = "B00                                                        B                    X0 BLOCK       1 REF ID: 8888 SV9    AE 6007772                                 X1 FX17   FILER NOT AUTHORIZED                                                  X1RF999   BATCH REJECTED                                                        Y           00003";
			testMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;// This is set when interchanges are processed
			Factory.Save();

			var processor = new ABIIncomingMessageProcessor();//GlbCompany.CurrentCompany.Branches are loaded and cached.
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();

			testMessage1.Reload();

			var logs = new ZStringBuilder();
			var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}

			AssertEquals(@"Processing Message #100/Saving.../1 message processed", logs.ToStringWithDelimiterBetweenAppends("/"));
			AssertEquals("Should have been processed", EDIMessage.Status.Received, testMessage1.EM_Status);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };//imitating different app domains
			var newBranch = newFactory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "!TT";
			newFactory.Save();

			var testMessage2 = CreateTestMessage("200");
			testMessage2.EM_MessageText = "B00                                                        B                    X0 BLOCK       1 REF ID: 8888 SV9    AE 6007772                                 X1 FX17   FILER NOT AUTHORIZED                                                  X1RF999   BATCH REJECTED                                                        Y           00003";
			testMessage2.EM_MessageType = Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;// This is set when interchanges are processed
			testMessage2.EM_GB = newBranch.PK;
			Factory.Save();

			processor = new ABIIncomingMessageProcessor();
			processor.Logger = new BatchProcessor.LoggingInformation();
			processor.ExecuteBatch();
			testMessage2.Reload();

			logs = new ZStringBuilder();
			enumerator = processor.Logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}

			AssertEquals(@"Processing Message #200/Saving.../1 message processed", logs.ToStringWithDelimiterBetweenAppends("/"));
			AssertEquals("Should have been processed", EDIMessage.Status.Received, testMessage2.EM_Status);
		}

		public void TestMessageProcessorProcessesMessage()
		{
			processor = new ABIIncomingMessageProcessor();
			DeclarationTestHelper.SetupForSendMessage();
			MQEDIMessage message = CreateTestMessage("102");
			Factory.Save();
			processor.ExecuteBatch();
			MQEDIMessage message2 = new BusinessObjectFactory().Load<MQEDIMessage>(message.PK);
			AssertEquals(MQEDIMessage.Status.Failed, message2.EM_Status);
		}

		public void TestMessageInEDIMessageQueueState_NotProcesses()
		{
			processor = new ABIIncomingMessageProcessor();
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
			EDIMessage testMessage1 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			MQEDIMessage testMessage2 = CreateTestMessage("200");
			MQEDIMessage testMessage3 = CreateTestMessage("300");
			MQEDIMessage testMessage4 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			MQEDIMessage testMessage5 = CreateTestMessage("400");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			MQEDIMessage testMessage6 = CreateTestMessage("500");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			MQEDIMessage testMessage7 = CreateTestMessage("600");
			Factory.Save();

			EDIMessage[] testMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("There should be 7 test messages in the file", 7, testMessages.Length);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "RCV");
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);

			ZQuery applicationCodeFilter = new ZQuery();
			applicationCodeFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, EDIMessage.ApplicationCodes.USCustomsImport);

			filter.AddToFilter(applicationCodeFilter);
			filter.MaximumRows = 5;
			ABIMessageProcessorForTest processor = new ABIMessageProcessorForTest();
			filter.OrderBy = processor.sortorder;
			AssertEquals("message order by", "EM_SystemCreateTimeUtc, EM_MessageNum", filter.OrderBy);

			testMessages = Factory.Load<EDIMessage>(filter);
			AssertEquals("filter should have returned only earliest 5 messages", 5, testMessages.Length);
			Assert("Messages with same message number should be ordered by creation time", testMessages[0].EM_SystemCreateTimeUtc < testMessages[1].EM_SystemCreateTimeUtc);
			AssertEquals("first returned message", "100", testMessages[0].EM_MessageNum);
			AssertEquals("last returned message", "400", testMessages[4].EM_MessageNum);
		}

		public void TestMessageProcessorDoesNotTrim_ADRWE0ReferenceDataText()
		{
			processor = new ABIIncomingMessageProcessor();
			MQEDIMessage message = CreateTestMessage("103");
			message.EM_MessageText = "B001001739DX                                           N   JWTJFKJFK_640680     E0 BLOCK  000001 REF ID: 1001 739    DE JWTJFKJFK_640680                        E0 SUMMRY 000001 REF ID: 739 81112839 X00081123                                 E0 EXPDES 000001 REF ID:  6211110000 020518TRUCK                                E1 F266   EXPORT / DESTROY INDICATOR MISSING                                    E1RF999   TRANSACTION DATA REJECTED                                             Y  1001739DX00000                                                               ";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQueryResponse;
			Factory.Save();
			processor.ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
		}

		MQEDIMessage CreateTestMessage(string messageNum)
		{
			MQEDIMessage testMessage = Factory.New<MQEDIMessage>();
			testMessage.EM_Status = MQEDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			testMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			testMessage.EM_MessageNum = messageNum;

			return testMessage;
		}
		ABIIncomingMessageProcessor processor;

		sealed class ABIMessageProcessorForTest : ABIIncomingMessageProcessor
		{
			public string sortorder => GetProcessableMessagesOrder();
		}
	}
}
