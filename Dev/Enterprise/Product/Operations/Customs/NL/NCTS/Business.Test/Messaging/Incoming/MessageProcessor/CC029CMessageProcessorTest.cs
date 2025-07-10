using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC029CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC029CMessageProcessor, ICC029CDataProvider>
{
	public void TestUpdateMRN()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("MRN should be updated", "TestMRN", nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
			AssertEquals("BM_EntryDate should be updated", new ZDateTime(1986, 9, 20), movementHeader.BM_EntryDate);
			AssertEquals("CE_IssueDate should be updated", new ZDateTime(1994, 2, 1), nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
		});
	}

	public void TestConfirmTransactions()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertEquals("There should be 2 transactions on the guarantee header", 2, guaranteeHeader.GetTransactions().Select(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed).Count());
	}

	public void TestDiscardMessage()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.Acknowledged, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EdiMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);

			var expectedMessage = "The message with interchange is discarded, because the Status at Customs is PRE, ACK, or AMR.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	protected override ICC029CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC029CDataProvider>(m =>
			m.LRN == "TestLRN" &&
			m.MRN == "TestMRN" &&
			m.ReleaseDate == new DateTime(1994, 2, 1) &&
			m.DeclarationAcceptanceDate == new DateTime(1986, 9, 20)
		);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.AcceptedBySystem;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";
		guaranteeHeader = IncomingMessageTestHelper.SetupGuarantee(nctsHeader, "TestLRN");
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

	protected override string ExpectedPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	CusGuaranteeHeader guaranteeHeader;
}
