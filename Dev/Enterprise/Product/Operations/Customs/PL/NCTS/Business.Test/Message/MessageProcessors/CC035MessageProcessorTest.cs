using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC035MessageProcessor))]
sealed class CC035MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC035MessageProcessor, IIE035>
{
	const string TestMrn = "1234567ABC";

	public void TestPreProcessMessage()
	{
		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = TestMrn;

		var message = Factory.New<EDIMessage>();

		MessageProcessor.PreProcessMessage(message);
		AssertEquals("EM_ApplicationReference", expected: TestMrn, message.EM_ApplicationReference);
	}

	public void TestProcessMessage()
	{
		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		testNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = TestMrn;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: "ACC", testNctsHeader.MovementHeader.BM_MessageStatus);
			var cusEntryNum = testNctsHeader.MovementReferenceEntryNumber;
			AssertEquals("Entry number CE_EntryNum", expected: TestMrn, cusEntryNum.CE_EntryNum);
			AssertEquals("Entry number CE_ParentTable", expected: "CusInBondHeader", cusEntryNum.CE_ParentTable);
			AssertEquals("Entry number CE_ParentID", expected: testNctsHeader.PK, cusEntryNum.CE_ParentID);
			AssertEquals("Entry number CE_EntryType", expected: "MRN", cusEntryNum.CE_EntryType);
			AssertEquals("Movement header BM_CustomsStatus", expected: EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure, testNctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE015000123";

		var correlationIdentifier = transmitMessageNum;
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(TestMrn);
		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE035_Recovery_Procedure_({TestMrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE035;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC035CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderMock.Setup(m => m.MessageType).Returns("CC035C");
		dataProviderMock.Setup(m => m.MRN).Returns(TestMrn);
		dataProviderMock.Setup(m => m.RecoveryNotification).Returns(Mock.Of<IRecoveryNotification>());
	}
}
