using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsWarehouseCollectionForProductWarehouse))]
	class WhsWarehouseCollectionForProductWarehouseTestCase : WhsBusinessObjectCollectionTestCase
	{
		#region TestAdditionalFilter

		public void TestAdditionalFilter()
		{
			var client = Helper.CreateClient("Client");
			var whs1 = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs1", WarehouseTypes.Codes.Product, true, true);
			var whs2 = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs2", WarehouseTypes.Codes.ContainerYard, false, true);
			var whs3 = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs3", WarehouseTypes.Codes.FreeTradeZone, false, true);
			var whs4 = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs4", WarehouseTypes.Codes.Transit, false, true);
			var whs5 = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs5", WarehouseTypes.Codes.Product, false, true);

			var collection = new WhsWarehouseCollectionForProductWarehouse(Factory);
			collection.Load();

			AssertCollectionContains(whs1, collection);
			AssertCollectionNotContains(whs2, collection);
			AssertCollectionNotContains(whs3, collection);
			AssertCollectionNotContains(whs4, collection);
			AssertCollectionContains(whs5, collection);
		}

		#endregion

		#region TestAddNotificationWhenAdditionalFilterNotMet

		#region TestAddNotificationWhenAdditionalFilterNotMet_NonProductVirtualWarehouse

		public void TestAddNotificationWhenAdditionalFilterNotMet_NonProductVirtualWarehouse()
		{
			var client = Helper.CreateClient("Client");
			var whs = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs", WarehouseTypes.Codes.ContainerYard, true, true);
			var warehouses = GetCollectionToTest();

			AssertEquals("Only Product Warehouses may be selected.", warehouses.GetAllNotificationsWhenAdditionalFilterNotMet(whs));
		}

		#endregion

		#region TestAddNotificationWhenAdditionalFilterNotMet_WarehouseType

		public void TestAddNotificationWhenAdditionalFilterNotMet_WarehouseType()
		{
			var client = Helper.CreateClient("Client");
			var whs = SetupWarehouseWithTypeIsVirtualityAndIsActive("Whs", WarehouseTypes.Codes.Product, false, true);
			var warehouseClients = GetCollectionToTest();
			AssertNotEquals("Only Product Warehouses may be selected.", warehouseClients.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouseClients = GetCollectionToTest();
			AssertEquals("Only Product Warehouses may be selected.", warehouseClients.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouseClients = GetCollectionToTest();
			AssertEquals("Only Product Warehouses may be selected.", warehouseClients.GetAllNotificationsWhenAdditionalFilterNotMet(whs));

			whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouseClients = GetCollectionToTest();
			AssertEquals("Only Product Warehouses may be selected.", warehouseClients.GetAllNotificationsWhenAdditionalFilterNotMet(whs));
		}

		#endregion

		#endregion

		#region Implementation

		WhsWarehouse SetupWarehouseWithTypeIsVirtualityAndIsActive(ZString name, ZString whsType, bool isVirtual, bool isActive)
		{
			var whs = Helper.CreateWarehouse(name);
			whs.WW_WarehouseType = whsType;
			whs.WW_IsVirtualWarehouse = isVirtual;
			whs.WW_IsActive = isActive;
			return whs;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsWarehouseCollectionForProductWarehouse(Factory);
		}

		#endregion
	}
}
