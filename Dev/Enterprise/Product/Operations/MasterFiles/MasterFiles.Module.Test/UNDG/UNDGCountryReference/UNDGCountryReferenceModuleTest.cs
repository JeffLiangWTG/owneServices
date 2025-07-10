using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNDGCountryReferenceModule))]
	sealed class UNDGCountryReferenceModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.UNDGCountryReference;

		protected override bool HasController() => false;
	}
}
