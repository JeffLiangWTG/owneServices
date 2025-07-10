using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC056MessageProcessor))]
sealed class CC056MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC056MessageProcessor, IIE056>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE056;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC056CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendNctsErrors;

	public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";
		const string testLrn = "12345ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		dataProviderMock.Setup(x => x.LRN).Returns(testLrn);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Invalid, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string mrn = "TEST_MRN";
		const string lrn = "TEST_LRN";
		const string transmitMessageNum = "24num123";
		var correlationIdentifier = transmitMessageNum.Insert(2, MessageNameList.Codes.IE015);
		const string subject = $"IE056_Functional_Error_({lrn})";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: subject, transmitMessageNum: transmitMessageNum, mrn: null, lrn);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mockTransitOperation = new Mock<ICC056CTransitOperation>();
		dataProviderMock.Setup(m => m.MessageType).Returns("CC056C");
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<ICC056CTransitOperation> mockTransitOperation;
}
