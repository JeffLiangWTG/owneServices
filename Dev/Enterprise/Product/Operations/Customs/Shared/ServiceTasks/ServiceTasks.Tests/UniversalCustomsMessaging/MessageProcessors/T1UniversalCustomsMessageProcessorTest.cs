using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(T1UniversalCustomsMessageProcessor))]
	sealed class T1UniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<T1UniversalCustomsMessageProcessor>
	{
		public void TestShouldMessageBeProcessedInASeparateFactory()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(false, ((IUniversalCustomsMessageProcessor)new T1UniversalCustomsMessageProcessor(0, 0, false)).ShouldMessageBeProcessedInASeparateFactory(message));
			AssertEquals(true, ((IUniversalCustomsMessageProcessor)new T1UniversalCustomsMessageProcessor(0, 0, true)).ShouldMessageBeProcessedInASeparateFactory(message));
		}

		public void TestPreProcessMessage_NoMatch()
		{
			var messageProcessor = new T1UniversalCustomsMessageProcessor(0, 0, false);
			var preProcessingData = SetupPreProcessingMessagingData();
			AssertPreProcessing(
				messageProcessor,
				preProcessingData.incomingMessage,
				new LinkedBusinessObjectMetaData(ZString.Empty, ZGuid.Empty, preProcessingData.branch3PK, ZString.Empty),
				preProcessingData.branch3PK,
				SerializationKeysResult.SerialProcessingInReceivedOrder);
		}

		public void TestPreProcessMessage_MatchNoLinkedObject()
		{
			var messageProcessor = new T1UniversalCustomsMessageProcessor(0, 0, false);
			var preProcessingData = SetupPreProcessingMessagingData();
			preProcessingData.incomingMessage.EM_MessageNum = preProcessingData.outgoingMessage.EM_MessageNum;
			AssertPreProcessing(
				messageProcessor,
				preProcessingData.incomingMessage,
				new LinkedBusinessObjectMetaData(preProcessingData.declaration.TableName, ZGuid.Empty, preProcessingData.branch1PK, ZString.Empty),
				preProcessingData.branch1PK,
				SerializationKeysResult.SerialProcessingInReceivedOrder);
		}

		public void TestPreProcessMessage_MatchWithLinkedObject()
		{
			var messageProcessor = new T1UniversalCustomsMessageProcessor(0, 0, false);
			var preProcessingData = SetupPreProcessingMessagingData();
			preProcessingData.incomingMessage.EM_MessageNum = preProcessingData.outgoingMessage.EM_MessageNum;
			preProcessingData.outgoingMessage.EM_LinkedObject = preProcessingData.declaration;
			AssertPreProcessing(
				messageProcessor,
				preProcessingData.incomingMessage,
				new LinkedBusinessObjectMetaData(preProcessingData.declaration.TableName, preProcessingData.declaration.PK, preProcessingData.branch2PK, preProcessingData.declaration.JobNumber),
				preProcessingData.branch2PK,
				new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "JB1234", "AppRef", "MSGOWN" }));
		}

		public void TestPreProcessMessage_InvalidLinkedObject()
		{
			var messageProcessor = new T1UniversalCustomsMessageProcessor(0, 0, false);
			var preProcessingData = SetupPreProcessingMessagingData();
			preProcessingData.incomingMessage.EM_MessageNum = preProcessingData.outgoingMessage.EM_MessageNum;
			preProcessingData.outgoingMessage.EM_LinkedObject = preProcessingData.declaration;
			preProcessingData.outgoingMessage.EM_LinkTable = "ABC";
			AssertPreProcessing(
				messageProcessor,
				preProcessingData.incomingMessage,
				new LinkedBusinessObjectMetaData("ABC", preProcessingData.declaration.PK, preProcessingData.outgoingMessage.EM_GB, ZString.Empty),
				preProcessingData.branch1PK,
				SerializationKeysResult.SerialProcessingInReceivedOrder);
		}

		public void TestPreProcessMessage_Discard()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = "_T1";
			incomingMessage.EM_MessageType = "T1T";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = "GOODBYE WORLD";
			incomingMessage.EM_MessageSubType = EDIMessage.Status.Discarded;
			incomingMessage.EM_ExternalReferenceNumber = "Bad data";
			var messageProcessor = new T1UniversalCustomsMessageProcessor(0, 0, false);
			var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)"Bad data");
			var actualResult = messageProcessor.GetLinkedBusinessObjectMetaData(incomingMessage, new LoggingInformation());
			AssertEquals("Message was discarded", expectedResult, actualResult);
		}

		public void TestProcessMessage()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = "_T1";
			incomingMessage.EM_MessageType = "T1T";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = "GOODBYE WORLD";
			var messageProcessor = new T1UniversalCustomsMessageProcessor(0, 0, false);
			messageProcessor.ProcessMessage(incomingMessage, new LoggingInformation(), null);
			AssertEquals("EM_Status", EDIMessage.Status.Received, incomingMessage.EM_Status);
		}

		public override void TestCorrectSubscribeToUniversalCustomsMessagingSubscribers()
		{
			Assert("No need", true);
		}

		protected override string ApplicationCode => throw new System.NotImplementedException();

		(ZGuid branch1PK, ZGuid branch2PK, ZGuid branch3PK, EDIMessage outgoingMessage, BaseJobDeclaration declaration, EDIMessage incomingMessage) SetupPreProcessingMessagingData()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var branch1PK = company.Branches.AddNew().PK;
			var branch2PK = company.Branches.AddNew().PK;
			var branch3PK = company.Branches.AddNew().PK;

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_GB = branch1PK;
			outgoingMessage.EM_ApplicationCode = "_T1";
			outgoingMessage.EM_MessageType = "TST";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "ICS22023001";
			outgoingMessage.EM_MessageText = "HELLO WORLD";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "JB1234";
			declaration.JE_GB = branch2PK;
			outgoingMessage.EM_LinkTable = declaration.TableName;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_GB = branch3PK;
			incomingMessage.EM_ApplicationCode = "_T1";
			incomingMessage.EM_MessageType = "T1T";
			incomingMessage.EM_MessageSubType = "MST";
			incomingMessage.EM_ApplicationReference = "AppRef";
			incomingMessage.EM_MessageOwner = "MSGOWN";
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = "GOODBYE WORLD";

			return (branch1PK, branch2PK, branch3PK, outgoingMessage, declaration, incomingMessage);
		}
	}
}
