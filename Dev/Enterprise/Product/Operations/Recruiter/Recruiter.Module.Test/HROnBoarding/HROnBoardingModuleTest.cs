using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HROnBoardingModule))]
	public class HROnBoardingModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (HROnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.HROnBoarding, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (HROnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.Staff, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (HROnBoardingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Recruiter, module.LicenceCheckPoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new HROnBoardingModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<HROnBoardingFilterControl>(controlForTest);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new HROnBoardingModule())
			{
				AssertEquals(typeof(HROnBoardingFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.HROnBoarding;
		#region HROnBoardingModuleForTest
		class HROnBoardingModuleForTest : HROnBoardingModule
		{
			public HROnBoardingFilterControl GetNewFilterControlForTest() => (HROnBoardingFilterControl)GetNewFilterControl();
		}
		#endregion
	}
}
