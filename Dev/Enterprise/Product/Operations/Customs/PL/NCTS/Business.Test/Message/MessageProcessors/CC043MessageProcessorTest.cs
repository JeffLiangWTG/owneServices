using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC043MessageProcessor))]
sealed class CC043MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC043MessageProcessor, IIE043>
{
	public void TestProcessMessage()
	{
		const string mrn = "1234567ABC";
		const string expectedDeclarationType = NctsPhase5DeclarationTypeList.Codes.TIR;

		var (nctsHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(NctsMovementType.Codes.Arrival, transitMessageNum: "", mrn, lrn: "");

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.TransitOperation.DeclarationType).Returns(expectedDeclarationType);

		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() =>
		{
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, arrivalMovementHeader.BM_MessageStatus);
			AssertEquals("Movement header BM_CustomsStatus", expected: NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, arrivalMovementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, inboundMessage.EM_Status);
			AssertEquals("BM_InBondEntryType", expected: expectedDeclarationType, arrivalMovementHeader.BM_InBondEntryType);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE0007000123";
		const string mrn = "TEST_MRN";
		const string expectedDeclarationType = NctsPhase5DeclarationTypeList.Codes.TIR;

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.TransitOperation.DeclarationType).Returns(expectedDeclarationType);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Arrival, expectedSubjectPrefix: $"IE043_Unloading_Permission_({mrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE043;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC043CMessageInterpreter);

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC043C");
	}
}
