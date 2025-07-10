using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(TariffsAndRatesReports))]
	public class TariffsAndRatesReportsTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TariffRateReports;
		}
	}
}
