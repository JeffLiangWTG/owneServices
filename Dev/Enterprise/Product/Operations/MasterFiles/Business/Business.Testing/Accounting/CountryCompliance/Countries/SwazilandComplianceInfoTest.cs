using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SwazilandComplianceInfo))]
	sealed class SwazilandComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Swaziland;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT TIN #";
	}
}
