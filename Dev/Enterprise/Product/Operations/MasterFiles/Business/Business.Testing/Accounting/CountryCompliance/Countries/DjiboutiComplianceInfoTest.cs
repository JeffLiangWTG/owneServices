using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(DjiboutiComplianceInfo))]
	sealed class DjiboutiComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => CountryCodes.Djibouti;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			AssertEquals("NIF", Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCodes.Djibouti));
		}

		public void TestGetConsumptionTaxDescription()
		{
			AssertEquals("TVA", Country.GetConsumptionTaxDescription(CountryCodes.Djibouti));
		}

		public void TestGetIsGSTRegistered()
		{
			AssertEquals(true, Country.GetIsGSTRegistered(CountryCodes.Djibouti));
		}
	}
}
