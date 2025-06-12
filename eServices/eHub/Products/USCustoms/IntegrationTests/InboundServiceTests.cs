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
using CargoWise.eServices.USCustoms.MQConfiguration;
using CargoWise.eServices.USCustoms.Tests;
using Common.Logging;
using IBM.WMQ;
using NUnit.Framework;
using Rhino.Mocks;
using ServiceBroker.Interface;
using Service = CargoWise.eServices.USCustoms.InboundService;

namespace CargoWise.eServices.USCustoms.IntegrationTests
{
	[TestFixture]
	public class InboundServiceTests : TestBase
	{
		#region RetrieveMQMessageTests
		[Test]
		public void InboundService_RetrieveMQMessage()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			CreateDefaultTestObjectStubs();

			mockRetriever.BeginTransaction();
			var result1Stream = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			var input1Streams = inputStreams.ToArray();
			var input1ReadUncommitted = inputReadUncommitted.ToArray();
			mockRetriever.CommitTransaction();

			mockRetriever.BeginTransaction();
			var result2Stream = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			var input2Streams = inputStreams.ToArray();
			var input2ReadUncommitted = inputReadUncommitted.ToArray();
			mockRetriever.CommitTransaction();

			mockRetriever.BeginTransaction();
			var result3Stream = mockInboundService.RetrieveMQMessage(mockSqlConnection);
			var input3Streams = inputStreams.ToArray();
			var input3ReadUncommitted = inputReadUncommitted.ToArray();

			assertMQTranBegun();
			Assert.AreSame(message1Stream, result1Stream);
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, input1Streams);
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, input1ReadUncommitted);
			Assert.AreSame(message2Stream, result2Stream);
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, input2Streams);
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, input2ReadUncommitted);
			Assert.IsNull(result3Stream);
			CollectionAssert.AreEqual(new Stream[] { }, inputStreams);
			CollectionAssert.AreEqual(new string[] { }, input3ReadUncommitted);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new string[0], logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_MQPoisonMessage()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			var mqMessageA = new MQMessage();
			var mqMessageParts = new[] { mqMessageA };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new MQException(0, 2110); })).Repeat.Once();
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(RetrieveMessageFromList)).Repeat.Once();
			mockRetriever.Stub(x => x.RetrieveSafe()).Do(new Action(() => { RetrieveMessageFromList(); throw new FailedMQMessageException("FAILED MQ EXCEPTION", mqMessageParts); }));
			CreateDefaultTestObjectStubs();

			var resultStream = mockInboundService.RetrieveMQMessage(mockSqlConnection);

			assertMQTranBegun();
			Assert.AreSame(message2Stream, resultStream);
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputReadUncommitted);
			CollectionAssert.AreEqual(new[] { mqMessageParts }, dlqMessages);
			CollectionAssert.AreEqual(new[] { "CompCode: 0, Reason: 2110" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] MQException ReasonCode: 2110, Reason: 2110, CompCode: 0, CompletionCode: 0. Message: 2110",
				"[WRN] Error code was in PoisonMessageMQErrorCode configuration list. Error details  Code 2110 - 2110.",
				"[WRN] Rollbacked MQ transaction",
				"[WRN] Began a new MQ transaction",
				"[WRN] Retrieved message from MQ using RetrieveSafe()",
				"[WRN] Error during message pulling from MQ. 1 messages will be moved to dead letter queue. [Exception:] FAILED MQ EXCEPTION",
				"[WRN] Message(s) moved to dead letter queue.",
				"[WRN] Committed MQ transaction",
				"[WRN] Began a new MQ transaction",
			}, logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_MQPoisonMessage_NoMsgs()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new MQException(0, 2110); })).Repeat.Once();
			CreateDefaultTestObjectStubs();

			AssertException<MQException>(() => mockInboundService.RetrieveMQMessage(mockSqlConnection), x => x.Message == "2110",
				"Expected exception of type 'MQException' with message containing '2110'");

			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] MQException ReasonCode: 2110, Reason: 2110, CompCode: 0, CompletionCode: 0. Message: 2110",
				"[WRN] Error code was in PoisonMessageMQErrorCode configuration list. Error details  Code 2110 - 2110.",
				"[WRN] Rollbacked MQ transaction",
				"[WRN] Began a new MQ transaction",
				"[WRN] Failed to retrieve message from MQ using RetrieveSafe()",
			}, logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_MQPoisonMessage_DlqFailure()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			var mqMessageA = CreateMQMessage("MESSAGEA");
			var mqMessageB = CreateMQMessage("MESSAGEB");
			var mqMessageParts = new[] { mqMessageA, mqMessageB };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new MQException(0, 2110); })).Repeat.Once();
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(RetrieveMessageFromList)).Repeat.Once();
			mockRetriever.Stub(x => x.RetrieveSafe()).Do(new Action(() => { RetrieveMessageFromList(); throw new FailedMQMessageException("FAILED MQ EXCEPTION", mqMessageParts); }));
			mockDeadMsgSender.Stub(x => x.Send(Arg.Is(mqMessageParts), Arg.Is(mockConfig.IsProduction))).Throw(new Exception("DLQ EXCEPTION"));
			CreateDefaultTestObjectStubs();

			var resultStream = mockInboundService.RetrieveMQMessage(mockSqlConnection);

			Assert.IsNull(resultStream);
			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new[] { "CompCode: 0, Reason: 2110" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] MQException ReasonCode: 2110, Reason: 2110, CompCode: 0, CompletionCode: 0. Message: 2110",
				"[WRN] Error code was in PoisonMessageMQErrorCode configuration list. Error details  Code 2110 - 2110.",
				"[WRN] Rollbacked MQ transaction",
				"[WRN] Began a new MQ transaction",
				"[WRN] Retrieved message from MQ using RetrieveSafe()",
				"[WRN] Error during message pulling from MQ. 2 messages will be moved to dead letter queue. [Exception:] FAILED MQ EXCEPTION",
				"[WRN] Unable send 2 to dead letter queue. Message contents follow. [Exception:] DLQ EXCEPTION",
				"[WRN] MESSAGEA",
				"[WRN] MESSAGEB",
				"[WRN] Committed MQ transaction",
				"[WRN] Began a new MQ transaction",
			}, logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_MQPoisonMessage_DlqFailure_SaveFailure()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			var mqMessageA = CreateMQMessage("MESSAGEA");
			var mqMessageB = CreateMQMessage("MESSAGEB");
			var mqMessageParts = new[] { mqMessageA, mqMessageB };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new MQException(0, 2110); })).Repeat.Once();
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(RetrieveMessageFromList)).Repeat.Once();
			mockRetriever.Stub(x => x.RetrieveSafe()).Do(new Action(() => { RetrieveMessageFromList(); throw new FailedMQMessageException("FAILED MQ EXCEPTION", mqMessageParts); }));
			mockInboundService.Stub(x => x.GetMQMessageText(mqMessageB)).Throw(new ApplicationException("SAVE MQ ERROR"));
			mockDeadMsgSender.Stub(x => x.Send(Arg.Is(mqMessageParts), Arg.Is(mockConfig.IsProduction))).Throw(new Exception("DLQ EXCEPTION"));
			CreateDefaultTestObjectStubs();

			AssertException<ApplicationException>(() => mockInboundService.RetrieveMQMessage(mockSqlConnection), x => x.Message.Contains("SAVE MQ ERROR"),
				"Expected exception of type 'ApplicationException' with message containing 'SAVE MQ ERROR'");

			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { }, inputReadUncommitted);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new[] { "CompCode: 0, Reason: 2110" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] MQException ReasonCode: 2110, Reason: 2110, CompCode: 0, CompletionCode: 0. Message: 2110",
				"[WRN] Error code was in PoisonMessageMQErrorCode configuration list. Error details  Code 2110 - 2110.",
				"[WRN] Rollbacked MQ transaction",
				"[WRN] Began a new MQ transaction",
				"[WRN] Retrieved message from MQ using RetrieveSafe()",
				"[WRN] Error during message pulling from MQ. 2 messages will be moved to dead letter queue. [Exception:] FAILED MQ EXCEPTION",
				"[WRN] Unable send 2 to dead letter queue. Message contents follow. [Exception:] DLQ EXCEPTION",
				"[WRN] MESSAGEA",
				"[ERR] Unable to save message contents. Messages will be removed. [Exception:] SAVE MQ ERROR",
				"[WRN] Committed MQ transaction",
				"[WRN] Began a new MQ transaction",
			}, logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_MQException()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new MQException(0, 2035); })).Repeat.Once();
			CreateDefaultTestObjectStubs();

			AssertException<MQException>(() => mockInboundService.RetrieveMQMessage(mockSqlConnection), x => x.Message == "2035",
				"Expected exception of type 'MQException' with message equal to '2035'");

			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, inputReadUncommitted);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] MQException ReasonCode: 2035, Reason: 2035, CompCode: 0, CompletionCode: 0. Message: 2035",
			}, logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_FailedMQ()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			var mqMessageA = new MQMessage();
			var mqMessageParts = new[] { mqMessageA };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new FailedMQMessageException("FAILED MQ EXCEPTION", mqMessageParts); })).Repeat.Once();
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(RetrieveMessageFromList)).Repeat.Once();
			CreateDefaultTestObjectStubs();

			var resultStream = mockInboundService.RetrieveMQMessage(mockSqlConnection);

			assertMQTranBegun();
			Assert.AreSame(message2Stream, resultStream);
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputReadUncommitted);
			CollectionAssert.AreEqual(new[] { mqMessageParts }, dlqMessages);
			CollectionAssert.AreEqual(new[] { "CargoWise.eServices.USCustoms.MQConfiguration.FailedMQMessageException: FAILED MQ EXCEPTION" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] Error during message pulling from MQ. 1 messages will be moved to dead letter queue. [Exception:] FAILED MQ EXCEPTION",
				"[WRN] Message(s) moved to dead letter queue.",
				"[WRN] Committed MQ transaction",
				"[WRN] Began a new MQ transaction",
			}, logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_OtherException()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new ApplicationException("OTHER EXCEPTION"); })).Repeat.Once();
			CreateDefaultTestObjectStubs();

			AssertException<ApplicationException>(() => mockInboundService.RetrieveMQMessage(mockSqlConnection), x => x.Message.Contains("OTHER EXCEPTION"),
				"Expected exception of type 'ApplicationException' with message containing 'OTHER EXCEPTION'");

			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, inputReadUncommitted);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new string[0], logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_StopEventSet_1()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			flagMQTranBegun = true;
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); flagStopEventSet = true; throw new MQException(0, 2110); })).Repeat.Once();
			CreateDefaultTestObjectStubs();

			AssertException<MQException>(() => mockInboundService.RetrieveMQMessage(mockSqlConnection), x => x.Message == "2110",
				"Expected exception of type 'MQException' with message equal to '2110'");

			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message1Stream, message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new Stream[] { message1Stream }, inputReadUncommitted);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new string[] { }, sentErrorMessages);
			CollectionAssert.AreEqual(new string[0], logEntries);
		}

		[Test]
		public void InboundService_RetrieveMQMessage_StopEventSet_2()
		{
			InitialiseCommonTestObjects();
			var message1Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1"));
			var message2Stream = new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2"));
			inputStreams = new List<Stream> { message1Stream, message2Stream };
			var mqMessageA = new MQMessage();
			var mqMessageParts = new[] { mqMessageA };
			mockConfig.Stub(x => x.PoisonMessageMQErrorCode).Return(new[] { "2110" });
			flagMQTranBegun = true;
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(() => { RetrieveMessageFromList(); throw new FailedMQMessageException("FAILED MQ EXCEPTION", mqMessageParts); })).Repeat.Once();
			mockDeadMsgSender.Stub(x => x.Send(Arg<MQMessage[]>.Is.Anything, Arg<bool>.Is.Anything)).Do(new Action<MQMessage[], bool>((m, b) => flagStopEventSet = true));
			CreateDefaultTestObjectStubs();

			var resultStream = mockInboundService.RetrieveMQMessage(mockSqlConnection);

			Assert.IsNull(resultStream);
			assertMQTranBegun();
			CollectionAssert.AreEqual(new Stream[] { message2Stream }, inputStreams);
			CollectionAssert.AreEqual(new MQMessage[][] { }, dlqMessages);
			CollectionAssert.AreEqual(new[] { "CargoWise.eServices.USCustoms.MQConfiguration.FailedMQMessageException: FAILED MQ EXCEPTION" }, sentErrorMessages);
			CollectionAssert.AreEqual(new[] {
				"[WRN] Error during message pulling from MQ. 1 messages will be moved to dead letter queue. [Exception:] FAILED MQ EXCEPTION",
				"[WRN] Message(s) moved to dead letter queue.",
				"[WRN] Committed MQ transaction",
				"[WRN] Began a new MQ transaction",
			}, logEntries);
		}

		[Test]
		public void ServiceTask_ProcessMessageThrowMQException_ResetMQMessageRetrieverAndContinue()
		{
			logEntries = new List<string>();
			var mockConfig = MockRepository.GenerateMock<IInboundServiceConfiguration>();
			var mockInboundService = MockRepository.GeneratePartialMock<Service.InboundService>("InboundService");
			mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var processException = new MQException(2, 2009);
			mockConfig.Stub(x => x.PullIntervalInSecond).Return(1);
			var mockMQMessageRetriever = MockRepository.GenerateMock<IMQMessageRetriever>();
			mockMQMessageRetriever.Expect(x => x.Dispose()).Repeat.Once();
			mockInboundService.Stub(x => x.ResolveDependencies()).Do(new Action(() =>
			{
				mockInboundService.Stub(x => x.Config).Return(mockConfig);
				mockInboundService.Stub(x => x.Logger).Return(mockLogger);
				mockInboundService.Stub(x => x.MessageRetriever).Return(mockMQMessageRetriever);
			}));
			mockInboundService.Expect(x => x.InitialiseMQMessageRetriever()).Repeat.Once();
			mockInboundService.Stub(x => x.ProcessMessages()).Throw(processException).Repeat.Once();
			mockInboundService.Stub(x => x.ProcessMessages());

			mockInboundService.StartTask(null);

			Thread.Sleep(1500);
			mockInboundService.StopTask();

			mockInboundService.AssertWasCalled(x => x.ProcessMessages(), x => x.Repeat.Twice());

			mockInboundService.VerifyAllExpectations();

			Assert.AreEqual(TaskStatus.RanToCompletion, mockInboundService.Task.Status);
			Assert.AreEqual(6, logEntries.Count);
			Assert.AreEqual("[DBG] Starting service 'InboundService'", logEntries[0]);
			Assert.AreEqual("[DBG] Starting pull cycle", logEntries[1]);
			Assert.IsTrue(logEntries[2].StartsWith("[ERR] Exception happened in current pull cycle. Waiting for the next pull cycle.\r\nCompCode: 2, Reason: 2009"));
			Assert.AreEqual("[DBG] Disposed MQ message retriever.", logEntries[3]);
			Assert.AreEqual("[DBG] Starting pull cycle", logEntries[4]);
			Assert.AreEqual("[DBG] Stopping service 'InboundService'", logEntries[5]);
		}

		#endregion  RetrieveMQMessageTests

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
			assertMQTranBegun = () => Assert.IsTrue(flagMQTranBegun, "MQ transaction not begun.");
			assertMQTranNotBegun = () => Assert.IsFalse(flagMQTranBegun, "MQ transaction already begun.");
			assertConnectionOpen = () => Assert.IsTrue(flagConnOpen, "Connection is not open.");
			assertConnectionClosed = () => Assert.IsFalse(flagConnOpen, "Connection already open.");
			assertSqlTranBegun = () => Assert.IsTrue(flagSqlTranBegun, "SQL transaction not begun.");
			assertSqlTranNotBegun = () => Assert.IsFalse(flagSqlTranBegun, "SQL transaction already begun.");
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
				assertMQTranNotBegun();
				flagMQTranBegun = true;
			}));
			mockRetriever.Stub(x => x.Retrieve()).Do(new Func<Stream>(RetrieveMessageFromList));
			mockRetriever.Stub(x => x.CommitTransaction()).Do(new Action(() =>
			{
				assertMQTranBegun();
				inputReadUncommitted.ForEach(s => inputStreams.Remove(s));
				inputReadUncommitted.Clear();
				flagMQTranBegun = false;
			}));
			mockRetriever.Stub(x => x.RollbackTransaction()).Do(new Action(() =>
			{
				assertMQTranBegun();
				inputReadUncommitted.Clear();
				flagMQTranBegun = false;
			}));
			mockInboundService.Stub(x => x.SendMessageToEHub(Arg<Message>.Is.Anything, Arg.Is(mockSqlConnection), Arg.Is(mockSqlTransaction))).Do(new Func<Message, SqlConnection, SqlTransaction, Tuple<string, string>>((m, c, t) =>
			{
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

		static MQMessage CreateMQMessage(string message)
		{
			var mqMessage = new MQMessage() { CharacterSet = 437, Format = MQC.MQFMT_NONE };
			mqMessage.WriteString(message);
			mqMessage.Seek(0);
			return mqMessage;
		}

		Stream RetrieveMessageFromList()
		{
			assertMQTranBegun();
			var currentStream = inputStreams.FirstOrDefault();
			if (currentStream != null) inputReadUncommitted.Add(currentStream);
			return currentStream;
		}
	}
}
