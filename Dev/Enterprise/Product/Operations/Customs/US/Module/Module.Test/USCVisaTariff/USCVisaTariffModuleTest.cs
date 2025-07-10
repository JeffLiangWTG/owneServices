using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCVisaTariffModule))]
	sealed class USCVisaTariffModuleTest : USCFilterGridModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.TariffRequiringVisa;

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.ImportBroker;
	}
}
