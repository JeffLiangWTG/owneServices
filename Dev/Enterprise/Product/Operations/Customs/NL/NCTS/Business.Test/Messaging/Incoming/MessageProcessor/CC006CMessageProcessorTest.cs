using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC006CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC006CMessageProcessor, ICC006CDataProvider>
{
	public void TestCustomsGuaranteeEventCreated()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			AssertEquals("CustomsGuaranteeUpdated event created", true, nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsGuaranteeUpdated.Code && x.SL_Reference == NLNctsConstants.Logs.References.CC006CMessageReceived && x.SL_EventTime.ToString("dd/MM/yyyy hh:mm:ss") == "01/04/2022 12:34:56").Any());
		});
	}

	public void TestTransactionsCreated()
	{
		incomingMessage = SetupAndProcessMessage(InitialCustomsStatus, InitialPhase, InitialMessageStatus, GetMessageDataProviderMock(), SetupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		CombineAssertions(() =>
		{
			var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(CusGuaranteeHeader));
			var guaranteeHeader = Factory.LoadTop1<CusGuaranteeHeader>(guaranteeHeaderQuery);
			var transactions = guaranteeHeader.GetTransactions();
			AssertEquals("There should be 4 transactions on the guarantee header", 4, transactions.Count());
			var newTransaction = transactions.ToArray()[3];
			AssertEquals("New transaction should be referenced to the LRN of the NCTS Header", "LRN1234567", newTransaction.CPL_Reference);
			AssertEquals("New transaction should be confirmed", "CON", newTransaction.CPL_TransactionStatus);
			AssertEquals("New transaction should have the opposite amount of the previous confirmed transaction", 500m, newTransaction.CPL_TranValue);
		});
	}

	protected override ICC006CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC006CDataProvider>(p =>
			p.MRN == "22NL000000000012J1" &&
			p.CustomsOfficeOfDestinationActualReferenceNumber == "RefNumber" &&
			p.ArrivalDateAndTimeActual == new DateTime(2022, 4, 1, 12, 34, 56)
		);
	}
	
	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool SetNewPhaseExpected => false;

	protected override bool SetNewCustomsStatusExpected => false;

	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "22NL000000000012J1";

		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader.CPH_Balance = 1000m;

		nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1234567";

		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = 1000m;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.OBL;

		var transactionPND = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transactionPND.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionPND.CPL_TranValue = -20m;
		transactionPND.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionPND.CPL_TransactionStatus = "PND";
		guaranteeHeader.CPH_Balance = 980;

		var transactionCON = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transactionCON.CPL_Reference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		transactionCON.CPL_TranValue = -500m;
		transactionCON.CPL_TransactionType = Customs.Business.PermitTransactionTypeList.Codes.TRA;
		transactionCON.CPL_TransactionStatus = "CON";

		nctsHeader.MovementHeader.Guarantees.DeleteAll();
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145;
	};
}
