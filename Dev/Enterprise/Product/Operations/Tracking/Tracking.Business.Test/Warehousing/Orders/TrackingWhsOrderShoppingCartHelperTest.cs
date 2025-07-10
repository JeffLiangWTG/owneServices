using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingWhsOrderShoppingCartHelperTest : TestCaseWithFactory
	{
		public void TestOrderLineShortfallQuantityCached()
		{
			var helper = new TrackingWhsOrderShoppingCartHelper(new TrackingSiteUser());
			var client = WarehouseHelper.CreateClient("CLIENT");
			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			var inventory = CreateInventory(client.PK, warehouse.PK, part, "R1", 100m);

			// committ some units to some order
			var order = WarehouseHelper.CreateWhsOrderWithOrderLine(client, warehouse, part, 20m);
			WarehouseHelper.CreatePickNew(order);
			AssertEquals(80m, inventory.WI_AvailableToTransferQuantity);

			Factory.Save();

			inventory.Quantity = 50m;
			helper.ShoppingCart.WhsOrder.WD_OH_Client = client.PK;
			helper.AddOrderLine(inventory);

			AssertEquals(1, helper.ShoppingCart.WhsOrder.Lines.Count);
			AssertEquals(0m, helper.ShoppingCart.WhsOrder.Lines[0].WE_ShortfallQuantityCached);

			// check that cached properdy doesn't change.
			helper.ShoppingCart.WhsOrder.Lines[0].WE_TransactionQuantity = 70;
			AssertEquals(0m, helper.ShoppingCart.WhsOrder.Lines[0].WE_ShortfallQuantityCached);

			helper.ShoppingCart.WhsOrder.Lines[0].WE_TransactionQuantity = 80;
			AssertEquals(0m, helper.ShoppingCart.WhsOrder.Lines[0].WE_ShortfallQuantityCached);

			helper.ShoppingCart.WhsOrder.Lines[0].WE_TransactionQuantity = 90;
			AssertEquals(10m, helper.ShoppingCart.WhsOrder.Lines[0].WE_ShortfallQuantityCached);
		}

		#region TestAddNewOrderLine

		public void TestAddNewOrderLine()
		{
			var siteUser = new TrackingSiteUser();
			var helper = new TrackingWhsOrderShoppingCartHelper(siteUser);
			var client = WarehouseHelper.CreateClient("CLIENT");
			Factory.Save();

			siteUser.LoginSupportForTest(client.OH_Code);
			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			part.OP_Desc = "Desc of super part";
			Factory.Save();
			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var quantity = 30m;
			var inventory = CreateInventory(client.PK, warehouse.PK, part, "R1", quantity);
			inventory.Quantity = quantity;
			helper.AddOrderLine(inventory);
			AssertEquals("One Order Line", 1, helper.ShoppingCart.WhsOrder.Lines.Count);
			AssertEquals("Order WD_WW_Whs", warehouse.PK, helper.ShoppingCart.WhsOrder.WD_WW_Whs);
			AssertEquals("Line WE_OP", part.PK, helper.ShoppingCart.WhsOrder.Lines[0].WE_OP);
			AssertEquals("SUPER PART NUM", helper.ShoppingCart.WhsOrder.Lines[0].SupplierPart.OP_PartNum);
			AssertEquals("Desc of super part", helper.ShoppingCart.WhsOrder.Lines[0].SupplierPart.OP_Desc);
			AssertEquals("Line WE_TransactionQuantity", quantity, helper.ShoppingCart.WhsOrder.Lines[0].WE_TransactionQuantity);
			AssertEquals("One CrossDock", 1, helper.ShoppingCart.WhsOrder.Lines[0].ReservedPickLines.Count);
			AssertEquals("CrossDock WZ_WE_InventoryLine", inventory.WI_WE_InDocketLine, helper.ShoppingCart.WhsOrder.Lines[0].ReservedPickLines[0].WZ_WE_InventoryLine);
			AssertEquals("CrossDock WZ_OriginalReservedQty", quantity, helper.ShoppingCart.WhsOrder.Lines[0].ReservedPickLines[0].WZ_OriginalReservedQty);
		}

		public void TestAddNewOrderLine_WithAttributes()
		{
			var siteUser = new TrackingSiteUser();
			var helper = new TrackingWhsOrderShoppingCartHelper(siteUser);
			var client = WarehouseHelper.CreateClient("CLIENT");
			Factory.Save();

			siteUser.LoginSupportForTest(client.OH_Code);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.One, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Two, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Three, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Serial, true);

			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.One, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Two, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Three, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Serial, true);
			part.OP_Desc = "Desc of super part";
			Factory.Save();
			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var inventory = CreateInventory(client.PK, warehouse.PK, part, "R1", 1m, "PA1", "PA2", "PA3", "SN1");
			inventory.Quantity = 1m;
			helper.AddOrderLine(inventory);
			AssertEquals("One Order Line", 1, helper.ShoppingCart.WhsOrder.Lines.Count);
			AssertEquals("Order WD_WW_Whs", warehouse.PK, helper.ShoppingCart.WhsOrder.WD_WW_Whs);

			var orderLine = helper.ShoppingCart.WhsOrder.Lines[0];
			AssertEquals("Line WE_OP", part.PK, orderLine.WE_OP);
			var orderLineProduct = orderLine.SupplierPart;
			AssertEquals("SUPER PART NUM", orderLineProduct.OP_PartNum);
			AssertEquals("Desc of super part", orderLineProduct.OP_Desc);

			AssertEquals("Line WE_TransactionQuantity", 1m, orderLine.WE_TransactionQuantity);
			AssertEquals("Line WE_PartAttribute1", "PA1", orderLine.WE_PartAttrib1);
			AssertEquals("Line WE_PartAttribute2", "PA2", orderLine.WE_PartAttrib2);
			AssertEquals("Line WE_PartAttribute3", "PA3", orderLine.WE_PartAttrib3);
			AssertEquals("Line WE_SerialNumber", "SN1", orderLine.WE_SerialNumber);

			AssertEquals("One CrossDock", 1, orderLine.ReservedPickLines.Count);
			var reservedPickLine = orderLine.ReservedPickLines[0];
			AssertEquals("CrossDock WZ_WE_InventoryLine", inventory.WI_WE_InDocketLine, reservedPickLine.WZ_WE_InventoryLine);
			AssertEquals("CrossDock WZ_OriginalReservedQty", 1m, reservedPickLine.WZ_OriginalReservedQty);
		}

		#endregion

		#region TestAddExistingOrderLine

		public void TestAddExistingOrderLine()
		{
			var today = ZDate.Today;
			var siteUser = new TrackingSiteUser();
			var helper = new TrackingWhsOrderShoppingCartHelper(siteUser);
			var client = WarehouseHelper.CreateClient("CLIENT");
			Factory.Save();

			siteUser.LoginSupportForTest(client.OH_Code);
			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			part.OP_Desc = "Desc of super part";
			Factory.Save();
			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var inventory1 = CreateInventory(client.PK, warehouse.PK, part, "R1", 20);
			inventory1.Quantity = 20m;
			inventory1.WI_ExpiryDate = today.AddDays(5);
			inventory1.WI_PackingDate = today.AddDays(-5);
			helper.AddOrderLine(inventory1);
			var inventory2 = CreateInventory(client.PK, warehouse.PK, part, "R2", 30);
			inventory2.Quantity = 10m;
			inventory2.WI_ExpiryDate = today.AddDays(5);
			inventory2.WI_PackingDate = today.AddDays(-5);
			helper.AddOrderLine(inventory2);
			AssertEquals("One Order Line", 1, helper.ShoppingCart.WhsOrder.Lines.Count);
			AssertEquals("Order WD_WW_Whs", warehouse.PK, helper.ShoppingCart.WhsOrder.WD_WW_Whs);
			AssertEquals("Line WE_ExpiryDate", today.AddDays(5), helper.ShoppingCart.WhsOrder.Lines[0].WE_ExpiryDate);
			AssertEquals("Line WE_PackingDate", today.AddDays(-5), helper.ShoppingCart.WhsOrder.Lines[0].WE_PackingDate);
			AssertEquals("Line WE_OP", part.PK, helper.ShoppingCart.WhsOrder.Lines[0].WE_OP);
			AssertEquals("SUPER PART NUM", helper.ShoppingCart.WhsOrder.Lines[0].SupplierPart.OP_PartNum);
			AssertEquals("Desc of super part", helper.ShoppingCart.WhsOrder.Lines[0].SupplierPart.OP_Desc);
			AssertEquals("Line WE_TransactionQuantity", 30m, helper.ShoppingCart.WhsOrder.Lines[0].WE_TransactionQuantity);

			var crossdocks = helper.ShoppingCart.WhsOrder.Lines[0].ReservedPickLines.ToArray();
			AssertNotNull("CrossDocks", crossdocks);
			AssertEquals("Two CrossDock", 2, crossdocks.Length);
			var crossdock1 = crossdocks[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine ? crossdocks[0] : crossdocks[1];
			var crossdock2 = crossdocks[1].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine ? crossdocks[1] : crossdocks[0];

			AssertEquals("CrossDock1 WZ_WE_InventoryLine", inventory1.WI_WE_InDocketLine, crossdock1.WZ_WE_InventoryLine);
			AssertEquals("CrossDock1 WZ_OriginalReservedQty", 20m, crossdock1.WZ_OriginalReservedQty);
			AssertEquals("CrossDock2 WZ_WE_InventoryLine", inventory2.WI_WE_InDocketLine, crossdock2.WZ_WE_InventoryLine);
			AssertEquals("CrossDock2 WZ_OriginalReservedQty", 10m, crossdock2.WZ_OriginalReservedQty);

			inventory1.Quantity = 10m;
			helper.AddOrderLine(inventory1);

			AssertEquals("Quantity not added", 10m, inventory1.Quantity);
			AssertHasErrorContaining(inventory1.QuantityInfo, "You cannot allocate more than the Available Quantity of");
		}

		public void TestAddExistingOrderLine_WithAttributes()
		{
			var siteUser = new TrackingSiteUser();
			var helper = new TrackingWhsOrderShoppingCartHelper(siteUser);
			var client = WarehouseHelper.CreateClient("CLIENT");
			Factory.Save();

			siteUser.LoginSupportForTest(client.OH_Code);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.One, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Two, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Three, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Serial, true);

			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.One, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Two, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Three, true);
			part.OP_Desc = "Desc of super part";
			Factory.Save();
			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var inventory1 = CreateInventory(client.PK, warehouse.PK, part, "R1", 10m, "PA1", "PA2", "PA3");
			inventory1.Quantity = 10m;
			helper.AddOrderLine(inventory1);
			var inventory2 = CreateInventory(client.PK, warehouse.PK, part, "R2", 30m, "PA1", "PA2", "PA3");
			inventory2.Quantity = 20m;
			helper.AddOrderLine(inventory2);
			AssertEquals("One Order Line", 1, helper.ShoppingCart.WhsOrder.Lines.Count);
			AssertEquals("Order WD_WW_Whs", warehouse.PK, helper.ShoppingCart.WhsOrder.WD_WW_Whs);

			var orderLine = helper.ShoppingCart.WhsOrder.Lines[0];
			AssertEquals("Line WE_OP", part.PK, orderLine.WE_OP);
			var orderLineProduct = orderLine.SupplierPart;
			AssertEquals("SUPER PART NUM", orderLineProduct.OP_PartNum);
			AssertEquals("Desc of super part", orderLineProduct.OP_Desc);

			AssertEquals("Line WE_TransactionQuantity", 30m, orderLine.WE_TransactionQuantity);
			AssertEquals("Line WE_PartAttribute1", "PA1", orderLine.WE_PartAttrib1);
			AssertEquals("Line WE_PartAttribute2", "PA2", orderLine.WE_PartAttrib2);
			AssertEquals("Line WE_PartAttribute3", "PA3", orderLine.WE_PartAttrib3);

			var crossdocks = helper.ShoppingCart.WhsOrder.Lines[0].ReservedPickLines.ToArray();
			AssertNotNull("CrossDocks", crossdocks);
			AssertEquals("Two CrossDocks", 2, crossdocks.Length);
			var crossdock1 = crossdocks.Single(crossdock => crossdock.WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("CrossDock1 WZ_OriginalReservedQty", 10m, crossdock1.WZ_OriginalReservedQty);
			var crossdock2 = crossdocks.Single(crossdock => crossdock.WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("CrossDock1 WZ_OriginalReservedQty", 20m, crossdock2.WZ_OriginalReservedQty);
		}

		#endregion

		#region TestAddProductsWithDifferentAttributes

		public void TestAddProductsWithDifferentAttributes()
		{
			TrackingWhsOrderShoppingCartHelper helper = new TrackingWhsOrderShoppingCartHelper(new TrackingSiteUser());

			var client = WarehouseHelper.CreateClient("CLIENT");
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.One, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Two, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Three, false);
			WarehouseHelper.SetClientAttributeType(client, AttributeNumber.Serial, true);

			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.One, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Two, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Three, true);
			WarehouseHelper.SetProductAttributeUse(client, part, AttributeNumber.Serial, true);
			part.OP_Desc = "Desc of super part";

			Factory.Save();

			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var inventory1 = CreateInventory(client.PK, warehouse.PK, part, "R1", 1m, "SMALL", "", "", "SN1");
			var inventory2 = CreateInventory(client.PK, warehouse.PK, part, "R2", 1m, "BIG", "", "", "SN2");
			var inventory3 = CreateInventory(client.PK, warehouse.PK, part, "R3", 1m, "", "", "", "SN3");
			var inventory4 = CreateInventory(client.PK, warehouse.PK, part, "R4", 1m, "SMALL", "COOL", "RED", "SN4");
			var inventory5 = CreateInventory(client.PK, warehouse.PK, part, "R5", 1m, "SMALL", "COOL", "GREEN", "SN5");
			var inventory6 = CreateInventory(client.PK, warehouse.PK, part, "R6", 1m, "SMALL", "COOL", "GREEN", "SN6");
			inventory1.Quantity = 1m;
			inventory2.Quantity = 1m;
			inventory3.Quantity = 1m;
			inventory4.Quantity = 1m;
			inventory5.Quantity = 1m;
			inventory6.Quantity = 1m;
			helper.AddOrderLine(inventory1);
			helper.AddOrderLine(inventory2);
			helper.AddOrderLine(inventory3);
			helper.AddOrderLine(inventory4);
			helper.AddOrderLine(inventory5);
			helper.AddOrderLine(inventory6);

			AssertEquals(6, helper.ShoppingCart.WhsOrder.Lines.Count);
		}

		#endregion

		#region TestAddProductsWithDifferentDates

		public void TestAddProductsWithDifferentDates()
		{
			var helper = new TrackingWhsOrderShoppingCartHelper(new TrackingSiteUser());
			var client = WarehouseHelper.CreateClient("CLIENT");
			var warehouse = WarehouseHelper.CreateWarehouse("WHS", "A");
			var part = WarehouseHelper.CreateProduct(client, "PART");
			part.OP_Desc = "Some part";

			Factory.Save();

			var orderLines = helper.ShoppingCart.WhsOrder.Lines;

			var inventory1 = CreateInventory(client.PK, warehouse.PK, part);
			inventory1.Quantity = 5m;
			inventory1.WI_PackingDate = ZDate.Today.AddDays(1);
			inventory1.WI_ExpiryDate = ZDate.Today.AddDays(2);

			var inventory2 = CreateInventory(client.PK, warehouse.PK, part);
			inventory2.Quantity = 20m;
			inventory2.WI_PackingDate = ZDate.Today.AddDays(1);
			inventory2.WI_ExpiryDate = ZDate.Today.AddDays(2);

			var inventory3 = CreateInventory(client.PK, warehouse.PK, part);
			inventory3.Quantity = 30m;
			inventory3.WI_PackingDate = ZDate.Today.AddDays(1);
			inventory3.WI_ExpiryDate = ZDate.Today.AddDays(3);

			var inventory4 = CreateInventory(client.PK, warehouse.PK, part);
			inventory4.Quantity = 40m;
			inventory4.WI_PackingDate = ZDate.Today.AddDays(2);
			inventory4.WI_ExpiryDate = ZDate.Today.AddDays(2);

			AssertEquals(0, orderLines.Count);

			helper.AddOrderLine(inventory1);
			AssertEquals(1, orderLines.Count);
			AssertEquals(5m, orderLines[0].WE_TransactionQuantity);
			AssertEquals(inventory1.WI_PackingDate, orderLines[0].WE_PackingDate);
			AssertEquals(inventory1.WI_ExpiryDate, orderLines[0].WE_ExpiryDate);

			helper.AddOrderLine(inventory2);
			AssertEquals(1, orderLines.Count);
			AssertEquals(25m, orderLines[0].WE_TransactionQuantity);
			AssertEquals(inventory1.WI_PackingDate, orderLines[0].WE_PackingDate);
			AssertEquals(inventory1.WI_ExpiryDate, orderLines[0].WE_ExpiryDate);

			helper.AddOrderLine(inventory3);
			AssertEquals(2, orderLines.Count);
			AssertEquals(30m, orderLines[1].WE_TransactionQuantity);
			AssertEquals(inventory3.WI_PackingDate, orderLines[1].WE_PackingDate);
			AssertEquals(inventory3.WI_ExpiryDate, orderLines[1].WE_ExpiryDate);

			helper.AddOrderLine(inventory4);
			AssertEquals(3, orderLines.Count);
			AssertEquals(40m, orderLines[2].WE_TransactionQuantity);
			AssertEquals(inventory4.WI_PackingDate, orderLines[2].WE_PackingDate);
			AssertEquals(inventory4.WI_ExpiryDate, orderLines[2].WE_ExpiryDate);
		}

		#endregion

		#region TestAddOrderLineWhenQuantityInfoHasErrors

		public void TestAddOrderLineWhenQuantityInfoHasErrors()
		{
			var helper = new TrackingWhsOrderShoppingCartHelper(new TrackingSiteUser());

			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var inventory = Factory.New<TrackingWhsInventory>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PROD1";
			part1.OP_Desc = "First Product";
			inventory.WI_OP = part1.PK;
			inventory.WI_TotalUnits = 2;
			inventory.Quantity = 3;
			inventory.RunPreSaveValidation();
			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			helper.AddOrderLine(inventory);
			Assert(inventory.QuantityInfo.HasErrors());
			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);
		}

		#endregion

		#region TestValidateWarehouse

		public void TestValidateWarehouse()
		{
			var helper = new TrackingWhsOrderShoppingCartHelper(new TrackingSiteUser());

			var client = WarehouseHelper.CreateClient("CLIENT");
			var warehouse1 = WarehouseHelper.CreateWarehouse("WHS1", "A");
			var warehouse2 = WarehouseHelper.CreateWarehouse("WHS2", "B");
			var part = WarehouseHelper.CreateProduct(client, "Super part num");
			part.OP_Desc = "Desc of super part";

			Factory.Save();

			AssertEquals(0, helper.ShoppingCart.WhsOrder.Lines.Count);

			var inventory1 = CreateInventory(client.PK, warehouse1.PK, part, "R1", 20);
			inventory1.Quantity = 20;

			helper.AddOrderLine(inventory1);

			var inventory2 = CreateInventory(client.PK, warehouse2.PK, part, "R2", 30);
			inventory2.Quantity = 10;

			AssertEquals(0, inventory2.QuantityInfo.GetErrors().Count());

			helper.AddOrderLine(inventory2);

			AssertEquals(1, inventory2.QuantityInfo.GetErrors().Count());
			AssertEquals("You can only select inventory from a single warehouse", inventory2.QuantityInfo.GetErrors().GetFirstMessage());
		}

		#endregion

		#region Implementation

		TrackingWhsInventory CreateInventory(ZGuid clientPK, ZGuid whsPK, OrgSupplierPart part, string reference = "", string attrib1Value = "", string attrib2Value = "", string attrib3Value = "", string serialNumberValue = "")
		{
			return CreateInventory(clientPK, whsPK, part, reference, 100m, attrib1Value, attrib2Value, attrib3Value, serialNumberValue);
		}

		TrackingWhsInventory CreateInventory(ZGuid clientPK, ZGuid whsPK, OrgSupplierPart part, string reference, ZDecimal availQuantity, string attrib1Value = "", string attrib2Value = "", string attrib3Value = "", string serialNumberValue = "")
		{
			var notify = new TestNotificationBuffer();
			var docket = WarehouseHelper.CreateWhsReceive(clientPK, whsPK, reference, notify);
			var invLine = WarehouseHelper.CreateWhsReceiveInventoryLine(docket, part.PK, availQuantity, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, attrib1Value, attrib2Value, attrib3Value, serialNumberValue, "");
			docket.AllocateLocationsWithMock();
			docket.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);
			AssertEquals("Available", availQuantity, invLine.WI_AvailableToTransferQuantity);
			AssertEquals("AvailableToPick", availQuantity, invLine.WI_AvailableToPickQuantity);

			Factory.Save();
			var inventory = Factory.Load<TrackingWhsInventory>(invLine.PK);
			inventory.WI_TotalUnits = availQuantity;
			AssertEquals("TrackingWhsInventory Available", availQuantity, inventory.WI_AvailableToTransferQuantity);
			AssertEquals("TrackingWhsInventory AvailableToPick", availQuantity, inventory.WI_AvailableToPickQuantity);

			inventory.Quantity = 10;
			return inventory;
		}

		#region WarehouseHelper

		WhsTestHelperFunctions WarehouseHelper => fWarehouseHelper ?? (fWarehouseHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions fWarehouseHelper;

		#endregion

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion

		#endregion
	}
}
