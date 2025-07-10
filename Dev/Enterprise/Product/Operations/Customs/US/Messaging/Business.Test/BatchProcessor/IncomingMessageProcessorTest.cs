using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class IncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessorProcessesMessage()
		{
			IncomingMessageProcessorForTesting processor = new IncomingMessageProcessorForTesting();

			var message = CreateTestMessage("102");
			Factory.Save();
			processor.ExecuteBatch();
			var message2 = new BusinessObjectFactory().Load<CBPMessageForTesting>(message.PK);
			AssertEquals(EDIMessage.Status.Received, message2.EM_Status);
		}

		public void TestGetProcessableMessageOrderByTimeThenMessageNum()
		{
			TestCaseHelper.ClearTable("EdiMessage");
			var testMessage1 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			var testMessage2 = CreateTestMessage("200");
			var testMessage3 = CreateTestMessage("300");
			var testMessage4 = CreateTestMessage("100");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			var testMessage5 = CreateTestMessage("400");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			var testMessage6 = CreateTestMessage("500");
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			var testMessage7 = CreateTestMessage("600");
			Factory.Save();

			EDIMessage[] testMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("There should be 7 test messages in the file", 7, testMessages.Length);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "RCV");
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);

			ZQuery applicationCodeFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, CBPEDIInterchange.ApplicationCodeForTesting);

			filter.AddToFilter(applicationCodeFilter);
			filter.MaximumRows = 5;
			IncomingMessageProcessorForTesting processor = new IncomingMessageProcessorForTesting();
			filter.OrderBy = processor.sortorder;
			AssertEquals("message order by", "EM_SystemCreateTimeUtc, EM_MessageNum", filter.OrderBy);

			testMessages = Factory.Load<EDIMessage>(filter);
			AssertEquals("filter should have returned only earliest 5 messages", 5, testMessages.Length);
			Assert("Messages with same message number should be ordered by creation time", testMessages[0].EM_SystemCreateTimeUtc < testMessages[1].EM_SystemCreateTimeUtc);
			AssertEquals("first returned message", "100", testMessages[0].EM_MessageNum);
			AssertEquals("last returned message", "400", testMessages[4].EM_MessageNum);
		}

		public void TestOrderAndHint()
		{
			var processor = new IncomingMessageProcessorForTesting();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_SystemCreateTimeUtc, EM_MessageNum", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum", hint.IndexName);
		}

		CBPMessageForTesting CreateTestMessage(string messageNum)
		{
			CBPMessageForTesting testMessage = Factory.New<CBPMessageForTesting>();
			testMessage.EM_Status = EDIMessage.Status.Queued;
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			testMessage.EM_MessageNum = messageNum;

			return testMessage;
		}

		sealed class IncomingMessageProcessorForTesting : IncomingMessageProcessor
		{
			public string sortorder => GetProcessableMessagesOrder();

			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
				result.Add(new MessageProcessorFactoryForTesting(Logger, CBPEDIInterchange.ApplicationCodeForTesting));
				return result;
			}
		}
	}
}
