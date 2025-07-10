using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCQuotaModule))]
	sealed class USCQuotaModuleTest : USCFilterGridModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Quota;

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.ImportBroker;
	}
}
