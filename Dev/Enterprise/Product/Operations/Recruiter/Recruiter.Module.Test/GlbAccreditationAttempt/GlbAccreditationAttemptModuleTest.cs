using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptModule))]
	public class GlbAccreditationAttemptModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbAccreditationAttempt;
		}

		public void TestCheckpoints()
		{
			using (var module = new GlbAccreditationAttemptModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlbAccreditationAttempt, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.LearningAndDevelopment, module.LicenceCheckPoint);
			}
		}
	}
}
