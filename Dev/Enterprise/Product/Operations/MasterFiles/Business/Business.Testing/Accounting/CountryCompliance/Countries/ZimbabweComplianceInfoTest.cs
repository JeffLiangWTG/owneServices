using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ZimbabweComplianceInfo))]
	sealed class ZimbabweComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Zimbabwe;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "TIN";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT TIN #";

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT VAT #:";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
