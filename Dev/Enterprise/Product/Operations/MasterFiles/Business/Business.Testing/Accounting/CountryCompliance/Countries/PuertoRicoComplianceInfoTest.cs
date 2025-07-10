using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(PuertoRicoComplianceInfo))]
	sealed class PuertoRicoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.PuertoRico;
	}
}
