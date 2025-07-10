using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(BurkinaFasoComplianceInfo))]
	public class BurkinaFasoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.BurkinaFaso;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "RCM";

		protected override string ExpectedRecipientTaxIDHeading => "CLIENT IFU #";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT RCCM #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
