using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(PakistanComplianceInfo))]
	sealed class PakistanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Pakistan;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Pakistan);
			var expected = "VAT";

			AssertEquals(expected, result);
		}

		public void TestGetIsGSTRegistered()
		{
			var result = Country.GetIsGSTRegistered(Core.Constants.CountryCodes.Pakistan);
			var expected = true;

			AssertEquals(expected, result);
		}

		public void TestIsGSTCashBasis()
		{
			var result = Country.IsGSTCashBasis(Core.Constants.CountryCodes.Pakistan);
			var expected = false;

			AssertEquals(expected, result);
		}
	}
}
