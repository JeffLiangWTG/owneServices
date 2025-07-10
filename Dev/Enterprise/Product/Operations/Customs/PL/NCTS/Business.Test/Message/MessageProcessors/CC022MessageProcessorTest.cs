using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC022MessageProcessor))]
sealed class CC022MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC022MessageProcessor, IIE022>
{
	public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";
		const string nctsHeaderNum = "TEST_HEADER_NUM";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;
		testNctsHeader.BH_JobReference = nctsHeaderNum;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);

		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: "ACC", testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Linked object BM_CustomsStatus", expected: "AMR",
				testNctsHeader.MovementHeader.BM_CustomsStatus);

			var cusEntryNum = testNctsHeader.MovementReferenceEntryNumber;
			AssertEquals("Entry number CE_EntryNum", expected: testMrn, cusEntryNum.CE_EntryNum);
			AssertEquals("Entry number CE_ParentTable", expected: "CusInBondHeader", cusEntryNum.CE_ParentTable);
			AssertEquals("Entry number CE_ParentID", expected: testNctsHeader.PK, cusEntryNum.CE_ParentID);
			AssertEquals("Entry number CE_EntryType", expected: "MRN", cusEntryNum.CE_EntryType);

			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE022;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC022CMessageInterpreter);

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC022C");
	}
}
