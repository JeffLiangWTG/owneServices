using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(FrenchGuianaComplianceInfo))]
	sealed class FrenchGuianaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => CountryCodes.FrenchGuyana;
		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => false;
	}
}
