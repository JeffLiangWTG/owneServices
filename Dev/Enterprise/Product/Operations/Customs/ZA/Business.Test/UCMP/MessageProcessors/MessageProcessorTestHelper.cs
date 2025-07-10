using CargoWise.Types;
using Enterprise.Customs.ZA.Business.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP;

static class MessageProcessorTestHelper
{
	internal static void AssertLinkedBusinessObjectMetaData(EDIMessage message, ProcessingResult<LinkedBusinessObjectMetaData> expectedResult)
	{
		var logger = new LoggingInformationForTesting();
		var actualResult = ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger)
			.GetLinkedBusinessObjectMetaData(message, logger);

		Assertion.AssertEquals(expectedResult, actualResult);
	}

	internal static void AssertBranch(EDIMessage message, ProcessingResult<ZGuid> expectedResult)
	{
		var logger = new LoggingInformationForTesting();
		var processor = ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger);
		var linkedBusinessObjectBranchPk = processor.GetLinkedBusinessObjectMetaData(message, logger).ReturnValue.BranchPk;
		var actualResult = processor.GetBranch(message, logger, linkedBusinessObjectBranchPk);

		Assertion.AssertEquals(expectedResult, actualResult);
	}

	internal static void AssertSerializationKeysResult(EDIMessage message, ProcessingResult<SerializationKeysResult> expectedResult)
	{
		var logger = new LoggingInformationForTesting();
		var processor = ZACMessageProcessorFactory.GetMessageProcessor(message.EM_MessageType, logger);
		var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, logger).ReturnValue;
		message.EM_LinkUniqueID = linkedBusinessObjectMetaData.LinkUniqueID;
		message.EM_LinkTable = linkedBusinessObjectMetaData.LinkTableName;
		var actualResult = processor.GetSerializationKeysResult(message, logger, linkedBusinessObjectMetaData);

		Assertion.AssertEquals(expectedResult, actualResult);
	}
}
