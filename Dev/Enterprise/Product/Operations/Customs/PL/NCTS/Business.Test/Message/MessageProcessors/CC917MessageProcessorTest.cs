using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC917MessageProcessor))]
sealed class CC917MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC917MessageProcessor, IIE917>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE917;

	protected override bool ExpectedIsFailureNotification => true;

	protected override Type ExpectedMessageInterpreterType => typeof(CC917CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendNctsErrors;

	public void TestProcessMessage()
	{
		const string year = "24";
		const string fullCorrelationIdentifier = $"{year}{MessageNameList.Codes.IE015}4345ABC";
		const string ediCorrelationIdentifier = $"{year}4345ABC";

		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var testEdiMessage = Factory.CreateNCTSMessage(messageText: string.Empty, linkedObject: nctsHeader.MovementHeader);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = nctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		message.EM_MessageNum = ediCorrelationIdentifier;

		dataProviderMock.Setup(m => m.CorrelationIdentifier).Returns(fullCorrelationIdentifier);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Error, nctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE01500123";
		var correlationIdentifier = transmitMessageNum;

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: "IE917_Syntax_Error", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null, isFailure: true);
	}

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC917C");
		dataProviderMock.Setup(m => m.CorrelationIdentifier).Returns("TestIdentifier");
		dataProviderMock.Setup(h => h.MRN).Returns("TestMRN");
		dataProviderMock.Setup(h => h.LRN).Returns("TestLRN");
	}
}
