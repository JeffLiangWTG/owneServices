using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC015CSenderTest : NCTSMessageSenderTest<CC015CSender, ICC015C>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CC015CSender(null));
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override ZString EntryType => NctsMessageTypeListNL.Codes.Declaration;

	protected override ZString ParentTableName => Customs.Business.AutoCusInBondMoveHeader.Schema.TableName;

	public void TestPendingTransactionAdded()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader = Factory.New<NL.Business.CusGuaranteeHeader>();
		guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
		guaranteeHeader.CPH_Number = "GUA1";
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_RN_NKCountryCode = "NL";
		guaranteeHeader.CPH_Balance = 1000m;

		var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
		transaction.CPL_Reference = "OPENING";
		transaction.CPL_TranValue = 1000m;
		transaction.CPL_Comment = "OPENING";
		transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;

		nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1234567";

		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145;

		var guaranteeZero = nctsHeader.MovementHeader.Guarantees.AddNew();
		guaranteeZero.PW_BondNumber = "GUA1";
		guaranteeZero.PW_BondAmount = 0;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Initial situation: 1 transaction on the guarantee header", 1, guaranteeHeader.GetTransactions().Count());
			messageSender.Send();
			AssertEquals("There should be 2 transactions on the guarantee header", 2, guaranteeHeader.GetTransactions().Count());
			var newTransaction = guaranteeHeader.GetTransactions().ToArray()[1];
			AssertEquals("New transaction should be referenced to the LRN of the NCTS Header", "LRN1234567", newTransaction.CPL_Reference);
			AssertEquals("New transaction should be pending", "PND", newTransaction.CPL_TransactionStatus);
		});
	}

	[TestDate]
	public void TestSettingValuationDate()
	{
		nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

		messageSender.Send();

		AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
	}
}
