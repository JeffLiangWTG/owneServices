using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(TurkmenistanComplianceInfo))]
	sealed class TurkmenistanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Turkmenistan;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT VAT #";
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
