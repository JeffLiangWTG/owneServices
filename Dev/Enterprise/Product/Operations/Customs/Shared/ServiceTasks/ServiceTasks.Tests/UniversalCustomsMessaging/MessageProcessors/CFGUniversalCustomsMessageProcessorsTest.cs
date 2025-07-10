using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(CFGUniversalCustomsMessageProcessors))]
	sealed class CFGUniversalCustomsMessageProcessorsTest : UniversalCustomsMessageProcessorTest<CFGUniversalCustomsMessageProcessors>
	{
		public void TestPreProcessMessage()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1PK = company.Branches.AddNew().PK;
			var branch2PK = company.Branches.AddNew().PK;

			var outgoingMessage1 = Factory.New<EDIMessage>();
			outgoingMessage1.EM_GB = branch1PK;
			outgoingMessage1.EM_ApplicationCode = "CFG";
			outgoingMessage1.EM_MessageType = "TST";
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageNum = "123456";
			outgoingMessage1.EM_MessageText = "Test Message 1";

			var incomingMessage1 = Factory.New<EDIMessage>();
			incomingMessage1.EM_GB = branch2PK;
			incomingMessage1.EM_ApplicationCode = "CFG";
			incomingMessage1.EM_MessageType = "TST";
			incomingMessage1.EM_ApplicationReference = "AppRef";
			incomingMessage1.EM_MessageOwner = "MSGOWN";
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_MessageText = "Incoming Test Message 1";
			incomingMessage1.EM_LinkUniqueID = outgoingMessage1.EM_LinkUniqueID;
			incomingMessage1.EM_LinkTable = "ABC";

			var outgoingMessage2 = Factory.New<EDIMessage>();
			outgoingMessage2.EM_GB = branch1PK;
			outgoingMessage2.EM_ApplicationCode = "CFG";
			outgoingMessage2.EM_MessageType = "ABC";
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageNum = "654321";
			outgoingMessage2.EM_MessageText = "Test Message 2";

			var incomingMessage2 = Factory.New<EDIMessage>();
			incomingMessage2.EM_GB = branch2PK;
			incomingMessage2.EM_ApplicationCode = "CFG";
			incomingMessage2.EM_MessageType = "ABC";
			incomingMessage2.EM_ApplicationReference = "AppRef";
			incomingMessage2.EM_MessageOwner = "MSGOWN";
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageText = "Incoming Test Message 2";
			incomingMessage2.EM_LinkUniqueID = outgoingMessage2.EM_LinkUniqueID;
			incomingMessage2.EM_LinkTable = "CBA";

			using (new CFGProcessorsRegistrationSubstitute([("TST", new MessageProcessorForTest()), ("ABC", new MessageProcessorForTest())]))
			{
				var messageProcessor = new CFGUniversalCustomsMessageProcessors();

				AssertPreProcessing(
					messageProcessor,
					incomingMessage1,
					new LinkedBusinessObjectMetaData("ABC", outgoingMessage1.EM_LinkUniqueID, incomingMessage1.EM_GB, ""),
					branch2PK,
					SerializationKeysResult.SerialProcessingInReceivedOrder);

				AssertPreProcessing(
					messageProcessor,
					incomingMessage2,
					new LinkedBusinessObjectMetaData("CBA", outgoingMessage1.EM_LinkUniqueID, incomingMessage2.EM_GB, ""),
					branch2PK,
					SerializationKeysResult.SerialProcessingInReceivedOrder);
			}
		}

		public void TestGetLinkedBusinessObjectMetaData_WithoutValidProcessor_ReturnsDefault()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = "TST";
			message.EM_LinkTable = "ABC";
			message.EM_LinkUniqueID = ZGuid.NewZGuid();
			message.EM_GB = ZGuid.NewZGuid();

			var processors = new CFGUniversalCustomsMessageProcessors();

			var linkedBusinessObjectMetaData = processors.GetLinkedBusinessObjectMetaData(message, new LoggingInformation()).ReturnValue;
			AssertEquals("LinkTableName should match the message.EM_LinkTable", message.EM_LinkTable, linkedBusinessObjectMetaData.LinkTableName);
			AssertEquals("LinkUniqueId should match the message.EM_LinkUniqueId", message.EM_LinkUniqueID, linkedBusinessObjectMetaData.LinkUniqueID);
			AssertEquals("BranchPk should match the message.EM_GB", message.EM_GB, linkedBusinessObjectMetaData.BranchPk);
			AssertEquals("JobNumber should be empty", ZString.Empty, linkedBusinessObjectMetaData.JobNumber);
		}

		public void TestGetSerializationKeysResult_WithLinkedBusinessObjectJobNumber_ReturnsJobNumberAsKey()
		{
			var message = Factory.New<EDIMessage>();
			var linkedBusinessObjectMetaData = new LinkedBusinessObjectMetaData("ABC", ZGuid.NewZGuid(), ZGuid.Empty, "123456");

			var processors = new CFGUniversalCustomsMessageProcessors();

			var result = processors.GetSerializationKeysResult(message, new LoggingInformation(), linkedBusinessObjectMetaData);
			var serializationKeysResult = result.ReturnValue;
			AssertEquals(
				"Serialization type should be KeysProvided",
				SerializationKeysResult.SerializationKeysResultType.KeysProvided,
				serializationKeysResult.ResultType);
			AssertContainsExactElementsInExactOrder(
				"Keys should contain the LinkedBusinessObjectMetaData job number",
				new HashSet<string>
				{
					linkedBusinessObjectMetaData.JobNumber
				},
				serializationKeysResult.Keys);
		}

		public void TestProcessMessage()
		{
			using (new CFGProcessorsRegistrationSubstitute([("TST", new MessageProcessorForTest()), ("ABC", new MessageProcessorForTest())]))
			{
				var processor = new CFGUniversalCustomsMessageProcessors();
				var message1 = Factory.New<EDIMessage>();
				message1.EM_Status = "QUE";
				message1.EM_ApplicationCode = "CFG";
				message1.EM_MessageType = "TST";

				var message2 = Factory.New<EDIMessage>();
				message2.EM_Status = "QUE";
				message2.EM_ApplicationCode = "CFG";
				message2.EM_MessageType = "ABC";

				processor.ProcessMessage(message1, new LoggingInformation(), null);
				processor.ProcessMessage(message2, new LoggingInformation(), null);

				AssertEquals("The message status should be set to 'Received'.", EDIMessage.Status.Received, message1.EM_Status);
				AssertEquals("The message status should be set to 'Received'.", EDIMessage.Status.Received, message2.EM_Status);
			}
		}

		public void TestProcessMessageErrorReport()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			void LinkToDeclaration(EDIMessage message, ILoggingInformation logger)
			{
				message.EM_LinkedObject = declaration;
			}

			var messageProcessor1 = new MessageProcessorForTest();
			messageProcessor1.AdditionalProcessMessageForTesting = LinkToDeclaration;
			var messageProcessor2 = new MessageProcessorForTest();
			messageProcessor2.AdditionalProcessMessageForTesting = LinkToDeclaration;

			using (new CFGProcessorsRegistrationSubstitute([("TST", messageProcessor1), ("ABC", messageProcessor2)]))
			{
				var processor = new WorkerUniversalCustomsApplicationTypeMessageProcessor(new LoggingInformation(), "CFG", new CFGUniversalCustomsMessageProcessors(), 1);
				var message1 = Factory.New<EDIMessage>();
				message1.EM_Status = "QUE";
				message1.EM_ApplicationCode = "CFG";
				message1.EM_MessageType = "TST";

				var message2 = Factory.New<EDIMessage>();
				message2.EM_Status = "QUE";
				message2.EM_ApplicationCode = "CFG";
				message2.EM_MessageType = "ABC";

				var processorName = typeof(CFGUniversalCustomsMessageProcessors).FullName;
				processor.ProcessMessage(message1);
				AssertEquals("The message status should be set to 'Received'.", EDIMessage.Status.Received, message1.EM_Status);
				AssertEquals("LastMessageReported", $"{processorName} (TST) is setting EM_LinkedObject data in ProcessMessage when it should have done in UCK", ErrorReporter.LastMessageReported);
				AssertEquals("LastKeyReported", $"{processorName} (TST) is setting EM_LinkedObject not in UCK", ErrorReporter.LastKeyReported);

				processor.ProcessMessage(message2);
				AssertEquals("The message status should be set to 'Received'.", EDIMessage.Status.Received, message2.EM_Status);
				AssertEquals("LastMessageReported", $"{processorName} (ABC) is setting EM_LinkedObject data in ProcessMessage when it should have done in UCK", ErrorReporter.LastMessageReported);
				AssertEquals("LastKeyReported", $"{processorName} (ABC) is setting EM_LinkedObject not in UCK", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		protected override string ApplicationCode => ApplicationCodeList.Codes.XHCredentialConfig;

		sealed class MessageProcessorForTest : ICFGUniversalCustomsMessageProcessor
		{
			public ZString GetJobNumberForKeys(EDIMessage message) => ZString.Empty;
			public Action<EDIMessage, ILoggingInformation> AdditionalProcessMessageForTesting;
			public void ProcessMessage(EDIMessage message, ILoggingInformation logger)
			{
				AdditionalProcessMessageForTesting?.Invoke(message, logger);
				message.EM_Status = EDIMessage.Status.Received;
			}

			public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, ILoggingInformation logger)
			{
				return new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, ZString.Empty);
			}
		}
	}
}
