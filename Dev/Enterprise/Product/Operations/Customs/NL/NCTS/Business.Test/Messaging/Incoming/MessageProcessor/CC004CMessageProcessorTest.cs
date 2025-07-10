using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC004CMessageProcessorTest : NCTSResponseMessageProcessorAbstractTest<CC004CMessageProcessor, ICC004CDataProvider>
{
	protected override ICC004CDataProvider GetMessageDataProviderMock()
	{
		return Mock.Of<ICC004CDataProvider>(m =>
			m.MRN == "TestMRN" &&
			m.CorrelationIdentifier == "TestId" &&
			m.AmendmentAcceptanceDateTime == new DateTime(1994, 2, 1) &&
			m.AmendmentSubmissionDateTime == new DateTime(1994, 2, 1)
		);
	}
	protected override Action<NctsHeader> SetupNctsHeader => (nctsHeader) =>
	{
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_SubApplicationCode = "D";
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		nctsHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTimeOffset.Now, reference: NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit));
	};

	public void TestGuaranteeTransactionPNDorCON()
	{
		Action<NctsHeader> setupNctsHeader = (nctsHeader) =>
		{
			var eoriCode1 = "123456789000";
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Netherlands);
			mrnEntryNumber.CE_EntryNum = "TestMRN";

			var movementHeader = nctsHeader.MovementHeader;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_SubApplicationCode = "D";
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_RN_NKCountryCode = "NL";

			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;

			guaranteeHeader.AddTransaction("2204528148060XXXXXX", "CMT-TO-CONF", "111", "", 100m, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader.AddTransaction("2204528148060XXXXXX", "CMT-CON", "111", "", -10, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);

			nctsHeader.MovementHeader.Guarantees.DeleteAll();
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondNumber = "19860101";
			guarantee.PW_BondAmount = 150m;
			movementHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: DateTime.Now, reference: "REL"));
		};

		var dataProviderMock = Mock.Of<ICC004CDataProvider>(m =>
									m.MRN == "TestMRN"
									);

		var incomingMessage = SetupAndProcessMessage(NctsMessageStatusList.Codes.DepartureDeclarationSent, NctsMovementHeaderTransactionStatusList.Codes.Declaration, InitialMessageStatus, dataProviderMock, setupNctsHeader);

		var movementHeader = incomingMessage.EM_LinkedObject as NctsDepartureMovementHeader;
		var nctsHeader = movementHeader.Header;

		var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(CusGuaranteeHeader));
		var guaranteeHeader = Factory.LoadTop1<CusGuaranteeHeader>(guaranteeHeaderQuery);
		var transactions = guaranteeHeader.GetTransactions();
		AssertEquals("There should be 2 transactions on the guarantee header", 2, transactions.Count());

		var transactionCmtToConf = transactions.ToArray()[0];
		AssertEquals("New transaction should be referenced to the LRN of the NCTS Header", "2204528148060XXXXXX", transactionCmtToConf.CPL_Reference);
		AssertEquals("After processing, the status of the first OBL transaction should be CONF (Confirmed)", PermitTransactionStatusList.Codes.Confirmed, transactionCmtToConf.CPL_TransactionStatus);

		var transactionCmtCon = transactions.ToArray()[1];
		AssertEquals("New transaction should be referenced to the LRN of the NCTS Header", "2204528148060XXXXXX", transactionCmtCon.CPL_Reference);
		AssertEquals("After processing, the status of the first TRA transaction should be CONF (Confirmed)", PermitTransactionStatusList.Codes.Confirmed, transactionCmtCon.CPL_TransactionStatus);
	}

	protected override string InitialCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

	protected override string ExpectedCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
}
