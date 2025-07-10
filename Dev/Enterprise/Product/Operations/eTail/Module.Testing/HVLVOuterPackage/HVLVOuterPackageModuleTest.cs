using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVOuterPackageModule))]
	class HVLVOuterPackageModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckpoint_IsNone()
		{
			using (var module = new HVLVOuterPackageModule())
			{
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckpoint_IsForwarder()
		{
			using (var module = new HVLVOuterPackageModule())
			{
				AssertEquals(Env.Licence.Forwarder, module.LicenceCheckPoint);
			}
		}

		public void TestAllowNewEditDeleteViewCopy_Disabled()
		{
			using (var module = new HVLVOuterPackageModule())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Allow New:", false, module.AllowNew);
					AssertEquals("Allow Edit:", false, module.AllowEdit);
					AssertEquals("Allow Delete:", false, module.AllowDelete);
					AssertEquals("Allow View:", false, module.AllowView);
					AssertEquals("Allow UniversalCopy:", false, module.AllowUniversalCopy);
				});
			}
		}

		public void TestShowRecentItems_Disabled()
		{
			using (var module = new HVLVOuterPackageModule())
			{
				AssertEquals(false, module.ShowRecentItems);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.HVLVOuterPackage;

		protected override bool HasController() => false;

		#endregion
	}
}
