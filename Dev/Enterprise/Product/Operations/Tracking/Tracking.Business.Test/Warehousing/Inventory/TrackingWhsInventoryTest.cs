using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsInventory))]
	[SetGlobalsIsWeb]
	[HttpContextEnabledTest]
	sealed class TrackingWhsInventoryTest : WhsInventoryViewTest
	{
		protected override Type GetExpectedValidationType()
		{
			return typeof(TrackingWhsInventoryValidation);
		}

		protected override Type GetExpectedUSValidationType()
		{
			return typeof(TrackingWhsInventoryValidation);
		}

		#region TestDocket_Tracking

		public void TestDocket_Tracking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryFromReceive = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - Adjustment is Finalised.", true, adjustment.IsFinalised);
			var inventoryFromAdjustment = adjustmentLine.Inventory[0];

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var inventoryFromReceiveInOtherFactory = otherFactory.Load<TrackingWhsInventory>(inventoryFromReceive.PK);
			var inventoryFromAdjustmentInOtherFactory = otherFactory.Load<TrackingWhsInventory>(inventoryFromAdjustment.PK);
			AssertEquals(typeof(WhsReceive), inventoryFromReceiveInOtherFactory.Docket.GetType());
			AssertEquals(typeof(WhsAdjustment), inventoryFromAdjustmentInOtherFactory.Docket.GetType());
		}

		#endregion

		#region TestPickAllocationsAsString

		public void TestPickAllocationsAsString()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2", Notify);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 15m);
			var reservedOrderLine = Helper.CreateWhsOrderLine(order3, data.Part1, 5m);
			var reservedPickLine = reservedOrderLine.ReserveStockIfAbleTo(data.Line111);
			AssertEquals("Precondition: Stock is reserved.", 5m, reservedPickLine.ReservedQuantity);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var availableInventory = pick3.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PickLineQuantity > 0m);
			availableInventory.PickLineQuantity = 0m;

			Factory.Save();

			var inventory = data.Line111;
			var trackingInventory = Factory.Load<TrackingWhsInventory>(inventory.PK);
			var docketLine = inventory.InDocketLine;
			AssertEquals(2, docketLine.PickAllocations.Count);
			AssertEquals("P00000001", docketLine.PickAllocations.ElementAt(0).WP_PickNo);
			AssertEquals("P00000002", docketLine.PickAllocations.ElementAt(1).WP_PickNo);
			AssertEquals("P00000001, P00000002", docketLine.PickAllocationsAsString);

			AssertEquals(trackingInventory.PickAllocationsAsString, docketLine.PickAllocationsAsString);
		}

		#endregion

		public void TestEnableOnlyAllocateColumnValidationOnPreSave()
		{
			Factory.SuspendValidation();

			var data = new TestDataSimpleEnvironment(Factory);
			var location1 = data.Whs1.Rows[0].Locations[0];
			var location2 = data.Whs1.Rows[0].Locations.AddNew();

			var receive = TrackingHelper.Get(Factory.New<WhsReceive>());
			var whsReceive = receive.WhsReceive;
			whsReceive.WD_OH_Client = data.Org1.PK;
			whsReceive.WD_WW_Whs = data.Whs1.PK;

			var inventory1 = receive.Lines.AddNew().Inventory[0];
			var inventory2 = receive.Lines.AddNew().Inventory[0];
			inventory1.WI_OP = data.Part1.PK;
			inventory2.WI_OP = data.Part1.PK;
			inventory1.Quantity = 12345678901234567890.1;
			inventory2.Quantity = 12345678901234567890.1;
			inventory1.WI_F3_NKPackType = "AAA";
			inventory2.WI_F3_NKPackType = "AAA";

			inventory1.EnableOnlyAllocateColumnValidationOnPreSave();

			Factory.ResumeValidation();
			receive.RunPreSaveValidation();

			Assert("WI_F3_NKPackType validation was disabled - should be no notifications", !inventory1.WI_F3_NKPackTypeInfo.HasNotifications());
			Assert("Validation was not disabled - WI_F3_NKPackType should fail", inventory2.WI_F3_NKPackTypeInfo.HasNotifications());
			Assert("Quantity validation is still enabled - should has notification", inventory1.QuantityInfo.HasNotifications());
			Assert("Quantity should fail with notofication", inventory2.QuantityInfo.HasNotifications());
		}

		public void TestGetAdditionalInformationFields()
		{
			OrgHeader testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "Test organization";
			testOrg.OH_Code = "TST";

			OrgCustomLabels orgCustomLabel1 = testOrg.CustomLabels.AddNew();
			orgCustomLabel1.OT_FieldName = "WhsDocketLine.CustomAttrib3";
			orgCustomLabel1.OT_Caption = "Custom Attribute 3 Test";

			OrgCustomLabels orgCustomLabel2 = testOrg.CustomLabels.AddNew();
			orgCustomLabel2.OT_FieldName = "WhsDocketLine.CustomFlag2";
			orgCustomLabel2.OT_Caption = "Custom Flag 2 Test";

			AssertEquals("There must be 2 custom labels (created for testing).", 2, testOrg.CustomLabels.Count);

			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			inventory.Docket.WD_OH_Client = testOrg.PK;
			var customLabels = inventory.GetAdditionalInformationFields();

			AssertEquals("All 6 custom labels must be retrieved.", 2, customLabels.Count);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(WhsInventoryView.Schema.WI_CustomAttrib3));
			CustomLabelInfo customLabel1 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(WhsInventoryView.Schema.WI_CustomAttrib3);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel1.OT_Caption, customLabel1.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel1.OT_FieldName, customLabel1.LabelName);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(WhsInventoryView.Schema.WI_CustomFlag2));
			CustomLabelInfo customLabel2 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(WhsInventoryView.Schema.WI_CustomFlag2);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel2.OT_Caption, customLabel2.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel2.OT_FieldName, customLabel2.LabelName);
		}

		public void TestWarehouseCodeAndWarehouseName()
		{
			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			AssertNull("Precondition: Warehouse should be null", inventory.Warehouse);

			var receive = Factory.New<WhsReceive>();
			var whs1 = Factory.New<WhsWarehouse>();
			whs1.WW_WarehouseCode = "ABC";
			whs1.WW_WarehouseName = "Sydney Warehouse";
			receive.WD_WW_Whs = whs1.PK;
			inventory.WI_WD = receive.PK;

			AssertEquals("WarehouseCode", "ABC", inventory.TrackingWarehouseCode);
			AssertEquals("WarehouseName", "Sydney Warehouse", inventory.TrackingWarehouseName);
		}

		public void TestWarehouseName_Translatable()
		{
			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			var receive = Factory.New<WhsReceive>();
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "ABC";
			warehouse.WW_WarehouseName = "Sydney Warehouse";
			receive.WD_WW_Whs = warehouse.PK;
			inventory.WI_WD = receive.PK;

			AssertEquals("WarehouseName in English.", "Sydney Warehouse", inventory.TrackingWarehouseName);
			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Sydney Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "悉尼仓库"));
				AssertEquals("WarehouseName in Chinese", "悉尼仓库", inventory.TrackingWarehouseName);
			}
		}

		public void TestProductCodeAndDescription()
		{
			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			AssertNull("Precondition - No Product selected", inventory.SupplierPart);
			AssertEquals("ProductDescription should be empty", ZString.Empty, inventory.TrackingProductDescription);

			OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PROD1";
			part1.OP_Desc = "First Product";
			inventory.WI_OP = part1.PK;
			AssertEquals("SupplierPart should be Product1", part1.PK, inventory.SupplierPart.PK);
			AssertEquals("Should be First Product", "PROD1", inventory.TrackingProductCode);
			AssertEquals("Should be First Product", "First Product", inventory.TrackingProductDescription);

			OrgSupplierPart part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PROD2";
			part2.OP_Desc = "Second Product";
			inventory.WI_OP = part2.PK;
			AssertEquals("SupplierPart should be Product1", part2.PK, inventory.SupplierPart.PK);
			AssertEquals("Should be Second Product", "PROD2", inventory.TrackingProductCode);
			AssertEquals("Should be Second Product", "Second Product", inventory.TrackingProductDescription);
		}

		public void TestProductDescriptionInfo()
		{
			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			ZPropertyInfoString info = inventory.TrackingProductDescriptionInfo as ZPropertyInfoString;
			AssertNotNull("Should be of type ZPropertyInfoString", info);
			AssertEquals("MaxLength", OrgSupplierPartSchema.OP_Desc.MaxLength, info.MaxLength);
			Assert("ReadOnly", info.ReadOnly);
		}

		public void TestQuantity()
		{
			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			AssertEquals("Default Value", 0m, inventory.Quantity);
			AssertNoErrors(inventory.QuantityInfo);

			inventory.Quantity = 10.55m;
			AssertEquals("Set Value", 10.55m, inventory.Quantity);
			AssertNoErrors(inventory.QuantityInfo);

			inventory.Quantity = 1111111111111111111m;
			AssertEquals("Set Value", 1111111111111111111m, inventory.Quantity);
			AssertHasErrors(inventory.QuantityInfo);
			AssertHasError(inventory.QuantityInfo, "The number 1,111,111,111,111,111,111 is too large, the maximum value allowed for selection is 999,999,999,999,999.999.");
		}

		public void TestWarehousePK()
		{
			var inventory = (TrackingWhsInventory)GetNewBusinessObject();
			AssertEquals(ZGuid.Empty, inventory.WarehousePK);

			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 2, 1);

			var receive = Factory.New<WhsReceive>();
			receive.WD_WW_Whs = whs1.PK;
			inventory.WI_WD = receive.PK;
			AssertEquals(whs1.PK, inventory.WarehousePK);

			inventory.WI_WL = whs1.Rows[0].Locations[0].PK;
			AssertEquals(whs1.PK, inventory.WarehousePK);

			inventory.WI_WL = whs2.Rows[0].Locations[0].PK;
			AssertEquals(whs2.PK, inventory.WarehousePK);
		}

		#region Internal Cache

		public void TestDocketCache()
		{
			var factory = new TestBusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			Factory.Save();

			AssertEquals(receive, inventory.Docket);
			factory.ClearCacheForTesting();
			AssertNotEquals(receive, factory.Load<WhsReceive>(receive.PK));
			AssertEquals(receive, inventory.Docket);
		}

		public void TestInDocketLineCache()
		{
			var factory = new TestBusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine = inventory.InDocketLine;

			Factory.Save();

			AssertEquals(receiveLine, inventory.InDocketLine);
			factory.ClearCacheForTesting();
			AssertNotEquals(receiveLine, factory.Load<WhsReceiveLine>(receiveLine.PK));
			AssertEquals(receiveLine, inventory.InDocketLine);
		}

		public void TestSupplierPartCache()
		{
			var factory = new TestBusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var part = inventory.SupplierPart;

			Factory.Save();

			AssertEquals(part, inventory.SupplierPart);
			factory.ClearCacheForTesting();
			AssertNotEquals(part, factory.Load<OrgSupplierPart>(part.PK));
			AssertEquals(part, inventory.SupplierPart);
		}

		public void TestTrackingClientUQ()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.InDocketLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("TST", inventory.WI_ClientUQ);
			AssertEquals("TST", inventory.TrackingClientUQ);
		}

		public void TestTrackingTotalUnits()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_TotalUnits = 100m;
			AssertEquals(100m, inventory.TrackingTotalUnits);
		}

		public void TestTrackingUnitsUQ()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("TST", inventory.WI_UnitsUQ);
			AssertEquals("TST", inventory.TrackingUnitsUQ);
		}

		public void TestTrackingSerialNumber()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_SerialNumber = "Serial#";
			AssertEquals("Serial#", inventory.TrackingSerialNumber);
		}

		public void TestTrackingPartAttrib1()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_PartAttrib1 = "Attrib1";
			AssertEquals("Attrib1", inventory.TrackingPartAttrib1);
		}

		public void TestTrackingPartAttrib2()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_PartAttrib2 = "Attrib2";
			AssertEquals("Attrib2", inventory.TrackingPartAttrib2);
		}

		public void TestTrackingPartAttrib3()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_PartAttrib3 = "Attrib3";
			AssertEquals("Attrib3", inventory.TrackingPartAttrib3);
		}

		public void TestTrackingPalletID()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_PalletID = "Pallet123";
			AssertEquals("Pallet123", inventory.TrackingPalletID);
		}

		public void TestTrackingCurrency()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_RX_NKLastWeightedCostCurr = "BTC";
			AssertEquals("BTC", inventory.TrackingCurrency);
		}

		public void TestTrackingHasEDocsAttached()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.InDocketLine.Notes.AddNew();
			AssertEquals(true, inventory.HasEDocsOrNotesAttached);
			AssertEquals(true, inventory.TrackingHasEDocsAttached);
		}

		public void TestTrackingTotalValue()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_LastCost = 500m;
			inventory.WI_TotalUnits = 5m;
			AssertEquals(2500m, inventory.TrackingTotalValue);
		}

		public void TestTrackingCrossDockQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			var inventory = (TrackingWhsInventory)receive.Inventory[0];

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 6m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals(10m, inventory.TrackingCrossDockQuantity);
		}

		public void TestTrackingCommittedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = (TrackingWhsInventory)receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals(10m, inventory.TrackingCommittedQuantity);
		}

		public void TestTrackingCurrentLocation()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			var location = Factory.NewWithValidTestData<WhsLocation>();
			location.Row.WR_Name = "ROW";
			location.Row.WR_Columns = 1;
			location.Row.WR_Levels = 2;
			inventory.InDocketLine.WE_WL = location.PK;
			AssertEquals("ROW-1-1", inventory.TrackingCurrentLocation);
		}

		public void TestTrackingHeldCode()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_HeldCode = "TST";
			AssertEquals("TST", inventory.TrackingHeldCode);
		}

		public void TestTrackingInventoryStatus()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_InventoryStatus = "TST";
			AssertEquals("TST", inventory.TrackingInventoryStatus);
		}

		public void TestTrackingArrivalDateOrETA()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			var expectedDate = new ZDateTimeOffset(2022, 09, 05);
			inventory.WI_ArrivalDate = expectedDate;
			AssertEquals(expectedDate, inventory.TrackingArrivalDateOrETA);
		}

		public void TestTrackingPackingDate()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			var expectedDate = new ZDate(2022, 09, 05);
			inventory.WI_PackingDate = expectedDate;
			AssertEquals(expectedDate, inventory.TrackingPackingDate);
		}

		public void TestTrackingExpiryDate()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			var expectedDate = new ZDate(2022, 09, 05);
			inventory.WI_ExpiryDate = expectedDate;
			AssertEquals(expectedDate, inventory.TrackingExpiryDate);
		}

		public void TestTrackingWarehouseCode()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.Warehouse.WW_WarehouseCode = "TST";
			AssertEquals("TST", inventory.TrackingWarehouseCode);
		}

		public void TestTrackingWarehouseName()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.Warehouse.WW_WarehouseName = "TEST WHS";
			AssertEquals("TEST WHS", inventory.TrackingWarehouseName);
		}

		public void TestTrackingProductCode()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_PartNum = "TEST123";
			AssertEquals("TEST123", inventory.TrackingProductCode);
		}

		public void TestTrackingProductDescription()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_Desc = "TEST PART";
			AssertEquals("TEST PART", inventory.TrackingProductDescription);
		}

		public void TestTrackingClientQuantity()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.WI_TotalUnits = 10m;
			AssertEquals(10m, inventory.TrackingClientQuantity);
		}

		[TestDate(2005, 1, 20)]
		public void TestTrackingAvailableToPickQuantity()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory);
			Factory.Save();

			var inventoryPK = data.FindInventory("E11AA1-5").First().PK;
			var inventory = Factory.Load<TrackingWhsInventory>(inventoryPK);
			AssertEquals(150m, inventory.TrackingAvailableToPickQuantity);
		}

		public void TestTrackingTotalWeight()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_Weight = 5m;
			inventory.WI_TotalUnits = 10m;
			AssertEquals(50m, inventory.TrackingTotalWeight);
		}

		public void TestTrackingTotalVolume()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.SupplierPart.OP_Cubic = 5m;
			inventory.WI_TotalUnits = 10m;
			AssertEquals(50m, inventory.TrackingTotalVolume);
		}

		public void TestTrackingReceiptReference()
		{
			var inventory = Factory.NewWithValidTestData<TrackingWhsInventory>();
			inventory.InDocketLine.Docket.WD_ExternalReference = "TEST DOCKET";
			AssertEquals("TEST DOCKET", inventory.TrackingReceiptReference);
		}

		public void TestPropertiesCache()
		{
			var factory = new TestBusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = (TrackingWhsInventory)Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			factory.Save();

			var trackingCurrency = inventory.TrackingCurrency;
			var trackingHasEDocsAttached = inventory.TrackingHasEDocsAttached;
			var trackingTotalValue = inventory.TrackingTotalValue;
			var trackingCrossDockQuantity = inventory.TrackingCrossDockQuantity;
			var trackingCommittedQuantity = inventory.TrackingCommittedQuantity;
			var trackingCurrentLocation = inventory.TrackingCurrentLocation;
			var trackingHeldCode = inventory.TrackingHeldCode;
			var trackingInventoryStatus = inventory.TrackingInventoryStatus;
			var trackingArrivalDateOrETA = inventory.TrackingArrivalDateOrETA;
			var trackingWarehouseCode = inventory.TrackingWarehouseCode;
			var trackingWarehouseName = inventory.TrackingWarehouseName;
			var trackingProductCode = inventory.TrackingProductCode;
			var trackingProductDescription = inventory.TrackingProductDescription;
			var trackingClientQuantity = inventory.TrackingClientQuantity;
			var trackingTotalWeight = inventory.TrackingTotalWeight;
			var trackingTotalVolume = inventory.TrackingTotalVolume;
			var trackingReceiptReference = inventory.TrackingReceiptReference;
			var trackingArrivalDate = inventory.TrackingArrivalDate;
			var trackingPalletID = inventory.TrackingPalletID;
			var trackingPartAttrib1 = inventory.TrackingPartAttrib1;
			var trackingPartAttrib2 = inventory.TrackingPartAttrib2;
			var trackingPartAttrib3 = inventory.TrackingPartAttrib3;
			var trackingSerialNumber = inventory.TrackingSerialNumber;
			var trackingPackingDate = inventory.TrackingPackingDate;
			var trackingExpiryDate = inventory.TrackingExpiryDate;
			var trackingAvailableToPickQuantity = inventory.TrackingAvailableToPickQuantity;
			var trackingUnitsUQ = inventory.TrackingUnitsUQ;
			var trackingTotalUnits = inventory.TrackingTotalUnits;
			var trackingClientUQ = inventory.TrackingClientUQ;
			factory.ClearCacheForTesting();

			AssertEquals(trackingCurrency, inventory.TrackingCurrency);
			AssertEquals(trackingHasEDocsAttached, inventory.TrackingHasEDocsAttached);
			AssertEquals(trackingTotalValue, inventory.TrackingTotalValue);
			AssertEquals(trackingCrossDockQuantity, inventory.TrackingCrossDockQuantity);
			AssertEquals(trackingCommittedQuantity, inventory.TrackingCommittedQuantity);
			AssertEquals(trackingCurrentLocation, inventory.TrackingCurrentLocation);
			AssertEquals(trackingHeldCode, inventory.TrackingHeldCode);
			AssertEquals(trackingInventoryStatus, inventory.TrackingInventoryStatus);
			AssertEquals(trackingArrivalDateOrETA, inventory.TrackingArrivalDateOrETA);
			AssertEquals(trackingWarehouseCode, inventory.TrackingWarehouseCode);
			AssertEquals(trackingWarehouseName, inventory.TrackingWarehouseName);
			AssertEquals(trackingProductCode, inventory.TrackingProductCode);
			AssertEquals(trackingProductDescription, inventory.TrackingProductDescription);
			AssertEquals(trackingClientQuantity, inventory.TrackingClientQuantity);
			AssertEquals(trackingTotalWeight, inventory.TrackingTotalWeight);
			AssertEquals(trackingTotalVolume, inventory.TrackingTotalVolume);
			AssertEquals(trackingReceiptReference, inventory.TrackingReceiptReference);
			AssertEquals(trackingArrivalDate, inventory.TrackingArrivalDate);
			AssertEquals(trackingPalletID, inventory.TrackingPalletID);
			AssertEquals(trackingPartAttrib1, inventory.TrackingPartAttrib1);
			AssertEquals(trackingPartAttrib2, inventory.TrackingPartAttrib2);
			AssertEquals(trackingPartAttrib3, inventory.TrackingPartAttrib3);
			AssertEquals(trackingSerialNumber, inventory.TrackingSerialNumber);
			AssertEquals(trackingPackingDate, inventory.TrackingPackingDate);
			AssertEquals(trackingExpiryDate, inventory.TrackingExpiryDate);
			AssertEquals(trackingAvailableToPickQuantity, inventory.TrackingAvailableToPickQuantity);
			AssertEquals(trackingUnitsUQ, inventory.TrackingUnitsUQ);
			AssertEquals(trackingTotalUnits, inventory.TrackingTotalUnits);
			AssertEquals(trackingClientUQ, inventory.TrackingClientUQ);
			AssertEquals(0, factory.CachedLoadedObjectsCount);
		}

		class TestBusinessObjectFactory : BusinessObjectFactory
		{
			public void ClearCacheForTesting()
			{
				last10BizObjsLoaded.Clear();
			}

			public int CachedLoadedObjectsCount => last10BizObjsLoaded.Count;
		}

		#endregion

		#region Documents test

		[TestedType(typeof(TrackingWhsInventory))]
		public class TrackingWhsInventoryIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
		{
			public void TestDocumentUploadHelper()
			{
				var inventory = (TrackingWhsInventory)GetNewBusinessObject();
				var uploadHelper = inventory.DocumentUploadHelper;

				AssertEquals(uploadHelper, inventory.DocumentUploadHelper);
				AssertEquals(inventory.Factory, uploadHelper.Factory);
			}

			public void TestResetDocumentHelper()
			{
				var inventory = (TrackingWhsInventory)GetNewBusinessObject();
				var docHelper = inventory.DocumentHelper;

				AssertNotNull("Precondition", inventory.DocumentHelper);
				AssertEquals(docHelper, inventory.DocumentHelper);

				inventory.ResetDocumentHelper();

				AssertNotNull(inventory.DocumentHelper);
				AssertNotEquals(docHelper, inventory.DocumentHelper);
			}

			protected override IWebDocumentsSupport GetNewBusinessObject()
			{
				return Factory.New<TrackingWhsInventory>();
			}

			protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
			{
				get { return Array.Empty<ZGuid>(); }
			}
		}

		#endregion

		#region WebUser Visible Notes Test

		[SetGlobalsIsWeb]
		public class TrackingWhsInventoryVisibleNotesTest : IWebUserVisibleNotesSupportTest
		{
			protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
			{
				return new TestHelper(Factory).CreateWhsInventory();
			}

			protected override void SetAgentNotesVisibility(IWebUserVisibleNotesSupport notesSupport, bool visibility)
			{ }

			protected override BusinessObject GetRelatedBusinessObject(IWebUserVisibleNotesSupport parent)
			{
				return null;
			}
		}

		#endregion

		#region Implementation

		protected override WhsReceive GetNewReceive()
		{
			return Factory.New<WhsReceive>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new TestHelper(Factory);

			return helper.CreateEmptyWhsInventory();
		}

		#endregion
	}
}
