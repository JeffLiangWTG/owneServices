using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsSalesChannelModule))]
	class WhsSalesChannelModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (WhsSalesChannelModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsSalesChannel, module.ID);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (WhsSalesChannelModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigSalesChannel, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (WhsSalesChannelModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new WhsSalesChannelModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<WhsSalesChannelFilterControl>(controlForTest);
			}
		}

		public void TestShowRecentItems()
		{
			using (var module = new WhsSalesChannelModuleForTest())
			{
				AssertEquals("WhsSalesChannelModule should show recent items.", true, module.ShowRecentItemsExposedForTest);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new WhsSalesChannelModule())
			{
				Assert("Delete option should  be available", module.AllowDelete);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new WhsSalesChannelModule())
			{
				AssertEquals(typeof(WhsSalesChannelFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsSalesChannel;

		#region WhsSalesChannelModuleForTest

		class WhsSalesChannelModuleForTest : WhsSalesChannelModule
		{
			public WhsSalesChannelFilterControl GetNewFilterControlForTest() => (WhsSalesChannelFilterControl)GetNewFilterControl();

			public bool ShowRecentItemsExposedForTest => ShowRecentItems;
		}

		#endregion
	}
}
