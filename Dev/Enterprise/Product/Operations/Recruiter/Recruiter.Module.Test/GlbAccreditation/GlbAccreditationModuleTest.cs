using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationModule))]
	public class GlbAccreditationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbAccreditation;
		}

		public void TestCheckpoints()
		{
			using (var module = new GlbAccreditationModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlbAccreditation, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.LearningAndDevelopment, module.LicenceCheckPoint);
			}
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (var module = new GlbAccreditationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Correct control type", filterControl is GlbAccreditationFilterControl);
				filterControl.Dispose();
			}
		}

		#endregion
		public class GlbAccreditationModuleForTest : GlbAccreditationModule
		{
			public IFilterControl NewFilterControl
			{
				get
				{
					return GetNewFilterControl();
				}
			}
		}
	}
}
