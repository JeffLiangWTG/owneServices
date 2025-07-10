using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WhsPutawayGroupModule))]
	class WhsPutawayGroupModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (WhsPutawayGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsConfigPutawayGroup, module.ID);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (WhsPutawayGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (WhsPutawayGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigPutawayGroup, module.SecurityCheckpoint);
			}
		}

		public void TestShowRecentItems()
		{
			using (var module = new WhsPutawayGroupModuleForTest())
			{
				AssertEquals("WhsPutawayGroupModule should show recent items.", true, module.ShowRecentItemsExposedForTest);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new WhsPutawayGroupModule())
			{
				Assert("Delete option should  be available", module.AllowDelete);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new WhsPutawayGroupModuleForTest())
			using (var controlForTest = module.GetNewFilterControlForTest())
			{
				AssertType<WhsPutawayGroupFilterControl>(controlForTest);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = new WhsPutawayGroupModule())
			{
				AssertEquals(typeof(WhsPutawayGroupFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigPutawayGroup;
		}

		#region WhsPutawayGroupModuleForTest

		class WhsPutawayGroupModuleForTest : WhsPutawayGroupModule
		{
			public WhsPutawayGroupFilterControl GetNewFilterControlForTest()
			{
				return (WhsPutawayGroupFilterControl)GetNewFilterControl();
			}

			public bool ShowRecentItemsExposedForTest => ShowRecentItems;
		}

		#endregion
	}
}
