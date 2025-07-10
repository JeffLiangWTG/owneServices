using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	public abstract class BlockingParallelProcessingProviderTest : TestCaseWithFactory
	{
		public void TestLinkedBusinessObjectMetaData()
		{
			PrepareTestingData();
			AssertLinkedBusinessObjectMetaData(incomingMessage, GenerateExpectedLinkedObject());
		}

		protected abstract LinkedBusinessObjectMetaData GenerateExpectedLinkedObject();

		public virtual void TestBranch()
		{
			PrepareTestingData();

			var logger = new LoggingInformation();
			var processor = UCMPMessageProcessorFactory.GetMessageProcessor(incomingMessage.EM_MessageType, logger);
			var linkedBusinessObjectBranchPk = processor.GetLinkedBusinessObjectMetaData(incomingMessage, logger).ReturnValue.BranchPk;
			var actualResult = processor.GetBranch(incomingMessage, logger, linkedBusinessObjectBranchPk);

			AssertEquals(ProcessingResult.New((outgoingMessage ?? incomingMessage).EM_GB), actualResult);
		}

		public void TestSerializationKeysResult()
		{
			PrepareTestingData();

			var logger = new LoggingInformation();
			var processor = UCMPMessageProcessorFactory.GetMessageProcessor(incomingMessage.EM_MessageType, logger);
			var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(incomingMessage, logger).ReturnValue;
			incomingMessage.EM_LinkUniqueID = linkedBusinessObjectMetaData.LinkUniqueID;
			incomingMessage.EM_LinkTable = linkedBusinessObjectMetaData.LinkTableName;
			var actualResult = processor.GetSerializationKeysResult(incomingMessage, logger, linkedBusinessObjectMetaData);

			AssertEquals(GenerateExpectedSerializationKeysResult(), actualResult);
		}

		protected abstract ProcessingResult<SerializationKeysResult> GenerateExpectedSerializationKeysResult();

		public void TestLinkedBusinessObjectMetaData_WithNoLinkedObject()
		{
			GenerateMessageWithNoLinkedObject();
			var expectedResult = GenerateExpectedMetaData_WithNoLinkedObject();
			AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
		}

		protected abstract void GenerateMessageWithNoLinkedObject();

		protected virtual ProcessingResult<LinkedBusinessObjectMetaData> GenerateExpectedMetaData_WithNoLinkedObject() => ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, UCMPMessageProcessorFactory.GetUnableToFindTheLinkedJobMessage(incomingMessage));

		protected void AssertLinkedBusinessObjectMetaData(EDIMessage message, ProcessingResult<LinkedBusinessObjectMetaData> expectedResult)
		{
			var logger = new LoggingInformation();
			var actualResult = UCMPMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger).GetLinkedBusinessObjectMetaData(message, logger);

			AssertEquals(expectedResult, actualResult);
		}

		protected EDIMessage outgoingMessage;
		protected EDIMessage incomingMessage;

		protected abstract void PrepareTestingData();
	}
}
