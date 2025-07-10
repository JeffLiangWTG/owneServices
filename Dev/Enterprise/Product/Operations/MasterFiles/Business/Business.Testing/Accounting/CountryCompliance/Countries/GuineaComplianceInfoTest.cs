using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(GuineaComplianceInfo))]
	sealed class GuineaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Guinea;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "NIF";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT NIF #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Guinea);
			var expected = "TVA";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Guinea);
			var expected = "TVA";

			AssertEquals(expected, result);
		}

		public void TestGetIsGSTRegistered()
		{
			var result = Country.GetIsGSTRegistered(Core.Constants.CountryCodes.Guinea);
			var expected = true;

			AssertEquals(expected, result);
		}

		public void TestIsCashBasisVAT()
		{
			var result = Country.IsGSTCashBasis(Core.Constants.CountryCodes.Guinea);
			var expected = false;

			AssertEquals(expected, result);
		}
	}
}
