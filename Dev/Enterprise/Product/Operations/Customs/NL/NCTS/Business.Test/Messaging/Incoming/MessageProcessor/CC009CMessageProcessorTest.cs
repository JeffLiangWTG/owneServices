using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC009CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC009CMessageProcessor, ICC009CDataProvider>
{
	public void TestProcessingInvalidationFalse_LRN()
	{
		var dataProviderMock = Mock.Of<ICC009CDataProvider>(m => 
									m.LRN == "TestLRN" &&
									m.Invalidation == Mock.Of<INCTSInvalidationProvider>(i => i.Decision == ZBool.False)
								);

		AssertStatus(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, dataProviderMock, SetupNctsHeader, InitialCustomsStatus);
	}

	public void TestProcessingInvalidationFalse_MRN()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "TestMRN";
			nctsHeader.MovementHeader.BM_SubApplicationCode = "D";
			SetupGuarantee(nctsHeader);
		};
		var dataProviderMock = Mock.Of<ICC009CDataProvider>(m =>
									m.MRN == "TestMRN" &&
									m.LRN == "WRONG" &&
									m.Invalidation == Mock.Of<INCTSInvalidationProvider>(i => i.Decision == ZBool.False)
								);

		AssertStatus(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, dataProviderMock, setupNctsHeader, InitialCustomsStatus);
	}

	public void TestProcessingInvalidationTrue_StatusNotWRO()
	{
		AssertStatus(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader, NCTS5DepartureCustomsStatusList.Codes.Cancelled);
	}

	public void TestProcessingInvalidationTrue_StatusWRO()
	{
		AssertStatus(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader, NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed);
	}

	public void TestProcessingInvalidationFalse()
	{
		var dataProviderMock = Mock.Of<ICC009CDataProvider>(m =>
									m.LRN == "TestLRN" &&
									m.Invalidation == Mock.Of<INCTSInvalidationProvider>(i => i.Decision == ZBool.False)
								);

		AssertStatus(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, dataProviderMock, SetupNctsHeader, InitialCustomsStatus);
	}

	public void TestTransactionsOnGuarantee()
	{
		AssertTransactions(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader, 3, 2, 0, 1, expectPositiveTransactionsCreated: true);
	}

	public void TestTransactionsOnGuaranteeWhenDecisionFalse()
	{
		var dataProviderMock = Mock.Of<ICC009CDataProvider>(m =>
									m.LRN == "TestLRN" &&
									m.Invalidation == Mock.Of<INCTSInvalidationProvider>(i => i.Decision == ZBool.False)
								);
		AssertTransactions(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, dataProviderMock, SetupNctsHeader, 2, 1, 1, 0);
	}

	protected override ICC009CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC009CDataProvider>(m =>
			m.LRN == "TestLRN" &&
			m.Invalidation == Mock.Of<INCTSInvalidationProvider>(i => i.Decision == ZBool.True)
		);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

	protected override string InitialPhase => NctsMovementHeaderTransactionStatusList.Codes.Cancellation;

	protected override string InitialMessageStatus => LogicalStatusList.Codes.Sent;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) => 
	{
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "TestLRN";
		SetupGuarantee(nctsHeader);
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Cancelled;

	protected override string ExpectedPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	void SetupGuarantee(NctsHeader nctsHeader)
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(nctsHeader.Factory);
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		GenerateGuaranteeHeader(org, "19860101", "TestLRN", "111", 100m);
		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.Declarant.E2_OA_Address = org.MainAddress.PK;
		var guarantee = nctsHeader.MovementHeader.Guarantees[0];
		guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
		guarantee.PW_BondAmount = 15m;
		nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
	}

	void GenerateGuaranteeHeader(OrgHeader org, ZString pW_Bondnumber, ZString transactionReference, ZString transactionAppId, ZDecimal transactionValue)
	{
		guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_Number = pW_Bondnumber;
		guaranteeHeader.CPH_OH_PermitHolder = org.PK;
		guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
		guaranteeHeader.AddTransaction("OpeningBalance", "CMT-TO-CONF", transactionAppId, "", transactionValue, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
		guaranteeHeader.AddTransaction(transactionReference, "CMT-CONF", transactionAppId, "", -10, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
		guaranteeHeader.AddTransaction(transactionReference, "CMT-PEND", transactionAppId, "", -5, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
	}

	void AssertStatus(string initialCustomsStatus, string initialPhase, string initialMessageStatus, ICC009CDataProvider setupMessageDataProviderMock, Action<NctsHeader> setupNctsHeader, string expectedCustomsStatus)
	{
		incomingMessage = SetupAndProcessMessage(initialCustomsStatus, initialPhase, initialMessageStatus, setupMessageDataProviderMock, setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;
		CombineAssertions(() =>
		{
			AssertEquals("EDIMessage - Status should be 'PRS'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusInBondMoveHeader - Phase should be changed to '015 - Declaration'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
			AssertEquals("NctsHeader - MessageStatus changed to ACC", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);

			if (!expectedCustomsStatus.Equals(initialCustomsStatus))
			{
				AssertEquals("CusInBondMoveHeader - Customs Status should be changed", expectedCustomsStatus, movementHeader.BM_CustomsStatus);
				var logs = movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == expectedCustomsStatus);
				AssertEquals("Event log is created successfully", 1, logs.Count());
			}
			else
			{
				AssertEquals("CusInBondMoveHeader - Customs Status should not be changed", expectedCustomsStatus, movementHeader.BM_CustomsStatus);
			}
		});
	}

	void AssertTransactions(string initialCustomsStatus, string initialPhase, string initialMessageStatus, ICC009CDataProvider setupMessageDataProviderMock, Action<NctsHeader> setupNctsHeader, ZInt expectedTotal, ZInt expectedConfirmed, ZInt expectedPending, ZInt expectedDeleted,bool expectPositiveTransactionsCreated = false)
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, NctsMovementHeaderTransactionStatusList.Codes.Amendment, InitialMessageStatus, setupMessageDataProviderMock, SetupNctsHeader);

		CombineAssertions(() =>
		{
			var transactions = guaranteeHeader.CusGuaranteeLineTransactions.Where(x => x.CPL_Reference == "TestLRN");
			AssertEquals("Number of transactions", expectedTotal, transactions.Count());
			AssertEquals("Confirmed transaction", expectedConfirmed, transactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed));
			AssertEquals("Pending transaction", expectedPending, transactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
			AssertEquals("Deleted transaction", expectedDeleted, transactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Deleted));
			if (expectPositiveTransactionsCreated)
			{
				AssertEquals("Amount of created transaction should be positive", 1, transactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed && x.CPL_TranValue > 0m));
			}
		});
	}

	CusGuaranteeHeader guaranteeHeader;
}
