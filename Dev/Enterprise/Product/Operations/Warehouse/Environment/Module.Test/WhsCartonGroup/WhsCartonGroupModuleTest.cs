using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsCartonGroupModule))]
	class WhsCartonGroupModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (WhsCartonGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsCartonGroup, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (WhsCartonGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (WhsCartonGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigCartonGroup, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestShowRecentItems

		public void TestShowRecentItems()
		{
			using (var module = new WhsCartonGroupModuleForTest())
			{
				AssertEquals("WhsCartonGroupModule should show recent items.", true, module.ShowRecentItemsExposedForTest);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = new WhsCartonGroupModule())
			{
				Assert("Delete option should  be available", module.AllowDelete);
			}
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (var module = new WhsCartonGroupModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<WhsCartonGroupFilterControl>(controlForTest);
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (var module = new WhsCartonGroupModule())
			{
				AssertEquals(typeof(WhsCartonGroupFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsCartonGroup;
		}

		#region WhsCartonGroupModuleForTest

		class WhsCartonGroupModuleForTest : WhsCartonGroupModule
		{
			public WhsCartonGroupFilterControl GetNewFilterControlForTest()
			{
				return (WhsCartonGroupFilterControl)GetNewFilterControl();
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
