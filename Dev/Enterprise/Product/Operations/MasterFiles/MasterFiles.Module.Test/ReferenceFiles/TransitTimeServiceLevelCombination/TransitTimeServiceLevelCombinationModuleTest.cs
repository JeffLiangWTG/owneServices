using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TransitTimeServiceLevelCombinationModule))]
	sealed class TransitTimeServiceLevelCombinationModuleTest : ZModuleBasherTest
	{
		public void TestAllowNewEditAndDelete()
		{
			using (var module = new TransitTimeServiceLevelCombinationModule())
			{
				AssertEquals("AllowNew", false, module.AllowNew);
				AssertEquals("AllowEdit", false, module.AllowEdit);
				AssertEquals("AllowDelete", false, module.AllowDelete);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TransitTimeServiceLevelCombination;
		}
	}
}
