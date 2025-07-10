using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(WarehouseModule))]
	sealed class WarehouseModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (WarehouseModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsConfigWarehouse, module.ID);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (WarehouseModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (WarehouseModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigWarehouse, module.SecurityCheckpoint);
			}
		}

		public void TestShowRecentItems()
		{
			using (var module = new WhsWarehouseModuleForTest())
			{
				AssertEquals("Warehouse Module itself should be able to see recent items.", true, module.ShowRecentItemsExposedForTest);
			}

			using (var module = new WhsWarehouseModuleForTest())
			{
				module.OverrideModuleDecisionProvider(new DummyModuleDecisionProvider(Factory));
				AssertEquals("Warehouse Module from a Find Box or other source should not see recent items.", false, module.ShowRecentItemsExposedForTest);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new WarehouseModule())
			{
				Assert("Delete option should not be available", !module.AllowDelete);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new WhsWarehouseModuleForTest())
			{
				var controlForTest = module.GetNewFilterControlForTest();
				Assert(controlForTest is WarehouseFilterControl);
				controlForTest.Dispose();
			}
		}

		public void TestGridCollection()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var productWarehouse = helper.CreateWarehouse("WHS");
			var transitWarehouse = helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			using (var module = new WarehouseModule())
			{
				AssertEquals(typeof(WhsWarehouseCollection), module.GridCollection.GetType());

				var warehouseCollection = (WhsWarehouseCollection)module.GridCollection;
				warehouseCollection.Load();
				AssertContainsExactElementsInAnyOrder("Warehouse Module itself should be able to see Transit Warehouses.",
					new[] { productWarehouse.PK, transitWarehouse.PK }, warehouseCollection.Select(w => w.PK));
			}

			using (var module = new WarehouseModule())
			{
				module.OverrideModuleDecisionProvider(new DummyModuleDecisionProvider(Factory));

				var warehouseCollection = (WhsWarehouseCollection)module.GridCollection;
				warehouseCollection.Load();
				AssertContainsExactElementsInAnyOrder("Warehouse Module from a Find Box or other source should not see Transit Warehouses.",
					new[] { productWarehouse.PK }, warehouseCollection.Select(w => w.PK));
			}
		}

		public void TestFilterBusinessObject()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var productWarehouse = helper.CreateWarehouse("WHS");
			var transitWarehouse = helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			using (var module = new WarehouseModule())
			{
				AssertEquals(typeof(WarehouseFilterBusinessObject), module.FilterBusinessObject.GetType());

				var warehouses = Factory.Load<WhsWarehouse>(module.FilterBusinessObject.Filter);
				AssertContainsExactElementsInAnyOrder("Warehouse Module itself should not filter out Transit Warehouses.", new[] { productWarehouse, transitWarehouse }, warehouses);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsConfigWarehouse;

		sealed class DummyModuleDecisionProvider : IModuleDecisionProvider
		{
			public DummyModuleDecisionProvider(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			public bool AllowExcelExport => throw new NotImplementedException();

			public bool EnablePreviousNextSupport => throw new NotImplementedException();

			public void HandleDefaultAction(BusinessObject[] selectedBusinessObject) => throw new NotImplementedException();

			public void InitialiseFindBoxControllerLink(ZController controller) => throw new NotImplementedException();

			public IBusinessObjectCollection List => new WhsWarehouseCollection(Factory);

			public void SetFindBoxCodeDescription(BusinessObject bizo) => throw new NotImplementedException();

			public bool ShouldDisplayNotifications => throw new NotImplementedException();

			public bool ShouldIgnoreAdditionalFilter => throw new NotImplementedException();

			public bool ShouldLoadFilterBizObj => false;

			public bool ShouldSaveFilterBizObj => false;

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject) => throw new NotImplementedException();

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}
		}

		sealed class WhsWarehouseModuleForTest : WarehouseModule
		{
			public IFilterControl GetNewFilterControlForTest() => GetNewFilterControl();

			public bool ShowRecentItemsExposedForTest => ShowRecentItems;
		}
	}
}
