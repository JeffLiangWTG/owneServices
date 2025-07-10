using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EDIFactMessageResponseProcessor))]
sealed class EDIFactMessageResponseProcessorTest : UniversalCustomsMessageProcessorTest<EDIFactMessageResponseProcessor>
{
	public void TestShouldMessageBeProcessedInASeparateFactory() =>
		AssertEquals(expected: false, processor.ShouldMessageBeProcessedInASeparateFactory(message));

	public void TestGetLinkedBusinessObjectMetaData()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkTable = cusEntryHeader.TableName;
		message.EM_LinkUniqueID = cusEntryHeader.PK;

		var loggerMock = new Mock<LoggingInformation>();

		CombineAssertions(() =>
		{
			var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(message.EM_LinkTable,
				message.EM_LinkUniqueID, message.EM_GB, ZString.Empty));

			var actualResult = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);

			AssertEquals(expectedResult, actualResult);
		});
	}

	public void TestGetBranch()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkTable = cusEntryHeader.TableName;
		message.EM_LinkUniqueID = cusEntryHeader.PK;

		var loggerMock = new Mock<LoggingInformation>();

		CombineAssertions(() =>
		{
			var expectedResult = ProcessingResult.New(message.EM_GB);

			var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);
			var actualResult = processor.GetBranch(message, loggerMock.Object, linkedBusinessObjectMetaData.ReturnValue.BranchPk);

			AssertEquals(expectedResult, actualResult);
		});
	}

	public void TestGetSerializationKeysResult()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
		cusEntryHeader.CH_BGMReference = "1234567892024010111223304";
		message.EM_LinkTable = cusEntryHeader.TableName;
		message.EM_LinkUniqueID = cusEntryHeader.PK;

		Factory.Save();

		var loggerMock = new Mock<LoggingInformation>();

		CombineAssertions(() =>
		{
			var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
			{
				cusEntryHeader.CH_BGMReference,
				declaration.JE_DeclarationReference
			}));

			var linkedBusinessObjectMetaData = processor.GetLinkedBusinessObjectMetaData(message, loggerMock.Object);
			var actualResult = processor.GetSerializationKeysResult(message, loggerMock.Object, linkedBusinessObjectMetaData.ReturnValue);

			AssertEquals(expectedResult, actualResult);
		});
	}

	protected override string ApplicationCode => ApplicationCodeList.Codes.NOCustoms;

	protected override void SetUp()
	{
		base.SetUp();
		processor = new EDIFactMessageResponseProcessor();
		message = Factory.New<CUSRESEDIMessage>();
	}
	IUniversalCustomsMessageProcessor processor;
	CUSRESEDIMessage message;
}
