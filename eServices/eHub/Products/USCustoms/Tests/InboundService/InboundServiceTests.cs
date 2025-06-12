using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eServices.USCustoms.Common;
using CargoWise.eServices.USCustoms.InboundService;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using Common.Logging;
using IBM.WMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using ServiceBroker.Interface;
using Service = CargoWise.eServices.USCustoms.InboundService;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass()]
	public class InboundServiceTest : TestBase
	{
		#region RetrieveMQMessageTests

		[TestMethod()]
		public void TestRetrieveMQMessage_ShouldAlwaysRetryWhenNoMessageIsRetrieved()
		{
			SetUpRetrieveMQMessageTests(throwOnFirstMessage: true);

			AddMessageToQueue(MQC.MQMT_DATAGRAM, "A", "Single-segment message");

			var stream = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(1, mockMQQueue.Count);
			Assert.AreEqual(0, dlqMessages.Count);

			stream = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(1, mockMQQueue.Count);
			Assert.AreEqual(0, dlqMessages.Count);
		}

		[TestMethod()]
		public void TestRetrieveMQMessage_RetryCountShouldBeResetIfMissingSegmentsAreReceived()
		{
			SetUpRetrieveMQMessageTests();

			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Normal message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Normal message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "B", "Single-segment message");

			var stream1 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream1);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(3, mockMQQueue.Count);
			Assert.AreEqual(0, dlqMessages.Count);
			Assert.AreEqual(1, mockMQRetriever.RetryCount);

			AddMessageToQueue(MQC.MQMT_APPL_LAST, "B", "Single-segment message 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "A", "Normal message part 3");

			stream1 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream1.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(2, mockMQQueue.Count);
			Assert.AreEqual(0, dlqMessages.Count);
			Assert.AreEqual(0, mockMQRetriever.RetryCount);
		}

		[TestMethod()]
		public void TestRetrieveMQMessage_WrongMessageIdShouldBeSentToDLQ_PoisonMessageDoesNotHaveLAST()
		{
			SetUpRetrieveMQMessageTests();

			AddMessageToQueue(MQC.MQMT_APPL_LAST, "A", "Single-segment message");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Poison message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "B", "Poison message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "C", "Normal message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "C", "Normal message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "C", "Normal message part 3");

			var stream1 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream1.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(5, mockMQQueue.Count);
			Assert.AreEqual(0, dlqMessages.Count);

			var stream2 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream2);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(5, mockMQQueue.Count, "We want to retry the poison message.");
			Assert.AreEqual(0, dlqMessages.Count);

			stream2 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream2);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(4, mockMQQueue.Count, "Reached MaxRetryCount so we put 1 poison message to DLQ. Then we want to retry the other poison message.");
			Assert.AreEqual(1, dlqMessages.Count);
			Assert.AreEqual(1, dlqMessages[0].Length);

			var stream3 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream3.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(0, mockMQQueue.Count, "Reached MaxRetryCount for the other poison message so we put it to DLQ, then the 3 normal messages are retrieved.");
			Assert.AreEqual(2, dlqMessages.Count);
			Assert.AreEqual(1, dlqMessages[1].Length);
		}

		[TestMethod()]
		public void TestRetrieveMQMessage_WrongMessageIdShouldBeSentToDLQ_PoisonMessageHasLAST()
		{
			SetUpRetrieveMQMessageTests();

			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Normal message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "A", "Normal message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "A", "Poison LAST message");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Poison message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Poison message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "B", "Poison message part 3");
			AddMessageToQueue(MQC.MQMT_DATAGRAM, "B", "Single-segment message");

			var stream1 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream1.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(5, mockMQQueue.Count);
			Assert.AreEqual(0, dlqMessages.Count);

			var stream2 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream2.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(4, mockMQQueue.Count, "Retrieved a poison LAST message, which should fail after it gets to eHub.");
			Assert.AreEqual(0, dlqMessages.Count);

			var stream3 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream3);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(4, mockMQQueue.Count, "We want to retry the poison messages.");
			Assert.AreEqual(0, dlqMessages.Count);

			stream3 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream3.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(1, mockMQQueue.Count, "Put 2 poison messages to DLQ, then retrieved a poison LAST message, which should fail after it gets to eHub.");
			Assert.AreEqual(1, dlqMessages.Count);
			Assert.AreEqual(2, dlqMessages[0].Length);

			var stream4 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream4.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(0, mockMQQueue.Count);
			Assert.AreEqual(1, dlqMessages.Count);
		}

		[TestMethod()]
		public void TestRetrieveMQMessage_WrongMessageIdShouldBeSentToDLQ_SegmentsAreNotConsecutive()
		{
			SetUpRetrieveMQMessageTests();

			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Poison message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "C", "Normal message part 1");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "D", "Poison LAST message");
			AddMessageToQueue(MQC.MQMT_APPL_FIRST, "A", "Poison message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "C", "Normal message part 2");
			AddMessageToQueue(MQC.MQMT_APPL_LAST, "B", "Poison message part 3");
			AddMessageToQueue(MQC.MQMT_DATAGRAM, "D", "Single-segment message");

			var stream1 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsNull(stream1);
			mockMQRetriever.RollbackTransaction();

			Assert.AreEqual(7, mockMQQueue.Count, "Will retry the poison messages.");
			Assert.AreEqual(0, dlqMessages.Count);

			stream1 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream1.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(3, mockMQQueue.Count, "Put 2 poison messages to DLQ, then retrieved 2 normal messages.");
			Assert.AreEqual(1, dlqMessages.Count);
			Assert.AreEqual(2, dlqMessages[0].Length);

			var stream2 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream2.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(2, mockMQQueue.Count, "Retrieved a poison LAST message, which should fail after it gets to eHub.");
			Assert.AreEqual(1, dlqMessages.Count);

			var stream3 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream3.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(1, mockMQQueue.Count, "Retrieved a poison LAST message, which should fail after it gets to eHub.");
			Assert.AreEqual(1, dlqMessages.Count);

			var stream4 = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			Assert.IsTrue(stream4.Length > 0);
			mockMQRetriever.CommitTransaction();

			Assert.AreEqual(0, mockMQQueue.Count, "Retrieved a single-segment message.");
			Assert.AreEqual(1, dlqMessages.Count);
		}

		void AddMessageToQueue(int messageType, string messageId, string messageContent)
		{
			var message = new MQMessage
			{
				Format = MQC.MQFMT_STRING,
				MessageType = messageType,
				MessageId = Encoding.UTF8.GetBytes(messageId)
			};
			message.WriteString(messageContent);
			message.DataOffset = 0;
			mockMQQueue.Add(message);
		}

		MQMessageRetriever mockMQRetriever;
		List<MQMessage> mockMQQueue;
		List<int> indexesToRemove;
		IMQConfiguration mockMQConfig;
		IMQQueueWrapper mockQueueWrapper;

		void SetUpRetrieveMQMessageTests(bool throwOnFirstMessage = false)
		{
			dlqMessages = new List<MQMessage[]>();
			mockMQQueue = new List<MQMessage>();
			indexesToRemove = new List<int>();

			mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			mockConfig.Stub(x => x.IsProduction).Return(false);
			mockLogger = MockRepository.GenerateMock<ILog>();
			mockMQConfig = MockRepository.GenerateMock<IMQConfiguration>();
			mockSqlConnection = MockRepository.GenerateMock<SqlConnection>();

			mockDeadMsgSender = MockRepository.GenerateMock<IMQDeadMessageSender>();
			mockDeadMsgSender.Stub(x => x.Send(null, false)).IgnoreArguments()
				.Do(new Action<MQMessage[], bool>((messages, isProduction) => { dlqMessages.Add(messages); }));

			mockQueueWrapper = MockRepository.GenerateMock<IMQQueueWrapper>();
			mockQueueWrapper.Stub(x => x.Get(null, null)).IgnoreArguments()
				.Do(new Action<MQMessage, MQGetMessageOptions>((message, options) =>
				{
					if (throwOnFirstMessage)
					{
						throw new MQException(MQC.MQCC_FAILED, MQC.MQRC_NO_MSG_AVAILABLE);
					}

					var index = 0;
					if (options.WaitInterval == 120000)
					{
						var lastIndex = indexesToRemove.Last();
						if (lastIndex == mockMQQueue.Count - 1)
						{
							throw new MQException(MQC.MQCC_FAILED, MQC.MQRC_NO_MSG_AVAILABLE);
						}

						index = mockMQQueue.FindIndex(lastIndex + 1, m => m.MessageId.SequenceEqual(message.MessageId));
						if (index == -1)
						{
							throw new MQException(MQC.MQCC_FAILED, MQC.MQRC_NO_MSG_AVAILABLE);
						}
					}

					var messageRetrieved = mockMQQueue[index];
					message.Format = messageRetrieved.Format;
					message.MessageType = messageRetrieved.MessageType;
					message.MessageId = messageRetrieved.MessageId;
					message.WriteString(messageRetrieved.ReadString(messageRetrieved.MessageLength));
					message.DataOffset = 0;
					messageRetrieved.DataOffset = 0;

					indexesToRemove.Add(index);
				}));

			mockMQRetriever = MockRepository.GeneratePartialMock<MQMessageRetriever>(mockMQConfig, mockLogger, mockLogger, "messageType");
			mockMQRetriever.Stub(x => x.BeginTransaction()).Do(new Action(() => { mockMQRetriever.queue = mockQueueWrapper; }));
			mockMQRetriever.Stub(x => x.CommitTransaction()).Do(new Action(() =>
			{
				for (var i = 0; i < indexesToRemove.Count; i++)
				{
					mockMQQueue.RemoveAt(indexesToRemove[i] - i);
				}

				indexesToRemove.Clear();
			}));
			mockMQRetriever.Stub(x => x.RollbackTransaction()).Do(new Action(() => { indexesToRemove.Clear(); }));

			mockMQRetriever.MaxRetryCount = 1;
			mockMQRetriever.BeginTransaction();

			mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			mockInboundService.Stub(x => x.SendErrorToEHub(null, null)).IgnoreArguments();
			mockInboundService.Stub(x => x.GetMQDeadMessageSender()).Return(mockDeadMsgSender);
			mockInboundService.Stub(x => x.Config).Return(mockConfig);
			mockInboundService.Stub(x => x.Logger).Return(mockLogger);
			mockInboundService.Stub(x => x.MessageRetriever).Return(mockMQRetriever);
		}

		#endregion

		#region SericeTaskTests
		[TestMethod()]
		public void InboundService_ServiceTask()
		{
			logEntries = new List<string>();
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			mockConfig.Stub(x => x.PullIntervalInSecond).Return(1);
			mockInboundService.Stub(x => x.ResolveDependencies()).Do(new Action(() =>
			{
				mockInboundService.Stub(x => x.Config).Return(mockConfig);
				mockInboundService.Stub(x => x.Logger).Return(mockLogger);
			}));
			mockInboundService.Stub(x => x.ProcessMessages());

			mockInboundService.StartTask(null);
			Thread.Sleep(1500);
			mockInboundService.StopTask();

			mockInboundService.AssertWasCalled(x => x.ProcessMessages(), x => x.Repeat.Twice());
			Assert.AreEqual(TaskStatus.RanToCompletion, mockInboundService.Task.Status);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Starting service 'InboundService'",
				"[DBG] Starting pull cycle",
				"[DBG] Starting pull cycle",
				"[DBG] Stopping service 'InboundService'",
			}, logEntries);
		}

		//[TestMethod()] -- This is failing intermittently on gated build.
		public void InboundService_ServiceTask_StartException()
		{
			logEntries = new List<string>();
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var startException = new Exception("START EXCEPTION");
			mockConfig.Stub(x => x.PullIntervalInSecond).Return(1);
			mockInboundService.Stub(x => x.ResolveDependencies()).Do(new Action(() =>
			{
				mockInboundService.Stub(x => x.Logger).Return(mockLogger);
				throw startException;
			}));

			mockInboundService.StartTask(null);
			while (!mockInboundService.Task.IsCompleted) { Thread.Sleep(100); }
			Thread.Sleep(100);

			mockInboundService.AssertWasNotCalled(x => x.ProcessMessages());
			mockInboundService.AssertWasCalled(x => x.WriteStartupErrorLogEntry("Error running service task", startException));
			Assert.AreEqual(TaskStatus.Faulted, mockInboundService.Task.Status);
			Assert.AreEqual(1, logEntries.Count);
			Assert.IsTrue(logEntries[0].StartsWith("[ERR] Error running service task\r\nSystem.Exception: START EXCEPTION"));
		}

		[TestMethod()]
		public void InboundService_ServiceTask_ProcessException()
		{
			logEntries = new List<string>();
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var processException = new Exception("LOAD EXCEPTION");
			mockConfig.Stub(x => x.PullIntervalInSecond).Return(1);
			mockInboundService.Stub(x => x.ResolveDependencies()).Do(new Action(() =>
			{
				mockInboundService.Stub(x => x.Config).Return(mockConfig);
				mockInboundService.Stub(x => x.Logger).Return(mockLogger);
			}));
			mockInboundService.Stub(x => x.ProcessMessages()).Throw(processException).Repeat.Once();
			mockInboundService.Stub(x => x.ProcessMessages());

			mockInboundService.StartTask(null);
			Thread.Sleep(1500);
			mockInboundService.StopTask();

			mockInboundService.AssertWasCalled(x => x.ProcessMessages(), x => x.Repeat.Twice());
			Assert.AreEqual(TaskStatus.RanToCompletion, mockInboundService.Task.Status);
			Assert.AreEqual(5, logEntries.Count);
			Assert.AreEqual("[DBG] Starting service 'InboundService'", logEntries[0]);
			Assert.AreEqual("[DBG] Starting pull cycle", logEntries[1]);
			Assert.IsTrue(logEntries[2].StartsWith("[ERR] Exception happened in current pull cycle. Waiting for the next pull cycle.\r\nSystem.Exception: LOAD EXCEPTION"));
			Assert.AreEqual("[DBG] Starting pull cycle", logEntries[3]);
			Assert.AreEqual("[DBG] Stopping service 'InboundService'", logEntries[4]);
		}

		[TestMethod()]
		public void InboundService_GetCommandLineOptions()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			string actualLogMessage = null;
			mockInboundService.Stub(x => x.Logger).Return(mockLogger);
			mockLogger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Do(new Action<string, object[]>((m, o) => actualLogMessage = String.Format(m, o)));
			Dictionary<string, string> cmdLineOpts;

			cmdLineOpts = mockInboundService.GetCommandLineOptions(new List<string>());
			CollectionAssert.AreEqual(new Dictionary<string, string>(), cmdLineOpts);

			cmdLineOpts = mockInboundService.GetCommandLineOptions(new List<string>() { "-inputpath", "INPUTPATH" });
			CollectionAssert.AreEqual(new Dictionary<string, string>() { { "-inputpath", "INPUTPATH" } }, cmdLineOpts);

			cmdLineOpts = mockInboundService.GetCommandLineOptions(new List<string>() { "-inputpath" });
			CollectionAssert.AreEqual(new Dictionary<string, string>(), cmdLineOpts);
			Assert.AreEqual("Missing value for command line option '-inputpath'. Options will be ignored.", actualLogMessage.Remove(actualLogMessage.IndexOf("\r\n")));

			cmdLineOpts = mockInboundService.GetCommandLineOptions(new List<string>() { "-inputpath", "-anotheropt" });
			CollectionAssert.AreEqual(new Dictionary<string, string>(), cmdLineOpts);
			Assert.AreEqual("Missing value for command line option '-inputpath'. Options will be ignored.", actualLogMessage.Remove(actualLogMessage.IndexOf("\r\n")));

			cmdLineOpts = mockInboundService.GetCommandLineOptions(new List<string>() { "-invalidopt", "INVALID" });
			CollectionAssert.AreEqual(new Dictionary<string, string>(), cmdLineOpts);
			Assert.AreEqual("Invalid command line option '-invalidopt'. Options will be ignored.", actualLogMessage.Remove(actualLogMessage.IndexOf("\r\n")));
		}
		#endregion SericeTaskTests

		#region ProcessMessagesTests
		[TestMethod()]
		public void InboundService_ProcessMessages_SendToEHub()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream>(new[] { message1Stream, message2Stream });
			mockReceivedMessageLog.Stub(x => x.IsInfoEnabled).Return(true);
			mockConfig.Stub(x => x.SendCopiesToTest).Return(true);
			CreateDefaultTestObjectStubs(isProd:true);

			mockInboundService.ProcessMessages();

			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new Stream[] { }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, sentToEHubStreams);
			CollectionAssert.AreEqual(new Stream[] { }, sentToEHubUncommitted);
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, sentCopyMessageToTestStreams);
			CollectionAssert.AreEqual(new Stream[] { }, sentToTestUncommitted);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] Retrieved message from MQ",
				"[DBG] Began a new eHub transaction",
				"[DBG] Sent message from //cargowise.com/eServices/USCustoms/eHubInboxService to //cargowise.com/eServices/USCustoms/InboundMessageProcessingService",
				"[DBG] Committed eHub transaction",
				"[DBG] Committed MQ transaction",
				"[INF] Processed message successfully",
				"[DBG] Began a new eHub transaction to send message copy to test",
				"[DBG] Sent message copy to test",
				"[DBG] Committed eHub transaction",
				"[DBG] Began a new MQ transaction",
				"[DBG] Retrieved message from MQ",
				"[DBG] Began a new eHub transaction",
				"[DBG] Sent message from //cargowise.com/eServices/USCustoms/eHubInboxService to //cargowise.com/eServices/USCustoms/InboundMessageProcessingService",
				"[DBG] Committed eHub transaction",
				"[DBG] Committed MQ transaction",
				"[INF] Processed message successfully",
				"[DBG] Began a new eHub transaction to send message copy to test",
				"[DBG] Sent message copy to test",
				"[DBG] Committed eHub transaction",
				"[DBG] Began a new MQ transaction",
				"[DBG] No message retrieved from MQ",
				"[DBG] Rollbacked MQ transaction",
				"[DBG] No new messages.",
			}, logEntries);
		}

		[TestMethod]
		public void InboundService_IsNotProd_ProcessMessages_SendToTest()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream>(new[] { message1Stream, message2Stream });
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new Stream[] { }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, sentToTestStreams);
			CollectionAssert.AreEqual(new Stream[] { }, sentToTestUncommitted);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_RetrieveError()
		{
			InitialiseCommonTestObjects();
			mockInboundService.Stub(x => x.RetrieveMQMessage(mockSqlConnection)).Throw(new Exception("RETRIEVE EXCEPTION"));
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new string[] { "System.Exception: RETRIEVE EXCEPTION" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] Rollbacked MQ transaction",
				"[ERR] MessageType: ABI, IsProduction: False. [Exception:] RETRIEVE EXCEPTION",
			}, logEntries);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_SendToEHubError()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			inputStreams = new List<Stream>(new[] { message1Stream });
			mockInboundService.Stub(x => x.SendMessageToEHub(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction)))
				.Callback(new Func<Message, SqlConnection, SqlTransaction, bool>((m, c, t) => m.Body == message1Stream)).Throw(new Exception("SEND EXCEPTION"));
			CreateDefaultTestObjectStubs(isProd: true);

			mockInboundService.ProcessMessages();

			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new Stream[] { }, sentToEHubStreams);
			CollectionAssert.AreEqual(new Stream[] { }, sentToEHubUncommitted);
			CollectionAssert.AreEqual(new string[] { "System.Exception: SEND EXCEPTION" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] Retrieved message from MQ",
				"[DBG] Began a new eHub transaction",
				"[ERR] Failed to process message.  [Exception:] SEND EXCEPTION",
				"[DBG] Rollbacked MQ transaction",
				"[ERR] MessageType: ABI, IsProduction: False. [Exception:] SEND EXCEPTION",
			}, logEntries);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_SendCopyMessageToTestError()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			inputStreams = new List<Stream>(new[] { message1Stream });
			mockInboundService.Stub(x => x.SendCopyMessageToTest(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction)))
				.Callback(new Func<Message, SqlConnection, SqlTransaction, bool>((m, c, t) => m.Body == message1Stream)).Throw(new Exception("SEND EXCEPTION"));
			mockConfig.Stub(x => x.SendCopiesToTest).Return(true);
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new Stream[] { }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, sentToTestStreams);
			CollectionAssert.AreEqual(new Stream[] { }, sentCopyMessageToTestStreams);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] Retrieved message from MQ",
				"[DBG] Began a new eHub transaction",
				"[DBG] Sent message from //cargowise.com/eServices/USCustoms/eHubInboxServiceTest to //cargowise.com/eServices/USCustoms/InboundMessageProcessingServiceTest",
				"[DBG] Committed eHub transaction",
				"[DBG] Committed MQ transaction",
				"[INF] Processed message successfully",
				"[DBG] Began a new eHub transaction to send message copy to test",
				"[WRN] Failed to send message copy to test. [Exception:] SEND EXCEPTION",
				"[DBG] Began a new MQ transaction",
				"[DBG] No message retrieved from MQ",
				"[DBG] Rollbacked MQ transaction",
				"[DBG] No new messages.",
			}, logEntries);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_StopEventSet_1()
		{
			InitialiseCommonTestObjects();
			mockSqlConnection.Stub(x => x.Open()).Do(new Action(() => flagStopEventSet = true));
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			mockInboundService.AssertWasNotCalled(x => x.RetrieveMQMessage(mockSqlConnection));
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Message retrieval stopped."
			}, logEntries);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_StopEventSet_2()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { flagStopEventSet = true; return message1Stream; }));
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			mockInboundService.AssertWasNotCalled(x => x.SendMessageToEHub(null, null, null), x => x.IgnoreArguments());
			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] No message retrieved from MQ",
				"[DBG] Rollbacked MQ transaction",
				"[DBG] Message retrieval stopped.",
			}, logEntries);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_StopEventSet_3()
		{
			InitialiseCommonTestObjects();
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { flagStopEventSet = true; throw new Exception("RETRIEVE ERROR"); }));
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			assertMQTranBegun();
			assertSqlTranNotBegun();
			assertConnectionClosed();
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] Rollbacked MQ transaction",
				"[ERR] MessageType: ABI, IsProduction: False. [Exception:] RETRIEVE ERROR",
			}, logEntries);
		}

		[TestMethod()]
		public void InboundService_ProcessMessages_StopEventSet_4()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			mockInboundService.Stub(x => x.SendMessageToEHub(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction)))
				.Do(new Func<Message, SqlConnection, SqlTransaction, Tuple<string, string>>((m, c, t) =>
				{
					assertSqlTranBegun();
					sentToTestStreams.Add(m.Body);
					sentToTestUncommitted.Add(m.Body);
					flagStopEventSet = true;

					return new Tuple<string, string>("ServiceBrokerConstants.eHubInboxServiceTestConstants.ServiceName", "ServiceBrokerConstants.InboundMessageProcessingServiceTestConstants.ServiceName");
				}));
			CreateDefaultTestObjectStubs();

			mockInboundService.ProcessMessages();

			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, sentToTestStreams);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[DBG] Began a new MQ transaction",
				"[DBG] Retrieved message from MQ",
				"[DBG] Began a new eHub transaction",
				"[DBG] Sent message from ServiceBrokerConstants.eHubInboxServiceTestConstants.ServiceName to ServiceBrokerConstants.InboundMessageProcessingServiceTestConstants.ServiceName",
				"[DBG] Committed eHub transaction",
				"[DBG] Committed MQ transaction",
				"[INF] Processed message successfully",
				"[DBG] Message retrieval stopped.",
			}, logEntries);
		}
		#endregion ProcessMessagesTests

		#region SendErrorToEHubTests
		[TestMethod()]
		public void InboundService_SendErrorToEHub()
		{
			InitialiseCommonTestObjects();
			flagConnOpen = true;
			CreateDefaultTestObjectStubs(stubSendErrorToEHub: false, isProd: true);

			mockInboundService.SendErrorToEHub(mockSqlConnection, "ERROR MESSAGE 1");

			assertSqlTranNotBegun();
			CollectionAssert.AreEqual(new string[] { "<ErrorNotification ErrorDescription=\"ERROR MESSAGE 1\" />" }, sentErrorStreams.Select(s => new StreamReader(s).ReadToEnd()).ToArray());
			CollectionAssert.AreEqual(new string[] { }, sentErrorUncommitted.Select(s => new StreamReader(s).ReadToEnd()).ToArray());
			CollectionAssert.AreEqual(new string[] { }, logEntries);
		}

		[TestMethod()]
		public void InboundService_SendErrorToEHub_Fail()
		{
			InitialiseCommonTestObjects();
			flagConnOpen = true;
			mockInboundService.Stub(x => x.SendErrorMessageToEHub(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction)))
				.Throw(new ApplicationException("SEND ERROR")).Repeat.Once();
			CreateDefaultTestObjectStubs(stubSendErrorToEHub: false);

			mockInboundService.SendErrorToEHub(mockSqlConnection, "ERROR MESSAGE 2");

			assertSqlTranNotBegun();
			CollectionAssert.AreEqual(new string[] { }, sentErrorStreams.Select(s => new StreamReader(s).ReadToEnd()).ToArray());
			CollectionAssert.AreEqual(new string[] { }, sentErrorUncommitted.Select(s => new StreamReader(s).ReadToEnd()).ToArray());
			CollectionAssert.AreEqual(new[] {
				"[ERR] Error sending error to eHub. [Exception:] SEND ERROR",
			}, logEntries);
		}
		#endregion SendErrorToEHubTests

		#region CommonTestObjects
		IMQMessageRetriever mockRetriever;
		SqlConnection mockSqlConnection;
		SqlTransaction mockSqlTransaction;
		IInboundServiceConfiguration mockConfig;
		IMQDeadMessageSender mockDeadMsgSender;
		Service.InboundService mockInboundService;
		ILog mockLogger;
		ILog mockReceivedMessageLog;
		bool flagStopEventSet;
		bool flagMQTranBegun;
		Action assertMQTranBegun;
		Action assertMQTranNotBegun;
		bool flagConnOpen;
		Action assertConnectionOpen;
		Action assertConnectionClosed;
		bool flagSqlTranBegun;
		Action assertSqlTranBegun;
		Action assertSqlTranNotBegun;
		List<string> logEntries;
		List<string> receivedMessagesLog;
		List<Stream> inputStreams;
		List<Stream> inputReadUncommitted;
		List<Stream> sentToEHubStreams;
		List<Stream> sentToEHubUncommitted;
		List<Stream> sentToTestStreams;
		List<Stream> sentToTestUncommitted;
		List<Stream> sentCopyMessageToTestStreams;
		List<Stream> sentCopyMessageToTestUncommitted;
		List<String> sentErrorMessages;
		List<Stream> sentErrorStreams;
		List<Stream> sentErrorUncommitted;
		List<Stream> sentErrorToTestStreams;
		List<Stream> sentErrorToTestUncommitted;
		List<MQMessage[]> dlqMessages;
		#endregion CommonTestObjects

		#region InitialiseCommonTestObjects
		void InitialiseCommonTestObjects()
		{
			logEntries = new List<string>();
			receivedMessagesLog = new List<string>();
			inputStreams = new List<Stream>();
			inputReadUncommitted = new List<Stream>();
			sentToEHubStreams = new List<Stream>();
			sentToEHubUncommitted = new List<Stream>();
			sentToTestStreams = new List<Stream>();
			sentToTestUncommitted = new List<Stream>();
			sentCopyMessageToTestStreams = new List<Stream>();
			sentCopyMessageToTestUncommitted = new List<Stream>();
			sentErrorMessages = new List<string>();
			sentErrorStreams = new List<Stream>();
			sentErrorUncommitted = new List<Stream>();
			sentErrorToTestStreams = new List<Stream>();
			sentErrorToTestUncommitted = new List<Stream>();
			dlqMessages = new List<MQMessage[]>();
			mockLogger = MockRepository.GenerateMock<ILog>();
			mockRetriever = MockRepository.GenerateMock<IMQMessageRetriever>();
			mockSqlConnection = MockRepository.GenerateMock<SqlConnection>();
			mockSqlTransaction = MockRepository.GenerateMock<SqlTransaction>();
			mockReceivedMessageLog = MockRepository.GenerateMock<ILog>();
			mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			mockDeadMsgSender = MockRepository.GenerateMock<IMQDeadMessageSender>();
			mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			flagStopEventSet = false;
			flagMQTranBegun = false;
			flagConnOpen = false;
			flagSqlTranBegun = false;
			assertMQTranBegun = new Action(() => Assert.IsTrue(flagMQTranBegun, "MQ transaction not begun."));
			assertMQTranNotBegun = new Action(() => Assert.IsFalse(flagMQTranBegun, "MQ transaction already begun."));
			assertConnectionOpen = new Action(() => Assert.IsTrue(flagConnOpen, "Connection is not open."));
			assertConnectionClosed = new Action(() => Assert.IsFalse(flagConnOpen, "Connection already open."));
			assertSqlTranBegun = new Action(() => Assert.IsTrue(flagSqlTranBegun, "SQL transaction not begun."));
			assertSqlTranNotBegun = new Action(() => Assert.IsFalse(flagSqlTranBegun, "SQL transaction already begun."));
		}

		void CreateDefaultTestObjectStubs(bool stubSendErrorToEHub = true, bool isProd = false)
		{
			mockConfig.Stub(x => x.MessageType).Return("ABI");
			mockInboundService.GetInboundConfiguration = (x) => (new CargoWise.eServices.USCustoms.MQConfigurationABIACE.MQABIACEConfiguration(isProd));
			var initialServiceName = isProd
				? ServiceBrokerConstants.eHubInboxServiceConstants.ServiceName
				: ServiceBrokerConstants.eHubInboxServiceTestConstants.ServiceName;
			var toServiceName = isProd
				? ServiceBrokerConstants.InboundMessageProcessingServiceConstants.ServiceName
				: ServiceBrokerConstants.InboundMessageProcessingServiceTestConstants.ServiceName;

			InitialiseLoggerStubs(mockLogger, logEntries);
			mockReceivedMessageLog.Stub(x => x.Info(Arg<object>.Is.Anything)).Do(new Action<object>(m => receivedMessagesLog.Add((string)m)));
			mockRetriever.Stub(x => x.BeginTransaction()).Do(new Action(() =>
			{
				flagMQTranBegun = true;
			}));
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => RetrieveMessageFromList()));
			mockRetriever.Stub(x => x.CommitTransaction()).Do(new Action(() =>
			{
				assertMQTranBegun();
				inputReadUncommitted.ForEach(s => inputStreams.Remove(s));
				inputReadUncommitted.Clear();
			}));
			mockRetriever.Stub(x => x.RollbackTransaction()).Do(new Action(() =>
			{
				assertMQTranBegun();
				inputReadUncommitted.Clear();
			}));
			mockInboundService.Stub(x => x.SendMessageToEHub(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction))).Do(new Func<Message, SqlConnection, SqlTransaction, Tuple<string, string>>((m, c, t) =>
			{
				assertSqlTranBegun();

				if (isProd)
				{
					sentToEHubStreams.Add(m.Body);
					sentToEHubUncommitted.Add(m.Body);
				}
				else
				{
					sentToTestStreams.Add(m.Body);
					sentToTestUncommitted.Add(m.Body);
				}

				return new Tuple<string, string>(initialServiceName, toServiceName);
			}));
			mockInboundService.Stub(x => x.SendCopyMessageToTest(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction))).Do(new Action<Message, SqlConnection, SqlTransaction>((m, c, t) =>
			{
				assertSqlTranBegun();
				sentCopyMessageToTestStreams.Add(m.Body);
				sentCopyMessageToTestUncommitted.Add(m.Body);

			}));
			if (stubSendErrorToEHub)
				mockInboundService.Stub(x => x.SendErrorToEHub(Arg.Is(mockSqlConnection), Arg<string>.Is.Anything)).Do(new Action<SqlConnection, string>((s, d) =>
				{
					assertSqlTranNotBegun();
					using (var sr = new StringReader(d))
					{
						string message = sr.ReadLine();
						sentErrorMessages.Add(message);
					}
				}));
			mockInboundService.Stub(x => x.SendErrorMessageToEHub(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction))).Do(new Action<Message, SqlConnection, SqlTransaction>((m, c, t) =>
			{
				var errorStream = new MemoryStream();
				m.Body.CopyTo(errorStream);

				if (isProd)
				{
					sentErrorStreams.Add(errorStream);
					sentErrorUncommitted.Add(errorStream);
				}
				else
				{
					sentErrorToTestStreams.Add(errorStream);
					sentErrorToTestUncommitted.Add(errorStream);
				}

				errorStream.Position = 0;
			}));
			mockSqlConnection.Stub(x => x.Open()).Do(new Action(() =>
			{
				assertConnectionClosed();
				flagConnOpen = true;
			}));
			mockSqlConnection.Stub(x => x.BeginTransaction()).Do(new Func<SqlTransaction>(() =>
			{
				assertConnectionOpen();
				assertSqlTranNotBegun();
				flagSqlTranBegun = true;
				return mockSqlTransaction;
			}));
			mockSqlConnection.Stub<IDisposable>(x => x.Dispose()).Do(new Action(() =>
			{
				assertSqlTranNotBegun();
				flagConnOpen = false;
			}));
			mockSqlTransaction.Stub(x => x.Commit()).Do(new Action(() =>
			{
				assertSqlTranBegun();
				sentToEHubUncommitted.Clear();
				sentToTestUncommitted.Clear();
				sentCopyMessageToTestUncommitted.Clear();
				sentErrorUncommitted.Clear();
				sentErrorToTestStreams.Clear();
				flagSqlTranBegun = false;
			}));
			mockSqlTransaction.Stub(x => x.Rollback()).Do(new Action(() =>
			{
				assertSqlTranBegun();
				mockSqlTransaction.Dispose();
			}));
			mockSqlTransaction.Stub<IDisposable>(x => x.Dispose()).Do(new Action(() =>
			{
				sentToEHubUncommitted.ForEach(s => sentToEHubStreams.Remove(s));
				sentToEHubUncommitted.Clear();
				sentToTestUncommitted.ForEach(s => sentToTestStreams.Remove(s));
				sentToTestUncommitted.Clear();
				sentCopyMessageToTestUncommitted.ForEach(s => sentCopyMessageToTestStreams.Remove(s));
				sentCopyMessageToTestUncommitted.Clear();
				sentErrorUncommitted.ForEach(s => sentErrorStreams.Remove(s));
				sentErrorUncommitted.Clear();
				sentErrorToTestUncommitted.ForEach(s => sentErrorToTestStreams.Remove(s));
				sentErrorToTestUncommitted.Clear();
				flagSqlTranBegun = false;
			}));
			mockDeadMsgSender.Stub(x => x.Send(Arg<MQMessage[]>.Is.Anything, Arg<bool>.Is.Anything)).Do(new Action<MQMessage[], bool>((m, p) => dlqMessages.Add(m)));
			mockInboundService.Stub(x => x.MessageRetriever).Return(mockRetriever);
			mockInboundService.Stub(x => x.Config).Return(mockConfig);
			mockInboundService.Stub(x => x.Logger).Return(mockLogger);
			mockInboundService.Stub(x => x.ReceivedMessageLog).Return(mockReceivedMessageLog);
			mockInboundService.Stub(x => x.StopEventSet).Do(new Func<bool>(() => flagStopEventSet));
			mockInboundService.Stub(x => x.GetConnection()).Return(mockSqlConnection);
			mockInboundService.Stub(x => x.GetMQDeadMessageSender()).Return(mockDeadMsgSender);
		}

		Stream RetrieveMessageFromList()
		{
			assertMQTranBegun();
			var currentStream = inputStreams.FirstOrDefault();
			if (currentStream != null) inputReadUncommitted.Add(currentStream);
			return currentStream;
		}
		#endregion InitialiseCommonTestObjects
	}
}
