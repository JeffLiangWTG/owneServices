using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(AndorraComplianceInfo))]
	sealed class AndorraComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Andorra;
		protected override string ExpectedRecipientTaxIDHeading => "CLIENT NRT #:";
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => false;
	}
}
