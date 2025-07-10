using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestModule))]
	sealed class eManifestModuleInternationalTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.eManifestIntl;
	}
}
