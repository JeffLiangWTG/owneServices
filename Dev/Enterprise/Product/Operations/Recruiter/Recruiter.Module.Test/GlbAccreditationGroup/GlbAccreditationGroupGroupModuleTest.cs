using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationGroupModule))]
	public class GlbAccreditationGroupModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbAccreditationGroup;
		}

		public void TestCheckpoints()
		{
			using (var module = new GlbAccreditationGroupModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlbAccreditationGroup, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.LearningAndDevelopment, module.LicenceCheckPoint);
			}
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (var module = new GlbAccreditationGroupGroupModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Correct control type", filterControl is GlbAccreditationGroupFilterControl);
				filterControl.Dispose();
			}
		}

		#endregion
		public class GlbAccreditationGroupGroupModuleForTest : GlbAccreditationGroupModule
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
