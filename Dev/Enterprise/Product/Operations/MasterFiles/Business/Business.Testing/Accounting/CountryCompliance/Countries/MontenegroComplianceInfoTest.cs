using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MontenegroComplianceInfo))]
	sealed class MontenegroComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Montenegro;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "PIB";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT PIB #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "PDV";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "PDV";

			AssertEquals(expected, result);
		}

		public void TestGetIsGSTRegistered()
		{
			var result = Country.GetIsGSTRegistered(CountryCode);
			var expected = true;

			AssertEquals(expected, result);
		}

		public void TestIsCashBasisVAT()
		{
			var result = Country.IsGSTCashBasis(CountryCode);
			var expected = false;

			AssertEquals(expected, result);
		}
	}
}
