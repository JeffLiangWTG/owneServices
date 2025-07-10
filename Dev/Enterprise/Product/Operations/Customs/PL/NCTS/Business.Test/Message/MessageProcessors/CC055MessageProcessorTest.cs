using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC055MessageProcessor))]
sealed class CC055MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC055MessageProcessor, IIE055>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE055;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC055CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";
		var testDate = CargoWise.Types.ZDateTime.UtcNow.Date.ToDateTime();

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		mockTransitOperation.Setup(x => x.DeclarationAcceptanceDate).Returns(testDate);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.MovementHeader.BM_MessageStatus);
			var movementHeader = testNctsHeader.MovementHeader;
			AssertEquals("Movement header BM_CustomsStatus", expected: EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, movementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24num123";
		const string lrn = "TEST_LRN";

		var correlationIdentifier = transmitMessageNum.Insert(2, MessageNameList.Codes.IE015);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE055_GUARANTEE_INVALID_({lrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mockTransitOperation = new Mock<ICC055CTransitOperation>();
		dataProviderMock.Setup(m => m.MessageType).Returns("CC055C");
		dataProviderMock.Setup(m => m.MessageIdentification).Returns("TEST_ID");
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<ICC055CTransitOperation> mockTransitOperation;
}
