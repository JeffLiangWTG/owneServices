using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestTypesModule))]
	internal class ExternalRequestTypesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ExternalRequestTypes;

		public void TestLicenceCheckPointCore()
		{
			using (var module = new ExternalRequestTypesModule())
			{
				AssertEquals(Env.Licence.Workflow, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new ExternalRequestTypesModule())
			{
				AssertEquals(Env.Security.ExternalRequestTypes, module.SecurityCheckpoint);
			}
		}
	}
}
