using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC025MessageProcessor))]
sealed class CC025MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC025MessageProcessor, IIE025>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE025;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC025CMessageInterpreter);

	public void TestProcessMessage()
	{
		const string transmitMessageNum = "TEST_NUM";
		const string testMrn = "TEST_MRN";

		var (nctsHeader, _, inboundMessage) = PrepareDataForInboundMessageTest(NctsMovementType.Codes.Arrival, transmitMessageNum, testMrn, lrn: "");
		nctsHeader.ArrivalMovementHeader.BM_AdditionalDeclarationType = "A";

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		dataProviderMock.Setup(x => x.TransitOperation.ReleaseIndicator).Returns(Constants.ReleaseType.ClosedFullRelease);

		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() =>
		{
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			AssertEquals("Linked object BM_MessageStatus", expected: "ACC", arrivalMovementHeader.BM_MessageStatus);
			AssertEquals("Movement header BM_AdditionalDeclarationType", expected: "A", arrivalMovementHeader.BM_AdditionalDeclarationType);

			var cusEntryNum = nctsHeader.MovementReferenceEntryNumber;
			AssertNotNull("Entry number", cusEntryNum);
			if (cusEntryNum != null)
			{
				AssertEquals("Entry number CE_EntryNum", expected: testMrn, cusEntryNum.CE_EntryNum);
				AssertEquals("Entry number CE_ParentTable", expected: "CusInBondHeader", cusEntryNum.CE_ParentTable);
				AssertEquals("Entry number CE_ParentID", expected: nctsHeader.PK, cusEntryNum.CE_ParentID);
				AssertEquals("Entry number CE_EntryType", expected: "MRN", cusEntryNum.CE_EntryType);
			}

			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, inboundMessage.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string mrn = "TEST_MRN";
		const string transmitMessageNum = "24IE007000123";
		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		foreach (var (releaseIndicator, subject) in new[] {
			("1", $"IE025_Full_Released_({mrn})"),
			("2", $"IE025_Partial_Released_({mrn})"),
			("3", $"IE025_Partial_Released_({mrn})"),
			("4", $"IE025_No_Released_({mrn})"),
		})
		{
			mockTransitOperation.Setup(x => x.ReleaseIndicator).Returns(releaseIndicator);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			ProcessMessageAndTestEmail(NctsMovementType.Codes.Arrival, expectedSubjectPrefix: subject, transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
		}
	}

	public void TestProcessMessageReleaseIndicator()
	{
		const string testMrn = "1234567ABC";
		var testDate = new DateTime(2022, 02, 01);

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		testNctsHeader.ArrivalMovementHeader.BM_AdditionalDeclarationType = "A";
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondHeader";
		message.EM_LinkUniqueID = testNctsHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		mockTransitOperation.Setup(x => x.ReleaseDate).Returns(testDate);

		CombineAssertions(() =>
		{
			AssertReleaseNotification(Constants.ReleaseType.ClosedFullRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);
			AssertReleaseNotification(Constants.ReleaseType.DiscrepancyResolutionPartialRelease, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease);
			AssertReleaseNotification(Constants.ReleaseType.ClosedPartialRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease);
			AssertReleaseNotification(Constants.ReleaseType.DiscrepancyResolutionNoRelease, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);
			AssertEquals("Issue Date should be updated", testDate, testNctsHeader.MovementReferenceIssueDate);

			mockTransitOperation.Setup(x => x.ReleaseIndicator).Returns("9");
			MessageProcessor.ProcessMessage(message);
			AssertEquals("Failed", EDIMessage.Status.Failed, message.EM_Status);
		});

		void AssertReleaseNotification(string releaseIndicator, string expectedCustomsStatus)
		{
			mockTransitOperation.Setup(x => x.ReleaseIndicator).Returns(releaseIndicator);
			MessageProcessor.ProcessMessage(message);
			AssertEquals($"Release Indicator is {releaseIndicator}, {expectedCustomsStatus} is expected", expectedCustomsStatus, testNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		mockTransitOperation = new Mock<ICC025CTransitOperation>();
		dataProviderMock.Setup(m => m.MessageType).Returns("CC025C");
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<ICC025CTransitOperation> mockTransitOperation;
}
