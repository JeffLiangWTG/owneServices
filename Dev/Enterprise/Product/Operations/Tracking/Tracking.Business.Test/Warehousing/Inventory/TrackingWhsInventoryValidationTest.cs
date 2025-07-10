using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	sealed class TrackingWhsInventoryValidationTest : WhsInventoryViewValidationTest
	{
		#region TestValidateQuantity

		public void TestValidateQuantity_AllocationPastAvailableQuantity()
		{
			var client = WarehouseHelper.CreateClient("CLIENT");
			Helper.SetClientAllAttributeType(client, true);
			Factory.Save();

			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A", 4, 4);
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			part.OP_Desc = "Desc of super part";
			Helper.SetProductAllAttributeUse(client, part, true);
			Factory.Save();

			var packingDate = ZDate.Today;
			var expiryDate = ZDate.Today.AddDays(7);

			var docket = WarehouseHelper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new TestNotificationBuffer());
			var invLine = WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, 1m, ZGuid.Empty, "", expiryDate, packingDate, "A1", "A2", "A3", "Serial", "");

			docket.AllocateLocationsWithMock();
			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - make sure Receive was finalised", true, docket.IsFinalised);
			Factory.Save();

			var matchingInventory = Factory.Load<TrackingWhsInventory>(invLine.PK);
			matchingInventory.WI_TotalUnits = 1m;
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, warehouse, "1", Notify);
			matchingInventory.ShoppingCart = new TrackingWhsOrder(order);
			AssertEquals("Inventory Line Available", 1m, matchingInventory.WI_AvailableForCrossDockQuantity);

			matchingInventory.Quantity = 0m;
			matchingInventory.Validation.ValidateQuantity();
			AssertEquals("Quantity", 0m, matchingInventory.Quantity);
			AssertNoErrors(matchingInventory.QuantityInfo);

			matchingInventory.Quantity = 5m;
			matchingInventory.Validation.ValidateQuantity();
			AssertEquals("Quantity", 5m, matchingInventory.Quantity);
			AssertHasError(matchingInventory.QuantityInfo, "You cannot allocate more than the Available Quantity of 1");
		}

		public void TestValidateQuantity_CrossDockReservationPastAvailableQuantity()
		{
			var client = WarehouseHelper.CreateClient("CLIENT");
			Helper.SetClientAllAttributeType(client, true);
			Factory.Save();

			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A", 4, 4);
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			part.OP_Desc = "Desc of super part";
			Helper.SetProductAllAttributeUse(client, part, true);
			Factory.Save();

			var packingDate = ZDate.Today;
			var expiryDate = ZDate.Today.AddDays(7);

			var docket = WarehouseHelper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new TestNotificationBuffer());
			WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, 1m, ZGuid.Empty, "", expiryDate, packingDate, "Mismatch", "A2", "A3", "Serial1", "");
			WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, 1m, ZGuid.Empty, "", expiryDate, packingDate, "A1", "Mismatch", "A3", "Serial2", "");
			WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, 1m, ZGuid.Empty, "", expiryDate, packingDate, "A1", "A2", "Mismatch", "Serial3", "");
			WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, 1m, ZGuid.Empty, "", expiryDate, packingDate, "A1", "A2", "A3", "Serial4", "");
			var invLine = WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, 1m, ZGuid.Empty, "", expiryDate, packingDate, "A1", "A2", "A3", "Serial5", "");

			docket.AllocateLocationsWithMock();
			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - make sure Receive was finalised", true, docket.IsFinalised);
			Factory.Save();

			var matchingInventory = Factory.Load<TrackingWhsInventory>(invLine.PK);
			matchingInventory.WI_TotalUnits = 1m;
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, warehouse, "1", Notify);
			var orderLine = CreateWhsOrderLine(order, part, 1m, expiryDate, packingDate, "A1", "A2", "A3", "Serial5");
			orderLine.ReserveStockIfAbleTo(matchingInventory, 1m);
			matchingInventory.ShoppingCart = new TrackingWhsOrder(order);
			AssertEquals("No inventory available for Cross Dock.", 0m, matchingInventory.WI_AvailableForCrossDockQuantity);

			matchingInventory.Validation.ValidateQuantity();
			AssertEquals("Quantity", 0m, matchingInventory.Quantity);
			AssertEquals("Reserved Quantity", 1m, orderLine.ReservedQuantity);
			AssertHasError(matchingInventory.QuantityInfo, "You cannot allocate more than the Available Quantity of 0");
		}

		#endregion

		#region Implementation

		protected override WhsInventoryView GetNewInventory()
		{
			return Factory.New<WhsReceiveLine>().Inventory[0];
		}

		WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZDate expiryDate, ZDate packingDate, string attrib1Value, string attrib2Value, string attrib3Value, string serialNumberValue)
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.WE_ExpiryDate = expiryDate;
			orderLine.WE_PackingDate = packingDate;
			orderLine.WE_PartAttrib1 = attrib1Value;
			orderLine.WE_PartAttrib2 = attrib2Value;
			orderLine.WE_PartAttrib3 = attrib3Value;
			orderLine.WE_SerialNumber = serialNumberValue;
			return orderLine;
		}

		WhsTestHelperFunctions WarehouseHelper => fWarehouseHelper ?? (fWarehouseHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions fWarehouseHelper;

		#endregion
	}
}
