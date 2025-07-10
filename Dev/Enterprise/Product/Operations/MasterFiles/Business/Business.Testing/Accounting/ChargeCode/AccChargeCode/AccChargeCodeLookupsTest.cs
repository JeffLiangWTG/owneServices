using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccChargeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGLAccountCollection()
		{
			var globalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			globalGLHeader.AG_AccountType = "BSH";
			globalGLHeader.AG_IsGlobal = true;
			var nonGlobalGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader1.AG_AccountType = "BSH";
			nonGlobalGLHeader1.AG_IsGlobal = false;
			var nonGlobalGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader2.AG_AccountType = "BSH";
			nonGlobalGLHeader2.AG_IsGlobal = false;
			nonGlobalGLHeader2.CompanyFilters.AddNew().ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			var lookups = new AccChargeCodeLookups(globalChargeCode);
			var result = Factory.Load<AccGLHeader>(lookups.GLAccrualAccountCollection.CompleteFilter);
			AssertEquals(true, result.Any(x => x.PK == globalGLHeader.PK));
			AssertEquals(false, result.Any(x => x.PK == nonGlobalGLHeader1.PK));
			AssertEquals(false, result.Any(x => x.PK == nonGlobalGLHeader2.PK));

			var nonGlobalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			nonGlobalChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			lookups = new AccChargeCodeLookups(nonGlobalChargeCode);
			result = Factory.Load<AccGLHeader>(lookups.GLAccrualAccountCollection.CompleteFilter);
			AssertEquals(true, result.Any(x => x.PK == globalGLHeader.PK));
			AssertEquals(true, result.Any(x => x.PK == nonGlobalGLHeader2.PK));
		}

		public void TestAC_IATACode_List()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeCodeLookups lookups = new AccChargeCodeLookups(chargeCode);
			byte[] expected = new CodeDescriptionPairList(OLookUpEditType.AWBChargeCodes).ToXMLByteArray();
			byte[] generated = lookups.AC_IATACode_List.ToXMLByteArray();
			AssertEquals(expected.Length, generated.Length);
			for (int i = 0; i < expected.Length; i++)
			{
				AssertEquals(expected[i], generated[i]);
			}
		}

		public void TestChargeCodeOtherGroupsList()
		{
			const string expectedValues =
				"AGC - Agent\r\n" +
				"PRC - Principal" +
				"";

			AssertMultilineASCIIEquals("ChargeOtherGroups", expectedValues, Lookups.ChargeOtherGroupsList.ElementsAsString);
		}

		public void TestTaxRateCollectionUseChargeCodeCompany()
		{
			GlbCompany chargeCodeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_GC = chargeCodeCompany.PK;

			AccChargeCodeLookups lookups = new AccChargeCodeLookups(chargeCode);
			AccTaxRate chargeTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			chargeTaxRate.AT_RN_NKCountry = chargeCodeCompany.GC_RN_NKCountryCode;

			AccTaxRate currentCompanyTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			currentCompanyTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var lookupFilter = chargeCode.Lookups.TaxRateCollection.CompleteFilter;
			var taxRates = Factory.Load<AccTaxRate>(lookupFilter);

			AssertCollectionContains(chargeTaxRate, taxRates);
			AssertCollectionNotContains(currentCompanyTaxRate, taxRates);
		}

		public void TestTaxRateCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			var lookupFilter = chargeCode.Lookups.TaxRateCollection.CompleteFilter;
			var taxRates = Factory.Load<AccTaxRate>(lookupFilter);
			Assert("Collection should present only the VAT Tax System", taxRates.All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestWithholdingCollectionUsesChargeCodeCompany()
		{
			GlbCompany chargeCodeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_GC = chargeCodeCompany.PK;

			AccChargeCodeLookups lookups = new AccChargeCodeLookups(chargeCode);
			AccWithholding chargeWithholding = Factory.NewWithValidTestData<AccWithholding>();
			chargeWithholding.AW_GC = chargeCodeCompany.PK;

			AccWithholding currentCompanyWithholding = Factory.NewWithValidTestData<AccWithholding>();
			currentCompanyWithholding.AW_GC = GlbCompany.CurrentCompany.PK;

			var lookupFilter = chargeCode.Lookups.WithholdingCollection.CompleteFilter;
			var withHoldingCollection = Factory.Load<AccWithholding>(lookupFilter);

			AssertCollectionContains(chargeWithholding, withHoldingCollection);
			AssertCollectionNotContains(currentCompanyWithholding, withHoldingCollection);
		}

		public void TestTaxOverrideGroupsUsesChargeCodeCompany()
		{
			GlbCompany chargeCodeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_GC = chargeCodeCompany.PK;

			AccChargeCodeLookups lookups = new AccChargeCodeLookups(chargeCode);
			AccTaxOverrideGroup chargeGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			chargeGroup.AX_RN_NKCountry = chargeCodeCompany.GC_RN_NKCountryCode;

			AccTaxOverrideGroup currentCompanyGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			currentCompanyGroup.AX_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();
			var lookupFilter = chargeCode.Lookups.TaxOverrideGroups.CompleteFilter;
			var taxOverrideGroupCollection = Factory.Load<AccTaxOverrideGroup>(lookupFilter);

			AssertCollectionContains(chargeGroup, taxOverrideGroupCollection);
			AssertCollectionNotContains(currentCompanyGroup, taxOverrideGroupCollection);
		}

		public void TestGLDisbursementSurplusAccountCollection()
		{
			SetUpAccountCollectionTests();

			var testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var glDisbursementSurplusAccountCollection = testAccChargeCode.Lookups.GLDisbursementSurplusAccountCollection;
			glDisbursementSurplusAccountCollection.Load();

			Assert("Profit and Loss GL Accounts should be in the collection", glDisbursementSurplusAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", glDisbursementSurplusAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = true;
			Factory.Save();
			glDisbursementSurplusAccountCollection.Load();

			Assert("Control accounts should not be in the collection", !glDisbursementSurplusAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", glDisbursementSurplusAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();
			glDisbursementSurplusAccountCollection.Load();

			Assert("Total account should not be in the collection", !glDisbursementSurplusAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));
			Assert("Profit and Loss GL Accounts should be in the collection", glDisbursementSurplusAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
		}

		public void TestGLDisbursementShortfallAccountCollection()
		{
			SetUpAccountCollectionTests();
			var testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var glDisbursementShortfallAccountCollection = testAccChargeCode.Lookups.GLDisbursementShortfallAccountCollection;
			glDisbursementShortfallAccountCollection.Load();

			Assert("Profit and Loss GL Accounts should be in the collection", glDisbursementShortfallAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", glDisbursementShortfallAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = true;
			Factory.Save();
			glDisbursementShortfallAccountCollection.Load();

			Assert("Control accounts should not be in the collection", !glDisbursementShortfallAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", glDisbursementShortfallAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();
			glDisbursementShortfallAccountCollection.Load();

			Assert("Total account should not be in the collection", !glDisbursementShortfallAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));
			Assert("Profit and Loss GL Accounts should be in the collection", glDisbursementShortfallAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
		}

		protected void SetUpAccountCollectionTests()
		{
			TestProfitAndLossGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			TestProfitAndLossGLHeader.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestProfitAndLossGLHeader.AG_AccountNum = "123";

			TestBalanceSheetAccountGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			TestBalanceSheetAccountGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountNum = "456";

			Factory.Save();
		}
		protected AccGLHeader TestProfitAndLossGLHeader;
		protected AccGLHeader TestBalanceSheetAccountGLHeader;

		#region Implementation

		AccChargeCode ChargeCode
		{
			get { return chargeCode ?? (chargeCode = Factory.New<AccChargeCode>()); }
		}
		AccChargeCode chargeCode;

		AccChargeCodeLookups Lookups
		{
			get { return ChargeCode.Lookups; }
		}

		#endregion
	}
}
