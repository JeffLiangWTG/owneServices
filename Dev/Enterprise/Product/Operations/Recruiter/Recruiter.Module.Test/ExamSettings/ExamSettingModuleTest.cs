using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(ExamSettingModule))]
	public class ExamSettingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ExamSetting;
		}

		public void TestCheckpoints()
		{
			using (var module = new ExamSettingModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ExamSetting, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.LearningAndDevelopment, module.LicenceCheckPoint);
			}
		}
	}
}
