using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SintMaartenComplianceInfo))]
	sealed class SintMaartenComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SintMaarten;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => false;
	}
}
