using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(NiueComplianceInfo))]
	sealed class NiueComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Niue;
	}
}
