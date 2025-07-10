using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefOrgPartCategoryModule))]
	sealed class RefOrgPartCategoryModuleTest : ZModuleBasherTest
	{
		#region TestRefOrgPartCategoryModuleProperties

		public void TestRefOrgPartCategoryModuleProperties()
		{
			using (var module = new RefOrgPartCategoryModule())
			{
				AssertEquals(ModuleIDs.RefOrgPartCategory, module.ID);
				AssertEquals(Env.Security.RefOrgPartCategory, module.SecurityCheckpoint);
			}
		}

		#endregion

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefOrgPartCategory;
		}
	}
}
