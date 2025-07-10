using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCCountryModule))]
	sealed class USCCountryModuleTest : USCFilterGridModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Country;

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.Broker;

		protected override bool HasController() => true;
	}
}
