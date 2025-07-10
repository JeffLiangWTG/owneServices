using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestInfoTemplateModule))]
	internal class ExternalRequestInfoTemplateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ExternalRequestInfoTemplate;

		public void TestLicenceCheckPointCore()
		{
			using (var module = new ExternalRequestInfoTemplateModule())
			{
				AssertEquals(Env.Licence.Workflow, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new ExternalRequestInfoTemplateModule())
			{
				AssertEquals(Env.Security.ExternalRequestInfoTemplate, module.SecurityCheckpoint);
			}
		}
	}
}
