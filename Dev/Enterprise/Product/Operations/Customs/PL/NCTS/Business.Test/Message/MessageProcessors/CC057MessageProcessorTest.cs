using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC057MessageProcessor))]
sealed class CC057MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC057MessageProcessor, IIE057>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE057;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC057CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendNctsErrors;

	public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondHeader";
		message.EM_LinkUniqueID = testNctsHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.ArrivalMovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE0070000123";
		const string mrn = "TEST_MRN";

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Arrival, expectedSubjectPrefix: $"IE057_Functional_Error_({mrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mockTransitOperation = new Mock<ICC057CTransitOperation>();
		dataProviderMock.Setup(m => m.MessageType).Returns("CC057C");
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<ICC057CTransitOperation> mockTransitOperation;
}
