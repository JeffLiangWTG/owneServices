using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP;

sealed class STATACMessageProcessorTest : TestCaseWithFactory
{
	public void TestLinkedBusinessObjectMetaData_STATAC()
	{
		var incomingMessage = TestMessageFactory.Get_STATAC(Factory);
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty);
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_STATAC()
	{
		var incomingMessage = TestMessageFactory.Get_STATAC(Factory);
		var expectedResult = ProcessingResult.New(incomingMessage.EM_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_STATAC()
	{
		var incomingMessage = TestMessageFactory.Get_STATAC(Factory);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			"8120050169|2016-07-10|2016-07-10|CTN|21044566|" + incomingMessage.Branch.GB_GC
		}));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}
}
