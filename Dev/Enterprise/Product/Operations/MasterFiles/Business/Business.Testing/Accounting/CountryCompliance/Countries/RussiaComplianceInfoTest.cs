using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(RussiaComplianceInfo))]
	sealed class RussiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Russia;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "KPP";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT KPP #";
	}
}
