using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC051MessageProcessor))]
sealed class CC051MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC051MessageProcessor, IIE051>
{
	public void TestProcessMessage()
	{
		const string testMRN = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMRN;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMRN);
		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: "ACC", testNctsHeader.MovementHeader.BM_MessageStatus);
			var movementHeader = testNctsHeader.MovementHeader;
			AssertEquals("Movement header BM_CustomsStatus", expected: "NRL", movementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE01500123";
		const string lrn = "TEST_LRN";

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE051_Not_Released_For_Transit_({lrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE051;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC051CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	protected override void SetUp()
	{
		base.SetUp();

		mockCountrySpecificDataPL = new Mock<IIE051CountrySpecificDataPL>();
		mockTransitOperation = new Mock<ICC051CTransitOperation>();
		dataProviderMock.Setup(m => m.CountrySpecificDataPL).Returns(mockCountrySpecificDataPL.Object);
		dataProviderMock.Setup(m => m.MessageType).Returns("CC051C");
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<IIE051CountrySpecificDataPL> mockCountrySpecificDataPL;
	Mock<ICC051CTransitOperation> mockTransitOperation;
}
