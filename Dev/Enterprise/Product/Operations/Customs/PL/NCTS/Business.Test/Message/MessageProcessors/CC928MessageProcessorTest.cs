using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC928MessageProcessor))]
sealed class CC928MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC928MessageProcessor, IIE928>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE928;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC928CMessageInterpreter);

	public void TestProcessMessage()
	{
		const string testLrn = "1234567ABC";
		var testDate = CargoWise.Types.ZDateTime.UtcNow.Date;

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.LRN).Returns(testLrn);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string lrn = "TEST_LRN";
		const string transmitMessageNum = "24IE01500123";
		var correlationIdentifier = transmitMessageNum;

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE928_Positive_Acknowledgment_({lrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataProviderMock.Setup(m => m.MessageType).Returns("CC928C");
		dataProviderMock.Setup(m => m.LRN).Returns("TestLRN");
	}
}
