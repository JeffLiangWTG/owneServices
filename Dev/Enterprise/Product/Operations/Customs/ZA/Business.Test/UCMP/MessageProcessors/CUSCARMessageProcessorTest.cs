using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP;

sealed class CUSCARMessageProcessorTest : TestCaseWithFactory
{
	public void TestLinkedBusinessObjectMetaData_CUSCAR()
	{
		var (incomingMessage, manifestHeader) = TestMessageFactory.Get_CUSCAR_WithLinkedAsycudaManifestHeader(Factory);
		var expectedResult = new LinkedBusinessObjectMetaData(manifestHeader.TableName, manifestHeader.PK, manifestHeader.AMA_GB, ZString.Empty);
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_CUSCAR()
	{
		var (incomingMessage, manifestHeader) = TestMessageFactory.Get_CUSCAR_WithLinkedAsycudaManifestHeader(Factory);
		var expectedResult = ProcessingResult.New(manifestHeader.AMA_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CUSCAR()
	{
		var (incomingMessage, manifestHeader) = TestMessageFactory.Get_CUSCAR_WithLinkedAsycudaManifestHeader(Factory);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			manifestHeader.AMA_JobReference.ToString()
		}));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSCAR_WithNoLinkedObject()
	{
		var incomingMessage = TestMessageFactory.Get_CUSCAR_WithNoLinkedObject(Factory);

		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)string.Empty));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, ProcessingResult.New(SerializationKeysResult.SerialProcessingInReceivedOrder));
	}
}
