using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Module.Test
{
	[TestedType(typeof(ReviewProcessNodeModule))]
	public class ReviewProcessNodeModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.ReviewProcessNode, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (ReviewProcessNodeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (ReviewProcessNodeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new ReviewProcessNodeModule())
			{
				AssertEquals(typeof(ReviewProcessNodeFilterStripBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ReviewProcessNode;
	}
}
