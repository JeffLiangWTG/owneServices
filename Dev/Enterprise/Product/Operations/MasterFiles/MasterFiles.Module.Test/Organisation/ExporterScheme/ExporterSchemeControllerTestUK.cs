using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExporterSchemeControllerUK))]
	sealed class ExporterSchemeControllerTestUK : ExporterSchemeControllerTest
	{
		protected override string CountryCode => Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
	}
}
