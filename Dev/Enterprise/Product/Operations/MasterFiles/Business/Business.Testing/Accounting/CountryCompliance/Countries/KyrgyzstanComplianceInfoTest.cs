using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(KyrgyzstanComplianceInfo))]
	sealed class KyrgyzstanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Kyrgyzstan;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT TIN #";
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
