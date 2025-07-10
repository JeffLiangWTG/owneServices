using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(LaoPeoplesDemocraticRepublicComplianceInfo))]
	sealed class LaoPeoplesDemocraticRepublicComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic;
	}
}
