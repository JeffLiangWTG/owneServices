using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(DemocraticRepublicOfCongoComplianceInfo))]
	sealed class DemocraticRepublicOfCongoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.DemocraticRepublicOfCongo;
	}
}
