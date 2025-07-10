using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP;
sealed class CONTRLMessageProcessorTest : TestCaseWithFactory
{
	public void TestLinkedBusinessObjectMetaData_CONTRL()
	{
		var (incomingMessage, outgoingMessageReplyingTo, cusEntryHeader, declaration) = TestMessageFactory.Get_CONTRL_WithLinkedCusResEntryHeaderOutgoingMessage(Factory);
		var expectedResult = new LinkedBusinessObjectMetaData(cusEntryHeader.TableName, cusEntryHeader.PK, outgoingMessageReplyingTo.EM_GB, ZString.Empty);
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_CONTRL()
	{
		var (incomingMessage, outgoingMessageReplyingTo, cusEntryHeader, declaration) = TestMessageFactory.Get_CONTRL_WithLinkedCusResEntryHeaderOutgoingMessage(Factory);
		var expectedResult = ProcessingResult.New(outgoingMessageReplyingTo.EM_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CONTRL()
	{
		var (incomingMessage, outgoingMessageReplyingTo, cusEntryHeader, declaration) = TestMessageFactory.Get_CONTRL_WithLinkedCusResEntryHeaderOutgoingMessage(Factory);
		var expectedResult = new SerializationKeysResult(
			SerializationKeysResult.SerializationKeysResultType.KeysProvided,
			new HashSet<string>
			{
				declaration.JE_DeclarationReference.ToString(), cusEntryHeader.CH_BGMReference.ToString(),
			});
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CONTRL_WithNoLinkedObject()
	{
		var incomingMessage = TestMessageFactory.GetIncomingCONTRLEDIMessage(Factory, "1");
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, ZACApplicationTypeMessageProcessor.GetUnableToFindTheLinkedJobMessage("CONTRL", incomingMessage));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}
}
