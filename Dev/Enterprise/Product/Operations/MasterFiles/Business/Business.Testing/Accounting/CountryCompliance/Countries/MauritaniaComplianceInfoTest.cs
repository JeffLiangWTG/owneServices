using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MauritaniaComplianceInfo))]
	sealed class MauritaniaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Mauritania;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Mauritania);
			var expected = "TVA";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Mauritania);
			var expected = "TVA";

			AssertEquals(expected, result);
		}

		public void TestGetIsGSTRegistered()
		{
			var result = Country.GetIsGSTRegistered(Core.Constants.CountryCodes.Mauritania);
			var expected = true;

			AssertEquals(expected, result);
		}

		public void TestIsGSTCashBasis()
		{
			var result = Country.IsGSTCashBasis(Core.Constants.CountryCodes.Mauritania);
			var expected = false;

			AssertEquals(expected, result);
		}
	}
}
