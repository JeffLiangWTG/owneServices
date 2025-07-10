using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using CusGuaranteeHeader = Enterprise.Customs.NL.Business.CusGuaranteeHeader;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC051CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC051CMessageProcessor, ICC051CDataProvider>
{
	public void TestDiscardedMessage()
	{
		incomingMessage = SetupAndProcessMessage(NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be set to 'DCD - Discarded'", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - Customs Status should not be changed", NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, movementHeader.BM_CustomsStatus);
			AssertEquals("CusInBondMoveHeader - Phase should not be changed", NCTS5DeparturePhaseList.Codes.Declaration, movementHeader.BM_Phase);
			AssertEquals("NctsHeader - Message Status should not be changed", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);

			var expectedMessage = "The message with interchange was discarded, because the Departure Status of the declaration is not ACK, PRE, MRN, GIV, CO1, CO2, CO3 or AMR.";
			var actualMessage = incomingMessage.Notes.FindByDescription(NLConstants.Notes.Descriptions.ProcessingLog).FirstOrDefault()?.ST_NoteText;
			AssertEquals("Discarded message, Declaration found but with invalid Customs Status - Log reason for discarded message", expectedMessage, actualMessage);
		});
	}

	public void TestGuaranteeTransactionsUpdated()
	{
		SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeaderWithTransactions);

		CombineAssertions(() =>
		{
			AssertEquals("Opening balance should never be deleted.", false, transaction.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
			AssertEquals("New transaction status deleted for Pending transaction.", true, transactionPND.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
			AssertEquals("New transaction status should never be deleted for Confirmed transaction.", false, transactionCON.CPL_TransactionStatus.EqualsIgnoringCase("DEL"));
		});
	}

	protected override ICC051CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC051CDataProvider>(p =>
			p.LRN == "LRN123" &&
			p.MRN == "22NL000000000012J1" &&
			p.NoReleaseMotivationCode == "G1" &&
			p.NoReleaseMotivationText == "Guarantee unknown"
		);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";

		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";
	};

	Action<NctsHeader> SetupNctsHeaderWithTransactions => (nctsHeader) =>
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123";
		nctsHeader.Company.GC_RN_NKCountryCode = "NL";

		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";

		var guaranteeNum1 = "GUA1";
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_RL_NKClosestPort = "NL";
		nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		guaranteeHeader.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
		guaranteeHeader.CPH_Number = guaranteeNum1;
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader.CPH_Balance = 1000m;

		transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = 1000m;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_IsAggregated = true;
		transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

		transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transactionPND.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionPND.CPL_TranValue = -20m;
		transactionPND.CPL_IsAggregated = true;
		transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionPND.CPL_TransactionStatus = "PND";
		guaranteeHeader.CPH_Balance = 980;

		transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transactionCON.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionCON.CPL_TranValue = -500m;
		transactionCON.CPL_IsAggregated = true;
		transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionCON.CPL_TransactionStatus = "CON";

		nctsHeader.MovementHeader.Guarantees.DeleteAll();
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_RN_NKCountryOfIssue = "NL";
		guarantee.PW_BondNumber = guaranteeNum1;
		guarantee.PW_BondAmount = 145;
	};
	BaseCusGuaranteeLineTransaction transaction;
	BaseCusGuaranteeLineTransaction transactionPND;
	BaseCusGuaranteeLineTransaction transactionCON;

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;
}
