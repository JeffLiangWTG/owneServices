using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupTaxConfigurationPivot))]
	sealed class AccTaxOverrideGroupTaxConfigurationPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
		}

		public void TestTaxRate()
		{
			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 256;
			taxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 100;
			AssertEquals("Rate", 2.56m, taxOverrideGroupTaxConfigurationPivot.Rate);

			taxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 2556;
			taxOverrideGroupTaxConfigurationPivot.AXP_RateDenominator = 0;
			AssertEquals("Rate", 0m, taxOverrideGroupTaxConfigurationPivot.Rate);
		}

		public void TestRateCorrectDecimalPlaces()
		{
			var decimalPlacesOnRate = typeof(AccTaxOverrideGroupTaxConfigurationPivot).GetProperty(nameof(AccTaxOverrideGroupTaxConfigurationPivot.Rate)).GetAttribute<DecimalPlacesAttribute>().DecimalPlaces;
			AssertEquals(9, decimalPlacesOnRate);
		}

		public void TestLedger()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Ledger = ZString.Empty;

			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertEquals(ZString.Empty, taxOverrideGroupTaxConfigurationPivot.Ledger);

			taxConfiguration.ETC_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(LedgerTypes.AccountsPayable, taxOverrideGroupTaxConfigurationPivot.Ledger);
		}

		public void TestTaxConfigurationDescription()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Description = ZString.Empty;

			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertEquals(ZString.Empty, taxOverrideGroupTaxConfigurationPivot.TaxConfigurationDescription);

			taxConfiguration.ETC_Description = "TEST DESCRIPTION";
			AssertEquals("TEST DESCRIPTION", taxOverrideGroupTaxConfigurationPivot.TaxConfigurationDescription);
		}

		public void TestTaxConfigurationBranch()
		{
			var company = GlbCompany.CurrentCompany;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "AAA";
			Factory.Save();

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = company.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertEquals("Tax Branch EMPTY when Company Level Tax Configuration", ZString.Empty, taxOverrideGroupTaxConfigurationPivot.TaxConfigurationBranch);

			taxConfiguration.ETC_ParentId = branch.PK;
			taxConfiguration.ETC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			AssertEquals("Tax Branch displays branch code when branch Level Tax Configuration", "AAA", taxOverrideGroupTaxConfigurationPivot.TaxConfigurationBranch);
		}

		public void TestTaxRateSource()
		{
			var taxID = Factory.NewWithValidTestData<AccTaxRate>();
			taxID.AT_RateSource = "TID";

			var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
			AssertEquals(ZString.Empty, taxOverrideGroupTaxConfigurationPivot.TaxRateSource);

			taxOverrideGroupTaxConfigurationPivot.AXP_AT_TaxID = taxID.PK;
			AssertEquals("TID", taxOverrideGroupTaxConfigurationPivot.TaxRateSource);
		}
	}
}
