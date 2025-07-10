using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC055CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC055CMessageProcessor, ICC055CDataProvider>
{
	public void TestTransactionsOnGuarantee_G01()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G01"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G01", shouldBeDeleted: true);
	}

	public void TestTransactionsOnGuarantee_G02()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G02"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G02", shouldBeDeleted: true);
	}

	public void TestTransactionsOnGuarantee_G03()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G03"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G03", shouldBeDeleted: false);
	}

	public void TestTransactionsOnGuarantee_G04()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G04"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G04", shouldBeDeleted: false);
	}

	public void TestTransactionsOnGuarantee_G05()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G05"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G05", shouldBeDeleted: true);
	}

	public void TestTransactionsOnGuarantee_G06()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G06"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G06", shouldBeDeleted: false);
	}

	public void TestTransactionsOnGuarantee_G07()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G07"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G07", shouldBeDeleted: false);
	}

	public void TestTransactionsOnGuarantee_G08()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G08"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G08", shouldBeDeleted: true);
	}

	public void TestTransactionsOnGuarantee_G09()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G09"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G09", shouldBeDeleted: true);
	}
	public void TestTransactionsOnGuarantee_G10()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G10"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G10", shouldBeDeleted: true);
	}

	public void TestTransactionsOnGuarantee_G11()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G11"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G11", shouldBeDeleted: false);
	}

	public void TestTransactionsOnGuarantee_G12()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock("G12"), SetupNctsHeader);
		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		AssertTransactions(nctsHeader, "G12", shouldBeDeleted: true);
	}

	protected override ICC055CDataProvider GetMessageDataProviderMock() => GetMessageDataProviderMock("G01");

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	protected override string InitialPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string InitialMessageStatus => LogicalStatusList.Codes.Accepted;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
		SetupGuaranteeTransaction(nctsHeader);
	};

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

	protected override string ExpectedPhase => NCTS5DeparturePhaseList.Codes.Declaration;

	protected override string ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	ICC055CDataProvider GetMessageDataProviderMock(string reasonCode)
	{
		return Mock.Of<ICC055CDataProvider>(provider =>
			provider.MRN == "TestMRN" &&
			provider.GuaranteeReferences == new[] {
						Mock.Of<INCTSGuaranteeReferenceProvider>(reference =>
						reference.GRN == "TestGRN" &&
						reference.InvalidGuaranteeReasons == new[] {
							Mock.Of<INCTSInvalidGuaranteeReasonProvider>(reason =>
								reason.Code == reasonCode &&
								reason.Text == "TestReasonText"
							),
						})
			}
		);
	}

	void SetupGuaranteeTransaction(NctsHeader nctsHeader)
	{
		var guaranteeNum1 = "TestGRN";
		var guaranteeNum2 = "GUA2";

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.NewWithValidTestData<NL.Business.CusGuaranteeHeader>();
		guaranteeHeader.CPH_Number = guaranteeNum1;
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader.CPH_Balance = 1000m;
		guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

		var guaranteeHeader2 = Factory.NewWithValidTestData<NL.Business.CusGuaranteeHeader>();
		guaranteeHeader2.CPH_Number = guaranteeNum2;
		guaranteeHeader2.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader2.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader2.CPH_Type = "TRA";
		guaranteeHeader2.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader2.CPH_Balance = 1000m;
		guaranteeHeader2.CPH_OH_PermitHolder = orgHeader.PK;

		nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1234567";

		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = 1000m;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_IsAggregated = true;
		transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

		var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transactionPND.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionPND.CPL_TranValue = -20m;
		transactionPND.CPL_IsAggregated = true;
		transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionPND.CPL_TransactionStatus = "PND";
		transactionPND.CPL_Comment = "transactionPND";
		guaranteeHeader.CPH_Balance = 980;

		var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transactionCON.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionCON.CPL_TranValue = -500m;
		transactionCON.CPL_IsAggregated = true;
		transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionCON.CPL_TransactionStatus = "CON";
		transactionCON.CPL_Comment = "transactionCON";

		var transaction2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
		transaction2.CPL_Reference = "OPENING";
		transaction2.CPL_TranValue = 1000m;
		transaction2.CPL_Comment = "OPENING";
		transaction2.CPL_IsAggregated = true;
		transaction2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;
		transaction2.CPL_Comment = "transaction2";

		var transactionPND2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
		transactionPND2.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionPND2.CPL_TranValue = -20m;
		transactionPND2.CPL_IsAggregated = true;
		transactionPND2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionPND2.CPL_TransactionStatus = "PND";
		transactionPND2.CPL_Comment = "transactionPND2";
		guaranteeHeader2.CPH_Balance = 980;

		var transactionCON2 = guaranteeHeader2.CusGuaranteeLineTransactions.AddNew();
		transactionCON2.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionCON2.CPL_TranValue = -500m;
		transactionCON2.CPL_IsAggregated = true;
		transactionCON2.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionCON2.CPL_TransactionStatus = "CON";
		transactionCON2.CPL_Comment = "transactionCON2";

		nctsHeader.MovementHeader.Guarantees.DeleteAll();
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = guaranteeNum1;
		guarantee.PW_BondAmount = 145;
		guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

		var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = guaranteeNum2;
		guarantee2.PW_CPH_Guarantee = guaranteeHeader2.PK;
		guarantee.PW_BondAmount = 999;

		Factory.Save();
	}

	void AssertTransactions(NctsHeader nctsHeader, string errorCode, bool shouldBeDeleted)
	{
		var guaranteeHeader = nctsHeader.MovementHeader.Guarantees[0].CusGuarantee;
		var guaranteeHeader2 = nctsHeader.MovementHeader.Guarantees[1].CusGuarantee;
		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.FirstOrDefault(x => x.CPL_Comment == "OPENING");
		var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.FirstOrDefault(x => x.CPL_Comment == "transactionPND");
		var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.FirstOrDefault(x => x.CPL_Comment == "transactionCON");
		var transaction2 = guaranteeHeader2.CusGuaranteeLineTransactions.FirstOrDefault(x => x.CPL_Comment == "transaction2");
		var transactionPND2 = guaranteeHeader2.CusGuaranteeLineTransactions.FirstOrDefault(x => x.CPL_Comment == "transactionPND2");
		var transactionCON2 = guaranteeHeader2.CusGuaranteeLineTransactions.FirstOrDefault(x => x.CPL_Comment == "transactionCON2");

		AssertEquals("Opening balance should never be deleted. Code = " + errorCode, false, transaction.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Deleted));
		AssertEquals("New transaction status deleted for Pending transaction. Code = " + errorCode, shouldBeDeleted, transactionPND.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Deleted));

		AssertEquals("New transaction status should never be deleted for Confirmed transaction. Code = " + errorCode, false, transactionCON.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Deleted));
		AssertEquals("Opening balance should never be deleted on guarantee that has no error. Code = " + errorCode, false, transaction2.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Deleted));
		AssertEquals("Transaction PND should never be deleted on guarantee that has no error. Code = " + errorCode, false, transactionPND2.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Deleted));
		AssertEquals("Transaction CON should never be deleted on guarantee that has no error. Code = " + errorCode, false, transactionCON2.CPL_TransactionStatus.EqualsIgnoringCase(PermitTransactionStatusList.Codes.Deleted));
	}
}
