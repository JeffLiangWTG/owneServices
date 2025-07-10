using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(PapuaNewGuineaComplianceInfo))]
	sealed class PapuaNewGuineaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.PapuaNewGuinea;
	}
}
