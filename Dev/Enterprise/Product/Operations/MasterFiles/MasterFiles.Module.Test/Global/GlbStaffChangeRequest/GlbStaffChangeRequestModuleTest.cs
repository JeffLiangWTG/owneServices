using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffChangeRequestModule))]
	public class GlbStaffChangeRequestModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (GlbStaffChangeRequestModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.GlbStaffChangeRequest, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (GlbStaffChangeRequestModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.GlbStaffChangeRequest, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (GlbStaffChangeRequestModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (var module = new GlbStaffChangeRequestModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<GlbStaffChangeRequestFilterControl>(controlForTest);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new GlbStaffChangeRequestModule())
			{
				AssertEquals(typeof(GlbStaffChangeRequestFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GlbStaffChangeRequest;

		#region GlbStaffChangeRequestModuleForTest

		class GlbStaffChangeRequestModuleForTest : GlbStaffChangeRequestModule
		{
			public GlbStaffChangeRequestFilterControl GetNewFilterControlForTest() => (GlbStaffChangeRequestFilterControl)GetNewFilterControl();
		}

		#endregion
	}
}
