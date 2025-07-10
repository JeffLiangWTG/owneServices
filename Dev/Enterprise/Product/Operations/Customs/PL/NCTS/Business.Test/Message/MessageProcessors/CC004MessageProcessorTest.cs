using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.PL.NCTS.Business.Testing.CC004TestHelper;
using MessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC004MessageProcessor))]
sealed class CC004MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC004MessageProcessor, IIE004>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE004;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC004CMessageInterpreter);

	public void TestProcessMessage()
	{
		const string nctsHeaderNum = "TEST_HEADER_NUM";
		const string transmitMessageNum = "TEST_ID";
		const string mrn = "TEST_MRN";

		var testDateTime = "1900-01-01T01:01:01+05:30";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_JobReference = nctsHeaderNum;

		var transmittedMessage = Factory.New<EDIMessage>();
		transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsNCTS;
		transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		transmittedMessage.EM_MessageNum = transmitMessageNum;
		transmittedMessage.EM_LinkedObject = nctsHeader.MovementHeader;
		transmittedMessage.EM_Status = EDIMessage.Status.Queued;

		var inboundMessage = Factory.New<EDIMessage>();
		inboundMessage.EM_LinkedObject = nctsHeader.MovementHeader;
		inboundMessage.EM_Status = MessageStatus.PreProcessedOK;

		mocks.IE004.Setup(x => x.CorrelationIdentifier).Returns(transmitMessageNum);
		mocks.IE004.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(transmitMessageNum);
		mocks.TransitOperation.Setup(x => x.AmendmentAcceptanceDateAndTime).Returns(testDateTime);
		mocks.TransitOperation.Setup(x => x.AmendmentSubmissionDateAndTime).Returns(testDateTime);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", MessageStatus.ProcessedOK, inboundMessage.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string lrn = "TEST_LRN";
		const string transmitMessageNum = "24IE01500123";
		var correlationIdentifier = transmitMessageNum;
		const string subjectPrefix = $"IE004_Amendment_Accepted_({lrn})";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: subjectPrefix, transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mocks = SetupDataProviderMock(dataProviderMock);
	}

	CC004Mocks mocks;
}
