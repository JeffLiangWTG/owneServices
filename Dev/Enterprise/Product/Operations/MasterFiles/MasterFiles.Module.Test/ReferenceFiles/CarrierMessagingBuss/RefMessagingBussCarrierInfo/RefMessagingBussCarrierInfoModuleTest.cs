using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefMessagingBussCarrierInfoModule))]
	sealed class RefMessagingBussCarrierInfoModuleTest : ZModuleBasherTest
	{
		public void TestImplementation()
		{
			using (var module = GetModule())
			{
				AssertEquals(expected: false, module.AllowNew);
				AssertEquals(expected: false, module.AllowEdit);
				AssertEquals(expected: false, module.AllowDelete);
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
				AssertEquals(Env.Security.RefMessagingBussCarrierInfo, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefMessagingBussCarrierInfo;
		}
	}
}
