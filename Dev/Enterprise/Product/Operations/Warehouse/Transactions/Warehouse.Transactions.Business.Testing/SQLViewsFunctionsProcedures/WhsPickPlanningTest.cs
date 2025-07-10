using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickPlanningTest : WhsTestCaseWithFactory
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

			var result1 = LoadView_ProductCategory(categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_ProductCategory(categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_ProductCategory(categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_ProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_ProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = categoryPK.IsEmpty
				? @"select * from WhsPickPlanningReport(null)"
				: @"select * from WhsPickPlanningReport(@ProductCategoryPK)";
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

		[TestDate(2024, 08, 28, 12, 55, 0)]
		public void TestView()
		{
			var orders = SetupData();
			var results = LoadView();

			AssertEquals(6, results.Count);
			AssertResult(orders[0].Lines[0], results[0]);
			AssertResult(orders[0].Lines[1], results[1]);
			AssertResult(orders[1].Lines[0], results[2]);
			AssertResult(orders[1].Lines[1], results[3]);
			AssertResult(orders[2].Lines[0], results[4]);
			AssertResult(orders[2].Lines[1], results[5]);
		}

		void AssertResult(WhsOrderLine line, DynamicBusinessObject result)
		{
			var order = line.Order;
			var warehouse = order.Warehouse;
			var client = order.Client;
			var part = line.SupplierPart;
			var transportCoDocAddress = order.TransportCoDocAddress;

			AssertEquals("WarehousePK", warehouse.PK, result["WarehousePK"]);
			AssertEquals("WarehouseName", warehouse.WW_WarehouseName, result["WarehouseName"]);
			AssertEquals("ClientPK", client.PK, result["ClientPK"]);
			AssertEquals("ClientCode", client.OH_Code, result["ClientCode"]);
			AssertEquals("Client", client.OH_FullName, result["Client"]);

			if (order.ConsigneeDocAddress.E2_AddressOverride)
			{
				AssertEquals("ConsigneePK", ZGuid.Empty, result["ConsigneePK"]);
				AssertEquals("Consignee", order.ConsigneeDocAddress.E2_CompanyName, result["Consignee"]);
			}
			else
			{
				AssertEquals("ConsigneePK", order.Consignee.PK, result["ConsigneePK"]);
				AssertEquals("Consignee", order.Consignee.OH_FullName, result["Consignee"]);
			}

			if (transportCoDocAddress.E2_AddressOverride)
			{
				AssertEquals("TransportCoPK", ZGuid.Empty, result["TransportCoPK"]);
				AssertEquals("TransportCo", transportCoDocAddress.E2_CompanyName, result["TransportCo"]);
			}
			else if (transportCoDocAddress.Organisation != null)
			{
				AssertEquals("TransportCoPK", order.TransportCoPK, result["TransportCoPK"]);
				AssertEquals("TransportCo", transportCoDocAddress.Organisation.OH_FullName, result["TransportCo"]);
			}

			AssertEquals("DocketPK", order.PK, result["DocketPK"]);
			AssertEquals("OrderReferenceNo", order.WD_ExternalReference, result["OrderReferenceNo"]);
			AssertEquals("RequireDate", order.WD_RequiredDate, result["RequiredDate"]);
			AssertEquals("ProductPK", part.PK, result["ProductPK"]);
			AssertEquals("ProductCode", part.OP_PartNum, result["ProductCode"]);
			AssertEquals("ProductDescription", part.OP_Desc, result["ProductDescription"]);
			AssertEquals("PartAttribute1", line.WE_PartAttrib1, result["PartAttrib1"]);
			AssertEquals("PartAttribute2", line.WE_PartAttrib2, result["PartAttrib2"]);
			AssertEquals("PartAttribute3", line.WE_PartAttrib3, result["PartAttrib3"]);
			AssertEquals("SerialNumber", line.WE_SerialNumber, result["SerialNumber"]);
			AssertEquals("ExpiryDate", line.WE_ExpiryDate, result["ExpiryDate"]);
			AssertEquals("PackingDate", line.WE_PackingDate, result["PackingDate"]);
			AssertEquals("Units", line.WE_TransactionQuantity, result["Units"]);
			AssertEquals("UnitsUQ", line.ProductUQ, result["UnitsUQ"]);
			AssertEquals("Weight", part.OP_Weight * line.WE_TransactionQuantity, result["Weight"]);
			AssertEquals("WeightUQ", part.OP_WeightUQ, result["WeightUQ"]);
			AssertEquals("Cubic", part.OP_Cubic * line.WE_TransactionQuantity, result["Cubic"]);
			AssertEquals("CubicUQ", part.OP_CubicUQ, result["CubicUQ"]);
			AssertEquals("CarrierServiceLevel", order.WD_PL_NKCarrierServiceLevel, result["CarrierServiceLevel"]);
			AssertEquals("ServiceLevel", order.WD_RS_NKServiceLevel, result["ServiceLevel"]);
			AssertEquals("CrossDockLocation", order.CrossDockLocation?.WLV_LocationString_UserFriendly ?? string.Empty,
				result["CrossDockLocation"]);

			var partRelation =
				part.RelatedOrganisations.FindByOrganisationAndRelationship(line.Docket.Client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation != null ? partRelation.Category : null;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			AssertEquals("ProductCategoryCode", expectedCategoryCode, result["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
				result["ProductCategoryDescription"]);
		}

		[TestDate(2024, 08, 28, 12, 55, 0)]
		public void TestView_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			CreateReceiveLine("SN1");
			CreateReceiveLine("SN2");
			CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN2";
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "SN33";
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should return 2 results", 2, results.Count);
			AssertResult(orderLine2, results[0]);
			AssertResult(orderLine3, results[1]);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_WithProductCategoryFilter

		public void TestView_FixedWidthLocationWarehouse()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZZ", 3, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "LOCZ", 4, 3, 2);
			var client = Helper.CreateClient("C1");
			Factory.Save();

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productCoke = Helper.CreateProduct(client, "Coke");
			var cokeRelationship = productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var vbRelationship = productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O1", productVB, 10m);
			order1.WD_WL_CrossDock = warehouse.FindLocation("LOCZ0010101").PK;
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O2", productCoke, 20m);
			order2.WD_WL_CrossDock = warehouse.FindLocation("LOCZ0010102").PK;
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "O3", productTea, 5m);
			order3.WD_WL_CrossDock = warehouse.FindLocation("LOCZ0010201").PK;

			Factory.Save();

			var results = LoadView_ProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, results.Count);
			AssertNotNull(results.FirstOrDefault(result => result["CrossDockLocation"].ToString() == "LOCZ-001-01-01"));
			AssertNotNull(results.FirstOrDefault(result => result["CrossDockLocation"].ToString() == "LOCZ-001-01-02"));
			AssertNotNull(results.FirstOrDefault(result => result["CrossDockLocation"].ToString() == "LOCZ-001-02-01"));
		}

		#endregion

		#region TestView_DistributionCentre

		[TestDate(2024, 08, 28, 12, 55, 0)]
		public void TestView_DistributionCentre()
		{
			var distributionCentre = Helper.CreateClient("C3");

			var orders = SetupData();
			orders[0].DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			Factory.Save();

			var results = LoadView();
			var orderLinesWithDcAddress = results.Where(o => (CargoWise.Types.ZGuid)o["DistributionCentrePK"] == distributionCentre.PK).ToArray();

			AssertEquals("Should filter by distribution centre.", 2, orderLinesWithDcAddress.Length);

			for (int i = 0; i < orderLinesWithDcAddress.Length; i++)
			{
				var businessObject = orderLinesWithDcAddress[i];
				AssertEquals(orders[0].PK, businessObject["DocketPK"]);
				AssertEquals(distributionCentre.PK, businessObject["DistributionCentrePK"]);
				AssertEquals(distributionCentre.OH_FullName, businessObject["DistributionCentreCoName"]);
			}
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"
select
	*
from
	WhsPickPlanningReport(null)
order by
	OrderReferenceNo,
	ProductCode asc,
	SerialNumber";

			result.Load(sql);
			return result;
		}

		#endregion

		#region SetupData

		WhsOrder[] SetupData()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			var warehouse2 = Helper.CreateWarehouse("WHS2", "Dock2", 2, 2);
			Factory.Save();

			var warehouse1DockDoorLocation = warehouse1.FindLocation(WhsWarehouse.DefaultDockDoorRowName);
			var warehouse2DockDoorLocation = warehouse2.FindLocation("Dock2-2-2");

			var client1 = Helper.CreateClient("CLT1");
			var client2 = Helper.CreateClient("CLT2");

			var consignee1 = Helper.CreateClient("CON1");
			consignee1.OH_FullName = "Consignee1";
			var consignee2 = Helper.CreateClient("CON2");
			consignee2.OH_FullName = "Consignee2";

			var transportCo1 = Helper.CreateClient("TRA1");
			transportCo1.OH_FullName = "TransportCo1";
			var serviceLevel1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("TRA2");
			transportCo2.OH_FullName = "TransportCo2";
			var serviceLevel2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "SL2";

			var transportCo3 = Helper.CreateClient("TRA3");
			transportCo3.OH_FullName = "TransportCo3";
			var serviceLevel3 = transportCo3.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel3.PL_Code = "SL3";

			var part1 = Helper.CreateProduct(client1, "Part1");
			var category1 = Helper.CreateProductCategory(client1, part1, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";
			part1.OP_StockKeepingUnit = "UNT";
			SetupPartClientRelation(part1, client1);

			var part2 = Helper.CreateProduct(client1, "Part2");
			part2.OP_StockKeepingUnit = "KG";
			SetupPartClientRelation(part2, client1);

			var part3 = Helper.CreateProduct(client2, "Part3");
			part3.OP_StockKeepingUnit = "CTN";
			SetupPartClientRelation(part3, client2);

			var part4 = Helper.CreateProduct(client2, "Part4");
			part4.OP_StockKeepingUnit = "PLT";
			SetupPartClientRelation(part4, client2);

			var order1 = Helper.CreateWhsOrder(client1, warehouse1, "REF1");
			order1.WD_WL_CrossDock = warehouse1DockDoorLocation.PK;

			var line1 = Helper.CreateWhsOrderLine(order1, part1, 10m);
			line1.WE_ExpiryDate = new ZDate(2008, 06, 22);
			line1.WE_PackingDate = new ZDate(2008, 06, 15);
			line1.WE_PartAttrib1 = "PA111";
			line1.WE_PartAttrib2 = "PA112";
			line1.WE_PartAttrib3 = "PA113";

			var line2 = Helper.CreateWhsOrderLine(order1, part2, 11m);
			line2.WE_ExpiryDate = new ZDate(2008, 06, 23);
			line2.WE_PackingDate = new ZDate(2008, 06, 16);
			line2.WE_PartAttrib1 = "PA121";
			line2.WE_PartAttrib2 = "PA122";
			line2.WE_PartAttrib3 = "PA123";

			order1.ConsigneePK = consignee1.PK;
			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order1.WD_RequiredDate = new DateTime(2008, 06, 12);

			var order2 = Helper.CreateWhsOrder(client1, warehouse2, "REF2");
			order2.WD_WL_CrossDock = warehouse2DockDoorLocation.PK;

			var line3 = Helper.CreateWhsOrderLine(order2, part1, 20m);
			line3.WE_ExpiryDate = new ZDate(2008, 05, 22);
			line3.WE_PackingDate = new ZDate(2008, 05, 15);
			line3.WE_PartAttrib1 = "PA211";
			line3.WE_PartAttrib2 = "PA212";
			line3.WE_PartAttrib3 = "PA213";

			var line4 = Helper.CreateWhsOrderLine(order2, part2, 21m);
			line4.WE_ExpiryDate = new ZDate(2008, 05, 23);
			line4.WE_PackingDate = new ZDate(2008, 05, 16);
			line4.WE_PartAttrib1 = "PA221";
			line4.WE_PartAttrib2 = "PA222";
			line4.WE_PartAttrib3 = "PA223";

			order2.ConsigneePK = consignee2.PK;
			order2.TransportCoPK = transportCo2.PK;
			order2.WD_PL_NKCarrierServiceLevel = "SL2";
			order2.WD_RS_NKServiceLevel = "TST";
			order2.WD_RequiredDate = new DateTime(2008, 06, 13);

			var order3 = Helper.CreateWhsOrder(client2, warehouse1, "REF3");
			order3.WD_WL_CrossDock = warehouse1DockDoorLocation.PK;

			var line5 = Helper.CreateWhsOrderLine(order3, part3, 30m);
			line5.WE_ExpiryDate = new ZDate(2008, 07, 22);
			line5.WE_PackingDate = new ZDate(2008, 07, 15);
			line5.WE_PartAttrib1 = "PA311";
			line5.WE_PartAttrib2 = "PA312";
			line5.WE_PartAttrib3 = "PA313";

			var line6 = Helper.CreateWhsOrderLine(order3, part4, 31m);
			line6.WE_ExpiryDate = new ZDate(2008, 07, 23);
			line6.WE_PackingDate = new ZDate(2008, 07, 16);
			line6.WE_PartAttrib1 = "PA321";
			line6.WE_PartAttrib2 = "PA322";
			line6.WE_PartAttrib3 = "PA323";

			order3.ConsigneePK = consignee1.PK;
			order3.TransportCoPK = transportCo1.PK;
			order3.WD_PL_NKCarrierServiceLevel = "SL3";
			order3.WD_RequiredDate = new DateTime(2008, 06, 14);

			var orderCancelled = Helper.CreateWhsOrder(client2, warehouse2);
			orderCancelled.WD_ExternalReference = "CANCELLED";
			Helper.CreateWhsOrderLine(orderCancelled, part3, 40m);
			Helper.CreateWhsOrderLine(orderCancelled, part4, 41m);
			orderCancelled.CancelReactivateDocket();

			var orderAttachedToPick = Helper.CreateWhsOrder(client1, warehouse1);
			orderAttachedToPick.WD_ExternalReference = "ATTACHEDTOPICK";
			Helper.CreateWhsOrderLine(orderAttachedToPick, part1, 50m);
			Helper.CreateWhsOrderLine(orderAttachedToPick, part2, 51m);
			Helper.CreatePickNew(orderAttachedToPick);
			orderAttachedToPick.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;

			var orderHeld = Helper.CreateWhsOrder(client1, warehouse2);
			orderHeld.WD_ExternalReference = "HELD";
			Helper.CreateWhsOrderLine(orderHeld, part1, 60m);
			Helper.CreateWhsOrderLine(orderHeld, part2, 61m);
			orderHeld.WD_DocketStatus = DocketStatus.Codes.Held;

			var orderError = Helper.CreateWhsOrder(client2, warehouse1);
			orderError.WD_ExternalReference = "ERROR";
			Helper.CreateWhsOrderLine(orderError, part3, 70m);
			Helper.CreateWhsOrderLine(orderError, part4, 71m);
			orderError.WD_DocketStatus = DocketStatus.Codes.Error;

			var orderFinalized = Helper.CreateWhsOrder(client1, warehouse1);
			orderFinalized.WD_ExternalReference = "FINALIZED";
			Helper.CreateWhsOrderLine(orderFinalized, part1, 80m);
			Helper.CreateWhsOrderLine(orderFinalized, part2, 81m);
			Helper.CreatePickNew(orderFinalized);
			orderFinalized.FinaliseDocket();

			order1.ConsigneeDocAddress.E2_AddressOverride = ZBool.True;
			order1.ConsigneeNameOrPK = "Overriden Consignee Company name";

			order2.TransportCoDocAddress.E2_AddressOverride = ZBool.True;
			order2.TransportCoNameOrPK = "Overriden TransportCo Company name";

			Factory.Save();

			AssertEquals(DocketStatus.Codes.Entered, order1.WD_DocketStatus);
			AssertEquals(DocketStatus.Codes.Entered, order2.WD_DocketStatus);
			AssertEquals(DocketStatus.Codes.Entered, order3.WD_DocketStatus);
			AssertEquals(true, orderCancelled.IsCancelled);
			AssertEquals(true, orderAttachedToPick.IsAttachedToPickButNotFinalised);
			AssertEquals(DocketStatus.Codes.Held, orderHeld.WD_DocketStatus);
			AssertEquals(true, orderError.IsJobInError);
			AssertEquals(true, orderFinalized.IsFinalised);

			return new[] { order1, order2, order3 };
		}

		void SetupPartClientRelation(OrgSupplierPart part, OrgHeader client)
		{
			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_UseSerialNumber = true;
			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = true;
		}

		#endregion
	}
}
