using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using WarehouseDO = Enterprise.UniversalDataBuss.DataObjects.Universal.Warehouse;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsHoldOrderDataObjectReaderTest : WhsUniversalTestCase
	{
		#region TestPopulateBusinessObject_SuccessfulImport

		public void TestPopulateBusinessObject_SuccessfulImport()
		{
			TestPopulateBusinessObject_SuccessfulImport(useNullDates: false);
		}

		public void TestPopulateBusinessObject_SuccessfulImport_WithNullDates()
		{
			// This was failing functionally but needed to add this to get it to fail locally - suffix generation for nulls was broken
			TestPopulateBusinessObject_SuccessfulImport(useNullDates: true);
		}

		void TestPopulateBusinessObject_SuccessfulImport(bool useNullDates)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			var client = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Helper.CreateProductClientRelationShip(client, data.Part1);
			Helper.CreateProductClientRelationShip(client, data.Part2);

			var receive = Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation, "", finalise: false);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation, "");
			Helper.SetInventoryAttributes(receive.Inventory[0], eD: today.AddDays(7), pD: today.AddDays(-1), pA1: "1", pA2: "2", pA3: "3", bEK: "", "SN");

			receive.NotificationManager.Push(Helper.Notify);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			inventory2.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			inventory2.InDocketLine.ChangeInventoryHeldCode(true);
			Factory.SaveForTesting();

			// Setup DataObjects
			var holdOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));
			holdOrderDO.Order.Warehouse = new WarehouseDO { Code = "1", Name = "SOMEWAREHOUSE" };

			var holdOrderLine1DO = new OrderLine();
			var holdOrderLine2DO = new OrderLine();
			holdOrderDO.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { holdOrderLine1DO, holdOrderLine2DO }));

			holdOrderLine1DO.LineNumber = 1;
			holdOrderLine1DO.Product = new Product { Code = "P1" };
			holdOrderLine1DO.OrderedQty = 5m;
			holdOrderLine1DO.ExpiryDate = useNullDates ? (ZDate?)null : today.AddDays(7);
			holdOrderLine1DO.PackingDate = useNullDates ? (ZDate?)null : today.AddDays(-1);
			holdOrderLine1DO.PartAttribute1 = "1";
			holdOrderLine1DO.PartAttribute2 = "2";
			holdOrderLine1DO.PartAttribute3 = "3";
			holdOrderLine1DO.SerialNumber = "SN";
			holdOrderLine1DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine1DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Damaged, Description = InventoryHoldCodes.Descriptions.Damaged };

			holdOrderLine2DO.LineNumber = 2;
			holdOrderLine2DO.Product = new Product { Code = "P2" };
			holdOrderLine2DO.OrderedQty = 1m;
			holdOrderLine2DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };
			holdOrderLine2DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };

			// Import DataObject
			var holdOrderBO = new WhsHoldOrderDataObjectReader(holdOrderDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", holdOrderBO);
			AssertEquals("Client should be 'WUFSHIJNB'", GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK, holdOrderBO.ClientPK);
			AssertEquals("Should have added two lines.", 2, holdOrderBO.Lines.Count);

			var holdOrderLine1BO = holdOrderBO.Lines[0];
			var holdOrderLine2BO = holdOrderBO.Lines[1];

			// Assert Bizo Populated correctly (i.e. ensure comprehensive tests in WhsHoldOrderLine will suffice)
			AssertNotNull("Precondition", holdOrderLine1BO);
			AssertNotNull("Precondition", holdOrderLine2BO);

			AssertEquals("Product", data.Part1, holdOrderLine1BO.Product);
			AssertEquals("Quantity", 5m, holdOrderLine1BO.Quantity);
			AssertEquals("PackingDate", useNullDates ? ZDate.Empty : today.AddDays(-1), holdOrderLine1BO.PackingDate);
			AssertEquals("ExpiryDate", useNullDates ? ZDate.Empty : today.AddDays(7), holdOrderLine1BO.ExpiryDate);
			AssertEquals("PartAttribute1", "1", holdOrderLine1BO.PartAttrib1);
			AssertEquals("PartAttribute2", "2", holdOrderLine1BO.PartAttrib2);
			AssertEquals("PartAttribute3", "3", holdOrderLine1BO.PartAttrib3);
			AssertEquals("SerialNumber", "SN", holdOrderLine1BO.SerialNumber);

			AssertEquals("Product", data.Part2, holdOrderLine2BO.Product);
			AssertEquals("Quantity", 1m, holdOrderLine2BO.Quantity);
			AssertEquals("PackingDate", ZDate.Empty, holdOrderLine2BO.PackingDate);
			AssertEquals("ExpiryDate", ZDate.Empty, holdOrderLine2BO.ExpiryDate);
			AssertEquals("PartAttribute1", ZString.Empty, holdOrderLine2BO.PartAttrib1);
			AssertEquals("PartAttribute2", ZString.Empty, holdOrderLine2BO.PartAttrib2);
			AssertEquals("PartAttribute3", ZString.Empty, holdOrderLine2BO.PartAttrib3);
			AssertEquals("SerialNumber", ZString.Empty, holdOrderLine2BO.SerialNumber);

			// Assert hold order "finalised" - Changed hold codes
			AssertEquals("Should have changed hold code of matched inventory", InventoryHoldCodes.Codes.Damaged, receive.Lines[0].WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should have changed hold code of matched inventory", "", receive.Lines[1].WE_WHC_NKCurrentInventoryHeldCode);

			AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(Logger));
		}

		#endregion

		#region TestPopulateBusinessObject_WithHoldReason

		public void TestPopulateBusinessObject_WithHoldReason()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.SaveForTesting();

			var holdOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.AddOrgAddress(new DataWritingManager(new DummyActionInfo()), data.Org1, DocAddressType.WarehouseClient);
			holdOrderDO.Order.Warehouse = new WarehouseDO { Code = "1", Name = "SOMEWAREHOUSE" };

			var holdOrderLineDO = new OrderLine(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { holdOrderLineDO }));

			holdOrderLineDO.LineNumber = 1;
			holdOrderLineDO.Product = new Product { Code = "P1" };
			holdOrderLineDO.OrderedQty = 10m;
			holdOrderLineDO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLineDO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Damaged, Description = InventoryHoldCodes.Descriptions.Damaged };
			holdOrderLineDO.CurrentHoldReason = "I want to hold you!";

			// Import DataObject
			var holdOrderBO = new WhsHoldOrderDataObjectReader(holdOrderDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition", holdOrderBO);
			AssertEquals("Should have added one line.", 1, holdOrderBO.Lines.Count);

			var holdOrderLine = holdOrderBO.Lines[0];
			AssertEquals(nameof(holdOrderLine.ToHoldCode), InventoryHoldCodes.Codes.Damaged, holdOrderLine.ToHoldCode);
			AssertEquals(nameof(holdOrderLine.HoldReason), "I want to hold you!", holdOrderLine.HoldReason);

			// Assert hold order "finalised" - Changed hold codes
			AssertEquals("Should have changed hold code of matched inventory.", InventoryHoldCodes.Codes.Damaged, receive.Lines[0].WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should have applied hold reason on matched inventory.", "I want to hold you!", receive.Lines[0].WE_CurrentHoldReason);

			AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(Logger));
		}

		#endregion

		#region TestPopulateBusinessObject_FailedImport

		public void TestPopulateBusinessObject_FailedImport()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			var client = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Helper.CreateProductClientRelationShip(client, data.Part1);
			Helper.CreateProductClientRelationShip(client, data.Part2);

			var receive = Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", data.Part1, 50m);
			AssertIsFinalisedPrecondition(receive);
			Factory.SaveForTesting();

			// Setup DataObjects
			var holdOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));
			holdOrderDO.Order.Warehouse = new WarehouseDO { Code = "1", Name = "SOMEWAREHOUSE" };

			var holdOrderLine1DO = new OrderLine();
			var holdOrderLine2DO = new OrderLine();
			var holdOrderLine3DO = new OrderLine();
			var holdOrderLine4DO = new OrderLine();
			holdOrderDO.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>(new[] { holdOrderLine1DO, holdOrderLine2DO, holdOrderLine3DO, holdOrderLine4DO }));

			holdOrderLine1DO.LineNumber = 1;
			holdOrderLine1DO.Product = new Product { Code = "P1" };
			holdOrderLine1DO.OrderedQty = 40m;
			holdOrderLine1DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine1DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };

			holdOrderLine2DO.LineNumber = 2;
			holdOrderLine2DO.Product = new Product { Code = "P2" };
			holdOrderLine2DO.OrderedQty = 20m;
			holdOrderLine2DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine2DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };

			holdOrderLine3DO.LineNumber = 3;
			holdOrderLine3DO.Product = new Product { Code = "P2" };
			holdOrderLine3DO.OrderedQty = 20m;
			holdOrderLine3DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine3DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };

			holdOrderLine4DO.LineNumber = 4;
			holdOrderLine4DO.Product = new Product { Code = "P1" };
			holdOrderLine4DO.OrderedQty = 10m;
			holdOrderLine4DO.OriginalHoldCode = new CodeDescriptionPair9Char { Code = "", Description = "None" };
			holdOrderLine4DO.CurrentHoldCode = new CodeDescriptionPair9Char { Code = "HEL", Description = "Held" };

			// Assert Error Message
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Cannot import Hold Order:
Line Error: Could not match inventory for
Product:P2
Quantity:20
Current Hold Code:
Line Error: Could not match inventory for
Product:P2
Quantity:20
Current Hold Code:", () => new WhsHoldOrderDataObjectReader(holdOrderDO, Logger, Factory).ReadIntoBusinessObject());

			// Assert doesnt change hold code on other lines if one line fails
			// Need to use the data context manager as there will be changes in the factory that are disposed when the data context manager doesnt save on failed import
			// Dodgy, but necessary as Universal uses exceptions as control flow
			AssertEquals("Should have failed", false, ((IShipmentDataContextManager)new WarehouseHoldOrderDataContextManager()).UseIncomingShipmentData(holdOrderDO, Logger, Factory));

			receive = new BusinessObjectFactory().Load<WhsReceive>(receive.PK);
			AssertEquals("Import failed - should not have changed hold code on Inventory Line 0", 50m, receive.Lines[0].WE_StockOnHand);
			AssertEquals("Import failed - should not have changed hold code on Inventory Line 0", "", receive.Lines[0].WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region TestPopulateBusinessObject_CannotFindClientAddress

		public void TestPopulateBusinessObject_CannotFindClientAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObject
			var holdOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			holdOrderDO.Order = new Order();
			holdOrderDO.Order.Warehouse = new WarehouseDO { Code = "1" };
			holdOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "TEST" } }));

			// Import DataObjects
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to match Client.", () => new WhsHoldOrderDataObjectReader(holdOrderDO, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBusinessObject_NullOrderOrWarehouse

		public void TestPopulateBusinessObject_NullOrderOrWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			Factory.SaveForTesting();

			// Setup DataObject
			var holdOrderDO = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var client = new OrganisationDataObjectReader(GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			holdOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)) }));

			// Import DataObjects
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Could not import due to missing Warehouse Information.", () => new WhsHoldOrderDataObjectReader(holdOrderDO, Logger, Factory).ReadIntoBusinessObject());

			holdOrderDO.Order = new Order();
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Could not import due to missing Warehouse Information.", () => new WhsHoldOrderDataObjectReader(holdOrderDO, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
