using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(FaeroeIslandsComplianceInfo))]
	sealed class FaeroeIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.FaeroeIslands;
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
		protected override string ExpectedRecipientTaxIDHeading => "CLIENT MVG #";
	}
}
