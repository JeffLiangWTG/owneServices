using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefJobEquipmentModule))]
	sealed class RefJobEquipmentModuleTest : ZModuleBasherTest
	{
		public void TestImplementation()
		{
			using (var module = GetModule())
			{
				AssertEquals(expected: true, module.AllowNew);
				AssertEquals(expected: true, module.AllowEdit);
				AssertEquals(expected: true, module.AllowDelete);
				AssertEquals(expected: false, module.SupportsWorkflow);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Security.RefJobEquipment, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefJobEquipment;
		}
	}
}
