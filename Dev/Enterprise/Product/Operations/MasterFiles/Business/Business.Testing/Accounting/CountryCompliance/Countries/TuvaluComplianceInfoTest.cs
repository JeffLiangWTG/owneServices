using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(TuvaluComplianceInfo))]
	sealed class TuvaluComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Tuvalu;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT TIN #";
	}
}
