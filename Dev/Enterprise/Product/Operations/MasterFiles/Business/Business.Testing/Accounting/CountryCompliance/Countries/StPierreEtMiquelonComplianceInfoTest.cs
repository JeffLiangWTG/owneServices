using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(StPierreEtMiquelonComplianceInfo))]
	sealed class StPierreEtMiquelonComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.StPierreEtMiquelon;
	}
}
