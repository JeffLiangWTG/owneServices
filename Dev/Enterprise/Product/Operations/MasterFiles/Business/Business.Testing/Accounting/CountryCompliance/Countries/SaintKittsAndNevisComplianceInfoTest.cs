using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SaintKittsAndNevisComplianceInfo))]
	sealed class SaintKittsAndNevisComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SaintKittsAndNevis;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT TIN #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
