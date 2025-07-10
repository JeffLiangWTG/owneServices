
namespace Enterprise.eManifest.Module.Testing
{
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ELoadListModule))]
	internal class ELoadListModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ELoadList;
		}

		protected override bool HasController()
		{
			return false;
		}
	}
}
