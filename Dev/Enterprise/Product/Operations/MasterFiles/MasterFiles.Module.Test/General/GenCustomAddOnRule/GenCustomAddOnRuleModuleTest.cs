using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleModule))]
	sealed class GenCustomAddOnRuleModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GenCustomAddOnRule;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (var module = new GenCustomAddOnRuleModuleForTest())
			{
				IFilterControl controlForTest = module.GetNewFilterControlForTest();
				Assert(controlForTest is GenCustomAddOnRuleFilterControl);
				controlForTest.Dispose();
			}
		}

		public void TestGridCollection()
		{
			using (var module = new GenCustomAddOnRuleModuleForTest())
			{
				AssertNotNull(module.GetNewGridCollectionForTest());
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new GenCustomAddOnRuleModuleForTest())
			{
				AssertNotNull(module.GetNewFilterBusinessObjectForTest());
			}
		}

		public void TestCheckPont()
		{
			using (var module = new GenCustomAddOnRuleModuleForTest())
			{
				AssertEquals(Env.Security.AddOnRules, module.SecurityCheckpoint);
			}
		}
	}
}
