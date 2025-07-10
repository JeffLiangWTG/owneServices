using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(GuamComplianceInfo))]
	sealed class GuamComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Guam;
	}
}
