using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Moq;
using NUnit.Framework;
using Country = Enterprise.Core.Constants.CountryCodes;
using Functionality = Enterprise.Customs.Universal.Constants.FunctionalityTypes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestsSubclassesOf(typeof(UniversalCustomsMessageProcessorBase<>))]
public abstract class UniversalCustomsMessageProcessorBaseTest<TMessageProcessor, TMessageProcessorFactory, TMessage>
	: UniversalCustomsMessageProcessorTest<TMessageProcessor>
	where TMessageProcessor : UniversalCustomsMessageProcessorBase<TMessageProcessorFactory>, new()
	where TMessageProcessorFactory : MessageProcessorFactoryBase, new()
	where TMessage : BaseEDIMessage
{
	public void TestShouldMessageBeProcessedInASeparateFactory()
	{
		var expectedResult = ExpectedShouldMessageBeProcessedInASeparateFactory;
		var actualResult = Processor.ShouldMessageBeProcessedInASeparateFactory(Message);
		AssertEquals(expectedResult, actualResult);
	}

	public void TestGetLinkedBusinessObjectMetaData()
	{
		var expectedResult = ExpectedGetLinkedBusinessObjectMetaDataResult;
		var actualResult = Processor.GetLinkedBusinessObjectMetaData(Message, Logger);
		AssertEquals(expectedResult, actualResult);
	}

	public void TestGetBranch()
	{
		var expectedResult = ExpectedGetBranchResult;
		var linkedBusinessObjectMetaData = Processor.GetLinkedBusinessObjectMetaData(Message, Logger).ReturnValue;
		var actualResult = Processor.GetBranch(Message, Logger, linkedBusinessObjectMetaData.BranchPk);
		AssertEquals(expectedResult, actualResult);
	}

	public void TestGetSerializationKeysResult()
	{
		var expectedResult = ExpectedGetSerializationKeysResult;
		var linkedBusinessObjectMetaData = Processor.GetLinkedBusinessObjectMetaData(Message, Logger).ReturnValue;
		var actualResult = Processor.GetSerializationKeysResult(Message, Logger, linkedBusinessObjectMetaData);
		AssertEquals(expectedResult, actualResult);
	}

	protected virtual bool ExpectedShouldMessageBeProcessedInASeparateFactory => false;

	protected virtual ProcessingResult<LinkedBusinessObjectMetaData> ExpectedGetLinkedBusinessObjectMetaDataResult
		=> new LinkedBusinessObjectMetaData(Message.EM_LinkTable, Message.EM_LinkUniqueID, Message.EM_GB, jobNumber: ZString.Empty);

	protected virtual ProcessingResult<ZGuid> ExpectedGetBranchResult => Message.EM_GB;

	protected virtual ProcessingResult<SerializationKeysResult> ExpectedGetSerializationKeysResult
		=> new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { Message.PK.ToStringKey() });

	protected override void SetUp()
	{
		base.SetUp();
		Processor = new TMessageProcessor();
		Message = Factory.New<TMessage>();
		Message.EM_GB = ZGuid.NewZGuid();
		Message.EM_LinkTable = "TEST_LinkTable";
		Message.EM_LinkUniqueID = ZGuid.NewZGuid();

		LoggerMock = new Mock<LoggingInformation>();
		Logger = LoggerMock.Object;
		TestContext.Reset();
		TestContext.SetFunctionality(Functionality.UCMPServiceTask, Country.Poland, ZDateTime.Now, enabled: true);
	}

	protected TMessageProcessor Processor { get; private set; }
	protected TMessage Message { get; private set; }
	protected Mock<LoggingInformation> LoggerMock { get; private set; }
	protected LoggingInformation Logger { get; private set; }
	protected FunctionalityTestContext TestContext { get; } = new();
}
