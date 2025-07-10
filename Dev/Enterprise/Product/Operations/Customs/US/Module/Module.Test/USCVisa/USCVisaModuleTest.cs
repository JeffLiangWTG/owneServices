using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCVisaModule))]
	sealed class USCVisaModuleTest : USCFilterGridModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Visa;

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.ImportBroker;
	}
}
