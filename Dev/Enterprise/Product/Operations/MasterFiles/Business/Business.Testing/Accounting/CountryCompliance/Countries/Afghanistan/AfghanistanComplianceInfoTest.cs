using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(AfghanistanComplianceInfo))]
	sealed class AfghanistanComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Afghanistan;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => false;
	}
}
