using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsReceivedOverUnderTest : WhsTestCaseWithFactory
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

			Factory.Save();

			var result1 = LoadView_WithProductCategory(categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WithProductCategory(categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WithProductCategory(categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_WithProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = categoryPK.IsEmpty
				? @"select * from WhsReceivedOverUnderReport(null)"
				: @"select * from WhsReceivedOverUnderReport(@ProductCategoryPK)";
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
			var whs1 = Helper.CreateWarehouse("1", "A", 5, 5);
			var client1 = Helper.CreateClient("1", "1");
			var part3 = Helper.CreateProduct(client1, "3");
			var receives = SetupData(whs1, client1, part3, commodity);
			var result1 = LoadView(whs1, client1, part3, commodity[0], false);
			AssertEquals(1, result1.Count);
			AssertResult(receives[3].Lines[1], result1[0], 3, commodity[0]);

			var result2 = LoadView(whs1, client1, part3, commodity[0], true);
			AssertEquals(2, result2.Count);
			AssertResult(receives[0].Lines[0], result2[0], 1, commodity[0]);
			AssertResult(receives[3].Lines[1], result2[1], 3, commodity[0]);
		}

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
			var inv1 = CreateReceiveLine("SN1");
			var inv2 = CreateReceiveLine("SN2");
			var inv3 = CreateReceiveLine("SN3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var result = LoadView(data.Whs1, data.Org1, data.Part1, commodity[0], true, "NON");
			AssertEquals("Should be 3 results", 3, result.Count);
			AssertResult(inv1, result[0], 0, commodity[0], "NON");
			AssertResult(inv2, result[1], 0, commodity[0], "NON");
			AssertResult(inv3, result[2], 0, commodity[0], "NON");

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		public void TestView_PickByBOM()
		{
			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			data.Part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			data.Part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			var inv1 = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", data.Part1, 5m);
			var kitOrderLine1 = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var componentPickLine = kitOrderLine1.ChildComponentLines.Single().PickLines.Single();
			componentPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();
			AssertEquals(true, pick.IsFinalised);
			var kitReceive = kitOrderLine1.PickLines[0].InventoryLine.Docket;
			var kitReceiveLine = kitReceive.Lines[0];
			AssertEquals("Precondition: Kit Receive Created and Finalised.", true, kitReceive.IsFinalised);
			AssertEquals("Precondition", data.Whs1.PK, kitReceive.WD_WW_Whs);
			AssertEquals("Precondition", data.Org1.PK, kitReceive.WD_OH_Client);
			AssertEquals("Precondition", data.Part1.PK, kitReceiveLine.WE_OP);

			var result1 = LoadView(data.Whs1, data.Org1, data.Part1, commodity[0], useCommodity: false, tranType: null);
			AssertEquals(0, result1.Count);

			var result2 = LoadView(data.Whs1, data.Org1, data.Part2, commodity[1], useCommodity: false, tranType: null);
			AssertEquals(1, result2.Count);
			AssertResult(inv1, result2[0], 0, commodity[1], "NON");
		}

		void AssertResult(WhsReceiveLine receiveLine, DynamicBusinessObject dynamicObject, ZDecimal undersOvers,
			RefCommodityCode commodity, string tranType = "OVE")
		{
			var receive = receiveLine.Docket;
			var client = receive.Client;
			var part = receiveLine.SupplierPart;
			var partRelation =
				part.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;

			AssertEquals("WarehousePK", receive.Warehouse.PK, dynamicObject["WarehousePK"]);
			AssertEquals("WarehouseName", receive.Warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
			AssertEquals("ClientPK", client.PK, dynamicObject["ClientPK"]);
			AssertEquals("ClientCode", client.OH_Code, dynamicObject["ClientCode"]);
			AssertEquals("Client", client.OH_FullName, dynamicObject["Client"]);
			AssertEquals("Reference", receive.WD_ExternalReference, dynamicObject["Reference"]);
			AssertEquals("FinalisedDate", receive.WD_FinalisedDate.Date,
				((ZDateTime)dynamicObject["FinalisedDate"]).Date);
			AssertEquals("ProductPK", part.PK, dynamicObject["ProductPK"]);
			AssertEquals("Product", part.OP_PartNum, dynamicObject["Product"]);
			AssertEquals("ProductDesc", part.OP_Desc, dynamicObject["ProductDesc"]);
			AssertEquals("CommodityCode", commodity.RH_Code, dynamicObject["CommodityCode"]);
			AssertEquals("CommodityPK", commodity.PK, dynamicObject["CommodityPK"]);
			AssertEquals("UndersOvers", undersOvers, dynamicObject["UndersOvers"]);
			AssertEquals("TranType", tranType, dynamicObject["TranType"]);
			AssertEquals("ProductCategory", expectedCategoryCode, dynamicObject["ProductCategoryCode"]);
			AssertEquals("PartAttrib1", receiveLine.WE_PartAttrib1, dynamicObject["PartAttrib1"]);
			var expectedAT2 = string.IsNullOrEmpty(receiveLine.WE_PartAttrib2)
				? ""
				: $"Attribute 2: {receiveLine.WE_PartAttrib2}";
			AssertEquals("PartAttrib2", expectedAT2, dynamicObject["PartAttrib2"]);
			var expectedAT3 = string.IsNullOrEmpty(receiveLine.WE_PartAttrib3)
				? ""
				: $"Attribute 3: {receiveLine.WE_PartAttrib3}";
			AssertEquals("PartAttrib3", expectedAT3, dynamicObject["PartAttrib3"]);
			var expectedSerial = string.IsNullOrEmpty(receiveLine.WE_SerialNumber)
				? ""
				: $"Serial Number: {receiveLine.WE_SerialNumber}";
			AssertEquals("SerialNumber", expectedSerial, dynamicObject["SerialNumber"]);
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product,
			RefCommodityCode commodity, ZBool useCommodity, string tranType = "OVE")
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql;
			if (useCommodity)
			{
				sql = @$"
SELECT
	*
FROM
	WhsReceivedOverUnderReport(null)
WHERE
	WarehousePK = @WarehousePK AND
	ClientPK = @ClientPK AND
	CommodityPK = @CommodityPK
	{(tranType != null ? "AND TranType = @TranType" : "")}
ORDER BY
	Reference,
	Units,
	SerialNumber";
			}
			else
			{
				sql = @$"
SELECT
	*
FROM
	WhsReceivedOverUnderReport(null)
WHERE
	WarehousePK = @WarehousePK AND
	ClientPK = @ClientPK AND
	ProductPK = @ProductPK
	{(tranType != null ? "AND TranType = @TranType" : "")}";
			}

			var parameters = new ZSqlParameterCollection
			{
				{ "@WarehousePK", whs.PK, WhsWarehouseSchema.PK },
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client }
			};

			if (useCommodity)
			{
				parameters.Add("@CommodityPK", product.CommodityCode.PK, RefCommodityCodeSchema.PK);
			}
			else
			{
				parameters.Add("@ProductPK", product.PK, WhsDocketLineSchema.WE_OP);
			}

			if (tranType != null)
			{
				parameters.Add("@TranType", tranType, WhsDocketSchema.GenericStringSchemaColumn);
			}

			result.Load(sql, parameters);
			return result;
		}

		#endregion

		#region SetupData

		WhsReceive[] SetupData(WhsWarehouse whs, OrgHeader client, OrgSupplierPart part, RefCommodityCode[] commodity)
		{
			var whs2 = Helper.CreateWarehouse("2", "A", 5, 5);
			var client2 = Helper.CreateClient("2", "2");
			Helper.CreateClient("3", "3");

			var part1 = Helper.CreateProduct(client, "1", OrgPartRelation.RelationshipTypes.Both);
			var part2 = Helper.CreateProduct(client2, "2");

			part.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part, "UNT", "PLT", 10);

			part1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			part2.OP_RH_NKCommodityCode = commodity[1].RH_Code;
			part.OP_RH_NKCommodityCode = commodity[0].RH_Code;

			var relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = "OWN";
			relation1.OU_OP = part.PK;
			relation1.OU_OH = client2.PK;

			var relation2 = part2.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = "OWN";
			relation2.OU_OP = part2.PK;
			relation2.OU_OH = client.PK;

			var category1 = Helper.CreateProductCategory(client, part1, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";
			var category2 = Helper.CreateProductCategory(client2, part2, "Cat2");
			category2.OPC_CategoryDescription = "Category 2";

			var receives = new[]
			{
				SetupReceive(client, whs, "11", part1, 111, part, 112, 110, 112),
				SetupReceive(client, whs, "12", part1, 121, part, 122, 121, 122),
				SetupReceive(client2, whs, "21", part2, 211, part, 212, 211, 212),
				SetupReceive(client, whs, "22", part2, 221, part, 222, 221, 219),
				SetupReceive(client, whs2, "31", part2, 311, part, 312, 311, 320),
				SetupReceive(client2, whs2, "32", part2, 321, part, 322, 321, 340)
			};
			Factory.Save();

			return receives;
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference,
			OrgSupplierPart warehousePart1, ZDecimal units1, OrgSupplierPart warehousePart2, ZDecimal units2,
			ZDecimal expectedUnits1, ZDecimal expectedUnits2)
		{
			Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(client, AttributeNumber.Three, false);

			Helper.SetProductAttributeUse(client, warehousePart1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(client, warehousePart1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(client, warehousePart1, AttributeNumber.Three, true);

			Helper.SetProductAttributeUse(client, warehousePart2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(client, warehousePart2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(client, warehousePart2, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(client, warehouse, reference, Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, warehousePart1, expectedUnits1);
			receiveLine1.WI_InDocketLineUnits = units1;
			receiveLine1.WI_PartAttrib1 = "PA11";
			receiveLine1.WI_PartAttrib2 = "PA12";
			receiveLine1.WI_PartAttrib2 = "PA13";

			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, warehousePart2, expectedUnits2);
			receiveLine2.WI_InDocketLineUnits = units2;
			receiveLine2.WI_PartAttrib1 = "PA21";
			receiveLine2.WI_PartAttrib2 = "PA22";
			receiveLine2.WI_PartAttrib2 = "PA23";

			receive.AllocateLocationsWithMock();
			AssertEquals("Receive IsPutaway", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Receive IsFinalised", true, receive.IsFinalised);

			receiveLine2.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine2.InDocketLine.HeldCodeChangeQuantity = 1;
			receiveLine2.InDocketLine.ChangeInventoryHeldCode(true);

			return receive;
		}

		#endregion
	}
}
