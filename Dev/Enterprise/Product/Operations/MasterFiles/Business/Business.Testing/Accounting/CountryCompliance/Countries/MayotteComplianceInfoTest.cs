using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MayotteComplianceInfo))]
	sealed class MayotteComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Mayotte;
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => false;
	}
}
