using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsJobHistoryOrdersDetailTest : WhsTestCaseWithFactory
	{
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

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", productVB, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", productCoke, 20m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O3", productTea, 5m);
			Factory.Save();

			var result1 = LoadView_WhsJobHistoryOrdersDetailReport(categoryPK: categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WhsJobHistoryOrdersDetailReport(categoryPK: categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WhsJobHistoryOrdersDetailReport(categoryPK: categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		#endregion

		#region TestView_Units

		public void TestView_Units_FinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var order1Line = order.Lines[0];
			var results = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1.PK, data.Org1.PK);
			var orderRecord1 = results.Single(o => (ZString)o["Reference"] == "O1");
			AssertEquals("Order1 Units Allocated should be equal 10m.", 10m, orderRecord1["UnitsAllocated"]);
			AssertEquals("Order1 Units Sent should be equal 10m.", 10m, orderRecord1["QuantityActual"]);
			AssertEquals("Order1 Units Picked should be equal 10m.", 10m, orderRecord1["UnitsPicked"]);
		}

		public void TestView_Units_UnfinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);
			var order2Line = order2.Lines[0];
			Factory.Save();

			var results = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1.PK, data.Org1.PK);
			var orderRecord2 = results.Single(o => (ZString)o["Reference"] == "O2");
			AssertEquals("Order2 Units Allocated should be equal 10m.", 10m, orderRecord2["UnitsAllocated"]);
			AssertEquals("Order2 Units Sent should be 0.", ZDecimal.Zero, orderRecord2["QuantityActual"]);
			AssertEquals("Order2 Units Picked should be 0.", ZDecimal.Zero, orderRecord2["UnitsPicked"]);
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
			var results = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1.PK, data.Org1.PK);
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

			var results = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1.PK, data.Org1.PK);
			var orderRecord1 = results.First(o => (ZString)o["Reference"] == "O1");
			AssertEquals("Order1 Units Allocated should be equal 10m.", 10m, orderRecord1["UnitsAllocated"]);
			AssertEquals("Order1 Units Sent should be equal 0.", ZDecimal.Zero, orderRecord1["QuantityActual"]);
			AssertEquals("Order1 Units Picked should be equal 5m.", 5m, orderRecord1["UnitsPicked"]);
		}

		#endregion

		#region TestView_DistributionCentre

		public void TestView_DistributionCentre()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var consignee = Helper.CreateClient("C2");
			var distributionCentre = Helper.CreateClient("C3");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateProductClientRelationShip(consignee, product, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 1m, true, true);
			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 1m);
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			order.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 1, resultNoFilter.Count);

			var resultFilterDistributionCentre = LoadView_WhsJobHistoryOrdersDetailReport(distributionCentrePK: distributionCentre.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by distribution centre", 1,
				(resultLine) => { return resultLine["DistributionCentrePK"].ToString() == distributionCentre.PK.ToString(); },
				resultFilterDistributionCentre);

			AssertEquals(distributionCentre.OH_FullName, resultFilterDistributionCentre[0]["DistributionCentreCoName"]);
			AssertEquals(distributionCentre.PK, resultFilterDistributionCentre[0]["DistributionCentrePK"]);

			resultFilterDistributionCentre = LoadView_WhsJobHistoryOrdersDetailReport(distributionCentrePK: ZGuid.NewZGuid());
			AssertEquals("Should not match with any distribution centre", 0, resultFilterDistributionCentre.Count);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_Warehouse

		public void TestView_WithProductCategoryFilter_Warehouse()
		{
			var warehouse1 = Helper.CreateWarehouse("W1", "A");
			var warehouse2 = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", product, 1m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R2", product, 5m, true, true);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O1", product, 1m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse2, "O2", product, 2m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse2, "O3", product, 3m);
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 3, resultNoFilter.Count);

			var resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(whsPK: warehouse1.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by warehouse 1", 1,
				(resultLine) => { return (ZGuid)resultLine["WarehousePK"] == warehouse1.PK; }, resultFilter1);

			var resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(whsPK: warehouse2.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by warehouse 2", 2,
				(resultLine) => { return (ZGuid)resultLine["WarehousePK"] == warehouse2.PK; }, resultFilter2);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_Client

		public void TestView_WithProductCategoryFilter_Client()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product = Helper.CreateProduct(client1, "ABC");
			Helper.CreateProductClientRelationShip(client2, product, OrgPartRelation.RelationshipTypes.Owner);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse, "R1", product, 1m, true, true);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse, "R2", product, 5m, true, true);
			Helper.CreateWhsOrderWithOrderLine(client1, warehouse, "O1", product, 1m);
			Helper.CreateWhsOrderWithOrderLine(client2, warehouse, "O2", product, 2m);
			Helper.CreateWhsOrderWithOrderLine(client2, warehouse, "O3", product, 3m);
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 3, resultNoFilter.Count);

			var resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(clientPK: client1.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by client 1", 1,
				(resultLine) => { return (ZGuid)resultLine["ClientPK"] == client1.PK; }, resultFilter1);

			var resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(clientPK: client2.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by client 2", 2,
				(resultLine) => { return (ZGuid)resultLine["ClientPK"] == client2.PK; }, resultFilter2);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_Product_And_CommodityPK

		public void TestView_WithProductCategoryFilter_Product_And_CommodityPK()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "ABC");
			var product2 = Helper.CreateProduct(client, "DEF");
			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			product1.OP_RH_NKCommodityCode = commodity1.RH_Code;
			product2.OP_RH_NKCommodityCode = commodity2.RH_Code;
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product1, 1m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", product2, 5m, true, true);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product1, 1m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", product2, 2m);
			Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O3", product2, 3m);
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 3, resultNoFilter.Count);

			var resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(productPK: product1.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by product 1", 1,
				(resultLine) => { return (ZGuid)resultLine["ProductPK"] == product1.PK; }, resultFilter1);

			var resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(productPK: product2.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by product 2", 2,
				(resultLine) => { return (ZGuid)resultLine["ProductPK"] == product2.PK; }, resultFilter2);

			var resultFilterCommodity1 = LoadView_WhsJobHistoryOrdersDetailReport(commodityPK: commodity1.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by commodity 1", 1,
				(resultLine) => { return (ZGuid)resultLine["CommodityPK"] == commodity1.PK; }, resultFilterCommodity1);

			var resultFilterCommodity2 = LoadView_WhsJobHistoryOrdersDetailReport(commodityPK: commodity2.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by commodity 2", 2,
				(resultLine) => { return (ZGuid)resultLine["CommodityPK"] == commodity2.PK; }, resultFilterCommodity2);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_Attributes_Receive

		public void TestView_WithProductCategoryFilter_Attributes_Receive()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			SetClientAttributesType(client);
			SetProductAttributesUse(client, product);
			var receive1 = Helper.CreateWhsReceive(client, warehouse, "R1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive1, product, 10m, ZDate.Today.AddDays(+1),
				ZDate.Today.AddDays(-1), "Att1", "Att2", "Att3", "");
			var receive2 = Helper.CreateWhsReceive(client, warehouse, "R2");
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive2, product, 10m, ZDate.Today.AddDays(+1),
				ZDate.Today.AddDays(-2), "Att1", "Att22", "Att33", "");
			var receive3 = Helper.CreateWhsReceive(client, warehouse, "R3");
			var inv3 = Helper.CreateWhsReceiveInventoryLine(receive3, product, 10m, ZDate.Today.AddDays(+2),
				ZDate.Today.AddDays(-3), "Att11", "Att22", "Att333", "");
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			receive3.AllocateLocationsWithMock();
			receive3.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 30m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition", 3, pick.GetAllPickLines().Count());
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 3, resultNoFilter.Count);

			var resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib1: "Att1");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 1", 2,
				(resultLine) => { return resultLine["PartAttrib1"].ToString() == "Att1"; }, resultFilter1);

			resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib1: "Att11");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 1", 1,
				(resultLine) => { return resultLine["PartAttrib1"].ToString() == "Att11"; }, resultFilter1);

			var resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib2: "Att2");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 2", 1,
				(resultLine) => { return resultLine["PartAttrib2"].ToString() == "Att2"; }, resultFilter2);

			resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib2: "Att22");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 2", 2,
				(resultLine) => { return resultLine["PartAttrib2"].ToString() == "Att22"; }, resultFilter2);

			var resultFilter3 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib3: "Att3");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 3", 1,
				(resultLine) => { return resultLine["PartAttrib3"].ToString() == "Att3"; }, resultFilter3);

			resultFilter3 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib3: "Att33");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 3", 1,
				(resultLine) => { return resultLine["PartAttrib3"].ToString() == "Att33"; }, resultFilter3);

			resultFilter3 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib3: "Att333");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 3", 1,
				(resultLine) => { return resultLine["PartAttrib3"].ToString() == "Att333"; }, resultFilter3);

			var resultFilterExpiryDate =
				LoadView_WhsJobHistoryOrdersDetailReport(expiryDate: ZDateTime.Today.AddDays(+1));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by expiry date", 2,
				(resultLine) => { return (ZDateTime)resultLine["ExpiryDate"] == ZDateTime.Today.AddDays(+1); },
				resultFilterExpiryDate);

			var resultFilterPackingDate =
				LoadView_WhsJobHistoryOrdersDetailReport(packingDate: ZDateTime.Today.AddDays(-2));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by packing date", 1,
				(resultLine) => { return (ZDateTime)resultLine["PackingDate"] == ZDateTime.Today.AddDays(-2); },
				resultFilterPackingDate);
		}

		public void TestView_WithProductCategoryFilter_SerialNumber_Receive()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(client, warehouse, "R1");
			var inv1 = Helper.CreateWhsReceiveLine(receive, product, 1m, warehouse.DefaultLocation);
			inv1.WE_SerialNumber = "SSW";
			var inv2 = Helper.CreateWhsReceiveLine(receive, product, 1m, warehouse.DefaultLocation);
			inv2.WE_SerialNumber = "SER";
			var inv3 = Helper.CreateWhsReceiveLine(receive, product, 1m, warehouse.DefaultLocation);
			inv3.WE_SerialNumber = "EEE";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, warehouse);
			var orderLine = Helper.CreateWhsOrderLine(order, product, 1m);
			orderLine.WE_SerialNumber = "SSW";
			Helper.CreateWhsOrderLine(order, product, 1m);
			Helper.CreateWhsOrderLine(order, product, 1m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition", 3, pick.GetAllPickLines().Count());
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 3, resultNoFilter.Count);

			var resultFilter4 = LoadView_WhsJobHistoryOrdersDetailReport(serialNumber: "SSW");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Serial Number", 1,
				(resultLine) => { return resultLine["SerialNumber"].ToString() == "SSW"; }, resultFilter4);

			resultFilter4 = LoadView_WhsJobHistoryOrdersDetailReport(serialNumber: "SER");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Serial Number", 1,
				(resultLine) => { return resultLine["SerialNumber"].ToString() == "SER"; }, resultFilter4);

			resultFilter4 = LoadView_WhsJobHistoryOrdersDetailReport(serialNumber: "EEE");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Serial Number", 1,
				(resultLine) => { return resultLine["SerialNumber"].ToString() == "EEE"; }, resultFilter4);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_Attributes_ReleaseCaptured

		public void TestView_WithProductCategoryFilter_Attributes_ReleaseCaptured()
		{
			var today = ZDate.Today;
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");

			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, false);
			Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, false);

			Helper.SetProductAttributeUse(client, product, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Two, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Three, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.ExpiryDate, true, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.PackingDate, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 10m);
			var receiveline = receive.Lines[0];
			receiveline.WE_ExpiryDate = today.AddDays(+10);
			receiveline.WE_PackingDate = today.AddDays(-5);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 10m);
			var orderLine = order.Lines[0];
			orderLine.WE_ExpiryDate = today.AddDays(+10);
			orderLine.WE_PackingDate = today.AddDays(-5);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			WhsReleaseLineCollection.CopyNonReleaseCapturedAttributes(releaseLine2, releaseLine1);

			releaseLine1.Quantity = 5m;
			releaseLine1.PartAttribute1 = "Red";
			releaseLine1.PartAttribute2 = "PL1";
			releaseLine1.PartAttribute3 = "XYZ";

			releaseLine2.Quantity = 5m;
			releaseLine2.PartAttribute1 = "Blue";
			releaseLine2.PartAttribute2 = "PL1";
			releaseLine2.PartAttribute3 = "KLM";

			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 2, resultNoFilter.Count);

			var resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib1: "Red");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 1", 1,
				(resultLine) => { return resultLine["PartAttrib1"].ToString() == "Red"; }, resultFilter1);

			resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib1: "Blue");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 1", 1,
				(resultLine) => { return resultLine["PartAttrib1"].ToString() == "Blue"; }, resultFilter1);

			var resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib2: "PL1");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 2", 2,
				(resultLine) => { return resultLine["PartAttrib2"].ToString() == "PL1"; }, resultFilter2);

			var resultFilter3 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib3: "XYZ");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 3", 1,
				(resultLine) => { return resultLine["PartAttrib3"].ToString() == "XYZ"; }, resultFilter3);

			resultFilter3 = LoadView_WhsJobHistoryOrdersDetailReport(partAttrib3: "KLM");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Attributes 3", 1,
				(resultLine) => { return resultLine["PartAttrib3"].ToString() == "KLM"; }, resultFilter3);

			var resultFilterExpiryDate =
				LoadView_WhsJobHistoryOrdersDetailReport(expiryDate: ZDateTime.Today.AddDays(+10));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by expiry date", 2,
				(resultLine) => { return (ZDateTime)resultLine["ExpiryDate"] == ZDateTime.Today.AddDays(+10); },
				resultFilterExpiryDate);

			resultFilterExpiryDate = LoadView_WhsJobHistoryOrdersDetailReport(expiryDate: ZDateTime.Today.AddDays(+12));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by expiry date", 0,
				(resultLine) => { return (ZDateTime)resultLine["ExpiryDate"] == ZDateTime.Today.AddDays(+12); },
				resultFilterExpiryDate);

			var resultFilterPackingDate =
				LoadView_WhsJobHistoryOrdersDetailReport(packingDate: ZDateTime.Today.AddDays(-5));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by packing date", 2,
				(resultLine) => { return (ZDateTime)resultLine["PackingDate"] == ZDateTime.Today.AddDays(-5); },
				resultFilterPackingDate);

			resultFilterPackingDate =
				LoadView_WhsJobHistoryOrdersDetailReport(packingDate: ZDateTime.Today.AddDays(-6));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by packing date", 0,
				(resultLine) => { return (ZDateTime)resultLine["PackingDate"] == ZDateTime.Today.AddDays(-6); },
				resultFilterPackingDate);
		}

		public void TestView_WithProductCategoryFilter_SerialNumber_ReleaseCaptured()
		{
			var today = ZDate.Today;
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");

			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(client, product, AttributeNumber.Serial, true, true);

			var receive = Helper.CreateWhsReceive(client, warehouse, "R1");
			Helper.CreateWhsReceiveLine(receive, product, 1m, warehouse.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, product, 1m, warehouse.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, warehouse);
			var orderLine1 = Helper.CreateWhsOrderLine(order, product, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, product, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickline1 = orderLine1.PickLines.Single();
			var pickline2 = orderLine2.PickLines.Single();

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			releaseLine1.SerialNumber = "NUM";

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 1m;
			releaseLine2.SerialNumber = "SER";

			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 2, resultNoFilter.Count);

			var resultFilter1 = LoadView_WhsJobHistoryOrdersDetailReport(serialNumber: "NUM");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Serial Number", 1,
				(resultLine) => { return resultLine["SerialNumber"].ToString() == "NUM"; }, resultFilter1);

			var resultFilter2 = LoadView_WhsJobHistoryOrdersDetailReport(serialNumber: "SER");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by Serial Number", 1,
				(resultLine) => { return resultLine["SerialNumber"].ToString() == "SER"; }, resultFilter2);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_Consignee

		public void TestView_WithProductCategoryFilter_Consignee()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var consignee = Helper.CreateClient("C2");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateProductClientRelationShip(consignee, product, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 1m, true, true);
			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 1m);
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 1, resultNoFilter.Count);

			var resultFilterConsignee = LoadView_WhsJobHistoryOrdersDetailReport(consigneePK: consignee.PK);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by consignee", 1,
				(resultLine) => { return resultLine["ConsigneePK"].ToString() == consignee.PK.ToString(); },
				resultFilterConsignee);

			resultFilterConsignee = LoadView_WhsJobHistoryOrdersDetailReport(consigneePK: ZGuid.NewZGuid());
			AssertEquals("Should not match with any consignee", 0, resultFilterConsignee.Count);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_ServiceLevel

		public void TestView_WithProductCategoryFilter_ServiceLevel()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var consignee = Helper.CreateClient("C2");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateProductClientRelationShip(consignee, product, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 2m, true, true);
			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 1m);
			order1.WD_RS_NKServiceLevel = "ABC";
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", product, 1m);
			order2.WD_RS_NKServiceLevel = "XYZ";
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 2, resultNoFilter.Count);

			var resultFilterServiceLevel = LoadView_WhsJobHistoryOrdersDetailReport(serviceLevel: "ABC");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by ServiceLevel", 1,
				(resultLine) => { return resultLine["ServiceLevel"].ToString() == "ABC"; }, resultFilterServiceLevel);

			resultFilterServiceLevel = LoadView_WhsJobHistoryOrdersDetailReport(serviceLevel: "111");
			AssertEquals("Should not match with any service level", 0, resultFilterServiceLevel.Count);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_DocketDate

		public void TestView_WithProductCategoryFilter_DocketDate()
		{
			var today = ZDateTime.UtcToday;
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 2m, true, true);

			var requiredDate = warehouse.GetWarehouseBranchDateTimeOffset(today);
			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 1m);
			order1.WD_RequiredDate = requiredDate.AddDays(-10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", product, 1m);
			order2.WD_RequiredDate = requiredDate.AddDays(-5);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition", false, order1.IsFinalised);
			AssertEquals("Precondition", false, order2.IsFinalised);
			AssertEquals("Precondition", false, pick.IsFinalised);
			Factory.Save();

			var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
			AssertEquals("Should show all rows", 2, resultNoFilter.Count);

			var resultFilterRequiredDate =
				LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddDays(-12), toDocketDate: requiredDate);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by RequiredDate ", 2,
				(resultLine) =>
				{
					return ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order1.WD_RequiredDate.ToString("dd-MMM-yyyy hh:mm:ss") ||
						   ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order2.WD_RequiredDate.ToString("dd-MMM-yyyy hh:mm:ss");
				}, resultFilterRequiredDate);

			resultFilterRequiredDate =
				LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddDays(-7), toDocketDate: requiredDate);
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by RequiredDate", 1,
				(resultLine) => ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order2.WD_RequiredDate.ToString("dd-MMM-yyyy hh:mm:ss"),
				resultFilterRequiredDate);

			resultFilterRequiredDate = LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddDays(-12),
				toDocketDate: requiredDate.AddDays(-11));
			AssertEquals("Not in this duration", 0, resultFilterRequiredDate.Count);

			resultFilterRequiredDate = LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddDays(-1),
				toDocketDate: requiredDate.AddDays(+1));
			AssertEquals("Not in this duration", 0, resultFilterRequiredDate.Count);

			order1.FinaliseDocket();
			AssertIsFinalisedPrecondition(order1);
			Factory.Save();

			var resultIsFinalised = LoadView_WhsJobHistoryOrdersDetailReport(isIncludeUnfinalisedJob: "");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by IsFinalised", 1,
				(resultLine) => { return resultLine["IsFinalised"].ToString() == "Y"; }, resultIsFinalised);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			order1 = newFactory.Load<WhsOrder>(order1.PK);
			order2 = newFactory.Load<WhsOrder>(order2.PK);
			var resultFilterFinalisedDate = LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddDays(-1),
				toDocketDate: requiredDate.AddDays(+1));
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by FinalisedDate", 2,
				(resultLine) =>
				{
					return ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order1.WD_FinalisedDate.ToString("dd-MMM-yyyy hh:mm:ss") ||
						   ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order2.WD_FinalisedDate.ToString("dd-MMM-yyyy hh:mm:ss");
				}, resultFilterFinalisedDate);

			resultIsFinalised = LoadView_WhsJobHistoryOrdersDetailReport(isIncludeUnfinalisedJob: "");
			AssertWhsJobHistoryOrdersDetailReportResult("Should filter by IsFinalised", 2,
				(resultLine) => { return resultLine["IsFinalised"].ToString() == "Y"; }, resultIsFinalised);
		}

		#endregion

		#region TestView_WithProductCategoryFilter_DocketDateTimeRange

		public void TestView_WithProductCategoryFilter_DocketDateTimeRange()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "ABC");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 2m, true, true);
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Env.CurrentDepartmentPK))
			{
				var now = ZDateTime.TruncateSeconds(ZDateTime.Now);
				var requiredDate = now.ToOffset();

				var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", product, 1m);
				order1.WD_RequiredDate = requiredDate.AddHours(-10);
				var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", product, 1m);
				order2.WD_RequiredDate = requiredDate.AddHours(-5);

				var pick = Helper.CreatePickNew(order1, order2);
				AssertEquals("Precondition", false, order1.IsFinalised);
				AssertEquals("Precondition", false, order2.IsFinalised);
				AssertEquals("Precondition", false, pick.IsFinalised);
				Factory.Save();

				var resultNoFilter = LoadView_WhsJobHistoryOrdersDetailReport();
				AssertEquals("Should show all rows", 2, resultNoFilter.Count);

				var resultFilterRequiredDate =
					LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddHours(-12), toDocketDate: requiredDate);
				AssertWhsJobHistoryOrdersDetailReportResult("Should filter by RequiredDate ", 2,
					(resultLine) =>
					{
						return ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order1.WD_RequiredDate.ToString("dd-MMM-yyyy hh:mm:ss") ||
							   ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order2.WD_RequiredDate.ToString("dd-MMM-yyyy hh:mm:ss");
					}, resultFilterRequiredDate);

				resultFilterRequiredDate =
					LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddHours(-7), toDocketDate: requiredDate);
				AssertWhsJobHistoryOrdersDetailReportResult("Should filter by RequiredDate", 1,
					(resultLine) => ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order2.WD_RequiredDate.ToString("dd-MMM-yyyy hh:mm:ss"),
					resultFilterRequiredDate);

				resultFilterRequiredDate = LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddHours(-12),
					toDocketDate: requiredDate.AddHours(-11));
				AssertEquals("Not in this duration", 0, resultFilterRequiredDate.Count);

				resultFilterRequiredDate = LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddHours(-1),
					toDocketDate: requiredDate.AddHours(+1));
				AssertEquals("Not in this duration", 0, resultFilterRequiredDate.Count);

				order1.FinaliseDocket();
				AssertIsFinalisedPrecondition(order1);
				Factory.Save();

				var resultIsFinalised = LoadView_WhsJobHistoryOrdersDetailReport(isIncludeUnfinalisedJob: "");
				AssertWhsJobHistoryOrdersDetailReportResult("Should filter by IsFinalised", 1,
					(resultLine) => { return resultLine["IsFinalised"].ToString() == "Y"; }, resultIsFinalised);

				pick.FinaliseAllOrders();
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order2);
				AssertIsFinalisedPrecondition(pick);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				order1 = newFactory.Load<WhsOrder>(order1.PK);
				order2 = newFactory.Load<WhsOrder>(order2.PK);
				var resultFilterFinalisedDate = LoadView_WhsJobHistoryOrdersDetailReport(fromDocketDate: requiredDate.AddHours(-1),
					toDocketDate: requiredDate.AddHours(+1));
				AssertWhsJobHistoryOrdersDetailReportResult("Should filter by FinalisedDate", 2,
					(resultLine) =>
					{
						return ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order1.WD_FinalisedDate.ToString("dd-MMM-yyyy hh:mm:ss") ||
							   ((ZDateTimeOffset)resultLine["DateTimeOffsetField"]).ToString("dd-MMM-yyyy hh:mm:ss") == order2.WD_FinalisedDate.ToString("dd-MMM-yyyy hh:mm:ss");
					}, resultFilterFinalisedDate);

				resultIsFinalised = LoadView_WhsJobHistoryOrdersDetailReport(isIncludeUnfinalisedJob: "");
				AssertWhsJobHistoryOrdersDetailReportResult("Should filter by IsFinalised", 2,
					(resultLine) => { return resultLine["IsFinalised"].ToString() == "Y"; }, resultIsFinalised);
			}
		}

		#endregion

		#region AssertWhsJobHistoryOrdersDetailReportResult

		static void AssertWhsJobHistoryOrdersDetailReportResult(string message, int numberofExpectedLines,
			Func<DynamicBusinessObject, bool> filter, DynamicBusinessObjectCollection result)
		{
			AssertEquals(message, numberofExpectedLines, result.Count);
			AssertEquals(message, true, result.All(filter));
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
			var result = LoadView_WhsJobHistoryOrdersDetailReport(whs, client, part);

			AssertEquals(2, result.Count);

			AssertResult(orders[0].Lines[1], result[0], commodity);
			AssertResult(orders[1].Lines[1], result[1], commodity);
		}

		public void TestView_LoadingStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var requiredDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcToday);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.WD_RequiredDate = requiredDate;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.WD_RequiredDate = requiredDate;
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

			var results = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should return 2 records.", 2, results.Count);
			AssertResult(order1.Lines.Cast<WhsOrderLine>().Single(), results[0], null);
			AssertResult(order2.Lines.Cast<WhsOrderLine>().Single(), results[1], null);
		}

		public void TestView_ProductCategory()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("2", "2");
			var part = Helper.CreateProduct(client, "2");
			var category = Helper.CreateProductCategory(client, part, "Cat1");
			category.OPC_CategoryDescription = "Category 1";

			var order = SetupDataForProductCategory(whs, client, part);
			var result =
				LoadView_WhsJobHistoryOrdersDetailReport(whsPK: whs.PK, clientPK: client.PK, categoryPK: category.PK);
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

			var result = LoadView_WhsJobHistoryOrdersDetailReport(orderBy: "DateTimeOffsetField");
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
			AssertEquals(orderLine.Order.SalesChannel?.WSH_Code ?? string.Empty, dynamicObject["SalesChannel"]);
		}

		void AssertResult(WhsOrderLine orderLine, DynamicBusinessObject dynamicObject, RefCommodityCode[] commodity)
		{
			var order = orderLine.Order;
			var warehouse = order.Warehouse;
			AssertEquals("WarehousePK", warehouse.PK, dynamicObject["WarehousePK"]);
			AssertEquals("WarehouseName", warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
			AssertEquals("ClientPK", order.Client.PK, dynamicObject["ClientPK"]);
			AssertEquals("ClientCode", order.Client.OH_Code, dynamicObject["ClientCode"]);
			AssertEquals("ClientFullName", order.Client.OH_FullName, dynamicObject["ClientFullName"]);

			if (order.IsFinalised)
			{
				AssertEquals("IsFinalised", "Y", dynamicObject["IsFinalised"]);
				AssertEquals("DateTimeOffsetField", order.WD_FinalisedDate, ((ZDateTimeOffset)dynamicObject["DateTimeOffsetField"]));
				AssertEquals("QuantityExpected", orderLine.WE_TransactionQuantity, dynamicObject["QuantityExpected"]);
				AssertEquals("QuantityActual", orderLine.WE_TransactionQuantity, dynamicObject["QuantityActual"]);
				AssertEquals("Packs",
					orderLine.SupplierPart.UnitConverter.Convert(orderLine.WE_TransactionQuantity,
						orderLine.SupplierPart.OP_StockKeepingUnit, orderLine.WE_F3_NKPackType),
					dynamicObject["Packs"]);
				AssertEquals("Weight", orderLine.SupplierPart.OP_Weight * orderLine.WE_TransactionQuantity,
					dynamicObject["Weight"]);
				AssertEquals("Volume", orderLine.SupplierPart.OP_Cubic * orderLine.WE_TransactionQuantity,
					dynamicObject["Volume"]);
			}
			else
			{
				AssertEquals("IsFinalised", "N", dynamicObject["IsFinalised"]);
				AssertEquals("DateTimeOffsetField", order.WD_RequiredDate, ((ZDateTimeOffset)dynamicObject["DateTimeOffsetField"]));
				AssertEquals("QuantityExpected", orderLine.WE_TransactionQuantity, dynamicObject["QuantityExpected"]);
				AssertEquals("QuantityActual", 0m, dynamicObject["QuantityActual"]);
				AssertEquals("Packs", 0m, dynamicObject["Packs"]);
				AssertEquals("Weight", 0m, dynamicObject["Weight"]);
				AssertEquals("Volume", 0m, dynamicObject["Volume"]);
			}

			AssertEquals("DocketPK", order.PK, dynamicObject["DocketPK"]);
			AssertEquals("Reference", order.WD_ExternalReference, dynamicObject["Reference"]);
			AssertEquals("FinalisedDate", order.WD_FinalisedDate.Date, ((ZDateTime)dynamicObject["FinalisedDate"]).Date);
			AssertEquals("OrderedDate", order.WD_BookingDate.Date, ((ZDateTimeOffset)dynamicObject["OrderedDate"]).Date);
			AssertEquals("RequiredDate", order.WD_RequiredDate.Date, ((ZDateTime)dynamicObject["RequiredDate"]).Date);
			AssertEquals("DocketStatus", order.WarehouseOrderStatus, dynamicObject["DocketStatus"]);
			AssertEquals("DocektSubType", order.WD_DocketSubType, dynamicObject["DocketSubType"]);
			AssertEquals("UnitsSent", order.WD_UnitsSent, dynamicObject["UnitsSent"]);
			AssertEquals("PackagesSent", order.WD_PackagesSent, dynamicObject["PackagesSent"]);
			AssertEquals("PalletsSent", order.WD_PalletsSent, dynamicObject["PalletsSent"]);
			AssertEquals("TotalWeight", order.WD_WeightSent, dynamicObject["TotalWeight"]);
			AssertEquals("TotalWeightUQ", order.WD_TotalWeightUnit, dynamicObject["TotalWeightUQ"]);
			AssertEquals("TotalVolume", order.WD_CubicSent, dynamicObject["TotalVolume"]);
			AssertEquals("TotalVolumeUQ", order.WD_TotalCubicUnit, dynamicObject["TotalVolumeUQ"]);
			AssertEquals("TransportReference", order.WD_TransportReference, dynamicObject["TransportReference"]);
			AssertEquals("Service Level", order.WD_RS_NKServiceLevel, dynamicObject["ServiceLevel"]);
			AssertEquals("Sales Channel", orderLine.Order.SalesChannel?.WSH_Code ?? string.Empty, dynamicObject["SalesChannel"]);

			if (order.TransportCoDocAddress.E2_AddressOverride)
			{
				AssertEquals("TransportCoName", order.TransportCoDocAddress.E2_CompanyName,
					dynamicObject["TransportCoName"]);
			}
			else if (order.TransportCoDocAddress.Organisation != null)
			{
				AssertEquals("TransportCoName", order.TransportCoDocAddress.Organisation.OH_FullName,
					dynamicObject["TransportCoName"]);
			}

			if (order.ConsigneeDocAddress.E2_AddressOverride)
			{
				AssertEquals("ConsigneePK", ZGuid.Empty, dynamicObject["ConsigneePK"]);
				AssertEquals("ConsigneeName", order.ConsigneeDocAddress.E2_CompanyName, dynamicObject["ConsigneeName"]);
			}
			else
			{
				AssertEquals("ConsigneePK", order.Consignee.PK, dynamicObject["ConsigneePK"]);
				AssertEquals("ConsigneeName", order.Consignee.OH_FullName, dynamicObject["ConsigneeName"]);
			}

			AssertEquals("PartAttrib1", orderLine.ReleaseLines[0].PartAttribute1, dynamicObject["PartAttrib1"]);
			AssertEquals("PartAttrib1Name", order.Client.MiscServ.OM_IMPartAttrib1Name,
				dynamicObject["PartAttrib1Name"]);
			AssertEquals("PartAttrib2", orderLine.ReleaseLines[0].PartAttribute2, dynamicObject["PartAttrib2"]);
			AssertEquals("PartAttrib3", orderLine.ReleaseLines[0].PartAttribute3, dynamicObject["PartAttrib3"]);
			AssertEquals("SerialNumber", orderLine.ReleaseLines[0].SerialNumber, dynamicObject["SerialNumber"]);
			AssertEquals("PackingDate", orderLine.ReleaseLines[0].PackingDate, dynamicObject["PackingDate"]);
			AssertEquals("ExpiryDate", orderLine.ReleaseLines[0].ExpiryDate, dynamicObject["ExpiryDate"]);
			AssertEquals("CODAmount", order.WD_ShipperCODAmount, dynamicObject["CODAmount"]);
			AssertEquals("InsuranceAmount", order.WD_LocalCartInsuranceCost, dynamicObject["InsuranceAmount"]);

			AssertEquals("Product", orderLine.SupplierPart.OP_PartNum, dynamicObject["Product"]);
			AssertEquals("ProductDesc", orderLine.SupplierPart.OP_Desc, dynamicObject["ProductDesc"]);
			AssertEquals("ProductPK", orderLine.SupplierPart.PK, dynamicObject["ProductPK"]);
			AssertEquals("QuantityUQ", orderLine.SupplierPart.OP_StockKeepingUnit, dynamicObject["QuantityUQ"]);
			AssertEquals("PackUQ", orderLine.WE_F3_NKPackType, dynamicObject["PackUQ"]);
			AssertEquals("WeightUQ", orderLine.SupplierPart.OP_WeightUQ, dynamicObject["WeightUQ"]);
			AssertEquals("VolumeUQ", orderLine.SupplierPart.OP_CubicUQ, dynamicObject["VolumeUQ"]);
			if (commodity != null)
			{
				AssertEquals("CommodityCode", commodity[0].RH_Code, dynamicObject["CommodityCode"]);
				AssertEquals("CommodityPK", commodity[0].PK, dynamicObject["CommodityPK"]);
			}

			var partRelation =
				orderLine.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(order.Client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation != null ? partRelation.Category : null;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			AssertEquals("ProductCategoryCode", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryCode,
				dynamicObject["ProductCategoryDescription"]);
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

			SetClientAttributesType(client);
			SetClientAttributesType(client2);
			SetProductAttributesUse(client, part1);
			SetProductAttributesUse(client2, part2);
			SetProductAttributesUse(client, part);

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part.OP_RH_NKCommodityCode = commodity[0].RH_Code;

			part.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part, "UNT", "PLT", 10);

			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part.PK;
			relation.OU_OH = client2.PK;

			var salesChannel1 = Helper.CreateWhsSalesChannel("SC1", "Sales Channel 1");
			var salesChannel2 = Helper.CreateWhsSalesChannel("SC2", "Sales Channel 2");

			SetupReceive(client, whs, "11", part1, 111, part, 112);
			SetupReceive(client, whs, "12", part1, 121, part, 122);
			SetupReceive(client2, whs, "21", part2, 211, part2, 212);
			SetupReceive(client2, whs2, "22", part2, 221, part2, 222);
			Factory.Save();

			var order11 = SetupOrders(client, whs, "11", consignee1, new ZDateTime(year, 1, 1), "TR11", "CR11", "ER11",
				transportCo1, part1, 111, part, 112, true, 55, 3, 100.0, 25.0, ZGuid.Empty);
			var order12 = SetupOrders(client, whs, "12", consignee2, new ZDateTime(year, 1, 1), "TR12", "CR12", "ER12",
				transportCo2, part1, 121, part, 122, false, 1, 3, 45.0, 250.0, salesChannel2.PK);
			var order21 = SetupOrders(client2, whs, "21", consignee1, new ZDateTime(year, 1, 1), "TR21", "CR21", "ER21",
				transportCo1, part2, 211, part, 212, true, 0, 1, 0, 0, ZGuid.Empty);
			var order22 = SetupOrders(client2, whs2, "22", consignee2, new ZDateTime(year, 1, 1), "TR22", "CR22",
				"ER22", transportCo2, part2, 221, part, 222, true, 0, 0, 0, 0, salesChannel1.PK);

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
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			var client1 = Helper.CreateClient("1", "1");

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

			var part3 = Helper.CreateProduct(client1, "3");

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

			part3.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part3, "UNT", "PLT", 10);

			var relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part3.PK;
			relation.OU_OH = client.PK;

			var salesChannel = Helper.CreateWhsSalesChannel("SC1", "Sales Channel");

			SetupReceive(client, whs, "21", part, 211, part, 212);
			Factory.Save();

			var order21 = SetupOrders(client, whs, "21", consignee1, new ZDateTime(year, 1, 1), "TR21", "CR21", "ER21",
				transportCo1, part, 211, part3, 212, true, 0, 1, 0, 0, salesChannel.PK);
			Factory.Save();

			SetupCancelledOrder(client1, whs, "41", consignee1, part3, 100);
			return order21;
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
		}

		WhsOrder SetupOrders(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader consignee,
			ZDateTime date, ZString trasportReference, ZString customerReference, ZString externalReference,
			OrgHeader transportCo, OrgSupplierPart warehousePart1, ZDecimal units1, OrgSupplierPart warehousePart2,
			ZDecimal units2, ZBool finalize, ZDecimal w1_Units1, ZDecimal w1_Units2, ZDecimal insuranceAmount,
			ZDecimal cODAmount, ZGuid salesChannelPK)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.WD_RequiredDate = warehouse.GetWarehouseBranchLocalDateTimeOffset(date);
			order.WD_TransportReference = trasportReference;
			order.WD_CustomerReference = customerReference;
			order.WD_ExternalReference = externalReference;
			order.TransportCoPK = transportCo.PK;
			order.WD_LocalCartInsuranceCost = insuranceAmount;
			order.WD_ShipperCODAmount = cODAmount;
			order.WD_WSH_SalesChannel = salesChannelPK;

			var orderLine1 = Helper.CreateWhsOrderLine(order, warehousePart1, units1);
			orderLine1.WE_F3_NKPackType = "PLT";
			var orderLine2 = Helper.CreateWhsOrderLine(order, warehousePart2, units2);

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

		#region TestView_ReleaseCapturedAttributes

		public void TestView_ReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			data.Part1.OP_RH_NKCommodityCode = Factory.NewWithValidTestData<RefCommodityCode>().RH_Code;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 6m;

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 4m;
			releaseLine1.PartAttribute1 = "Red";
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 2m;
			releaseLine2.PartAttribute1 = "Blue";

			Factory.Save();

			// before order is finalised report should show Picked qty.
			var results1 = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should return 2 line.", 2, results1.Count);
			AssertResult(orderLine, results1, 0m, "Red", "");
			AssertResult(orderLine, results1, 0m, "Blue", "");

			orderLine.ReleaseLines.ClearCollection();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			// after job is finalised report should show Attributes met.
			var results2 = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should return 2 lines.", 2, results2.Count);
			AssertResult(orderLine, results2, 4m, "Red", "");
			AssertResult(orderLine, results2, 2m, "Blue", "");
		}

		public void TestView_ReleaseCapturedAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);
			data.Part1.OP_RH_NKCommodityCode = Factory.NewWithValidTestData<RefCommodityCode>().RH_Code;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 2m;

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			releaseLine1.PartAttribute1 = "Red";
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			releaseLine2.PartAttribute1 = "Blue";

			Factory.Save();

			// before order is finalised report should show Picked qty.
			var results1 = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should return 2 line.", 2, results1.Count);
			AssertResult(orderLine, results1, 0m, "Red", "");
			AssertResult(orderLine, results1, 0m, "Blue", "");

			releaseLine1.SerialNumber = "SS2";
			releaseLine2.SerialNumber = "GTG";

			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			// after job is finalised report should show Attributes met.
			var results2 = LoadView_WhsJobHistoryOrdersDetailReport(data.Whs1, data.Org1, data.Part1);
			AssertEquals("Should return 2 lines.", 2, results2.Count);
			AssertResult(orderLine, results2, 1m, "Red", "SS2");
			AssertResult(orderLine, results2, 1m, "Blue", "GTG");
		}

		void AssertResult(WhsOrderLine orderLine, DynamicBusinessObjectCollection results, ZDecimal expectedActualQty,
			ZString expectedColor, ZString expectedSerial)
		{
			var order = orderLine.Order;
			var result = results
				.Where(r => r["QuantityActual"].Equals(expectedActualQty) && r["PartAttrib1"].Equals(expectedColor))
				.ToArray().Single();
			AssertEquals("QuantityExpected", orderLine.WE_TransactionQuantity, result["QuantityExpected"]);
			AssertEquals("QuantityActual", expectedActualQty, result["QuantityActual"]);
			AssertEquals("Packs",
				orderLine.SupplierPart.UnitConverter.Convert(expectedActualQty,
					orderLine.SupplierPart.OP_StockKeepingUnit, orderLine.WE_F3_NKPackType), result["Packs"]);
			AssertEquals("Weight", orderLine.SupplierPart.OP_Weight * expectedActualQty, result["Weight"]);
			AssertEquals("Volume", orderLine.SupplierPart.OP_Cubic * expectedActualQty, result["Volume"]);
			AssertEquals("PartAttrib1", expectedColor, result["PartAttrib1"]);
			AssertEquals("PartAttrib1Name", order.Client.MiscServ.OM_IMPartAttrib1Name, result["PartAttrib1Name"]);
			AssertEquals("SerialNumber", expectedSerial, result["SerialNumber"]);
		}

		#endregion

		#region TestView_IncludeUnfinalisedJobs

		public void TestView_IncludeUnfinalisedJobsIsTrue()
		{
			TestView_IncludeUnfinalisedJobsCore(true);
		}

		public void TestView_IncludeUnfinalisedJobsIsFalse()
		{
			TestView_IncludeUnfinalisedJobsCore(false);
		}

		void TestView_IncludeUnfinalisedJobsCore(bool isIncludedUnfinalisedJobs)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);

			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1, order2);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(order2);
			AssertEquals("Precondition:", false, order3.IsFinalised);

			if (isIncludedUnfinalisedJobs)
			{
				var result = LoadView_WhsJobHistoryOrdersDetailReport(isIncludeUnfinalisedJob: "Y");
				AssertEquals("Should return 3 finalised orders", 3, result.Count);
				AssertContainsExactElementsInAnyOrder(new[] { order1.PK, order2.PK, order3.PK },
					result.Select(r => r["DocketPK"]));
			}
			else
			{
				var result = LoadView_WhsJobHistoryOrdersDetailReport(isIncludeUnfinalisedJob: "");
				AssertEquals("Should return 2 finalised orders", 2, result.Count);
				AssertContainsExactElementsInAnyOrder(new[] { order1.PK, order2.PK },
					result.Select(r => r["DocketPK"]));
			}
		}

		#endregion

		#region TestView_GroupOrderRows_DifferentInventory

		public void TestView_GroupOrderRows_DifferentInventory()
		{
			GroupOrderRows_DifferentInventoryCore(AttributeNumber.One, (line) => { line.WE_PartAttrib1 = "Red"; },
				(line) => { line.WE_PartAttrib1 = "Blue"; });
			GroupOrderRows_DifferentInventoryCore(AttributeNumber.Two, (line) => { line.WE_PartAttrib2 = "123"; },
				(line) => { line.WE_PartAttrib2 = "ABC"; });
			GroupOrderRows_DifferentInventoryCore(AttributeNumber.Three, (line) => { line.WE_PartAttrib3 = "B1"; },
				(line) => { line.WE_PartAttrib3 = "B2"; });
			GroupOrderRows_DifferentInventoryCore(AttributeNumber.ExpiryDate,
				(line) => { line.WE_ExpiryDate = ZDate.Today.AddDays(+5); },
				(line) => { line.WE_ExpiryDate = ZDate.Today.AddDays(+10); });
			GroupOrderRows_DifferentInventoryCore(AttributeNumber.PackingDate,
				(line) => { line.WE_PackingDate = ZDate.Today.AddDays(-1); },
				(line) => { line.WE_PackingDate = ZDate.Today.AddDays(-2); });
		}

		void GroupOrderRows_DifferentInventoryCore(AttributeNumber attribNo, Action<WhsReceiveLine> actionLine1,
			Action<WhsReceiveLine> actionLine2)
		{
			var idToMakeUniueName = (int)attribNo;
			var whs = Helper.CreateWarehouse("whs" + idToMakeUniueName, "row" + idToMakeUniueName, 3, 1);
			var client = Helper.CreateClient("client" + idToMakeUniueName);
			var part = Helper.CreateProduct(client, "product" + idToMakeUniueName);
			Helper.SetClientAttributeType(client, attribNo, true);
			Helper.SetProductAttributeUse(client, part, attribNo, true);
			part.OP_RH_NKCommodityCode = Factory.NewWithValidTestData<RefCommodityCode>().RH_Code;

			var receive = Helper.CreateWhsReceive(client, whs, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, part, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, part, 10m);
			actionLine1(receiveLine1);
			actionLine2(receiveLine2);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, "O1", part, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView_WhsJobHistoryOrdersDetailReport(whs, client, part);
			AssertEquals("Should return 2 line.", 2, results.Count);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("QuantityExpected", new decimal[] { 20m, 20m },
					results.Select(r => decimal.Parse(r["QuantityExpected"].ToString())));
				AssertContainsExactElementsInAnyOrder("QuantityActual", new decimal[] { 10m, 10m },
					results.Select(r => decimal.Parse(r["QuantityActual"].ToString())));
				AssertContainsExactElementsInAnyOrder("PartAttrib1",
					new List<string> { receiveLine1.WE_PartAttrib1, receiveLine2.WE_PartAttrib1 },
					results.Select(r => r["PartAttrib1"].ToString()));
				AssertContainsExactElementsInAnyOrder("PartAttrib2",
					new List<string> { receiveLine1.WE_PartAttrib2, receiveLine2.WE_PartAttrib2 },
					results.Select(r => r["PartAttrib2"].ToString()));
				AssertContainsExactElementsInAnyOrder("PartAttrib3",
					new List<string> { receiveLine1.WE_PartAttrib3, receiveLine2.WE_PartAttrib3 },
					results.Select(r => r["PartAttrib3"].ToString()));
				AssertContainsExactElementsInAnyOrder("ExpiryDate",
					new List<ZDateTime> { receiveLine1.WE_ExpiryDate, receiveLine2.WE_ExpiryDate },
					results.Select(r => new ZDateTime(r["ExpiryDate"].ToString())));
				AssertContainsExactElementsInAnyOrder("PackingDate",
					new List<ZDateTime> { receiveLine1.WE_PackingDate, receiveLine2.WE_PackingDate },
					results.Select(r => new ZDateTime(r["PackingDate"].ToString())));
			});
		}

		#endregion

		#region TestView_GroupOrderRows_DifferentOrder

		public void TestView_GroupOrderRows_DifferentOrder()
		{
			GroupOrderRows_DifferentOrderCore(AttributeNumber.One, (line) => { line.WE_PartAttrib1 = "Red"; },
				(line) => { line.WE_PartAttrib1 = "Blue"; });
			GroupOrderRows_DifferentOrderCore(AttributeNumber.Two, (line) => { line.WE_PartAttrib2 = "123"; },
				(line) => { line.WE_PartAttrib2 = "ABC"; });
			GroupOrderRows_DifferentOrderCore(AttributeNumber.Three, (line) => { line.WE_PartAttrib3 = "B1"; },
				(line) => { line.WE_PartAttrib3 = "B2"; });
			GroupOrderRows_DifferentOrderCore(AttributeNumber.ExpiryDate,
				(line) => { line.WE_ExpiryDate = ZDate.Today.AddDays(+5); },
				(line) => { line.WE_ExpiryDate = ZDate.Today.AddDays(+10); });
			GroupOrderRows_DifferentOrderCore(AttributeNumber.PackingDate,
				(line) => { line.WE_PackingDate = ZDate.Today.AddDays(-1); },
				(line) => { line.WE_PackingDate = ZDate.Today.AddDays(-2); });
		}

		void GroupOrderRows_DifferentOrderCore(AttributeNumber attribNo, Action<WhsDocketLine> actionLine1,
			Action<WhsDocketLine> actionLine2)
		{
			var idToMakeUniueName = (int)attribNo;
			var whs = Helper.CreateWarehouse("whs" + idToMakeUniueName, "row" + idToMakeUniueName, 3, 1);
			var client = Helper.CreateClient("client" + idToMakeUniueName);
			var part = Helper.CreateProduct(client, "product" + idToMakeUniueName);
			Helper.SetClientAttributeType(client, attribNo, true);
			Helper.SetProductAttributeUse(client, part, attribNo, true);
			part.OP_RH_NKCommodityCode = Factory.NewWithValidTestData<RefCommodityCode>().RH_Code;

			var receive = Helper.CreateWhsReceive(client, whs, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, part, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, part, 10m);
			actionLine1(receiveLine1);
			actionLine2(receiveLine2);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs, "Order1" + idToMakeUniueName);
			var orderLine1 = Helper.CreateWhsOrderLine(order, part, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 1m);
			actionLine1(orderLine1);
			actionLine2(orderLine2);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView_WhsJobHistoryOrdersDetailReport(whs, client, part);
			AssertEquals("Should return 2 line.", 2, results.Count);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("QuantityExpected", new decimal[] { 2m, 2m },
					results.Select(r => decimal.Parse(r["QuantityExpected"].ToString())));
				AssertContainsExactElementsInAnyOrder("QuantityActual", new decimal[] { 1m, 1m },
					results.Select(r => decimal.Parse(r["QuantityActual"].ToString())));
				AssertContainsExactElementsInAnyOrder("PartAttrib1",
					new List<string> { receiveLine1.WE_PartAttrib1, receiveLine2.WE_PartAttrib1 },
					results.Select(r => r["PartAttrib1"].ToString()));
				AssertContainsExactElementsInAnyOrder("PartAttrib2",
					new List<string> { receiveLine1.WE_PartAttrib2, receiveLine2.WE_PartAttrib2 },
					results.Select(r => r["PartAttrib2"].ToString()));
				AssertContainsExactElementsInAnyOrder("PartAttrib3",
					new List<string> { receiveLine1.WE_PartAttrib3, receiveLine2.WE_PartAttrib3 },
					results.Select(r => r["PartAttrib3"].ToString()));
				AssertContainsExactElementsInAnyOrder("ExpiryDate",
					new List<ZDateTime> { receiveLine1.WE_ExpiryDate, receiveLine2.WE_ExpiryDate },
					results.Select(r => new ZDateTime(r["ExpiryDate"].ToString())));
				AssertContainsExactElementsInAnyOrder("PackingDate",
					new List<ZDateTime> { receiveLine1.WE_PackingDate, receiveLine2.WE_PackingDate },
					results.Select(r => new ZDateTime(r["PackingDate"].ToString())));
			});

			var orderWithOneLine = Helper.CreateWhsOrder(client, whs, "Order2" + idToMakeUniueName);
			var orderLineWithOneLine = Helper.CreateWhsOrderLine(orderWithOneLine, part, 5m);
			actionLine1(orderLineWithOneLine);
			var pick2 = Helper.CreatePickNew(orderWithOneLine);
			orderWithOneLine.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(orderWithOneLine);
			AssertIsFinalisedPrecondition(pick2);
			Factory.Save();
			var singleResult = LoadView_WhsJobHistoryOrdersDetailReport(whs, client, part)
				.Where(r => r["DocketPK"].ToString() == orderWithOneLine.PK.ToString()).Single();
			CombineAssertions(() =>
			{
				AssertEquals("QuantityExpected", 5m, singleResult["QuantityExpected"]);
				AssertEquals("QuantityActual", 5m, singleResult["QuantityActual"]);
				AssertEquals("PartAttrib1", receiveLine1.WE_PartAttrib1, singleResult["PartAttrib1"]);
				AssertEquals("PartAttrib2", receiveLine1.WE_PartAttrib2, singleResult["PartAttrib2"]);
				AssertEquals("PartAttrib3", receiveLine1.WE_PartAttrib3, singleResult["PartAttrib3"]);
			});
		}

		#endregion

		#region TestView_GroupOrderRows_DifferentReleaseLine

		public void TestView_GroupOrderRows_DifferentReleaseLine()
		{
			View_GroupOrderRows_DifferentReleaseLineCore(AttributeNumber.One,
				(line) => { line.PartAttribute1 = "Red"; }, (line) => { line.PartAttribute1 = "Blue"; });
			View_GroupOrderRows_DifferentReleaseLineCore(AttributeNumber.Two,
				(line) => { line.PartAttribute2 = "123"; }, (line) => { line.PartAttribute2 = "ABC"; });
			View_GroupOrderRows_DifferentReleaseLineCore(AttributeNumber.Three,
				(line) => { line.PartAttribute3 = "B1"; }, (line) => { line.PartAttribute3 = "B2"; });
		}

		void View_GroupOrderRows_DifferentReleaseLineCore(AttributeNumber attribNo, Action<WhsReleaseLine> actionLine1,
			Action<WhsReleaseLine> actionLine2)
		{
			var idToMakeUniueName = (int)attribNo;
			var whs = Helper.CreateWarehouse("whs" + idToMakeUniueName, "row" + idToMakeUniueName, 3, 1);
			var client = Helper.CreateClient("client" + idToMakeUniueName);
			var part = Helper.CreateProduct(client, "product" + idToMakeUniueName);
			Helper.SetClientAttributeType(client, attribNo, true);
			Helper.SetProductAttributeUse(client, part, attribNo, true, true);
			part.OP_RH_NKCommodityCode = Factory.NewWithValidTestData<RefCommodityCode>().RH_Code;

			var receive = Helper.CreateWhsReceive(client, whs, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, part, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, part, 5m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			actionLine1(releaseLine1);
			releaseLine1.Quantity = 2m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			actionLine2(releaseLine2);
			releaseLine2.Quantity = 3m;

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var results = LoadView_WhsJobHistoryOrdersDetailReport(whs, client, part);
			AssertEquals("Should return 2 line.", 2, results.Count);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("QuantityExpected", new decimal[] { 5m, 5m },
					results.Select(r => decimal.Parse(r["QuantityExpected"].ToString())));
				AssertContainsExactElementsInAnyOrder("QuantityActual", new decimal[] { 2m, 3m },
					results.Select(r => decimal.Parse(r["QuantityActual"].ToString())));
				AssertContainsExactElementsInAnyOrder("PartAttrib1",
					new List<string> { releaseLine1.PartAttribute1, releaseLine2.PartAttribute1 },
					results.Select(r => r["PartAttrib1"].ToString()));
				AssertContainsExactElementsInAnyOrder("PartAttrib2",
					new List<string> { releaseLine1.PartAttribute2, releaseLine2.PartAttribute2 },
					results.Select(r => r["PartAttrib2"].ToString()));
				AssertContainsExactElementsInAnyOrder("PartAttrib3",
					new List<string> { releaseLine1.PartAttribute3, releaseLine2.PartAttribute3 },
					results.Select(r => r["PartAttrib3"].ToString()));
			});
		}

		#endregion

		#region Implementation

		#region LoadView

		DynamicBusinessObjectCollection LoadView_WhsJobHistoryOrdersDetailReport(ZGuid? whsPK = null,
			ZGuid? clientPK = null, ZGuid? productPK = null, ZGuid? commodityPK = null, ZGuid? categoryPK = null,
			ZGuid? consigneePK = null, ZString? serviceLevel = null
			, ZString? partAttrib1 = null, ZString? partAttrib2 = null, ZString? partAttrib3 = null,
			ZDateTime? expiryDate = null, ZDateTime? packingDate = null, ZDateTimeOffset? fromDocketDate = null,
			ZDateTimeOffset? toDocketDate = null, string isIncludeUnfinalisedJob = "Y",
			ZString? serialNumber = null, string orderBy = "Reference, QuantityActual",
			ZGuid? distributionCentrePK = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			//When DocEngine process macros, any invalid ZType is replaced with an empty string
			var sql = $@"
			select
				*
			from
				WhsJobHistoryOrdersDetailReport
						( {(!whsPK.HasValue ? "null" : $"'{whsPK}'")}
						, {(!clientPK.HasValue ? "null" : $"'{clientPK}'")}
						, {(!productPK.HasValue ? "null" : $"'{productPK}'")}
						, {(!commodityPK.HasValue ? "null" : $"'{commodityPK}'")}
						, {(!categoryPK.HasValue ? "null" : $"'{categoryPK}'")}
						, {(!consigneePK.HasValue ? "null" : $"'{consigneePK}'")}
						, {(!distributionCentrePK.HasValue ? "null" : $"'{distributionCentrePK}'")}
						, {(!serviceLevel.HasValue ? "null" : $"'{serviceLevel}'")}
						, {(!partAttrib1.HasValue ? "null" : $"'{partAttrib1}'")}
						, {(!partAttrib2.HasValue ? "null" : $"'{partAttrib2}'")}
						, {(!partAttrib3.HasValue ? "null" : $"'{partAttrib3}'")}
						, {(!serialNumber.HasValue ? "null" : $"'{serialNumber}'")}
						, {(!expiryDate.HasValue ? "null" : $"'{expiryDate.Value.ToShortDateString()}'")}
						, {(!packingDate.HasValue ? "null" : $"'{packingDate.Value.ToShortDateString()}'")}
						, {(!fromDocketDate.HasValue ? "null" : $"'{fromDocketDate.Value.ToString("dd-MMM-yyyy HH:mm:ss zzz")}'")}
						, {(!toDocketDate.HasValue ? "null" : $"'{toDocketDate.Value.ToString("dd-MMM-yyyy HH:mm:ss zzz")}'")}
						, {(string.IsNullOrEmpty(isIncludeUnfinalisedJob) ? "null" : $"'{isIncludeUnfinalisedJob}'")}
			)
			order by {orderBy} asc";

			result.Load(sql);
			return result;
		}

		DynamicBusinessObjectCollection LoadView_WhsJobHistoryOrdersDetailReport(WhsWarehouse whs, OrgHeader client,
			OrgSupplierPart part)
		{
			var commodity =
				Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code,
					part.OP_RH_NKCommodityCode));
			return LoadView_WhsJobHistoryOrdersDetailReport(whsPK: whs.PK, clientPK: client.PK, productPK: part.PK,
				commodityPK: commodity?.PK);
		}

		#endregion

		#endregion
	}
}
