using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(TrinidadAndTobagoComplianceInfo))]
	sealed class TrinidadAndTobagoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.TrinidadAndTobago;
	}
}
