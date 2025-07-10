using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC013CSenderTest : NCTSMessageSenderTest<CC013CSender, ICC013C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC013CSender(null));
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Amendment;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.Amendment;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

	public void TestTransactionAddedForDeletedGuarantee()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		guaranteeHeader.CPH_Balance = 1000.0m;
		guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
		var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader2.CPH_Number = "GUA2";
		guaranteeHeader2.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader2.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader2.CPH_Type = "TRA";
		guaranteeHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		guaranteeHeader2.CPH_Balance = 2000.0m;
		guaranteeHeader2.CPH_OH_PermitHolder = org1.PK;
		var transaction2 = guaranteeHeader2.AddTransaction("OPENING2", "OPENING2", ZString.Empty, ZString.Empty, 2000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145.0m;
		guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

		guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"001",
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee.PW_BondNumber = "GUA2";
		guarantee.PW_CPH_Guarantee = guaranteeHeader2.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial balance GUA1", 855.0m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);
			AssertEquals("Initial balance GUA2", 2000.0m, guaranteeHeader2.CPH_Calc_TotalBalanceIncludingPendingDecimal);
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
			AssertEquals("Initial situation GUA2", 1, guaranteeHeader2.GetTransactions().Count());
			AssertEquals("Booked amount on guarantee", guaranteeHeader.GetBookedAmountOnGuarantee(nctsHeader.MovementHeader.BM_PaperlessInbondNum), guarantee.PW_BondAmount * -1);
			messageSender.Send();

			AssertEquals("New balance GUA1", 1000.0m, guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal);
			AssertEquals("Extra transaction on GUA1", 3, guaranteeHeader.GetTransactions().Count());
			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be 145", 145.0m, newTransactionGUA1.CPL_TranValue);

			AssertEquals("New balance GUA2", 1855.0m, guaranteeHeader2.CPH_Calc_TotalBalanceIncludingPendingDecimal);
			AssertEquals("Extra transaction on GUA2", 2, guaranteeHeader2.GetTransactions().Count());
			var newTransactionGUA2 = guaranteeHeader2.GetTransactions().Last();
			AssertEquals("New transaction GUA2 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA2.CPL_TransactionStatus);
			AssertEquals("New transaction GUA2 should reference the LRN of the NCTS Header", "LRN123456789", newTransactionGUA2.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -145", -145.0m, newTransactionGUA2.CPL_TranValue);
		});
	}

	public void TestTransactionAddedForChangedGuaranteeAmount()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "BE";
		guaranteeHeader.CPH_Balance = 1000.0m;
		guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
		var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

		nctsHeader.GetEffectiveGuarantees().RemoveAndDeleteAll();
		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145.0m;

		guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"001",
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee.PW_BondAmount = 150.0m;
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());

			messageSender.Send();

			AssertEquals("Extra transaction on GUA1", 3, guaranteeHeader.GetTransactions().Count());
			var newTransactionGUA1 = guaranteeHeader.GetTransactions().Last();
			AssertEquals("New transaction GUA1 should have status 'PND'", Customs.Business.PermitTransactionStatusList.Codes.Pending, newTransactionGUA1.CPL_TransactionStatus);
			AssertEquals("New transaction GUA1 should reference the LRN of the NCTS Header", "LRN123456789", newTransactionGUA1.CPL_Reference);
			AssertEquals("New transaction GUA1 amount must be -5", -5.0m, newTransactionGUA1.CPL_TranValue);
		});
	}

	[TestDate]
	public void TestSettingValuationDate()
	{
		messageSender.Send();

		AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);

		nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;
		messageSender.Send();

		AssertEquals(ZDateTime.BrettsBirthday, nctsHeader.MovementHeader.BM_ValuationDate);
	}
}
