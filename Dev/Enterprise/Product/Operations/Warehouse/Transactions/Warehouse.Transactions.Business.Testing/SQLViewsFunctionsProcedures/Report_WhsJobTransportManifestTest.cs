using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Report_WhsJobTransportManifestTest : WhsTestCaseWithFactory
	{
		#region TestView_ReleaseCapturedAttribs

		public void TestView_ReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty,
				"", "BATCH123", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.PartAttribute2 = "BATCH123";
			Factory.Save();

			var results1 = LoadView(data.Whs1, "BATCH123", WhsDocketLineSchema.WE_PartAttrib2);
			AssertEquals("Should return the Order, BATCH123 is picked.", 1, results1.Count);
			AssertEqualInformation(order, results1[0]);

			var results2 = LoadView(data.Whs1, "RED", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals("Should return the Order, RED is Release Captured.", 1, results2.Count);
			AssertEqualInformation(order, results2[0]);

			var results3 = LoadView(data.Whs1, "BLUE", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals("Should return the Order, BLUE is Release Captured.", 1, results3.Count);
			AssertEqualInformation(order, results3[0]);

			var results4 = LoadView(data.Whs1, "BATCH123", WhsDocketLineSchema.WE_PartAttrib2);
			AssertEquals("Should return the Order, BATCH123 is picked.", 1, results4.Count);
			AssertEqualInformation(order, results4[0]);

			var results5 = LoadView(data.Whs1, "RED", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals("Should return the Order, RED is Release Captured.", 1, results5.Count);
			AssertEqualInformation(order, results5[0]);

			var results6 = LoadView(data.Whs1, "BLUE", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals("Should return the Order, BLUE is Release Captured.", 1, results6.Count);
			AssertEqualInformation(order, results6[0]);
		}

		#endregion

		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship =
				productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productCoke = Helper.CreateProduct(client, "Coke");
			var cokeRelationship =
				productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var vbRelationship =
				productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m, true, true);

			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", productVB, 10m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", productCoke, 20m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O3", productTea, 5m);

			Factory.Save();

			var result1 = LoadView_ProductCategory(warehouse.PK, categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_ProductCategory(warehouse.PK, categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_ProductCategory(warehouse.PK, categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_ProductCategory(warehouse.PK, ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_ProductCategory(ZGuid warehousePK, ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = string.Empty;
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", warehousePK, WhsWarehouseSchema.PK);
			sqlParams.Add("@IncludeUnfinalised", 'Y', WhsDocketLineSchema.WE_IsValid);
			if (categoryPK.IsEmpty)
			{
				sql = string.Format(@"
				SELECT	*
				FROM	WhsJobTransportManifestReport(@WarehousePK, null, null, '', null, null, null, null, null, '', '', '', '', null, null, @IncludeUnfinalised, null, null)
				ORDER BY ClientCode, TransportCoName, TransportReference asc");
			}
			else
			{
				sql = string.Format(@"
				SELECT	*
				FROM	WhsJobTransportManifestReport(@WarehousePK, null, null, '', null, null, {0}, null, null, '', '', '', '', null, null, @IncludeUnfinalised, null, null)
				ORDER BY ClientCode, TransportCoName, TransportReference asc", "@ProductCategoryPK");
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestFunction

		public void TestFunction()
		{
			var warehouse = Helper.CreateWarehouse("1", "A", 5, 5);
			var client = Helper.CreateClient("2", "Client2");
			var part = Helper.CreateProduct(client, "P3");
			var orders = SetupData(warehouse, client, part);
			AssertResults(warehouse, "A1", WhsDocketLineSchema.WE_PartAttrib1, orders);
			AssertResults(warehouse, "PA1", WhsDocketLineSchema.WE_PartAttrib1, orders);
			AssertResults(warehouse, "PA2", WhsDocketLineSchema.WE_PartAttrib2, orders);
			AssertResults(warehouse, "PA3", WhsDocketLineSchema.WE_PartAttrib3, orders);
		}

		public void TestFunction_ProductCategoryFilter()
		{
			var warehouse = Helper.CreateWarehouse("1", "A", 5, 5);
			var client = Helper.CreateClient("2", "Client2");
			var part = Helper.CreateProduct(client, "P3");
			var category = Helper.CreateProductCategory(client, part, "Cat1");
			category.OPC_CategoryDescription = "Category 1";
			var orders = SetupData(warehouse, client, part);
			AssertResults_ProductCategoryFilter(warehouse, "A1", WhsDocketLineSchema.WE_PartAttrib1, category, orders);
			AssertResults_ProductCategoryFilter(warehouse, "PA1", WhsDocketLineSchema.WE_PartAttrib1, category, orders);
			AssertResults_ProductCategoryFilter(warehouse, "PA2", WhsDocketLineSchema.WE_PartAttrib2, category, orders);
			AssertResults_ProductCategoryFilter(warehouse, "PA3", WhsDocketLineSchema.WE_PartAttrib3, category, orders);
		}

		#endregion

		#region TestView_DepartedOrderLineWithoutIncludeUnfinalised

		public void TestView_DepartedOrderLineWithoutIncludeUnfinalised()
		{
			var warehouse = Helper.CreateWarehouse("1", "A", 5, 5);
			var client = Helper.CreateClient("1", "Client1");
			var part = Helper.CreateProduct(client, "P1");
			var consignee = Helper.CreateClient("C1", "Consignee", "C1 City", "C1 State");
			var transportCo1 = Helper.CreateClient("T1", "Transport1");
			var order = CreateOrder("O1", client, warehouse, transportCo1, consignee, "Tr. Ref. 1", ZDateTimeOffset.Today, part);

			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();

			Factory.Save();

			var viewLoadedResult = LoadView(warehouse, "A1", WhsDocketLineSchema.WE_PartAttrib1, includeUnfinalised: "N");
			AssertEquals("Should return departed order line when includeUnfinalised is false", 1, viewLoadedResult.Count);
		}

		#endregion

		#region TestWithCrossDockedOrderLines

		public void TestWithCrossDockedOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty,
				"PA1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_DocketStatus = DocketStatus.Codes.Held;
			AssertNotNull("Precondition - divot is created.", orderLine.ReserveStockIfAbleTo(inventory));

			Factory.Save();

			var results1 = LoadView(data.Whs1, "PA1", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals("Should not get Attribute information from reserved stock.", 0, results1.Count);

			var results2 = LoadView(data.Whs1, "", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals(1, results2.Count);

			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			Factory.Save();

			var results3 = LoadView(data.Whs1, "PA1", WhsDocketLineSchema.WE_PartAttrib1);
			AssertEquals("Should not get Attribute information from 0 unit PickLines.", 0, results3.Count);
		}

		#endregion

		#region TestReport_SerialNumber

		public void TestReport_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory2.WI_SerialNumber = "SN2";
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part2, 1m);

			var pick = Helper.CreatePickNew(order1, order2, order3);
			var releaseLine = ((WhsReleaseLine)orderLine3.ReleaseLines.Single()).SerialNumber = "SN3";

			pick.FinaliseAllOrders();
			AssertEquals("Order1 should be finalized", true, order1.IsFinalised);
			AssertEquals("Order2 should be finalized", true, order2.IsFinalised);
			AssertEquals("Order2 should be finalized", true, order3.IsFinalised);
			pick.FinalisePick();
			AssertEquals("Pick should be finalized", true, pick.IsFinalised);
			Factory.Save();

			var results1 = LoadView(data.Whs1, "SN1", WhsDocketLineSchema.WE_SerialNumber);
			AssertEquals("Should return the Order, SN1 is Ordered.", 1, results1.Count);
			AssertEqualInformation(order1, results1[0]);

			var results2 = LoadView(data.Whs1, "SN2", WhsDocketLineSchema.WE_SerialNumber);
			AssertEquals("Should return the Order, SN2 is inventory.", 1, results2.Count);
			AssertEqualInformation(order2, results2[0]);

			var results3 = LoadView(data.Whs1, "SN3", WhsDocketLineSchema.WE_SerialNumber);
			AssertEquals("Should return the Order, SN3 is release captured.", 1, results3.Count);
			AssertEqualInformation(order3, results3[0]);
		}

		#endregion

		#region TestReport_Loading

		public void TestReport_Loading()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory2.WI_SerialNumber = "SN2";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN2";
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order1.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 2m);
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";

			var package3 = order2.PackageJob.Packages.AddNew("CTN");
			package3.Pack(order2.Lines[0].ReleaseLines[0], 3m);
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(order2);
			Factory.Save();

			var results1 = LoadView(data.Whs1, "SN1", WhsDocketLineSchema.WE_SerialNumber);
			AssertEquals("Should return the Order with SN1.", 1, results1.Count);
			AssertEqualInformation(order1, results1[0]);

			var results2 = LoadView(data.Whs1, "SN2", WhsDocketLineSchema.WE_SerialNumber);
			AssertEquals("Should return the Order with SN2.", 1, results2.Count);
			AssertEqualInformation(order2, results2[0]);
		}

		#endregion

		#region TestReport_DistributionCentre

		public void TestReport_DistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var distributionCentre = Helper.CreateClient("C3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory.WI_SerialNumber = "SN1";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "SN1";
			order.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;

			Factory.Save();

			var results1 = LoadView(data.Whs1, "SN1", WhsDocketLineSchema.WE_SerialNumber, distributionCentrePK: distributionCentre.PK);
			AssertEquals("Should filter by distribution centre.", 1, results1.Count);
			AssertEquals(order.PK, results1[0]["DocketPK"]);
			AssertEquals(distributionCentre.PK, results1[0]["DistributionCentrePK"]);
			AssertEquals(distributionCentre.OH_FullName, results1[0]["DistributionCentreCoName"]);

			var results2 = LoadView(data.Whs1, "SN1", WhsDocketLineSchema.WE_SerialNumber, distributionCentrePK:  ZGuid.NewZGuid());
			AssertEquals("Should not match with any distribution centre.", 0, results2.Count);
		}

		#endregion

		#region AssertResults

		void AssertResults_ProductCategoryFilter(WhsWarehouse warehouse, ZString partAttrib,
			SchemaStringColumn partAttribColumn, OrgPartCategory category, WhsOrder[] orders)
		{
			var viewLoadedResult = LoadView(warehouse, partAttrib, partAttribColumn, category);
			var manuallyLoadedResult = LoadManually(warehouse, partAttrib, partAttribColumn, orders, category);
			AssertEquals("View and Manual load should return same Count", manuallyLoadedResult.Count,
				viewLoadedResult.Count);

			for (int i = 0; i < manuallyLoadedResult.Count; i++)
			{
				AssertEqualInformation(manuallyLoadedResult[i], viewLoadedResult[i]);
			}
		}

		void AssertResults(WhsWarehouse warehouse, ZString partAttrib, SchemaStringColumn partAttribColumn,
			WhsOrder[] orders)
		{
			var viewLoadedResult = LoadView(warehouse, partAttrib, partAttribColumn);
			var manuallyLoadedResult = LoadManually(warehouse, partAttrib, partAttribColumn, orders);
			AssertNotEquals("Precondition", 0, viewLoadedResult.Count);
			AssertEquals("View and Manual load should return same Count", manuallyLoadedResult.Count,
				viewLoadedResult.Count);

			for (int i = 0; i < manuallyLoadedResult.Count; i++)
			{
				AssertEqualInformation(manuallyLoadedResult[i], viewLoadedResult[i]);
			}
		}

		void AssertEqualInformation(WhsOrder order, DynamicBusinessObject obj)
		{
			if (order.IsFinalised)
			{
				AssertEquals("DateField", order.WD_FinalisedDate.Date, ((ZDateTimeOffset)obj["DateField"]).Date);
				AssertEquals("IsFinalised", "Y", obj["IsFinalised"]);
			}
			else
			{
				AssertEquals("DateField", order.WD_RequiredDate.Date, ((ZDateTimeOffset)obj["DateField"]).Date);
				AssertEquals("IsFinalised", "N", obj["IsFinalised"]);
			}

			AssertEquals("DocketPK", order.PK, obj["DocketPK"]);
			AssertEquals("DocketID", order.WD_DocketID, obj["DocketID"]);
			AssertEquals("FinalisedDate", order.WD_FinalisedDate.Date, ((ZDateTime)obj["FinalisedDate"]).Date);
			AssertEquals("RequiredDate", order.WD_RequiredDate.Date, ((ZDateTime)obj["RequiredDate"]).Date);
			AssertEquals("DocketType", order.WD_DocketType, obj["DocketType"]);
			AssertEquals("TransportReference", order.WD_TransportReference, obj["TransportReference"]);
			AssertEquals("ClientPK", order.Client.PK, obj["ClientPK"]);
			AssertEquals("ClientCode", order.Client.OH_Code, obj["ClientCode"]);
			AssertEquals("ClientName", order.Client.OH_FullName, obj["ClientName"]);
			AssertEquals("WW_PK", order.WD_WW_Whs, obj["WarehousePK"]);
			AssertEquals("WarehouseName", order.Warehouse.WW_WarehouseName, obj["WarehouseName"]);

			if (order.TransportCoDocAddress.E2_AddressOverride)
			{
				AssertEquals("TransportCoPK", ZGuid.Empty, obj["TransportCoPK"]);
				AssertEquals("TransportCoName", order.TransportCoDocAddress.E2_CompanyName, obj["TransportCoName"]);
			}
			else if (order.TransportCoDocAddress.Organisation != null)
			{
				AssertEquals("TransportCoPK", order.TransportCoPK, obj["TransportCoPK"]);
				AssertEquals("TransportCoName", order.TransportCoDocAddress.Organisation.OH_FullName,
					obj["TransportCoName"]);
			}

			if (order.CarrierServiceLevel != null)
			{
				AssertEquals("ServicePK", order.CarrierServiceLevel.PK, obj["ServicePK"]);
				AssertEquals("ServiceLevel", order.CarrierServiceLevel.PL_Code, obj["ServiceLevel"]);
			}

			AssertEquals("OrderNo", order.WD_ExternalReference, obj["OrderNo"]);
			AssertEquals("DocketStatus", order.WarehouseOrderStatus, obj["DocketStatus"]);

			if (order.ConsigneeDocAddress.E2_AddressOverride)
			{
				AssertEquals("ConsigneePK", ZGuid.Empty, obj["ConsigneePK"]);
				AssertEquals("ConsigneeName", order.ConsigneeDocAddress.E2_CompanyName, obj["ConsigneeName"]);
				AssertEquals("ConsigneeCity", order.ConsigneeDocAddress.E2_City, obj["ConsigneeCity"]);
				AssertEquals("ConsigneeState", order.ConsigneeDocAddress.E2_State, obj["ConsigneeState"]);
			}
			else
			{
				AssertEquals("ConsigneePK", order.Consignee.PK, obj["ConsigneePK"]);
				AssertEquals("ConsigneeName", order.Consignee.OH_FullName, obj["ConsigneeName"]);
				AssertEquals("ConsigneeCity", order.ConsigneeDocAddress.Address.OA_City, obj["ConsigneeCity"]);
				AssertEquals("ConsigneeState", order.ConsigneeDocAddress.Address.OA_State, obj["ConsigneeState"]);
			}

			AssertEquals("PalletsSent", order.WD_PalletsSent, obj["PalletsSent"]);
			AssertEquals("PackagesSent", order.WD_PackagesSent, obj["PackagesSent"]);
			AssertEquals("PackagesUQ", order.WD_F3_NKTotalPackType, obj["PackagesUQ"]);
			AssertEquals("WeightSent", order.WD_WeightSent, obj["WeightSent"]);
			AssertEquals("WeightUQ", order.WD_TotalWeightUnit, obj["WeightUQ"]);
			AssertEquals("VolumeSent", order.WD_CubicSent, obj["VolumeSent"]);
			AssertEquals("VolumeUQ", order.WD_TotalCubicUnit, obj["VolumeUQ"]);
		}

		#endregion

		#region SetupData

		WhsOrder[] SetupData(WhsWarehouse warehouse, OrgHeader client, OrgSupplierPart part)
		{
			var warehouse2 = Helper.CreateWarehouse("Whs2", "B", 5, 5);
			var client1 = Helper.CreateClient("1", "Client1");
			var client3 = Helper.CreateClient("3", "Client3");
			var transportCo1 = Helper.CreateClient("T1", "Transport1");
			var transportCo2 = Helper.CreateClient("T2", "Transport2");
			var consignee1 = Helper.CreateClient("C1", "Consignee1", "C1 City", "C1 State");
			var consignee2 = Helper.CreateClient("C2", "Consignee2", "C2 City", "C2 State");
			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client1, "P2");
			var part4 = Helper.CreateProduct(client3, "P4");
			var part5 = Helper.CreateProduct(client3, "P5");

			SetClientAttributesType(client1);
			SetClientAttributesType(client);
			SetClientAttributesType(client3);
			SetProductAttributesUse(client1, part1);
			SetProductAttributesUse(client1, part2);
			SetProductAttributesUse(client, part);
			SetProductAttributesUse(client3, part4);
			SetProductAttributesUse(client3, part5, setReleaseCaptured: true);

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part4.OP_RH_NKCommodityCode = commodity[1].RH_Code;

			CreateReceive(client1, warehouse, "R1", part1, 20m, part2, 20m);
			CreateReceive(client, warehouse, "R2", part, 20m, part, 20m);
			CreateReceive(client3, warehouse, "R3", part4, 10m, part4, 10m);
			CreateReceive(client3, warehouse2, "R4", part4, 10m, part4, 10m);
			CreateReceive(client3, warehouse, "R5", part5, 10m);

			var order1 = CreateOrder("O1", client1, warehouse, null, consignee1, "Tr. Ref. 1", ZDateTimeOffset.Today, part1);
			var order2 = CreateOrder("O2", client1, warehouse, transportCo1, consignee1, "Tr. Ref. 2", ZDateTimeOffset.Today,
				part2);
			var order3 = CreateOrder("O3", client, warehouse, transportCo1, consignee2, "Tr. Ref. 3", ZDateTimeOffset.Today,
				part);
			var order4 = CreateOrder("O4", client3, warehouse, transportCo2, consignee2, "Tr. Ref. 4", ZDateTimeOffset.Today,
				part4);
			var order5 = CreateOrder("O5", client3, warehouse2, transportCo1, consignee1, "Tr. Ref. 5", ZDateTimeOffset.Today,
				part4);

			CreateOrderLine(order1, part1, 10m, "A1", "A2", "A3", ZDate.Today.AddMonths(2), ZDate.Today.AddDays(-2));
			CreateOrderLine(order2, part2, 10m, "A1", "B2", "B3", ZDate.Today.AddMonths(3), ZDate.Today.AddDays(-3));
			CreateOrderLine(order3, part, 10m, "B1", "B2", "B3", ZDate.Today.AddMonths(3), ZDate.Today.AddDays(-3));
			var orderLine = CreateOrderLine(order4, part5, 10m, "", "", "", ZDate.Today.AddMonths(2),
				ZDate.Today.AddDays(-2));

			order1.ConsigneeDocAddress.E2_AddressOverride = ZBool.True;
			order1.ConsigneeNameOrPK = "Overriden Consignee Company name";
			order1.ConsigneeDocAddress.E2_State = "OverState";
			order1.ConsigneeDocAddress.E2_City = "OverCity";

			order2.TransportCoDocAddress.E2_AddressOverride = ZBool.True;
			order2.TransportCoNameOrPK = "Overriden TransportCo Company name";
			Factory.Save();

			Helper.CreatePickNew(order3);
			var pick = Helper.CreatePickNew(order4);
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "PA1";
			releaseLine.PartAttribute2 = "PA2";
			releaseLine.PartAttribute3 = "PA3";

			pick.FinaliseAllOrders();
			AssertEquals("Order should be finalized", true, order4.IsFinalised);
			pick.FinalisePick();
			AssertEquals("Pick should be finalized", true, pick.IsFinalised);
			Factory.Save();

			return new[] { order1, order2, order3, order4, order5 };
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

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part, bool setReleaseCaptured = false)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, true, setReleaseCaptured);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, true, setReleaseCaptured);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, true, setReleaseCaptured);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, true);
		}

		void CreateReceive(OrgHeader client, WhsWarehouse whs, ZString reference, OrgSupplierPart part, ZDecimal units)
		{
			var receive = Helper.CreateWhsReceive(client, whs, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, units, ZDate.Today.AddMonths(2),
				ZDate.Today.AddDays(-2), "", "", "", "");
			FinaliseReceive(receive);
		}

		void CreateReceive(OrgHeader client, WhsWarehouse whs, ZString reference, OrgSupplierPart part1,
			ZDecimal units1, OrgSupplierPart part2, ZDecimal units2)
		{
			var receive = Helper.CreateWhsReceive(client, whs, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, units1, ZDate.Today.AddMonths(2),
				ZDate.Today.AddDays(-2), "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, units2, ZDate.Today.AddMonths(3),
				ZDate.Today.AddDays(-3), "B1", "B2", "B3", "");
			FinaliseReceive(receive);
		}

		void FinaliseReceive(WhsReceive receive)
		{
			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPuttingAway", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
		}

		WhsOrder CreateOrder(ZString reference, OrgHeader client, WhsWarehouse whs, OrgHeader transportCo,
			OrgHeader consignee, ZString transportRef, ZDateTimeOffset requiredDate, OrgSupplierPart part)
		{
			var order = Helper.CreateWhsOrder(client, whs, reference);
			order.WD_RequiredDate = requiredDate;
			if (transportCo != null)
			{
				order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			}

			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			CreateOrderLine(order, part, 10m, "A1", "A2", "A3", ZDate.Today.AddMonths(2), ZDate.Today.AddDays(-2));

			return order;
		}

		WhsOrderLine CreateOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZString partAttrib1,
			ZString partAttrib2, ZString partAttrib3, ZDate expiryDate, ZDate packingDate)
		{
			var line = order.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = units;
			line.WE_PartAttrib1 = partAttrib1;
			line.WE_PartAttrib2 = partAttrib2;
			line.WE_PartAttrib3 = partAttrib3;
			line.WE_ExpiryDate = expiryDate;
			line.WE_PackingDate = packingDate;

			return line;
		}

		#endregion

		#region Loaders

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs,
			ZString partAttrib,
			SchemaStringColumn partAttributeColumn,
			OrgPartCategory category = null,
			ZGuid? distributionCentrePK = null,
			String includeUnfinalised = "Y")
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string attributeString;
			if (partAttributeColumn == WhsDocketLineSchema.WE_PartAttrib1)
			{
				attributeString = "@PartAttrib, '', '', ''";
			}
			else if (partAttributeColumn == WhsDocketLineSchema.WE_PartAttrib2)
			{
				attributeString = "'', @PartAttrib, '', ''";
			}
			else if (partAttributeColumn == WhsDocketLineSchema.WE_PartAttrib3)
			{
				attributeString = "'', '', @PartAttrib, ''";
			}
			else
			{
				attributeString = "'', '', '', @PartAttrib";
			}

			string sql;
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", whs.PK, WhsWarehouseSchema.PK);
			sqlParams.Add("@PartAttrib", partAttrib, partAttributeColumn);
			sqlParams.Add("@IncludeUnfinalised", includeUnfinalised, WhsDocketLineSchema.WE_IsValid);

			sql = $@"
			SELECT 
				*
			FROM
				WhsJobTransportManifestReport(
			 		@WarehousePK, 
					null, 
					null, 
					'', 
					null, 
					null, 
					{(category != null ? "@ProductCategoryPK" : "null")}, 
					null, 
					{(distributionCentrePK != null ? "@DistributionCentre" : "null")},
					{attributeString}, 
					null, 
					null, 
					@IncludeUnfinalised, 
					null, 
					null)
			ORDER BY ClientCode, TransportCoName, TransportReference asc";

			if (category != null)
			{
				sqlParams.Add("@ProductCategoryPK", category.PK, OrgPartCategorySchema.PK);
			}

			if (distributionCentrePK != null)
			{
				sqlParams.Add("@DistributionCentre", distributionCentrePK, OrgHeaderSchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		List<WhsOrder> LoadManually(WhsWarehouse whs, ZString partAttrib, SchemaStringColumn partAttributeColumn,
			WhsOrder[] orders, OrgPartCategory category = null)
		{
			var result = new List<WhsOrder>();
			var tempOrders = new[] { orders[0], orders[1], orders[2], orders[3], orders[4] };

			foreach (var order in tempOrders)
			{
				if (order.WD_WW_Whs == whs.PK &&
					HasOrderLineWithCorrectAttribute(order, partAttrib, partAttributeColumn) &&
					(category == null || HasOrderLineWithProductCategory(order, category)))
				{
					result.Add(order);
				}
			}

			return result;
		}

		bool HasOrderLineWithProductCategory(WhsOrder order, OrgPartCategory category)
		{
			var result = false;

			foreach (WhsOrderLine line in order.Lines)
			{
				var partRelation =
					line.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(order.Client,
						OrgPartRelation.RelationshipTypes.Owner);
				var lineCategory = partRelation?.Category;
				if (lineCategory != null && lineCategory.PK == category.PK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		bool HasOrderLineWithCorrectAttribute(WhsOrder order, ZString partAttrib,
			SchemaStringColumn partAttributeColumn)
		{
			if (order.WD_DocketStatus == DocketStatus.Codes.Entered ||
					order.WD_DocketStatus == DocketStatus.Codes.Held ||
					order.WD_DocketStatus == DocketStatus.Codes.Error)
			{
				foreach (WhsOrderLine line in order.Lines)
				{
					if ((ZString)line[partAttributeColumn] == partAttrib)
					{
						return true;
					}
				}
			}
			else
			{
				string releaseLinePartAttributePropertyName;

				if (partAttributeColumn == WhsDocketLineSchema.WE_PartAttrib1)
				{
					releaseLinePartAttributePropertyName = WhsReleaseLine.Schema.PartAttribute1;
				}
				else if (partAttributeColumn == WhsDocketLineSchema.WE_PartAttrib2)
				{
					releaseLinePartAttributePropertyName = WhsReleaseLine.Schema.PartAttribute2;
				}
				else
				{
					releaseLinePartAttributePropertyName = WhsReleaseLine.Schema.PartAttribute3;
				}

				foreach (WhsOrderLine line in order.Lines)
				{
					if (line.ReleaseLines.Count > 0 &&
						(ZString)line.ReleaseLines[0][releaseLinePartAttributePropertyName] == partAttrib)
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion
	}
}
