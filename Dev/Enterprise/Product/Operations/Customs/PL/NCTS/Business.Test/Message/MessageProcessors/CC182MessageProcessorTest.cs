using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC182MessageProcessor))]
sealed class CC182MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC182MessageProcessor, IIE182>
{
	public void TestProcessMessage()
	{
		const string testMrn = "TST_MRN";
		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);

		var header = Factory.NewWithValidTestData<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = header.MovementHeader;

		var transmittedMessage = Factory.New<EDIMessage>();
		transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsNCTS;
		transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		transmittedMessage.EM_LinkedObject = movementHeader;
		transmittedMessage.EM_Status = EDIMessage.Status.Sent;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = movementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("Movement header BM_MessageStatus", Common.EU.LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("Movement header BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, movementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE182;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC182CMessageInterpreter);

	public void TestEmail()
	{
		const string transitMessageNum = "24num123";
		const string mrn = "TEST_MRN";

		var correlationIdentifier = transitMessageNum.Insert(2, MessageNameList.Codes.IE182);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE182_Incident_Reported_During_Transit_({mrn})", transitMessageNum, mrn, lrn: string.Empty);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataProviderMock.Setup(x => x.MessageType).Returns("CC182C");
	}
}
