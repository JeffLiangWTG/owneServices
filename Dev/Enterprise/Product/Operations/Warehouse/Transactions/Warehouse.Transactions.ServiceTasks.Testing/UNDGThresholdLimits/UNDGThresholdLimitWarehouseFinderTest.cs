using System.Linq;
using CargoWise.Application;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	public class UNDGThresholdLimitWarehouseFinderTest : WhsTestCaseWithFactory
	{
		#region TestLoadWarehouse_NoWarehouse

		public void TestLoadWarehouse_NoWarehouse()
		{
			var warehouses = ObjectFactory.Get<IUNDGThresholdLimitWarehouseFinder>().LoadWarehousesWithDGLimits();
			AssertEquals(0, warehouses.Count);
		}

		#endregion

		#region TestLoadWarehouse_Product

		public void TestLoadWarehouse_Product_DGNotEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Product, false, false, false);
		}

		public void TestLoadWarehouse_Product_DGNotEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Product, false, true, false);
		}

		public void TestLoadWarehouse_Product_DGEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Product, true, false, false);
		}

		public void TestLoadWarehouse_Product_DGEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Product, true, true, true);
		}

		#endregion

		#region TestLoadWarehouse_FTZ

		public void TestLoadWarehouse_FTZ_DGNotEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.FreeTradeZone, false, false, false);
		}

		public void TestLoadWarehouse_FTZ_DGNotEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.FreeTradeZone, false, true, false);
		}

		public void TestLoadWarehouse_FTZ_DGEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.FreeTradeZone, true, false, false);
		}

		public void TestLoadWarehouse_FTZ_DGEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.FreeTradeZone, true, true, true);
		}

		#endregion

		#region TestLoadWarehouse_Transit

		public void TestLoadWarehouse_Transit_DGNotEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Transit, false, false, false);
		}

		public void TestLoadWarehouse_Transit_DGNotEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Transit, false, true, false);
		}

		public void TestLoadWarehouse_Transit_DGEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Transit, true, false, false);
		}

		public void TestLoadWarehouse_Transit_DGEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.Transit, true, true, true);
		}

		#endregion

		#region TestLoadWarehouse_ContainerYard

		public void TestLoadWarehouse_ContainerYard_DGNotEnabled_Inactive()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.ContainerYard, false, false, false);
		}

		public void TestLoadWarehouse_ContainerYard_DGNotEnabled_Active()
		{
			TestLoadWarehousesWithDGLimitCore(WarehouseTypes.Codes.ContainerYard, false, true, false);
		}

		#endregion

		#region TestLoadWarehouse_WarehouseTypes

		public void TestLoadWarehouse_WarehouseTypes()
		{
			var warehouse1 = Helper.CreateWarehouse("PRD");
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse1.WW_IsDangerousGoodsManagementEnabled = true;

			var warehouse2 = Helper.CreateWarehouse("TRN");
			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse2.WW_IsDangerousGoodsManagementEnabled = true;

			var warehouse3 = Helper.CreateWarehouse("FTZ");
			warehouse3.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse3.WW_IsDangerousGoodsManagementEnabled = true;

			var warehouse4 = Helper.CreateWarehouse("XXX");
			warehouse4.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse4.WW_IsDangerousGoodsManagementEnabled = false;
			Factory.Save();

			var warehouses = ObjectFactory.Get<IUNDGThresholdLimitWarehouseFinder>().LoadWarehousesWithDGLimits();
			var expected = new[] { warehouse1.PK, warehouse2.PK, warehouse3.PK }.OrderBy(x => x).ToArray();
			var actual = warehouses.Select(s => s.PK).OrderBy(x => x).ToArray();
			AssertArrayEqualsByElements(expected, actual);
		}

		#endregion

		#region Implementation

		void TestLoadWarehousesWithDGLimitCore(string warehouseType, bool isDGManagementEnabled, bool isActive, bool shouldLoad)
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			warehouse.WW_WarehouseType = warehouseType;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;
			warehouse.WW_IsActive = isActive;
			Factory.Save();

			var warehouses = ObjectFactory.Get<IUNDGThresholdLimitWarehouseFinder>().LoadWarehousesWithDGLimits();
			AssertEquals(shouldLoad, warehouses.Count > 0);
		}

		#endregion
	}
}
