using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC028CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC028CMessageProcessor, ICC028CDataProvider>
{
	public void TestMRN()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertEquals("MRN should be updated", "TestMRN", nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
		AssertEquals("BM_EntryDate should be updated", new ZDateTime(2024, 2, 29), movementHeader.BM_EntryDate);
	}

	protected override ICC028CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC028CDataProvider>(p =>
			p.LRN == "TestLRN" &&
			p.MRN == "TestMRN" &&
			p.DeclarationAcceptanceDate == new DateTime(2024, 2, 29) &&
			p.CorrelationIdentifier == "532e7463-b89a40e8-9348-3ee366abc850"
		);
	}

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => { nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN"; };

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	protected override string ExpectedPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;
}
