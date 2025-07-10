using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsKPIReleaseTest : WhsTestCaseWithFactory
	{
		#region TestView_ReleaseCapturedAttribs

		[TestDate(2016, 10, 25)]
		public void TestView_ReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

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
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute1 = "BLUE";
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);
			AssertEquals("Should return the Order, BATCH123 is picked.", 2, results.Count);

			var result1 = results.Single(r => (ZString)r["PartAttrib1"] == "RED");
			var result2 = results.Single(r => (ZString)r["PartAttrib1"] == "BLUE");
			AssertResult(orderLine, result1, "", "RED", "BATCH123", "");
			AssertResult(orderLine, result2, "", "BLUE", "BATCH123", "");
		}

		[TestDate(2016, 10, 25)]
		public void TestView_ReleaseCapturedAttribs_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true,
				setReleaseCaptured: true);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty,
				"", "BATCH123", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.SerialNumber = "PSER";

			var releaseLine2 = orderLine2.ReleaseLines[0];
			releaseLine2.Quantity = 1m;
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.SerialNumber = "DERS";
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);
			AssertEquals("Should return the Order, BATCH123 is picked.", 2, results.Count);

			var result1 = results.Single(r => (ZString)r["PartAttrib1"] == "RED");
			var result2 = results.Single(r => (ZString)r["PartAttrib1"] == "BLUE");
			AssertResult(orderLine1, result1, "", "RED", "BATCH123", "PSER");
			AssertResult(orderLine2, result2, "", "BLUE", "BATCH123", "DERS");
		}

		#endregion

		#region TestView_SerialNumber

		[TestDate(2016, 10, 25)]
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
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN2";
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "SN33";
			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);
			AssertEquals("Should return 3 results", 3, results.Count);

			var result1 = results.Single(r => (ZString)r["SerialNumber"] == "SN1");
			var result2 = results.Single(r => (ZString)r["SerialNumber"] == "SN2");
			var result3 = results.Single(r => (ZString)r["SerialNumber"] == "SN33");
			AssertResult(orderLine1, result1, "", "", "", "SN1");
			AssertResult(orderLine2, result2, "", "", "", "SN2");
			AssertResult(orderLine3, result3, "", "", "", "SN33");

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_DistributionCentre

		[TestDate(2016, 10, 25)]
		public void TestView_DistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var distributionCentre = Helper.CreateClient("C3");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");

			Factory.Save();

			var results = LoadView(data.Whs1, data.Org1);
			var resultsWithDcAddress = results.Where(o => (CargoWise.Types.ZGuid)o["DistributionCentrePK"] == distributionCentre.PK).ToArray();

			AssertEquals("Should filter by distribution centre.", 1, resultsWithDcAddress.Length);

			var businessObject = resultsWithDcAddress[0];

			AssertEquals(order.PK, businessObject["DocketPK"]);
			AssertEquals(distributionCentre.PK, businessObject["DistributionCentrePK"]);
			AssertEquals(distributionCentre.OH_FullName, businessObject["DistributionCentreCoName"]);
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
				? @"select * from WhsKPIReleaseReport(null)"
				: @"select * from WhsKPIReleaseReport(@ProductCategoryPK)";
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

		[TestDate(2016, 1, 1)]
		public void TestView()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var client = Helper.CreateClient("1", "1");
			var orders = SetupData(whs, client);
			var results = LoadView(whs, client);

			AssertEquals(7, results.Count);
			AssertResult(orders[0].Lines[0], results[0], "SI11");
			AssertResult(orders[0].Lines[1], results[1], "SI11");
			AssertResult(orders[0].Lines[2], results[2], "SI11");
			AssertResult(orders[1].Lines[0], results[3], "");
			AssertResult(orders[1].Lines[1], results[4], "");
			AssertResult(orders[4].Lines[0], results[5], "SI31");
			AssertResult(orders[4].Lines[1], results[6], "SI31");
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
			AssertNotNull("Precondition - divot created.", orderLine.ReserveStockIfAbleTo(inventory));

			Factory.Save();

			var results1 = LoadView(data.Whs1, data.Org1);
			AssertEquals(1, results1.Count);
			AssertEquals("Should not get Attribute information from reserved stock.", "", results1[0]["PartAttrib1"]);

			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			Factory.Save();
			var results2 = LoadView(data.Whs1, data.Org1);
			AssertEquals(1, results2.Count);
			AssertEquals("Should not get Attribute information from reserved stock.", "", results2[0]["PartAttrib1"]);
		}

		#endregion

		#region TestSpecialInstructions

		public void TestSpecialInstructions()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("CLIENT");
			var consignee = Helper.CreateClient("CONSIGNEE");
			var transportCo = Helper.CreateClient("TRANSPORTCO");
			client.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description,
				"Client Special Instruction");
			client.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description,
				"Client Handling Instruction");
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description,
				"Consignee Special Instruction");
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description,
				"Consignee Handling Instruction");
			transportCo.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description,
				"TransportCo Special Instruction");
			transportCo.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description,
				"TransportCo Handling Instruction");

			var order = Helper.CreateWhsOrder(client, whs);
			order.ConsigneePK = consignee.PK;
			order.TransportCoPK = transportCo.PK;
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description,
				"Order Goods Handling Instruction");

			Factory.Save();
			var results = LoadView(whs, client);

			AssertEquals("Precondition - ensure that order was loaded.", 1, results.Count);

			var actualSpecialInstructions = (ZString)results[0]["SpecialInstructions"];
			AssertEquals("Handling Instruction from Job should be concatenated.", true,
				actualSpecialInstructions.Contains("Order Goods Handling Instruction"));
			AssertEquals("Special Instruction from Client should be concatenated.", true,
				actualSpecialInstructions.Contains("Client Special Instruction"));
			AssertEquals("Handling Instruction from Client should be concatenated.", true,
				actualSpecialInstructions.Contains("Client Handling Instruction"));
			AssertEquals("Special Instruction from Consignee should be concatenated.", true,
				actualSpecialInstructions.Contains("Consignee Special Instruction"));
			AssertEquals("Handling Instruction from Consignee should be concatenated.", true,
				actualSpecialInstructions.Contains("Consignee Handling Instruction"));
			AssertEquals("Special Instruction from TransportCo should be concatenated.", true,
				actualSpecialInstructions.Contains("TransportCo Special Instruction"));
			AssertEquals("Handling Instruction from TransportCo should be concatenated.", true,
				actualSpecialInstructions.Contains("TransportCo Handling Instruction"));
		}

		#endregion

		#region AssertResult

		void AssertResult(WhsOrderLine orderLine, DynamicBusinessObject dObject, ZString handlingInstructions)
		{
			var order = orderLine.Order;
			var releaseLine = order.IsFinalised ? orderLine.ReleaseLines[0] : null;

			var partAttrib1 = order.IsFinalised ? releaseLine.PartAttribute1 : orderLine.WE_PartAttrib1;
			var partAttrib2 = order.IsFinalised ? releaseLine.PartAttribute2 : orderLine.WE_PartAttrib2;
			var serialNumber = order.IsFinalised ? releaseLine.SerialNumber : orderLine.WE_SerialNumber;

			AssertResult(orderLine, dObject, handlingInstructions, partAttrib1, partAttrib2, serialNumber);
		}

		void AssertResult(WhsOrderLine orderLine, DynamicBusinessObject dObject, ZString handlingInstructions,
			ZString partAttribute1, ZString partAttribute2, ZString serialNumber)
		{
			var order = orderLine.Order;
			var warehouse = order.Warehouse;
			var client = order.Client;
			var address = order.TransportCoDocAddress;
			AssertEquals("WarehousePK", warehouse.PK, dObject["WarehousePK"]);
			AssertEquals("WarehouseName", warehouse.WW_WarehouseName, dObject["WarehouseName"]);
			AssertEquals("ClientPK", client.PK, dObject["ClientPK"]);
			AssertEquals("ClientCode", client.OH_Code, dObject["ClientCode"]);
			AssertEquals("Client", client.OH_FullName, dObject["Client"]);

			if (order.ConsigneeDocAddress.E2_AddressOverride)
			{
				AssertEquals("ConsigneePK", ZGuid.Empty, dObject["ConsigneePK"]);
				AssertEquals("Consignee", order.ConsigneeDocAddress.E2_CompanyName, dObject["Consignee"]);
			}
			else
			{
				AssertEquals("ConsigneePK", order.Consignee.PK, dObject["ConsigneePK"]);
				AssertEquals("Consignee", order.Consignee.OH_FullName, dObject["Consignee"]);
			}

			if (address.E2_AddressOverride)
			{
				AssertEquals("TransportCo", address.E2_CompanyName, dObject["TransportCo"]);
			}
			else if (address.Organisation != null)
			{
				AssertEquals("TransportCo", address.Organisation.OH_FullName, dObject["TransportCo"]);
			}

			AssertEquals("DocketNo", order.WD_DocketID, dObject["DocketNo"]);
			AssertEquals("OrderReferenceNo", order.WD_ExternalReference, dObject["OrderReferenceNo"]);
			AssertEquals("CustomerOrderNo", order.WD_CustomerReference, dObject["CustomerReferenceNo"]);
			AssertEquals("TransportReferenceNo", order.WD_TransportReference, dObject["ConnoteNo"]);
			AssertEquals("ServiceLevel", order.WD_RS_NKServiceLevel, dObject["ServiceLevel"]);
			AssertEquals("BookingDate", order.WD_BookingDate.Date, ((ZDateTimeOffset)dObject["BookingDate"]).Date);
			AssertEquals("RequiredDate", order.WD_RequiredDate.Date, ((ZDateTime)dObject["RequiredDate"]).Date);
			AssertEquals("FinalisedDate", order.WD_FinalisedDate.Date, ((ZDateTime)dObject["FinalisedDate"]).Date);
			TimeSpan correctDays;
			if (order.WD_FinalisedDate.IsEmpty)
			{
				var requiredDate = order.WD_RequiredDate.Date;

				// the SQL this test invokes uses SELECT GETDATE() so we must do the same to ensure equality.
				var today = ((DateTime)TestConnection.ExecuteScalar("SELECT GETDATE()")).Date;
				var zToday = new ZDateTime(today);
				correctDays = zToday - requiredDate;

				AssertEquals("RangeDate", requiredDate, ((ZDateTimeOffset)dObject["RangeDate"]).Date);
			}
			else
			{
				AssertEquals("RangeDate", order.WD_FinalisedDate.Date, ((ZDateTimeOffset)dObject["RangeDate"]).Date);
				correctDays = order.WD_FinalisedDate.Date - order.WD_RequiredDate.Date;
			}

			if (order.IsFinalised)
			{
				var releaseLine = orderLine.ReleaseLines[0];
				AssertEquals("ExpiryDate", releaseLine.ExpiryDate, ((ZDateTime)dObject["ExpiryDate"]).Date);
				AssertEquals("PackingDate", releaseLine.PackingDate, ((ZDateTime)dObject["PackingDate"]).Date);
				AssertEquals("PartAttrib3", releaseLine.PartAttribute3, dObject["PartAttrib3"]);
			}
			else
			{
				AssertEquals("ExpiryDate", orderLine.WE_ExpiryDate, ((ZDateTime)dObject["ExpiryDate"]).Date);
				AssertEquals("PackingDate", orderLine.WE_PackingDate, ((ZDateTime)dObject["PackingDate"]).Date);
				AssertEquals("PartAttrib3", orderLine.WE_PartAttrib3, dObject["PartAttrib3"]);
			}

			AssertEquals("PartAttrib1", partAttribute1, dObject["PartAttrib1"]);
			AssertEquals("PartAttrib2", partAttribute2, dObject["PartAttrib2"]);
			AssertEquals("SerialNumber", serialNumber, dObject["SerialNumber"]);

			AssertEquals("DaysDelay", correctDays.Days, dObject["DaysDelay"]);
			//assert special instructions
			AssertEquals("SpecialInstructions", handlingInstructions, dObject["SpecialInstructions"]);
			AssertEquals("ShowException", correctDays.Days <= 0 ? "Ex" : "", dObject["ShowException"]);

			AssertEquals("DocketPK", order.PK, dObject["DocketPK"]);
			AssertEquals("ProductPK", orderLine.SupplierPart.PK, dObject["ProductPK"]);

			var partRelation =
				orderLine.SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(order.Client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation != null ? partRelation.Category : null;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedCategoryDescription = category != null ? category.OPC_CategoryDescription : ZString.Empty;
			AssertEquals("ProductCategoryCode", expectedCategoryCode, dObject["ProductCategoryCode"]);
			AssertEquals("ProductCategoryDescription", expectedCategoryDescription,
				dObject["ProductCategoryDescription"]);
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"
select *
from WhsKPIReleaseReport(null)
left join dbo.OrgSupplierPart on OP_PK = ProductPK
where WarehouseName = @Warehouse
and ClientPK = @ClientPK
order by OrderReferenceNo, OP_PartNum";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@Warehouse", whs.WW_WarehouseName, WhsWarehouseSchema.WW_WarehouseName);
			sqlParams.Add("@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region SetupData

		WhsOrder[] SetupData(WhsWarehouse whs, OrgHeader client)
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
			var part3 = Helper.CreateProduct(client, "3");
			var part4 = Helper.CreateProduct(client, "4");

			var category1 = Helper.CreateProductCategory(client2, part2, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";

			SetClientAttributesType(client);
			SetClientAttributesType(client2);
			SetProductAttributesUse(client, part1);
			SetProductAttributesUse(client2, part2);
			SetProductAttributesUse(client, part3);
			SetProductAttributesUse(client, part4, setReleaseCaptured: true);

			// product 3 is owned by both clients
			var relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OP = part3.PK;
			relation.OU_OH = client2.PK;

			SetupReceive(client, whs, "11", part1, 111, part3, 112, "TS1");
			SetupReceive(client, whs, "RELEASE", part4, 10m);
			Factory.Save();

			var order11 = CreateOrder(client, whs, "ER11", consignee1, new ZDateTimeOffset(year, 1, 1), "SI11", "TR11",
				"CR11", transportCo1, part1, 111, part3, 112, true, new ZDateTimeOffset(year, 08, 21), part4, "TS2");
			order11.ConsigneeDocAddress.E2_AddressOverride = ZBool.True;
			order11.ConsigneeNameOrPK = "Overriden Consignee Company name";
			Factory.Save();

			SetupReceive(client, whs, "12", part1, 121, part3, 122);
			Factory.Save();

			var order12 = CreateOrder(client, whs, "ER12", consignee2, new ZDateTimeOffset(year, 1, 1), "", "TR12", "CR12",
				transportCo2, part1, 121, part3, 122, true, new ZDateTimeOffset(year, 08, 22));
			order12.TransportCoDocAddress.E2_AddressOverride = ZBool.True;
			order12.TransportCoNameOrPK = "Overriden TransportCo Company name";
			Factory.Save();

			SetupReceive(client2, whs, "21", part2, 211, part2, 212);
			Factory.Save();

			var order21 = CreateOrder(client2, whs, "ER21", consignee1, new ZDateTimeOffset(year, 1, 1), "SI21", "TR21",
				"CR21", transportCo1, part2, 211, part3, 212, true, new ZDateTimeOffset(year, 08, 23));
			Factory.Save();

			SetupReceive(client2, whs2, "22", part2, 221, part2, 222);
			Factory.Save();

			var order22 = CreateOrder(client2, whs2, "ER22", consignee2, new ZDateTimeOffset(year, 1, 1), "SI22", "TR22",
				"CR22", transportCo2, part2, 221, part3, 222, true, new ZDateTimeOffset(year, 08, 24));
			Factory.Save();

			var order31 = CreateOrder(client, whs, "ER31", consignee1, new ZDateTimeOffset(year, 1, 1), "SI31", "TR31",
				"CR31", transportCo2, part2, 321, part3, 322, false, ZDateTimeOffset.Empty);
			Factory.Save();

			SetupCancelledOrder(client, whs, "41", consignee1, part1, 100);

			return new[] { order11, order12, order21, order22, order31 };
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

		void SetupCancelledOrder(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader consignee,
			OrgSupplierPart part, ZDecimal units, string serviceLevel = "")
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.WD_RS_NKServiceLevel = serviceLevel;
			Helper.CreateWhsOrderLine(order, part, units);

			Factory.Save();

			order.CancelReactivateDocket();

			Factory.Save();

			AssertEquals(true, order.IsCancelled);
		}

		void SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part1,
			ZDecimal units1, OrgSupplierPart part2, ZDecimal units2, string serviceLevel = "")
		{
			var year = ZDateTime.Now.Year;

			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			receive.WD_RS_NKServiceLevel = serviceLevel;
			Helper.CreateWhsReceiveInventoryLine(receive, part1, units1, new ZDate(year, 1, 2), new ZDate(year, 2, 1),
				"PA1", "PA2", "PA3", "BEK");
			Helper.CreateWhsReceiveInventoryLine(receive, part2, units2, new ZDate(year, 1, 2), new ZDate(year, 2, 1),
				"PA1", "PA2", "PA3", "BEK");

			FinaliseReceive(receive);
		}

		void SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgSupplierPart part,
			ZDecimal units)
		{
			var year = ZDateTime.Now.Year;

			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, units, new ZDate(year, 1, 2), new ZDate(year, 2, 1), "",
				"", "", "BEK");

			FinaliseReceive(receive);
		}

		static void FinaliseReceive(WhsReceive receive)
		{
			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPuttingAway", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Receive IsFinalised", true, receive.IsFinalised);
		}

		WhsOrder CreateOrder(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrgHeader consignee,
			ZDateTimeOffset date,
			ZString handlingInstructions, ZString transportReference, ZString customerReference, OrgHeader transportCo,
			OrgSupplierPart part1, ZDecimal units1, OrgSupplierPart part2, ZDecimal units2, bool finalise,
			ZDateTimeOffset finaliseDate, OrgSupplierPart releaseCapturedProduct = null, string serviceLevel = "")
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference, Notify);
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.WD_RequiredDate = date;
			order.WD_HandlingInstructions = handlingInstructions;
			order.WD_TransportReference = transportReference;
			order.WD_CustomerReference = customerReference;
			order.WD_RS_NKServiceLevel = serviceLevel;
			order.TransportCoPK = transportCo.PK;
			Helper.CreateWhsOrderLine(order, part1, units1);
			Helper.CreateWhsOrderLine(order, part2, units2);
			WhsOrderLine orderLine3 = null;

			if (releaseCapturedProduct != null)
			{
				orderLine3 = Helper.CreateWhsOrderLine(order, releaseCapturedProduct, 10m);
			}

			if (finalise)
			{
				var pick = Helper.CreatePickNew(order);

				if (orderLine3 != null)
				{
					var releaseLine = orderLine3.ReleaseLines[0];
					releaseLine.PartAttribute1 = "RA1";
					releaseLine.PartAttribute2 = "RA2";
					releaseLine.PartAttribute3 = "RA3";
				}

				pick.FinaliseAllOrders();
				order.WD_FinalisedDate = finaliseDate;
				pick.FinalisePick();
				AssertEquals("Order IsFinalised", true, order.IsFinalised);
				AssertEquals("Pick IsFinalised", true, pick.IsFinalised);
			}

			Factory.Save();

			return order;
		}

		#endregion
	}
}
