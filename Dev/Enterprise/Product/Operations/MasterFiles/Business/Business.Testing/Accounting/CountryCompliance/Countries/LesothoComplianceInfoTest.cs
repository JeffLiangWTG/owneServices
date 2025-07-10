using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(LesothoComplianceInfo))]
	sealed class LesothoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Lesotho;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "TIN";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT TIN #";

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT VAT #:";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "VAT";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "VAT";

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
