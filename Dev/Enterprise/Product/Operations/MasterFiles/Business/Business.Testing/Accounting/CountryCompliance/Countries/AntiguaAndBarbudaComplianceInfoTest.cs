using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(AntiguaAndBarbudaComplianceInfo))]
	sealed class AntiguaAndBarbudaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.AntiguaAndBarbuda;
	}
}
