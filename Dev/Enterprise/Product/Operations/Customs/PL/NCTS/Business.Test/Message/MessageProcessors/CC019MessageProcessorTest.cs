using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC019MessageProcessor))]
sealed class CC019MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC019MessageProcessor, IIE019>
{
	const string TestMrn = "1234567ABC";

	public void TestProcessMessage()
	{
		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = TestMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_CustomsStatus", expected: EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.DiscrepanciesAtDestination, testNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.ArrivalMovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE007000123";
		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		ProcessMessageAndTestEmail(NctsMovementType.Codes.Arrival, expectedSubjectPrefix: $"IE019_Major_Discrepancies_({TestMrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE019;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC019CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC019C");
		dataProviderMock.Setup(x => x.MRN).Returns(TestMrn);
		dataProviderMock.Setup(m => m.TransitOperation).Returns(Mock.Of<ICC019CTransitOperation>());
	}
}
