using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using NUnit.Framework;
using static Enterprise.Customs.PL.NCTS.Business.Testing.CC029TestHelper;
#pragma warning disable IDE0001 // Simplify Names - Would lose functionality
using MessageStatus = Enterprise.Customs.PL.Business.EDIMessage.Status;
#pragma warning restore IDE0001 // Simplify Names

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC029MessageProcessor))]
sealed class CC029MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC029MessageProcessor, IIE029>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE029;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC029CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	public void TestProcessMessage_LRN()
	{
		const string correlationIdentifier = "TEST_NUM";
		const string lrn = "TEST_LRN";

		var (nctsHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(NctsMovementType.Codes.Departure, correlationIdentifier, "", lrn: lrn);

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		mocks.IE029.Setup(x => x.MRN).Returns("");
		mocks.IE029.Setup(x => x.LRN).Returns(lrn);

		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, nctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", expected: NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", MessageStatus.ProcessedOK, inboundMessage.EM_Status);
			AssertNotEquals("Has not empty interpretation", ZString.Empty, inboundMessage.EM_MessageInterpretation);
		});
	}

	public void TestProcessMessage_MRN()
	{
		const string correlationIdentifier = "TEST_NUM";
		const string mrn = "TEST_MRN";

		var (nctsHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(NctsMovementType.Codes.Departure, correlationIdentifier, mrn, lrn: "");

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		mocks.IE029.Setup(x => x.LRN).Returns(string.Empty);
		mocks.IE029.Setup(x => x.MRN).Returns(mrn);

		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, nctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", expected: NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", MessageStatus.ProcessedOK, inboundMessage.EM_Status);
			AssertNotEquals("Has not empty interpretation", ZString.Empty, inboundMessage.EM_MessageInterpretation);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE015000123";
		const string mrn = "TEST_MRN";

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns((string)null);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE029 Released For Transit ({mrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mocks = SetupDataProviderMock(dataProviderMock);
	}

	CC029Mocks mocks;
}
