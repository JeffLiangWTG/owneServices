using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(DialogDefaultModule))]
	sealed class DialogDefaultModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DialogDefault;
		}

		public void TestCantAccessModuleWithoutPermissions()
		{
			using (var module = new DialogDefaultModule())
			{
				Env.Security.DialogDefault.IsAllowed = true;

				Assert("User should be able to modify", module.SecurityCheckpoint.IsAllowed);

				Env.Security.DialogDefault.IsAllowed = false;

				Assert("User shouldn't be able to modify", !module.SecurityCheckpoint.IsAllowed);
			}
		}
	}
}
