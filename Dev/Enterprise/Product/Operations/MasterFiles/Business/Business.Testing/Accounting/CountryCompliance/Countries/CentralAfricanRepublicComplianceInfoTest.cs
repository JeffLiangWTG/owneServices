using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(CentralAfricanRepublicComplianceInfo))]
	sealed class CentralAfricanRepublicComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.CentralAfricanRepublic;
	}
}
