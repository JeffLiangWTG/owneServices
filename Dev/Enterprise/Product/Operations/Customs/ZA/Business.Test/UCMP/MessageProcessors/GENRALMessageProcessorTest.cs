using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP;

sealed class GENRALMessageProcessorTest : TestCaseWithFactory
{
	public void TestLinkedBusinessObjectMetaData_GENRAL()
	{
		var incomingMessage = TestMessageFactory.Get_GENRAL(Factory);
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty);
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_GENRAL()
	{
		var incomingMessage = TestMessageFactory.Get_GENRAL(Factory);
		var expectedResult = ProcessingResult.New(incomingMessage.EM_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_GENRAL()
	{
		var incomingMessage = TestMessageFactory.Get_GENRAL(Factory);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			incomingMessage.PK.ToStringKey()
		}));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}
}
