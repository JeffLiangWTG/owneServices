using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC906MessageProcessor))]
sealed class CC906MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC906MessageProcessor, IIE906>
{
	public void TestProcessMessage()
	{
		const string testLrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testNctsHeader.MovementHeader.BM_PaperlessInbondNum = testLrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
			dataProviderMock.Setup(x => x.LRN).Returns(testLrn);
			MessageProcessor.ProcessMessage(message);
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Error, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail_MRN()
	{
		const string transmitMessageNum = "24IE015000123";
		const string mrn = "TEST_MRN";
		const string lrn = "TEST_LRN";

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE906_Functional_Error_MRN({mrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	public void TestEmail_LRN()
	{
		const string transmitMessageNum = "24IE015000123";
		const string lrn = "TEST_LRN";

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns((string)null);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE906_Functional_Error_LRN({lrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE906;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC906CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendNctsErrors;
}
