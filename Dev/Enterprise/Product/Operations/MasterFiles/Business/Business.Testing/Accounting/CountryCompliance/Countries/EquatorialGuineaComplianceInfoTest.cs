using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(EquatorialGuineaComplianceInfo))]
	sealed class EquatorialGuineaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.EquatorialGuinea;
	}
}
