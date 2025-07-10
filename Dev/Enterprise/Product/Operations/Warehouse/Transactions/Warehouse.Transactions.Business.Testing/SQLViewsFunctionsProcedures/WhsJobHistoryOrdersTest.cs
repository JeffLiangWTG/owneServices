using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsJobHistoryOrdersTest : WhsTestCaseWithFactory
	{
		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea", categorySoftDrinks);
			var productCoke = Helper.CreateProduct(client, "Coke", categorySoftDrinks);
			var productVB = Helper.CreateProduct(client, "VB", categoryBeers);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m, true, true);

			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", productVB, 10m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", productCoke, 20m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O3", productTea, 5m);

			Factory.Save();

			AssertEquals("ResultSet Count", 1, LoadView_ProductCategory(categoryBeers.PK).Count);
			AssertEquals("ResultSet Count", 2, LoadView_ProductCategory(categorySoftDrinks.PK).Count);
			AssertEquals("ResultSet Count", 3, LoadView_ProductCategory(categoryBeverages.PK).Count);
			AssertEquals("ResultSet Count", 3, LoadView_ProductCategory(ZGuid.Empty).Count);
		}

		public void TestView_WithProductCategoryFilter_WorkOrders()
		{
			var vehicles = Helper.CreateProductCategory("VEHICLE", "All Vehicles");
			var warehouse = Helper.CreateWarehouse("W1", "A", "2");
			var client = Helper.CreateClient("C1");

			var bikeFrame = Helper.CreateProduct(client, "FRAME", vehicles);
			var bikeWheel = Helper.CreateProduct(client, "WHEEL", vehicles);
			var bomBike = Helper.CreateProduct(client, "BIKE", vehicles);

			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveLine(receive, bikeFrame, 10m);
			Helper.CreateWhsReceiveLine(receive, bikeWheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			var workOrder = Helper.CreateWhsWorkOrder(client, warehouse);
			Helper.CreateWhsWorkOrderLine(workOrder, bomBike, 10m);
			Helper.CreatePickByAttachingOrders(workOrder);
			Factory.Save();

			AssertEquals("ResultSet Count", 0, LoadView_ProductCategory(vehicles.PK).Count);
		}

		public void TestView_WithProductCategoryFilter_CancelledOrders()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1", categoryBeverages);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m, true, true);

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 10m);
			Factory.Save();

			AssertEquals("ResultSet Count", 1, LoadView_ProductCategory(categoryBeverages.PK).Count);

			order.CancelReactivateDocket();
			Factory.Save();

			AssertEquals("ResultSet Count", 0, LoadView_ProductCategory(categoryBeverages.PK).Count);
		}

		DynamicBusinessObjectCollection LoadView_ProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = categoryPK.IsEmpty
				? @"select * from WhsJobHistoryOrdersReport(null)"
				: @"select * from WhsJobHistoryOrdersReport(@ProductCategoryPK)";
			var sqlParams = new ZSqlParameterCollection();
			if (!categoryPK.IsEmpty)
			{
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView

		public void TestView()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("1", "1");
			var part = Helper.CreateProduct(client, "3");
			var orders = SetupData(whs, client, part, commodity);
			var result = LoadView(whs, client, part, commodity);

			AssertEquals(2, result.Count);

			orders[0].Lines.ApplySort("WE_TransactionQuantity", System.ComponentModel.ListSortDirection.Ascending);
			orders[1].Lines.ApplySort("WE_TransactionQuantity", System.ComponentModel.ListSortDirection.Ascending);

			AssertResult(orders[0].Lines[1], result[0], commodity);
			AssertResult(orders[1].Lines[1], result[1], commodity);
		}

		public void TestView_Units_FinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var inventoryLocation1 = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, inventoryLocation1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, inventoryLocation1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);

			var orderRecord1 = results[0];
			AssertEquals("Order1 Units Allocated should be equal 20m.", 20m, orderRecord1["UnitsAllocated"]);
			AssertEquals("Order1 Units Sent should be equal 20m.", 20m, orderRecord1["QuantityActual"]);
			AssertEquals("Order1 Units Picked should be equal 20m.", 20m, orderRecord1["UnitsPicked"]);

			var orderRecord2 = results[1];
			AssertEquals("Order1 Units Allocated should be equal 20m.", 20m, orderRecord2["UnitsAllocated"]);
			AssertEquals("Order1 Units Sent should be equal 20m.", 20m, orderRecord2["QuantityActual"]);
			AssertEquals("Order1 Units Picked should be equal 20m.", 20m, orderRecord2["UnitsPicked"]);
		}

		public void TestView_Units_UnfinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);

			var orderRecord1 = results[0];
			AssertEquals("Order1 Units Allocated should be equal 20m.", 20m, orderRecord1["UnitsAllocated"]);
			AssertEquals("Order2 Units Sent should be 0.", ZDecimal.Zero, orderRecord1["QuantityActual"]);
			AssertEquals("Order2 Units Picked should be 0.", ZDecimal.Zero, orderRecord1["UnitsPicked"]);

			var orderRecord2 = results[1];
			AssertEquals("Order1 Units Allocated should be equal 20m.", 20m, orderRecord2["UnitsAllocated"]);
			AssertEquals("Order2 Units Sent should be 0.", ZDecimal.Zero, orderRecord2["QuantityActual"]);
			AssertEquals("Order2 Units Picked should be 0.", ZDecimal.Zero, orderRecord2["UnitsPicked"]);
		}

		public void TestView_DistributionCentre()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("1", "1");
			var distributionCentre = Helper.CreateClient("C3");

			var part = Helper.CreateProduct(client, "3");
			var orders = SetupData(whs, client, part, commodity);

			orders[0].DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			Factory.Save();

			var result = LoadView(whs, client, part, commodity);
			var ordersWithDcAddress = result.Where(o => (CargoWise.Types.ZGuid)o["DistributionCentrePK"] == distributionCentre.PK).ToArray();

			AssertEquals("Should filter by distribution centre.", 1, ordersWithDcAddress.Length);

			var businessObject = ordersWithDcAddress[0];
			AssertEquals(orders[0].PK, businessObject["DocketPK"]);
			AssertEquals(distributionCentre.PK, businessObject["DistributionCentrePK"]);
			AssertEquals(distributionCentre.OH_FullName, businessObject["DistributionCentreCoName"]);
		}

		public void TestView_Units_PartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var inventoryLocation1 = data.Whs1.FindLocation("A-1");
			var inventoryLocation2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");

			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, inventoryLocation1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, inventoryLocation2);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var testStaff1 = Helper.CreateGlbStaff("HIL", "Hilary Clinton");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var pick1 = Helper.CreatePickNew(order1);
			var order1Line = order1.Lines[0];
			var pick1Line = order1Line.PickLines[0];
			pick1Line.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick1Line.WZ_GS_NKAssignedTo = testStaff1.GS_Code;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}
			var results = LoadView(data.Whs1, data.Org1);
			var orderRecord1 = results.First(o => (ZString)o["Reference"] == "O1");
			AssertEquals("Order1 Units Allocated should be equal 10m.", 10m, orderRecord1["UnitsAllocated"]);
			AssertEquals("Order1 Units Sent should be equal 0.", ZDecimal.Zero, orderRecord1["QuantityActual"]);
			AssertEquals("Order1 Units Picked should be equal 5m.", 5m, orderRecord1["UnitsPicked"]);
		}

		public void TestView_Units_PartiallyPickedInTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var inventoryLocation1 = data.Whs1.FindLocation("A-1");
			var inventoryLocation2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");

			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, inventoryLocation1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, inventoryLocation2);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var testStaff1 = Helper.CreateGlbStaff("HIL", "Hilary Clinton");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var pick1 = Helper.CreatePickNew(order1);
			var order1Line = order1.Lines[0];
			var pick1Line = order1Line.PickLines[0];
			pick1Line.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick1Line.WZ_GS_NKAssignedTo = testStaff1.GS_Code;

			Factory.Save();

			AssertEquals("PickedDateTime is empty while in transit.", ZDateTimeOffset.Empty, pick1Line.WZ_PickedDateTime);
			AssertNotEquals("WZ_WE_OriginalPickedInventoryLine is not empty while in transit.", Guid.Empty, pick1Line.WZ_WE_OriginalPickedInventoryLine);

			var results = LoadView(data.Whs1, data.Org1);
			var orderRecord1 = results.First(o => (ZString)o["Reference"] == "O1");
			AssertEquals("Order1 Units Allocated should be equal 10m.", 10m, orderRecord1["UnitsAllocated"]);
			AssertEquals("Order1 Units Sent should be equal 0.", ZDecimal.Zero, orderRecord1["QuantityActual"]);
			AssertEquals("Order1 Units Picked should be equal 5m.", 5m, orderRecord1["UnitsPicked"]);
		}

		[TestDate(2024, 08, 28, 12, 55, 0)]
		public void TestLoadView_Loaded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order1.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);
			AssertEquals("Should return 2 records.", 2, results.Count);
			AssertResult(order1.Lines[0], results[0], null);
			AssertResult(order2.Lines[0], results[1], null);
		}

		public void TestView_ProductCategory()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("2", "2");
			var part = Helper.CreateProduct(client, "2");
			var category = Helper.CreateProductCategory(client, part, "Cat1");
			category.OPC_CategoryDescription = "Category 1";
			var order = SetupDataForProductCategory(whs, client, part);
			var result = LoadView_ProductCategory(whs, client, category);
			AssertEquals(1, result.Count);
			AssertResult_ProductCategory(order.Lines[0], result[0]);
		}

		public void TestView_SortByDateTimeOffsetField()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 2m, true, true);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 1m);
			order1.WD_RequiredDate = new ZDateTimeOffset(2024, 10, 23, 9, 1, 0, TimeSpan.FromHours(8));
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", product, 1m);
			order2.WD_RequiredDate = new ZDateTimeOffset(2024, 10, 23, 9, 2, 0, TimeSpan.FromHours(11));
			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = "select * from WhsJobHistoryOrdersReport(null) order by DateTimeOffsetField asc";
			result.Load(sql);
			AssertEquals(2, result.Count);
			AssertEquals(order2.WD_RequiredDate.ToString(), result[0]["DateTimeOffsetField"].ToString());
			AssertEquals(order1.WD_RequiredDate.ToString(), result[1]["DateTimeOffsetField"].ToString());
		}

		void AssertResult_ProductCategory(WhsOrderLine orderLine, DynamicBusinessObject dynamicObject)
		{
			var partRelation =
				orderLine.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(orderLine.Order.Client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation != null ? partRelation.Category : null;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			AssertEquals("ProductCategoryCode", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
				dynamicObject["ProductCategoryDescription"]);
			AssertEquals("SalesChannel", orderLine.Order.SalesChannel?.WSH_Code, dynamicObject["SalesChannel"]);
		}

		void AssertResult(WhsOrderLine orderLine, DynamicBusinessObject businessObject, RefCommodityCode[] commodity)
		{
			var order = orderLine.Order;
			AssertEquals("WarehousePK", order.Warehouse.PK, businessObject["WarehousePK"]);
			AssertEquals("WarehouseName", order.Warehouse.WW_WarehouseName, businessObject["WarehouseName"]);
			AssertEquals("ClientPK", order.Client.PK, businessObject["ClientPK"]);
			AssertEquals("ClientCode", order.Client.OH_Code, businessObject["ClientCode"]);
			AssertEquals("ClientFullName", order.Client.OH_FullName, businessObject["ClientFullName"]);

			if (order.IsFinalised)
			{
				AssertEquals("IsFinalised", "Y", businessObject["IsFinalised"]);
				AssertEquals("DateTimeOffsetField", order.WD_FinalisedDate, ((ZDateTimeOffset)businessObject["DateTimeOffsetField"]));
			}
			else
			{
				AssertEquals("IsFinalised", "N", businessObject["IsFinalised"]);
				AssertEquals("DateTimeOffsetField", order.WD_RequiredDate, ((ZDateTimeOffset)businessObject["DateTimeOffsetField"]));
			}
			AssertEquals("DocketPK", order.PK, businessObject["DocketPK"]);
			AssertEquals("Reference", order.WD_ExternalReference, businessObject["Reference"]);
			AssertEquals("CustomerReference", order.WD_CustomerReference, businessObject["CustomerReference"]);
			AssertEquals("FinalisedDate", order.WD_FinalisedDate.Date,
				((ZDateTime)businessObject["FinalisedDate"]).Date);

			AssertEquals("OrderedDate", order.WD_BookingDate.Date, ((ZDateTimeOffset)businessObject["OrderedDate"]).Date);
			AssertEquals("RequiredDate", order.WD_RequiredDate.Date, ((ZDateTime)businessObject["RequiredDate"]).Date);
			AssertEquals("DocketStatus", order.WarehouseOrderStatus, businessObject["DocketStatus"]);
			AssertEquals("DocketSubType", order.WD_DocketSubType, businessObject["DocketSubType"]);
			AssertEquals("UnitsSent", order.WD_UnitsSent, businessObject["UnitsSent"]);
			AssertEquals("PackagesSent", order.WD_PackagesSent, businessObject["PackagesSent"]);
			AssertEquals("PalletsSent", order.WD_PalletsSent, businessObject["PalletsSent"]);
			AssertEquals("TotalWeight", order.WD_WeightSent, businessObject["TotalWeight"]);
			AssertEquals("TotalWeightUQ", order.WD_TotalWeightUnit, businessObject["TotalWeightUQ"]);
			AssertEquals("TotalVolume", order.WD_CubicSent, businessObject["TotalVolume"]);
			AssertEquals("TotalVolumeUQ", order.WD_TotalCubicUnit, businessObject["TotalVolumeUQ"]);
			AssertEquals("TransportReference", order.WD_TransportReference, businessObject["TransportReference"]);
			AssertEquals("Service Level", order.WD_RS_NKServiceLevel, businessObject["ServiceLevel"]);
			AssertEquals("SalesChannel", orderLine.Order.SalesChannel?.WSH_Code ?? string.Empty, businessObject["SalesChannel"]);

			if (order.TransportCoDocAddress.E2_AddressOverride)
			{
				AssertEquals("TransportCoName", order.TransportCoDocAddress.E2_CompanyName,
					businessObject["TransportCoName"]);
			}
			else if (order.TransportCoDocAddress.Organisation != null)
			{
				AssertEquals("TransportCoName", order.TransportCoDocAddress.Organisation.OH_FullName,
					businessObject["TransportCoName"]);
			}

			if (order.ConsigneeDocAddress.E2_AddressOverride)
			{
				AssertEquals("ConsigneePK", ZGuid.Empty, businessObject["ConsigneePK"]);
				AssertEquals("ConsigneeName", order.ConsigneeDocAddress.E2_CompanyName,
					businessObject["ConsigneeName"]);
			}
			else
			{
				AssertEquals("ConsigneePK", order.Consignee.PK, businessObject["ConsigneePK"]);
				AssertEquals("ConsigneeName", order.Consignee.OH_FullName, businessObject["ConsigneeName"]);
			}

			AssertEquals("ProductPK", orderLine.SupplierPart.PK, businessObject["ProductPK"]);
			AssertEquals("CODAmount", order.WD_ShipperCODAmount, businessObject["CODAmount"]);
			AssertEquals("InsuranceAmount", order.WD_LocalCartInsuranceCost, businessObject["InsuranceAmount"]);

			if (commodity != null)
			{
				AssertEquals("CommodityCode", commodity[0].RH_Code, businessObject["CommodityCode"]);
				AssertEquals("CommodityPK", commodity[0].PK, businessObject["CommodityPK"]);
			}

			AssertEquals("PartAttrib1", orderLine.ReleaseLines[0].PartAttribute1, businessObject["PartAttrib1"]);
			AssertEquals("PartAttrib2", orderLine.ReleaseLines[0].PartAttribute2, businessObject["PartAttrib2"]);
			AssertEquals("PartAttrib3", orderLine.ReleaseLines[0].PartAttribute3, businessObject["PartAttrib3"]);
			AssertEquals("SerialNumber", orderLine.ReleaseLines[0].SerialNumber, businessObject["SerialNumber"]);
			AssertEquals("PackingDate", orderLine.ReleaseLines[0].PackingDate, businessObject["PackingDate"]);
			AssertEquals("ExpiryDate", orderLine.ReleaseLines[0].ExpiryDate, businessObject["ExpiryDate"]);
		}

		DynamicBusinessObjectCollection LoadView_ProductCategory(WhsWarehouse whs, OrgHeader client,
			OrgPartCategory category)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"select	*
						   from		WhsJobHistoryOrdersReport(@ProductCategoryPK)
						   where	WarehousePK = @WarehousePK
							and		ClientPK = @ClientPK
							and		ProductCategoryPK = @ProductCategoryPK
						   order by Reference, QuantityActual asc";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client },
				{ "@ProductCategoryPK", category.PK, OrgPartCategorySchema.PK }
			};

			result.Load(sql, sqlParams);
			return result;
		}

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"select	*
						   from		WhsJobHistoryOrdersReport(null)
						   where	WarehousePK = @WarehousePK
							and		ClientPK = @ClientPK
						   order by Reference, QuantityActual asc";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client }
			};

			result.Load(sql, sqlParams);
			return result;
		}

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part,
			RefCommodityCode[] commodity)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"select	*
						   from		WhsJobHistoryOrdersReport(null)
						   where	WarehousePK = @WarehousePK
							and		ClientPK = @ClientPK
							and		ProductPK = @ProductPK
							and		CommodityCode = @CommodityCode
						   order by Reference, SerialNumber";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client },
				{ "@ProductPK", part.PK, WhsDocketLineSchema.WE_OP },
				{ "@CommodityCode", commodity[0].RH_Code, OrgSupplierPartSchema.OP_RH_NKCommodityCode }
			};

			result.Load(sql, sqlParams);
			return result;
		}

		WhsOrder[] SetupData(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part, RefCommodityCode[] commodity)
		{
			var year = ZDateTime.Now.Year;
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			var client2 = Helper.CreateClient("2", "2");

			var consignee1 = Helper.CreateClient("3", "3");
			var consignee2 = Helper.CreateClient("4", "4");

			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo1.OH_FullName = "TransportCo1";
			transportCo1.Addresses.AddNewMainAddress();
			transportCo1.OH_IsShippingProvider = true;

			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo2.OH_FullName = "TransportCo2";
			transportCo2.Addresses.AddNewMainAddress();
			transportCo2.OH_IsShippingProvider = true;

			var part1 = Helper.CreateProduct(client, "1", OrgPartRelation.RelationshipTypes.Both);
			var part2 = Helper.CreateProduct(client2, "2");
			var category1 = Helper.CreateProductCategory(client2, part2, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";

			part.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part, "UNT", "PLT", 10);

			SetClientAttributesType(client);
			SetClientAttributesType(client2);
			SetProductAttributesUse(client, part1);
			SetProductAttributesUse(client2, part2);
			SetProductAttributesUse(client, part);

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part.OP_RH_NKCommodityCode = commodity[0].RH_Code;

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part.PK;
			relation.OU_OH = client2.PK;

			var salesChannel1 = Helper.CreateWhsSalesChannel("SC1", "Sales Channel 1");
			var salesChannel2 = Helper.CreateWhsSalesChannel("SC2", "Sales Channel 2");

			SetupReceive(client, whs, "11", part1, 111, part, 112);
			var order11 = SetupOrders(client, whs, "11", consignee1, new ZDateTimeOffset(year, 1, 1), "TR11", "CR11", "ER11",
				transportCo1, part1, 111, part, 112, 100.0, 25.0, true, salesChannel1.PK);

			SetupReceive(client, whs, "12", part1, 121, part, 122);
			var order12 = SetupOrders(client, whs, "12", consignee2, new ZDateTimeOffset(year, 1, 1), "TR12", "CR12", "ER12",
				transportCo2, part1, 121, part, 122, 200.0, 50.0, false, salesChannel2.PK);

			SetupReceive(client2, whs, "21", part2, 211, part2, 212);
			var order21 = SetupOrders(client2, whs, "21", consignee1, new ZDateTimeOffset(year, 1, 1), "TR21", "CR21", "ER21",
				transportCo1, part2, 211, part, 212, 0, 0, true, ZGuid.Empty);

			SetupReceive(client2, whs2, "22", part2, 221, part2, 222);
			var order22 = SetupOrders(client2, whs2, "22", consignee2, new ZDateTimeOffset(year, 1, 1), "TR22", "CR22",
				"ER22", transportCo2, part2, 221, part, 222, 0, 0, true, ZGuid.Empty);

			order11.ConsigneeDocAddress.E2_AddressOverride = ZBool.True;
			order11.ConsigneeNameOrPK = "Overriden Consignee Company name";
			order11.WD_RS_NKServiceLevel = "TST";
			order12.TransportCoDocAddress.E2_AddressOverride = ZBool.True;
			order12.TransportCoNameOrPK = "Overriden TransportCo Company name";
			Factory.Save();

			SetupCancelledOrder(client, whs, "41", consignee1, part, 100);

			return new[] { order11, order12, order21, order22 };
		}

		WhsOrder SetupDataForProductCategory(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part)
		{
			var year = ZDateTime.Now.Year;
			var client1 = Helper.CreateClient("1", "1");

			var consignee1 = Helper.CreateClient("3", "3");
			var consignee2 = Helper.CreateClient("4", "4");

			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo1.OH_FullName = "TransportCo1";
			transportCo1.Addresses.AddNewMainAddress();
			transportCo1.OH_IsShippingProvider = true;

			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCo1.OH_FullName = "TransportCo2";
			transportCo1.Addresses.AddNewMainAddress();
			transportCo1.OH_IsShippingProvider = true;

			var part3 = Helper.CreateProduct(client1, "3");
			part3.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part3, "UNT", "PLT", 10);

			SetClientAttributesType(client1);
			SetClientAttributesType(client);
			SetProductAttributesUse(client, part);
			SetProductAttributesUse(client1, part3);

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			part.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part3.OP_RH_NKCommodityCode = commodity[0].RH_Code;

			var relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part3.PK;
			relation.OU_OH = client.PK;

			var salesChanne = Helper.CreateWhsSalesChannel("SC1", "Sales Channel 1");

			SetupReceive(client, whs, "21", part, 211, part, 212);
			Factory.Save();

			var order = SetupOrders(client, whs, "21", consignee1, new ZDateTimeOffset(year, 1, 1), "TR21", "CR21", "ER21",
				transportCo1, part, 211, part3, 212, 0, 0, true, salesChanne.PK);
			Factory.Save();

			SetupCancelledOrder(client1, whs, "41", consignee1, part3, 100);
			return order;
		}

		void SetClientAttributesType(OrgHeader client)
		{
			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
		}

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, true);
		}

		void SetupCancelledOrder(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader consignee,
			OrgSupplierPart part, ZDecimal units)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			Helper.CreateWhsOrderLine(order, part, units);

			Factory.Save();
			order.CancelReactivateDocket();
			Factory.Save();
			AssertEquals(true, order.IsCancelled);
		}

		void SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart warehousePart1,
			ZDecimal units1, OrgSupplierPart warehousePart2, ZDecimal units2)
		{
			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, warehousePart1, units1, ZDate.Today.AddDays(+10), ZDate.Today,
				"Attrib11", "Attrib12", "Attrib13", "BEK1");
			Helper.CreateWhsReceiveInventoryLine(receive, warehousePart2, units2, ZDate.Today.AddDays(+5),
				ZDate.Today.AddDays(-2), "Attrib21", "Attrib22", "Attrib23", "BEK2");
			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPuttingAway", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
			Factory.Save();
		}

		WhsOrder SetupOrders(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader consignee,
			ZDateTimeOffset date, ZString transportReference, ZString customerReference,
			ZString externalReference, OrgHeader transportCo, OrgSupplierPart warehousePart1, ZDecimal units1,
			OrgSupplierPart warehousePart2, ZDecimal units2, ZDecimal cODAmount,
			ZDecimal insuranceAmount, ZBool finalize, ZGuid salesChannel)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.WD_RequiredDate = date;
			order.WD_TransportReference = transportReference;
			order.WD_CustomerReference = customerReference;
			order.WD_ExternalReference = externalReference;
			order.TransportCoPK = transportCo.PK;
			order.WD_LocalCartInsuranceCost = insuranceAmount;
			order.WD_ShipperCODAmount = cODAmount;
			order.WD_WSH_SalesChannel = salesChannel;

			var orderLine1 = Helper.CreateWhsOrderLine(order, warehousePart1, units1);
			orderLine1.WE_F3_NKPackType = "PLT";
			Helper.CreateWhsOrderLine(order, warehousePart2, units2);

			var pick = Helper.CreatePickNew(order);
			if (finalize)
			{
				pick.FinaliseAllOrders();
				AssertEquals("Order IsFinalised", true, order.IsFinalised);
				pick.FinalisePick();
				AssertEquals("Pick IsFinalised", true, pick.IsFinalised);
			}

			Factory.Save();
			return order;
		}

		#endregion

		#region TestView_SerialNumber

		[TestDate(2024, 08, 28, 12, 55, 0)]
		public void TestView_SerialNumber()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			data.Part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			data.Part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			CreateReceiveLine("SN1");
			CreateReceiveLine("SN2");
			CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN2";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "SN3";
			Helper.CreatePickNew(order);
			Factory.Save();

			var result = LoadView(data.Whs1, data.Org1, data.Part1, commodity);
			AssertEquals("Should be 3 results", 3, result.Count);

			AssertResult(orderLine1, result[0], commodity);
			AssertResult(orderLine2, result[1], commodity);
			AssertResult(orderLine3, result[2], commodity);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion
	}
}
