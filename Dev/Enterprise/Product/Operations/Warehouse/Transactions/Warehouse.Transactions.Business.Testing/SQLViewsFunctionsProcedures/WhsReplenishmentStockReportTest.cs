using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReplenishmentStockReportTest : WhsTestCaseWithFactory
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

			Helper.CreateProductParamsByWhsAndClient(productVB, client, warehouse, 100m, 200m);
			Helper.CreateProductParamsByWhsAndClient(productCoke, client, warehouse, 1m, 99m);
			Helper.CreateProductParamsByWhsAndClient(productTea, client, warehouse, 10m, 20m);

			Helper.CreateStock(warehouse.PK, client.PK, "VBStock", productVB.PK, 50m);
			Helper.CreateStock(warehouse.PK, client.PK, "TeaStock", productTea.PK, 2m);

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
				? @"select * from WhsReplenishmentStockReport(null)"
				: @"select * from WhsReplenishmentStockReport(@ProductCategoryPK)";
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
			var whs = Helper.CreateWarehouse("1");
			Helper.CreateRowAndGenerateLocations(whs, "ROW1", 5, 5);
			var whs2 = Helper.CreateWarehouse("2");
			Helper.CreateRowAndGenerateLocations(whs2, "ROW1", 5, 5);

			var products = SetupData(whs, whs2);
			var resultSet = LoadView();
			AssertResults(resultSet, products, whs, whs2);
		}

		#endregion

		#region TestView_Transfers

		public void TestView_Transfers()
		{
			var client = Helper.CreateClient("CLIENT");
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var part = Helper.CreateProduct(client, "P1");
			var partParam = Helper.CreateProductParamsByWhsAndClient(part, client, whs, 150, 300);
			Factory.Save();

			var receive =
				Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 100m, whs.FindLocation("A-1"), "");

			var adjustment = Helper.CreateWhsAdjustment(client, whs);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, part, -7, whs.FindLocation("A-1"));
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 7m, adjustmentLine.CommittedQuantity);

			var transfer = Helper.CreateWhsTransfer(client, whs, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, part, 10m, "A-1", "A-2");
			var transferLine = Helper.CreateWhsTransferLine(transfer, part, 15m, "A-1", "A-2");
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			transfer.RunPreSaveValidation(); // to commit stock.
			Factory.Save();

			var results = LoadView();
			AssertEquals(1, results.Count);
			AssertLineMatch(receive.Inventory[0], partParam, 100m, 17m, results);
		}

		#endregion

		#region TestNoDivideByZeroExceptionDueToPalletConversion

		[TestDate(2015, 03, 27)]
		public void TestNoDivideByZeroExceptionDueToPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "UNT", "CAS", 0.0001);
			Helper.CreateProductUnit(data.Part1, "CAS", "PLT", 0.0001);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 150, 300);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			transferLine.FinaliseDocketLine();

			// so after all rounding we will get 0 PLT in a UNT

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R", data.Part1, 1m);
			Factory.Save();

			var result = Load_OrgSupplierPartUnitsPerPallet(data.Part1.PK);
			AssertEquals("Precondition: OrgSupplierPartUnitsPerPallet should return 0.", 0m,
				result[0]["NumPalletsPerUnit"]);

			AssertNoExceptionThrown("No Division by zero exception should be thrown.", () => LoadView());
			AssertEquals(1, LoadView().Count);
		}

		DynamicBusinessObjectCollection Load_OrgSupplierPartUnitsPerPallet(ZGuid productPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"SELECT UnitsPerPallet as NumPalletsPerUnit from dbo.OrgSupplierPartUnitsPerPallet(@ProductPK) as tableResult";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ProductPK", productPK, WhsDocketLineSchema.WE_OP);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView_ReplenishmentMultipleSpecified

		public void TestView_ReplenishmentMultipleSpecified()
		{
			var whs = Helper.CreateWarehouse("Warehouse1");
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var products = SetupData_ReplenishmentMultipleSpecified(whs);
			var resultSet = LoadView();
			AssertResult_ReplenishmentMultipleSpecified(resultSet, products, whs);
		}

		#endregion

		#region Asserts

		void AssertResults(DynamicBusinessObjectCollection resultSet, IReadOnlyList<OrgSupplierPart> products,
			WhsWarehouse whs, WhsWarehouse whs2)
		{
			AssertEquals(3, resultSet.Count);
			AssertCorrectData(products[0], whs, 100m, 200m, 50m, 150m, resultSet[0]);
			AssertCorrectData(products[1], whs, 1m, 99m, 0m, 99m, resultSet[1]);
			AssertCorrectData(products[3], whs2, 200m, 400m, 150m, 250m, resultSet[2]);
		}

		void AssertResult_ReplenishmentMultipleSpecified(DynamicBusinessObjectCollection resultSet,
			IReadOnlyList<OrgSupplierPart> products, WhsWarehouse whs)
		{
			AssertEquals(5, resultSet.Count);
			AssertCorrectData(products[0], whs, 50m, 60m, 45m, 20m, resultSet[0]);
			AssertCorrectData(products[1], whs, 50m, 60m, 40m, 20m, resultSet[1]);
			AssertCorrectData(products[2], whs, 50m, 60m, 35m, 20m, resultSet[2]);
			AssertCorrectData(products[3], whs, 50m, 60m, 45m, 20m, resultSet[3]);
			AssertCorrectData(products[4], whs, 50m, 60m, 45m, 15m, resultSet[4]);
		}

		void AssertCorrectData(OrgSupplierPart product, WhsWarehouse warehouse, decimal minimum, decimal economicQty,
			decimal availableUnits, decimal reOrderQty, DynamicBusinessObject resultLine)
		{
			AssertEquals(product.PK, resultLine["ProductPK"]);
			AssertEquals(product.OP_PartNum, resultLine["PrCode"]);
			AssertEquals(warehouse.PK, resultLine["WarehousePK"]);
			AssertEquals(warehouse.WW_WarehouseName, resultLine["WarehouseName"]);
			AssertEquals(minimum, resultLine["ReplenishmentMinimum"]);
			AssertEquals(economicQty, resultLine["EconomicQuantity"]);
			AssertEquals(availableUnits, resultLine["AvailableUnits"]);
			AssertEquals(reOrderQty, resultLine["ReorderQty"]);
		}

		void AssertLineMatch(WhsInventoryView expectedInventory, WhsProductParamsByWhsAndClient partParam,
			ZDecimal expectedTotalUnits, ZDecimal expectedCommittedUnits, DynamicBusinessObjectCollection results)
		{
			var actualInventory = results.Single(l => (ZDecimal)l["TotalUnits"] == expectedTotalUnits);
			var docket = expectedInventory.Docket;
			var client = docket.Client;
			var whs = docket.Warehouse;
			var part = expectedInventory.SupplierPart;
			var partRelation = part.RelatedOrganisations[0];
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;
			var expectedLocalPartNum = (partRelation.OU_LocalPartNumber.IsEmpty)
				? part.OP_PartNum
				: partRelation.OU_LocalPartNumber;
			var expectedLocalDesc = (partRelation.OU_LocalPartNumber.IsEmpty)
				? part.OP_Desc
				: partRelation.OU_LocalPartDescription;
			var expectedCommodityPK = (part.CommodityCode != null) ? part.CommodityCode.PK : ZGuid.Empty;
			var expectedUNDG = (part.UNDGs.Count > 0 && part.UNDGs[0].Substance != null)
				? part.UNDGs[0].Substance.DG_Code
				: ZString.Empty;
			var expectedTotalPallets = (part.OP_StockKeepingUnitPerPallet != 0m)
				? expectedTotalUnits / part.OP_StockKeepingUnitPerPallet
				: 0m;

			if (partParam.W3_ReplenishmentMultiple == 0m)
			{
				partParam.W3_ReplenishmentMultiple = 1m;
			}

			var expectedReorderQty = (partParam.W3_EconomicQuantity <= expectedTotalUnits)
				? 0m
				: partParam.W3_EconomicQuantity - expectedTotalUnits;

			expectedReorderQty = (expectedTotalUnits - expectedCommittedUnits) < partParam.W3_ReplenishmentMinimum
				? Math.Ceiling(
					  (partParam.W3_EconomicQuantity - expectedTotalUnits) / partParam.W3_ReplenishmentMultiple) *
				  partParam.W3_ReplenishmentMultiple
				: 0m;

			CombineAssertions(() =>
			{
				AssertEquals("ProductPK", expectedInventory.WI_OP, actualInventory["ProductPK"]);
				AssertEquals("PrCode", part.OP_PartNum, actualInventory["PrCode"]);
				AssertEquals("ProductCategoryCode", expectedCategoryCode, actualInventory["ProductCategoryCode"]);
				AssertEquals("PrDesc", part.OP_Desc, actualInventory["PrDesc"]);
				AssertEquals("ProductBrandName", part.OP_Brand, actualInventory["ProductBrandName"]);
				AssertEquals("ProductModel", part.OP_Model, actualInventory["ProductModel"]);
				AssertEquals("PrCodeORP", expectedLocalPartNum, actualInventory["PrCodeORP"]);
				AssertEquals("PrDescOPR", expectedLocalDesc, actualInventory["PrDescOPR"]);
				AssertEquals("CommodityCode", part.OP_RH_NKCommodityCode, actualInventory["CommodityCode"]);
				AssertEquals("CommodityPK", expectedCommodityPK, actualInventory["CommodityPK"]);
				AssertEquals("UNDGCode", expectedUNDG, actualInventory["UNDGCode"]);
				AssertEquals("ClientPK", expectedInventory.WI_OH_Client, actualInventory["ClientPK"]);
				AssertEquals("ClientCode", client.OH_Code, actualInventory["ClientCode"]);
				AssertEquals("ClientName", client.OH_FullName, actualInventory["ClientName"]);
				AssertEquals("Role", partRelation.OU_Relationship, actualInventory["Role"]);
				AssertEquals("StockUnit", part.OP_StockKeepingUnit, actualInventory["StockUnit"]);
				AssertEquals("PalletSize", part.OP_StockKeepingUnitPerPallet, actualInventory["PalletSize"]);
				AssertEquals("ReplenishmentMinimum", partParam.W3_ReplenishmentMinimum,
					actualInventory["ReplenishmentMinimum"]);
				AssertEquals("EconomicQuantity", partParam.W3_EconomicQuantity, actualInventory["EconomicQuantity"]);
				AssertEquals("WarehousePK", docket.WD_WW_Whs, actualInventory["WarehousePK"]);
				AssertEquals("WarehouseCode", whs.WW_WarehouseCode, actualInventory["WarehouseCode"]);
				AssertEquals("WarehouseName", whs.WW_WarehouseName, actualInventory["WarehouseName"]);
				AssertEquals("TotalPallets", expectedTotalPallets, actualInventory["TotalPallets"]);
				AssertEquals("TotalUnits", expectedTotalUnits, actualInventory["TotalUnits"]);
				AssertEquals("CommittedUnits", expectedCommittedUnits, actualInventory["CommittedUnits"]);
				AssertEquals("AvailableUnits", expectedTotalUnits - expectedCommittedUnits,
					actualInventory["AvailableUnits"]);
				AssertEquals("ReorderQty", expectedReorderQty, actualInventory["ReorderQty"]);
			});
		}

		#endregion

		#region SetupData

		IReadOnlyList<OrgSupplierPart> SetupData(WhsWarehouse whs, WhsWarehouse whs2)
		{
			var client = Helper.CreateClient();
			var client2 = Helper.CreateClient("CLIENT2");

			var product1 = Helper.CreateProduct(client, "PRODUCT1", "OWN");
			Helper.CreateProductClientRelationShip(client2, product1, "OWN");

			var product2 = Helper.CreateProduct(client, "PRODUCT2", "OWN");
			var product3 = Helper.CreateProduct(client, "PRODUCT3", "OWN");
			var product4 = Helper.CreateProduct(client, "PRODUCT4", "OWN");

			var category1 = Helper.CreateProductCategory(client2, product1, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";
			var category2 = Helper.CreateProductCategory(client, product2, "Cat2");
			category2.OPC_CategoryDescription = "Category 2";

			Helper.CreateProductParamsByWhsAndClient(product1, client, whs, 100m, 200m);
			Helper.CreateStock(whs.PK, client.PK, "ABC", product1.PK, 50m);
			Helper.CreateStock(whs2.PK, client.PK, "ERT", product1.PK, 1000m);
			Helper.CreateProductParamsByWhsAndClient(product2, client, whs, 1m, 99m);
			Helper.CreateProductParamsByWhsAndClient(product3, client, whs, 10m, 20m);
			Helper.CreateStock(whs.PK, client.PK, "XYZ", product3.PK, 10m);
			Helper.CreateProductParamsByWhsAndClient(product4, client, whs2, 200m, 400m);
			Helper.CreateStock(whs2.PK, client.PK, "BLA", product4.PK, 150m);
			Factory.Save();

			return new List<OrgSupplierPart>(new[] { product1, product2, product3, product4 });
		}

		IReadOnlyList<OrgSupplierPart> SetupData_ReplenishmentMultipleSpecified(WhsWarehouse whs)
		{
			var client = Helper.CreateClient();

			var product1 = Helper.CreateProduct(client, "PRODUCT1", "OWN");
			var product2 = Helper.CreateProduct(client, "PRODUCT2", "OWN");
			var product3 = Helper.CreateProduct(client, "PRODUCT3", "OWN");
			var product4 = Helper.CreateProduct(client, "PRODUCT4", "OWN");
			var product5 = Helper.CreateProduct(client, "PRODUCT5", "OWN");

			Helper.CreateProductParamsByWhsAndClient(product1, client, whs, 50m, 60m, 10m, "");
			Helper.CreateStock(whs.PK, client.PK, "ABC", product1.PK, 45m);
			Helper.CreateProductParamsByWhsAndClient(product2, client, whs, 50m, 60m, 10m, "");

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product2, 45m, whs.FindLocation("A-1"), "");
			var adjustment = Helper.CreateWhsAdjustment(client, whs);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, product2, -5, whs.FindLocation("A-1"));
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 5m, adjustmentLine.CommittedQuantity);

			Helper.CreateProductParamsByWhsAndClient(product3, client, whs, 50m, 60m, 10m, "");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product3, 40m, whs.FindLocation("A-2"), "");
			Helper.CreateWhsAdjustment(client, whs);
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, product3, -5, whs.FindLocation("A-2"));
			adjustmentLine2.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 5m, adjustmentLine2.CommittedQuantity);

			Helper.CreateProductParamsByWhsAndClient(product4, client, whs, 50m, 60m, 20m, "");
			Helper.CreateStock(whs.PK, client.PK, "DEF", product4.PK, 45m);

			Helper.CreateProductParamsByWhsAndClient(product5, client, whs, 50m, 60m, 1m, "");
			Helper.CreateStock(whs.PK, client.PK, "HIJ", product5.PK, 45m);
			Factory.Save();

			return new List<OrgSupplierPart>(new[] { product1, product2, product3, product4, product5 });
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView()
		{
			var sql = @"select * from WhsReplenishmentStockReport(null) order by PrCode, WarehouseCode";

			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			return result;
		}

		#endregion
	}
}
