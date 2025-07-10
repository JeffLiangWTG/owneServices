using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC028MessageProcessor))]
sealed class CC028MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC028MessageProcessor, IIE028>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE028;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC028CMessageInterpreter);

	public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";
		const string testLrn = "TST_LRN_01234";
		var testDate = CargoWise.Types.ZDateTime.UtcNow.Date;

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.MovementHeader.BM_AdditionalDeclarationType = "D";

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		dataProviderMock.Setup(x => x.LRN).Returns(testLrn);
		dataProviderMock.Setup(x => x.DeclarationAcceptanceDate).Returns(testDate.ToDateTime());

		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: "ACC", testNctsHeader.MovementHeader.BM_MessageStatus);
			var movementHeader = testNctsHeader.MovementHeader;
			AssertEquals("Movement header BM_CustomsStatus", expected: "MRN", movementHeader.BM_CustomsStatus);
			AssertEquals("Movement header BM_EntryDate", expected: testDate, movementHeader.BM_EntryDate);
			AssertEquals("Movement header BM_AdditionalDeclarationType", expected: "A", movementHeader.BM_AdditionalDeclarationType);

			var cusEntryNum = testNctsHeader.MovementReferenceEntryNumber;
			AssertEquals("Entry number CE_EntryNum", expected: testMrn, cusEntryNum.CE_EntryNum);
			AssertEquals("Entry number CE_ParentTable", expected: "CusInBondHeader", cusEntryNum.CE_ParentTable);
			AssertEquals("Entry number CE_ParentID", expected: testNctsHeader.PK, cusEntryNum.CE_ParentID);
			AssertEquals("Entry number CE_EntryType", expected: "MRN", cusEntryNum.CE_EntryType);

			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("CusInBondHeader Should have a CES event logged", true, testNctsHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus));
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24num123";
		const string mrn = "TEST_MRN";
		const string lrn = "TEST_LRN";

		var correlationIdentifier = transmitMessageNum.Insert(2, MessageNameList.Codes.IE015);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE028_MRN_Allocated_({lrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC028C");
	}
}
