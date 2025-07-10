using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRHiringRequestModule))]
	public class HRHiringRequestModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (HRHiringRequestModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.HRHiringRequest, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (HRHiringRequestModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.HRHiringRequest, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (HRHiringRequestModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new HRHiringRequestModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<HRHiringRequestFilterControl>(controlForTest);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new HRHiringRequestModule())
			{
				AssertEquals(typeof(HRHiringRequestFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.HRHiringRequest;
		#region HRHiringRequestModuleForTest
		class HRHiringRequestModuleForTest : HRHiringRequestModule
		{
			public HRHiringRequestFilterControl GetNewFilterControlForTest() => (HRHiringRequestFilterControl)GetNewFilterControl();
		}
		#endregion
	}
}
