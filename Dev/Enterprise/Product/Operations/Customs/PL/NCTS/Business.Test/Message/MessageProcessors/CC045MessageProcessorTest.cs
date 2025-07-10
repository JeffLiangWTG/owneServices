using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC045MessageProcessor))]
sealed class CC045MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC045MessageProcessor, IIE045>
{
	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE045;

	protected override Type ExpectedMessageInterpreterType => typeof(CC045CMessageInterpreter);

	protected override bool ExpectedIsFailureNotification => false;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	public void TestEmail()
	{
		const string transitMessageNum = "24num123";
		const string mrn = "TEST_MRN";

		var correlationIdentifier = transitMessageNum.Insert(2, MessageNameList.Codes.IE045);
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE045_Write_Off_({mrn})", transitMessageNum, mrn, lrn: "");
	}

		public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(m => m.WriteOffDate).Returns(DateTime.MinValue);

		MessageProcessor.ProcessMessage(message);

		var cusEntryNum = testNctsHeader.MovementReferenceEntryNumber;
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: "ACC", testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Entry number CE_EntryNum", expected: testMrn, cusEntryNum.CE_EntryNum);
			AssertEquals("Entry number CE_ParentTable", expected: "CusInBondHeader", cusEntryNum.CE_ParentTable);
			AssertEquals("Entry number CE_ParentID", expected: testNctsHeader.PK, cusEntryNum.CE_ParentID);
			AssertEquals("Entry number CE_EntryType", expected: "MRN", cusEntryNum.CE_EntryType);
			AssertEquals("Movement header BM_CustomsStatus", expected: EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, testNctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	[TestDate(year: 2024, month: 09, day: 30, hour: 23, minute: 05, second: 30)]
	public void TestProcessMessage_SameDay()
	{
		const string testMrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		dataProviderMock.Setup(m => m.WriteOffDate).Returns(new DateTime(year: 2024, month: 09, day: 30, hour: 00, minute: 00, second: 00));

		MessageProcessor.ProcessMessage(message);

		var movementHeader = testNctsHeader.MovementHeader;
		var expectedEventTime = new ZDateTime(year: 2024, month: 09, day: 30, hour: 23, minute: 05, second: 30);
		Assert(
			message: "CusInBondHeader should have CLR event logged",
			condition: testNctsHeader.MovementHeader.Logs.HasLogWith(log =>
				log.SL_EventTime == expectedEventTime &&
				log.SL_SE_NKEvent == AutoEvents.CustomsCleared.Code &&
				log.SL_GB_NKBranch == Env.CurrentBranch.Code &&
				log.SL_GE_NKDepartment == Env.CurrentDepartment.Code &&
				log.SL_Reference == movementHeader.BM_CustomsStatus));
	}

	[TestDate(year: 2024, month: 10, day: 01, hour: 01, minute: 02, second: 15)]
	public void TestProcessMessage_NextDay()
	{
		const string testMrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = testMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;

		dataProviderMock.Setup(m => m.WriteOffDate).Returns(new DateTime(year: 2024, month: 09, day: 30, hour: 00, minute: 00, second: 00));

		MessageProcessor.ProcessMessage(message);

		var movementHeader = testNctsHeader.MovementHeader;
		var expectedEventTime = new ZDateTime(year: 2024, month: 09, day: 30, hour: 00, minute: 00, second: 00);
		Assert(
			message: "CusInBondHeader should have CLR event logged",
			condition: testNctsHeader.MovementHeader.Logs.HasLogWith(log =>
				log.SL_EventTime == expectedEventTime &&
				log.SL_SE_NKEvent == AutoEvents.CustomsCleared.Code &&
				log.SL_GB_NKBranch == Env.CurrentBranch.Code &&
				log.SL_GE_NKDepartment == Env.CurrentDepartment.Code &&
				log.SL_Reference == movementHeader.BM_CustomsStatus));
	}

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC045C");
	}
}
