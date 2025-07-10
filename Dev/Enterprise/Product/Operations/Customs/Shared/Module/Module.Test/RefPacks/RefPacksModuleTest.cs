using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Shared.Module.Testing
{
	[TestedType(typeof(RefPacksModule))]
	sealed class RefPacksModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.RefPacks;

		protected override string GetIsSystemDefinedDefaultProperty() => "All";
	}
}
