using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC009MessageProcessor))]
sealed class CC009MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC009MessageProcessor, IIE009>
{
	public void TestProcessMessage()
	{
		const string testLrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testNctsHeader.MovementHeader.BM_PaperlessInbondNum = testLrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		dataProviderMock.Setup(x => x.LRN).Returns(testLrn);

		CombineAssertions(() =>
		{
			dataProviderMock.Setup(x => x.Invalidation.Decision).Returns(Decision.Item1);
			MessageProcessor.ProcessMessage(message);
			AssertEquals("Decision is Item1. Linked object BM_CustomsStatus", expected: NCTS5DepartureCustomsStatusList.Codes.Cancelled, testNctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Decision is Item1. Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Decision is Item1. Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);

			dataProviderMock.Setup(x => x.Invalidation.Decision).Returns(Decision.Item0);
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
			MessageProcessor.ProcessMessage(message);
			AssertEquals("Decision is Item0. Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Invalid, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Decision is Item0. Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);

			dataProviderMock.Setup(x => x.Invalidation.Decision).Returns((Decision?)null);
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
			MessageProcessor.ProcessMessage(message);
			AssertEquals("Decision is null. Message EM_Status", EDIMessage.Status.Failed, message.EM_Status);
		});
	}

	public void TestEmail_LRN()
	{
		const string lrn = "TEST_LRN";
		const string transmitMessageNum = "24IE01500123";
		var correlationIdentifier = transmitMessageNum;
		const string expectedSubjectPrefix = $"IE009_Invalidation_Response_({lrn})";

		dataProviderMock.Setup(x => x.Invalidation.Decision).Returns(Decision.Item1);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix, transmitMessageNum, null, null);
	}

	public void TestEmail_MRN()
	{
		const string mrn = "TEST_MRN";
		const string transmitMessageNum = "24IE01500123";
		var correlationIdentifier = transmitMessageNum;
		const string expectedSubjectPrefix = $"IE009_Invalidation_Response_({mrn})";

		dataProviderMock.Setup(x => x.Invalidation.Decision).Returns(Decision.Item1);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix, transmitMessageNum, null, null);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE009;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC009CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
}
