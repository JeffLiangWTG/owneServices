using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxRate))]
	sealed class AccOrgTaxRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOnSaving_OrgTaxConfigurationIsNull_ShouldNotThrowNullReferenceException()
		{
			OrgTaxRate = Factory.NewWithValidTestData<AccOrgTaxRate>();
			OrgTaxRate.OTR_StartDate = new ZDate(2022, 06, 05);
			OrgTaxRate.OTR_EndDate = new ZDate(2022, 06, 25);
			OrgTaxRate.OTR_Source = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code;
			OrgTaxRate.OTR_RateNumerator = 6;
			OrgTaxRate.OTR_RateDenominator = 1;

			OrgTaxRate.OTR_OTC = ZGuid.Empty;
			AssertNull(OrgTaxConfiguration);
			AssertNoExceptionThrown("OrgTaxConfiguration is null should not throw NullReferenceException.", () => OrgTaxRate.OnSaving());

			OrgTaxRate.OTR_OTC = ZGuid.Invalid;
			AssertNull(OrgTaxConfiguration);
			AssertNoExceptionThrown("OrgTaxConfiguration is null should not throw NullReferenceException.", () => OrgTaxRate.OnSaving());
		}

		public void TestRateCorrectDecimalPlaces()
		{
			var decimalPlacesOnRate = typeof(AccOrgTaxRate).GetProperty(nameof(AccOrgTaxRate.Rate)).GetAttribute<DecimalPlacesAttribute>().DecimalPlaces;
			AssertEquals(9, decimalPlacesOnRate);
		}

		public void TestSetTaxRateFromRateNumeratorValueChange()
		{
			OrgTaxRate = Factory.NewWithValidTestData<AccOrgTaxRate>();
			AssertEquals("Precondition : Rate is", 0m, OrgTaxRate.Rate);

			OrgTaxRate.OTR_RateNumerator = 25;
			OrgTaxRate.OTR_RateDenominator = 10;
			AssertEquals("Setting OTR_RateNumerator sets Rate value", 2.5M, OrgTaxRate.RateInfo.Value);

			OrgTaxRate.OTR_RateNumerator = 1000;
			AssertEquals("Setting OTR_RateNumerator sets Rate value", 100M, OrgTaxRate.Rate);

			OrgTaxRate.OTR_RateNumerator = 120;
			AssertEquals("Setting OTR_RateNumerator sets Rate value", 12M, OrgTaxRate.Rate);
		}

		public void TestSetTaxRateFromRateDenominatorValueChange()
		{
			OrgTaxRate = Factory.NewWithValidTestData<AccOrgTaxRate>();
			AssertEquals("Precondition : Rate is", 0m, OrgTaxRate.Rate);

			OrgTaxRate.OTR_RateNumerator = 32;
			OrgTaxRate.OTR_RateDenominator = 0;
			AssertEquals("Setting OTR_RateDenominator sets Rate value", 0M, OrgTaxRate.Rate);

			OrgTaxRate.OTR_RateDenominator = 5;
			AssertEquals("Setting OTR_RateDenominator sets Rate value", 6.4M, OrgTaxRate.Rate);

			OrgTaxRate.OTR_RateDenominator = 10;
			AssertEquals("Setting OTR_RateDenominator sets Rate value", 3.2M, OrgTaxRate.Rate);
		}

		readonly AccOrgTaxConfiguration OrgTaxConfiguration;
		AccOrgTaxRate OrgTaxRate;
	}
}
