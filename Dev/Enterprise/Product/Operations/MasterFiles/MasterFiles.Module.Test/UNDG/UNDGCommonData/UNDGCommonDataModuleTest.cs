using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNDGCommonDataModule))]
	sealed class UNDGCommonDataModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.UNDGCommonData;
		}

		public void TestSecurityCheckpoint()
		{
			using (UNDGCommonDataModule module = new UNDGCommonDataModule())
			{
				AssertEquals("Should have correct SecurityCheckpoint", Env.Security.UNDGCommonData, module.SecurityCheckpoint);
			}
		}
	}
}
