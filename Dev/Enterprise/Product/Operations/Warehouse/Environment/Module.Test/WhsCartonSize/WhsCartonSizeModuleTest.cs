using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsCartonSizeModule))]
	class WhsCartonSizeModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (WhsCartonSizeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsCartonSize, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (WhsCartonSizeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (WhsCartonSizeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigCartonSize, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestShowRecentItems

		public void TestShowRecentItems()
		{
			using (var module = new WhsCartonSizeModuleForTest())
			{
				AssertEquals("WhsCartonSizeModule should show recent items.", true, module.ShowRecentItemsExposedForTest);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new WhsCartonSizeModule())
			{
				Assert("Delete option should  be available", module.AllowDelete);
			}
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (var module = new WhsCartonSizeModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<WhsCartonSizeFilterControl>(controlForTest);
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = new WhsCartonSizeModule())
			{
				AssertEquals(typeof(WhsCartonSizeFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsCartonSize;
		}

		#region WhsCartonSizeModuleForTest

		class WhsCartonSizeModuleForTest : WhsCartonSizeModule
		{
			public WhsCartonSizeFilterControl GetNewFilterControlForTest()
			{
				return (WhsCartonSizeFilterControl)GetNewFilterControl();
			}

			public bool ShowRecentItemsExposedForTest
			{
				get { return ShowRecentItems; }
			}
		}

		#endregion

		#endregion
	}
}
