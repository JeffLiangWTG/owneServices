using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SudanComplianceInfo))]
	sealed class SudanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => CountryCodes.Sudan;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			AssertEquals("VAT", Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCodes.Sudan));
		}

		public void TestGetConsumptionTaxDescription()
		{
			AssertEquals("VAT", Country.GetConsumptionTaxDescription(CountryCodes.Sudan));
		}

		public void TestGetIsGSTRegistered()
		{
			AssertEquals(true, Country.GetIsGSTRegistered(CountryCodes.Sudan));
		}
	}
}
