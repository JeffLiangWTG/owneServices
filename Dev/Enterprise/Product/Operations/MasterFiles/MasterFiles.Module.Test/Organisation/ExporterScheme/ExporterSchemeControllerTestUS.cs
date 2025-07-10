using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExporterSchemeControllerUS))]
	sealed class ExporterSchemeControllerTestUS : ExporterSchemeControllerTest
	{
		protected override string CountryCode
		{
			get { return Enterprise.Core.Constants.CountryCodes.UnitedStates; }
		}
	}
}
