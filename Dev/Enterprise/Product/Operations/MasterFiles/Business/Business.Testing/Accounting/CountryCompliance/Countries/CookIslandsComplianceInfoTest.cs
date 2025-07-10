using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(CookIslandsComplianceInfo))]
	sealed class CookIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.CookIslands;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT RMD #";
	}
}
