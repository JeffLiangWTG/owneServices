using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SaintMartinComplianceInfo))]
	sealed class SaintMartinComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.SaintMartin;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT TGCA #";
	}
}
