using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(MicronesiaComplianceInfo))]
	sealed class MicronesiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Micronesia;
	}
}
