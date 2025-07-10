using Enterprise.Environment;
using Enterprise.HRM.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(ReviewProcessModule))]
	public class ReviewProcessModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (ReviewProcessModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.ReviewProcess, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (ReviewProcessModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (ReviewProcessModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new ReviewProcessModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<ReviewProcessFilterControl>(controlForTest);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new ReviewProcessModule())
			{
				AssertEquals(typeof(ReviewProcessFilterStripBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ReviewProcess;
		#region ReviewProcessModuleForTest
		class ReviewProcessModuleForTest : ReviewProcessModule
		{
			public ReviewProcessFilterControl GetNewFilterControlForTest() => (ReviewProcessFilterControl)GetNewFilterControl();
		}
		#endregion
	}
}
