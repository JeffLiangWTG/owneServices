using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(BosniaAndHerzegovinaComplianceInfo))]
	sealed class BosniaAndHerzegovinaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.BosniaAndHerzegovina;
	}
}
