using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(EntryLineModule))]
	sealed class EntryLineModuleTest : Customs.Module.Testing.EntryLineModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.EntryLine;
	}
}
