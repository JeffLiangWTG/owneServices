using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(BonaireSintEustatiusAndSabaComplianceInfo))]
	sealed class BonaireSintEustatiusAndSabaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.BonaireSintEustatiusAndSaba;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT CRIB #";
	}
}
