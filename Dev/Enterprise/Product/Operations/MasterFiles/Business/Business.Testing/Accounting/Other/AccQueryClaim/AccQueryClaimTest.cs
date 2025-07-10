using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccQueryClaim))]
	public abstract class AccQueryClaimTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		AccQueryClaim fQueryClaim;
		AccQueryClaim QueryClaim
		{
			get
			{
				if (fQueryClaim == null)
				{
					fQueryClaim = (AccQueryClaim)Factory.New(GetExpectedBusinessObjectType());
				}
				return fQueryClaim;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BusinessObject result = base.GetNewBusinessObjectForDeleteTest(factory);
			result[AccQueryClaimSchema.AY_OC] = factory.NewWithValidTestData<OrgContact>().PK;
			return result;
		}

		#endregion

		public void TestConcurrencyPolicy()
		{
			AssertEquals(nameof(AccQueryClaim.AY_QueryClaimAmountInfo), ConcurrencyPolicy.Strict, QueryClaim.AY_QueryClaimAmountInfo.ConcurrencyPolicy);
			AssertEquals(nameof(AccQueryClaim.AY_AHInfo), ConcurrencyPolicy.Strict, QueryClaim.AY_AHInfo.ConcurrencyPolicy);
			AssertEquals(nameof(AccQueryClaim.AY_OH_DebtorInfo), ConcurrencyPolicy.Strict, QueryClaim.AY_OH_DebtorInfo.ConcurrencyPolicy);
			AssertEquals(nameof(AccQueryClaim.AY_QueryClaimStatusInfo), ConcurrencyPolicy.Strict, QueryClaim.AY_QueryClaimStatusInfo.ConcurrencyPolicy);
		}

		public virtual void TestIsHoldOptionVisible()
		{
			AssertEquals(false, QueryClaim.IsHoldOptionVisible);
		}

		public void TestAutoLoggingIsEnabled()
		{
			Assert(QueryClaim.IsAutoLoggedInternal);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccQueryClaim()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = QueryClaim.Ledger;
			header.AH_TransactionType = TransactionTypes.Invoice;
			QueryClaim.AY_AH = header.PK;

			var osList = new List<string>
				{
					nameof(QueryClaim.AY_QueryClaimAmount)
				};

			var tester = new DecimalPlacesAttributeTester(QueryClaim);
			tester.CheckNonLocalCurrency(osList, nameof(QueryClaim.CurrencyDecimals), nameof(QueryClaim.TransactionHeader.AH_RX_NKTransactionCurrency), QueryClaim.TransactionHeader);
		}

		public void TestSettingAY_OH_DebtorSetsDefaultAY_OC()
		{
			Assert("Default", QueryClaim.AY_OC.IsEmpty);
			QueryClaim.AY_OH_Debtor = Factory.NewWithValidTestData<OrgHeader>().PK;
			Assert("Default", !QueryClaim.AY_OC.IsEmpty);
		}

		public void TestDetailsGetAndSet()
		{
			AssertEquals("Default", "", QueryClaim.Details);
			QueryClaim.Details = "some string";
			AssertEquals("Details filled in", "some string", QueryClaim.Details);
		}

		public void TestDetailsGetsWrittenToTheDatabase()
		{
			QueryClaim.FillWithValidTestData();
			QueryClaim.Details = "something";
			QueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AccQueryClaim queryClaimReloaded = (AccQueryClaim)factory2.Load(GetExpectedBusinessObjectType(), QueryClaim.PK);
			AssertEquals("Details written in database", "something", queryClaimReloaded.Details);
		}

		public virtual void TestSetDefaultValues()
		{
			Assert("Claim Status should not have errors", !QueryClaim.AY_QueryClaimStatusInfo.HasErrors());
			Assert("Claim Type should not have errors", !QueryClaim.AY_QueryClaimTypeInfo.HasErrors());
			Assert("Claim Reason Code should not have errors", !QueryClaim.AY_QueryClaimReasonCodeInfo.HasErrors());

			AssertEquals("Next Follow Up is defaulted", ZDateTime.Today.AddDays(7), QueryClaim.AY_QueryClaimNextFollowUp);
			AssertEquals("Branch is Current Branch", Env.CurrentBranch.PK, QueryClaim.AY_GB);
			AssertEquals("Staff member is Current User", Env.CurrentUser.Initials, QueryClaim.AY_GS_NKStaffAssignedTo);
		}

		public void TestAY_GS_Creator()
		{
			QueryClaim.Logs.AddedLog.SL_GS_NKUser = "ABC";
			QueryClaim.AY_OH_Debtor = Factory.NewWithValidTestData<OrgHeader>().PK;
			QueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();
			AssertEquals("ABC", QueryClaim.AY_GS_NKCreator);
		}

		[TestDate(2008, 08, 27, 11, 3, 0)]
		public void TestAddToLog()
		{
			QueryClaim.Details = "Test Message";
			QueryClaim.AddToLog("Another test message.");
			AssertEquals(@"27-Aug-08 11:03 " + GlbStaff.CurrentUser.GS_Code + @" - EDI - Another test message.
----------------------------------------------------------------------------------------------------------------------
Test Message", QueryClaim.Details);
		}

		public void TestAY_RX_TransactionCurrencyCode()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = QueryClaim.Ledger;
			header.AH_TransactionType = TransactionTypes.Invoice;
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_IsActive, true));
			header.AH_RX_NKTransactionCurrency = currency.RX_Code;
			QueryClaim.AY_AH = header.PK;
			AssertEquals(currency.RX_Code, QueryClaim.AY_RX_TransactionCurrencyCode);
		}

		public void TestAY_OH_TransactionBranchOrgProxy()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = QueryClaim.Ledger;
			header.AH_TransactionType = TransactionTypes.Invoice;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = orgProxy.PK;
			header.AH_GB = branch.PK;
			QueryClaim.AY_AH = header.PK;
			AssertEquals(orgProxy.PK, QueryClaim.AY_OH_TransactionBranchOrgProxy);
		}

		public void TestAY_GB_TransactionBranch()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = QueryClaim.Ledger;
			header.AH_TransactionType = TransactionTypes.Invoice;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			header.AH_GB = branch.PK;
			QueryClaim.AY_AH = header.PK;
			AssertEquals(branch.PK, QueryClaim.AY_GB_TransactionBranch);
		}

		public void TestAY_OH_Debtor()
		{
			AssertNotNull("Prerequisite: current branch's org proxy", GlbBranch.CurrentBranch.OrgProxy);

			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			QueryClaim.AY_OH_Debtor = debtor.PK;
			QueryClaim.AY_AH = invoice.PK;
			AssertEquals("Should have returned debtor set on the claim", debtor.PK, QueryClaim.AY_OH_Debtor);

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = orgProxy.PK;
			invoice.AH_GB = branch.PK;
			AssertEquals("Should have returned transaction branch's org proxy", orgProxy.PK, QueryClaim.AY_OH_Debtor);
		}

		public void TestAY_OH_Debtor_Original()
		{
			AssertNotNull("Prerequisite: current branch's org proxy", GlbBranch.CurrentBranch.OrgProxy);

			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			QueryClaim.AY_OH_Debtor = debtor.PK;
			QueryClaim.AY_AH = invoice.PK;
			AssertEquals("Should have returned debtor set on the claim", debtor.PK, QueryClaim.AY_OH_Debtor_Original);

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			branch.GB_OH_OrgProxy = orgProxy.PK;
			invoice.AH_GB = branch.PK;
			AssertEquals("Should have still returned debtor set on the claim", debtor.PK, QueryClaim.AY_OH_Debtor_Original);
		}

		public void TestAY_GB_ReadOnly()
		{
			Assert(QueryClaim.AY_GBInfo.ReadOnly);
		}

		public void TestAY_GS_NKStaffAssignedTo_ReadOnly()
		{
			Assert(QueryClaim.AY_GS_NKStaffAssignedToInfo.ReadOnly);
		}

		public void TestAY_HoldOption()
		{
			AssertEquals(AccountingMasterFilesConstants.CreditorGroupConstants.DefaultHoldOption, QueryClaim.AY_HoldOption);
		}

		public void TestAY_HoldOption_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbCompanySchema.GC_Code.Name });
			QueryClaim.AY_AH = header.PK;

			Assert(!QueryClaim.IsIntercompanyClaim);
			Assert(!QueryClaim.IsPropertiesReadOnlyInternal);
			if (QueryClaim.Ledger == LedgerTypes.AccountsPayable)
			{
				Assert("Only AP claims need to consider hold option.", !QueryClaim.AY_HoldOption_ReadOnly);
			}
			else
			{
				Assert("For non-AP claims, hold option does not involve in logic and should always be true.", QueryClaim.AY_HoldOption_ReadOnly);
			}

			header.AH_GC = nonCurrentCompany.PK;
			Assert(QueryClaim.IsIntercompanyClaim);
			Assert(QueryClaim.IsPropertiesReadOnlyInternal);
			Assert(QueryClaim.AY_HoldOption_ReadOnly);
		}

		public void TestCurrencyAndCurrencyDecimals()
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccQueryClaim queryClaim = (AccQueryClaim)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			queryClaim.AY_AH = invoice.PK;
			invoice.AH_RX_NKTransactionCurrency = string.Empty;
			AssertNull(queryClaim.Currency);
			AssertEquals("Should be Local Currency decimals without currency", GlbCompany.CurrentCompany.LocalCurrency.Decimals, queryClaim.CurrencyDecimals);

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			int saveDecimals = localCurrency.RX_SubUnitRatio;
			try
			{
				localCurrency.RX_SubUnitRatio = 10;
				AssertEquals("One Decimal Place", 1, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals("One Decimal Place", 1, queryClaim.CurrencyDecimals);
			}
			finally
			{
				localCurrency.RX_SubUnitRatio = saveDecimals;
			}

			invoice.AH_RX_NKTransactionCurrency = "IDR";
			AssertNotNull(queryClaim.Currency);
			AssertEquals("IDR", queryClaim.Currency.RX_Code);
			AssertEquals("Should be 0 decimals for IDR", 0, queryClaim.CurrencyDecimals);

			invoice.AH_RX_NKTransactionCurrency = "USD";
			AssertNotNull(queryClaim.Currency);
			AssertEquals("USD", queryClaim.Currency.RX_Code);
			AssertEquals("Should be 2 decimals for USD", 2, queryClaim.CurrencyDecimals);
		}

		public void TestAccQueryClaimWithStmALog()
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			var claim = (AccQueryClaim)Factory.New<Enterprise.Integration.Accounting.IARAccQueryClaim>();
			claim.FillWithValidTestData();
			claim.AY_AH = invoice.PK;
			var log = claim.GetLogs().AddNew(Events.Authorised);
			Factory.Save();

			log = new BusinessObjectFactory().LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_Parent, claim.PK));
			AssertNotNull(log.Master);
		}
	}
}
