using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC029SCMessageProcessor))]
sealed class CC029SCMessageProcessorTest : BaseNctsMessageProcessorTestCase<CC029SCMessageProcessor, IIE029SC>
{
	public override void TestPreProcessMessage_CorrelationIdentifier()
	{
		Assert("CC029SC does not have the CorrelationIdentifier", true);
	}

	public void TestProcessMessage()
	{
		const string testLrn = "1234567ABC";
		var testReleaseDate = new DateTime(2024, 10, 15);

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testLrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		dataProviderMock.Setup(x => x.LRN).Returns(testLrn);
		dataProviderMock.Setup(x => x.ReleaseDate).Returns(testReleaseDate);

		MessageProcessor.ProcessMessage(message);

		var movementHeader = testNctsHeader.MovementHeader;
		var mostRecentLog = movementHeader.Logs.MostRecentLog;

		CombineAssertions(() =>
		{
			AssertNotNull("New log should be created", mostRecentLog);
			AssertEquals("Linked object: BM_CustomsStatus", expected: NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, testNctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Linked object: BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message should be processed successfully: EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string mrn = "TEST_MRN";
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE029SC_Released_For_Transit_({mrn})", string.Empty, mrn, lrn: string.Empty);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE029SC;

	protected override bool ExpectedIsFailureNotification => false;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	protected override Type ExpectedMessageInterpreterType => typeof(CC029SCCMessageInterpreter);
}
