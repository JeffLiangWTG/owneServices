using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESTIRIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessorProcessesMessage()
		{
			DeclarationTestHelper.SetupForSendMessage();
			AESTIREDIMessage message = CreateTestMessage("101");
			Factory.Save();
			processor = new AESTIRIncomingMessageProcessor();
			processor.ExecuteBatch();
			AESTIREDIMessage message2 = new BusinessObjectFactory().Load<AESTIREDIMessage>(message.PK);
			AssertEquals(MQEDIMessage.Status.Failed, message2.EM_Status);
		}

		public void TestGetProcessableMessageOrderByTimeThenMessageNum()
		{
			TestCaseHelper.ClearTable("EdiMessage");
			AESTIREDIMessage testMessage1 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			AESTIREDIMessage testMessage2 = CreateTestMessage("200");
			AESTIREDIMessage testMessage3 = CreateTestMessage("300");
			AESTIREDIMessage testMessage4 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			AESTIREDIMessage testMessage5 = CreateTestMessage("400");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			AESTIREDIMessage testMessage6 = CreateTestMessage("500");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			AESTIREDIMessage testMessage7 = CreateTestMessage("600");
			Factory.Save();

			EDIMessage[] testMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("There should be 7 test messages in the file", 7, testMessages.Length);

			ZQuery filter = new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, AESTIREDIMessage.Direction.Receive);
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			filter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, EDIMessage.ApplicationCodes.USCustomsExport);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.AES.CommodityShipmentResponse);
			filter.MaximumRows = 5;
			AESTIRMessageProcessorForTest processor = new AESTIRMessageProcessorForTest();
			filter.OrderBy = processor.sortorder;
			AssertEquals("message order by", "EM_SystemCreateTimeUtc, EM_MessageNum", filter.OrderBy);

			testMessages = Factory.Load<EDIMessage>(filter);
			AssertEquals("filter should have returned only earliest 5 messages", 5, testMessages.Length);
			Assert("Messages with same message number should be ordered by creation time", testMessages[0].EM_SystemCreateTimeUtc < testMessages[1].EM_SystemCreateTimeUtc);
			AssertEquals("first returned message", "100", testMessages[0].EM_MessageNum);
			AssertEquals("last returned message", "400", testMessages[4].EM_MessageNum);
		}

		AESTIREDIMessage CreateTestMessage(string messageNum)
		{
			AESTIREDIMessage testMessage = Factory.New<AESTIREDIMessage>();
			testMessage.EM_Status = AESTIREDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Receive;
			testMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			testMessage.EM_MessageNum = messageNum;

			return testMessage;
		}
		AESTIRIncomingMessageProcessor processor;

		sealed class AESTIRMessageProcessorForTest : AESTIRIncomingMessageProcessor
		{
			public string sortorder => GetProcessableMessagesOrder();
		}
	}
}
