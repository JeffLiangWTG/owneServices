using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC928CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC928CMessageProcessor, ICC928CDataProvider>
{
	public void TestCorrelationIdentifier()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var cidEntryNumber = CusEntryNumber.Load(movementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, Core.Constants.CountryCodes.Netherlands);

		AssertEquals("CorrelationIdentifier should be updated", "2a8c6fa0-43fc4973-98ef-8fb22eb94c87", cidEntryNumber.CE_EntryNum);
	}

	public void TestCustomsStatus_015_Blank_AddDeclTypeA()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";
			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		};
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), setupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;

		AssertEquals("CusInBondMoveHeader - BM_CustomsStatus should be set to 'ACK - Acknowledged'", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, movementHeader.BM_CustomsStatus);
	}

	public void TestEdiMessageStatus_PhaseIsEmpty()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, string.Empty, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		AssertEquals("EDIMessage - EM_Status should be set to 'PRS - Processed'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
	}

	public void TestDiscardedMessage_CustomsStatusIsNotEmpty()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - EM_Status should be set to 'DCD - Discarded", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - BM_CustomsStatus should not be changed", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, movementHeader.BM_CustomsStatus);

			var expectedMessage = "The message with interchange is discarded, because the 'Phase Status' of the declaration has not the value 015 or blanks and the 'Status at Customs' has not the value blanks.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	public void TestDiscardedMessage_CustomsStatusIsNotEmptyAndPhaseIsEmpty()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, string.Empty, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - EM_Status should be set to 'DCD - Discarded", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - BM_CustomsStatus should not be changed", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, movementHeader.BM_CustomsStatus);

			var expectedMessage = "The message with interchange is discarded, because the 'Phase Status' of the declaration has not the value 015 or blanks and the 'Status at Customs' has not the value blanks.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	public void TestDiscardedMessage_CustomsStatusIsNotEmptyAndPhaseIsNot015OrEmpty()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DeparturePhaseList.Codes.Amendment, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - EM_Status should be set to 'DCD - Discarded", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - BM_CustomsStatus should not be changed", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, movementHeader.BM_CustomsStatus);

			var expectedMessage = "The message with interchange is discarded, because the 'Phase Status' of the declaration has not the value 015 or blanks and the 'Status at Customs' has not the value blanks.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	public void TestMRN()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertEquals("MRN should be updated", "TestMRN", nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
	}

	protected override ICC928CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC928CDataProvider>(p =>
			p.LRN == "TestLRN" &&
			p.MRN == "TestMRN" &&
			p.CorrelationIdentifier == "2a8c6fa0-43fc4973-98ef-8fb22eb94c87"
		);
	}

	protected override string InitialCustomsStatus => string.Empty;

	protected override string InitialPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";
		nctsHeader.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.PreLodged;
}
