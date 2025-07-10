using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(BelizeComplianceInfo))]
	sealed class BelizeComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Belize;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT TIN #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
