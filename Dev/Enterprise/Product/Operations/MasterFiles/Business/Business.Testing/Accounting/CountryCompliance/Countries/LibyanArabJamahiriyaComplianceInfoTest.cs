using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(LibyanArabJamahiriyaComplianceInfo))]
	sealed class LibyanArabJamahiriyaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.LibyanArabJamahiriya;
	}
}
