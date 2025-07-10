using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(NorthernMarianaIslandsComplianceInfo))]
	sealed class NorthernMarianaIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.NorthernMarianaIslands;
	}
}
