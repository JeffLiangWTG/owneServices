using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsProductListingTest : WhsTestCaseWithFactory
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

			if (categoryPK.IsEmpty)
			{
				var sql = @"select * from WhsProductListingReport(null)";
				result.Load(sql);
			}
			else
			{
				var sql = @"select * from WhsProductListingReport(@ProductCategoryPK)";
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
				result.Load(sql, sqlParams);
			}

			return result;
		}

		#endregion

		#region TestProductCategory

		public void TestProductCategory()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS", "A");

			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			var part11 = Helper.CreateProduct(client1, "P11");
			var part12 = Helper.CreateProduct(client1, "P12");
			var part21 = Helper.CreateProduct(client2, "P21");
			var part22 = Helper.CreateProduct(client2, "P22");

			var category1 = Helper.CreateProductCategory(client1, part11, "Cat1");
			category1.OPC_CategoryDescription = "Category 1";

			var category2 = Helper.CreateProductCategory(client2, part21, "Cat2");
			category2.OPC_CategoryDescription = "Category 2";

			Factory.Save();

			var results = LoadView();

			AssertEquals(4, results.Count);

			AssertRow_ClientProductAndCategory(results[0], client1, part11, "Cat1", "Category 1");
			AssertRow_ClientProductAndCategory(results[1], client1, part12, "", "");
			AssertRow_ClientProductAndCategory(results[2], client2, part21, "Cat2", "Category 2");
			AssertRow_ClientProductAndCategory(results[3], client2, part22, "", "");
		}

		void AssertRow_ClientProductAndCategory(DynamicBusinessObject result, OrgHeader client, OrgSupplierPart part,
			ZString categoryCode, ZString categoryDescription)
		{
			AssertEquals(result["ClientCode"], client.OH_Code);
			AssertEquals(result["Product"], part.OP_PartNum);
			AssertEquals(result["ProductCategoryCode"], categoryCode);
			AssertEquals(result["ProductCategoryDescription"], categoryDescription);
		}

		#endregion

		#region TestWhsProductListingReportShouldNotMixClientSetting

		public void TestWhsProductListingReportShouldNotMixClientSetting()
		{
			var year = ZDateTime.Now.Year;
			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product = Helper.CreateProduct(client1, "P1");
			var abcCategory1 = Factory.New<WhsABCCategory>();
			abcCategory1.WJ_OH_Client = client1.PK;
			abcCategory1.WJ_OP_Product = product.PK;
			abcCategory1.WJ_WW_Warehouse = productWarehouse.PK;
			abcCategory1.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 3, 1);
			abcCategory1.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 3, 2);
			abcCategory1.WJ_AnalysisPeriod = "MLY";

			var relation =
				Helper.CreateProductClientRelationShip(client2, product, OrgPartRelation.RelationshipTypes.Owner);
			var abcCategory2 = Factory.New<WhsABCCategory>();
			abcCategory2.WJ_OH_Client = client2.PK;
			abcCategory2.WJ_OP_Product = product.PK;
			abcCategory2.WJ_WW_Warehouse = productWarehouse.PK;
			abcCategory2.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 3, 1);
			abcCategory2.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 3, 2);
			abcCategory2.WJ_AnalysisPeriod = "WKY";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			AssertEquals("Period is monthly", abcCategory1.WJ_AnalysisDateFrom.AddDays(-30 + 1).ToDateTime(),
				results.Single(r => (ZGuid)r["ClientPK"] == client1.PK)["PeriodStart"]);
			AssertEquals("Period is weekly", abcCategory2.WJ_AnalysisDateFrom.AddDays(-7 + 1).ToDateTime(),
				results.Single(r => (ZGuid)r["ClientPK"] == client2.PK)["PeriodStart"]);
		}

		#endregion

		#region TestWhsProductListingReportShouldNotMixClientSetting_WithCategory

		public void TestWhsProductListingReportShouldNotMixClientSetting_WithCategory()
		{
			var year = ZDateTime.Now.Year;
			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product = Helper.CreateProduct(client1, "P1");

			var abcCategory1 = Factory.New<WhsABCCategory>();
			abcCategory1.WJ_OH_Client = client1.PK;
			abcCategory1.WJ_OP_Product = product.PK;
			abcCategory1.WJ_WW_Warehouse = productWarehouse.PK;
			abcCategory1.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 3, 1);
			abcCategory1.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 3, 2);
			abcCategory1.WJ_Category = "A";

			Helper.CreateProductClientRelationShip(client2, product, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results.Single(r => (ZGuid)r["ClientPK"] == client1.PK);
			AssertEquals("Previous ABC Category", "A", result1["PreviousABCCategory"]);
			AssertEquals("Current ABC Category", "A", result1["ABCCategoryName"]);

			var result2 = results.Single(r => (ZGuid)r["ClientPK"] == client2.PK);
			AssertEquals("Previous ABC Category", "", result2["PreviousABCCategory"]);
			AssertEquals("Current ABC Category", "", result2["ABCCategoryName"]);
		}

		#endregion

		#region TestView

		public void TestViewFiltersOutTransitWarehouses()
		{
			var year = ZDateTime.Now.Year;

			var client = Helper.CreateClient();
			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var transitWarehouse = Helper.CreateWarehouse("TRA", "A");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var product = Helper.CreateProduct(client, "P1");
			var categoryForProductWarehouse = Factory.New<WhsABCCategory>();
			categoryForProductWarehouse.WJ_OH_Client = client.PK;
			categoryForProductWarehouse.WJ_OP_Product = product.PK;
			categoryForProductWarehouse.WJ_WW_Warehouse = productWarehouse.PK;
			categoryForProductWarehouse.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 1, 1);
			categoryForProductWarehouse.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 1, 2);

			var categoryForTransitWarehouse = Factory.New<WhsABCCategory>();
			categoryForTransitWarehouse.WJ_OH_Client = client.PK;
			categoryForTransitWarehouse.WJ_OP_Product = product.PK;
			categoryForTransitWarehouse.WJ_WW_Warehouse = transitWarehouse.PK;
			categoryForTransitWarehouse.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 1, 1);
			categoryForTransitWarehouse.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 1, 2);
			Factory.Save();

			var results = LoadView();
			AssertEquals("Should only return 1 row, for Product P1, only for Product Warehouse.", 1, results.Count);
			AssertEquals("WarehousePK", productWarehouse.PK, results[0]["WarehousePK"]);
		}

		#endregion

		#region TestClientPK

		public void TestClientPK()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(client1.PK, result1["ClientPK"]);
			AssertEquals(client2.PK, result2["ClientPK"]);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("C1", result1["ClientCode"]);
			AssertEquals("C2", result2["ClientCode"]);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "4930JA2023A", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "7465E", OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("4930JA2023A", result1["Product"]);
			AssertEquals("7465E", result2["Product"]);
		}

		#endregion

		#region TestProductDesc

		public void TestProductDesc()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Desc = "FIBER CHEM EXCLUDING S-21";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Desc = "OVERLOAD PROTECTOR";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("FIBER CHEM EXCLUDING S-21", result1["ProductDesc"]);
			AssertEquals("OVERLOAD PROTECTOR", result2["ProductDesc"]);
		}

		#endregion

		#region TestLastCost

		public void TestLastCost()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_LastCost = 3m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_LastCost = 4.5m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(3m, result1["LastCost"]);
			AssertEquals(4.5m, result2["LastCost"]);
		}

		#endregion

		#region TestCurrency

		public void TestCurrency()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_RX_NKLastWeightedCostCurr = "AUD";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_RX_NKLastWeightedCostCurr = "USD";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("AUD", result1["Currency"]);
			AssertEquals("USD", result2["Currency"]);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Brand = "BRAND";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Brand = "ADIDAS";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("BRAND", result1["ProductBrandName"]);
			AssertEquals("ADIDAS", result2["ProductBrandName"]);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Model = "MODEL";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Model = "MINI";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("MODEL", result1["ProductModel"]);
			AssertEquals("MINI", result2["ProductModel"]);
		}

		#endregion

		#region TestProductCubic

		public void TestProductCubic()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Cubic = 10.1m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Cubic = 0.9m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(10.1m, result1["ProductCubic"]);
			AssertEquals(0.9m, result2["ProductCubic"]);
		}

		#endregion

		#region TestCubicUQ

		public void TestCubicUQ()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_CubicUQ = "M3";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_CubicUQ = "ML";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("M3", result1["CubicUQ"]);
			AssertEquals("ML", result2["CubicUQ"]);
		}

		#endregion

		#region TestDepartment

		public void TestDepartment()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Department = "DEPARTMENT";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Department = "TRAINING";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("DEPARTMENT", result1["Department"]);
			AssertEquals("TRAINING", result2["Department"]);
		}

		#endregion

		#region TestDivision

		public void TestDivision()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Division = "DIVISION";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Division = "SCOPE";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("DIVISION", result1["Division"]);
			AssertEquals("SCOPE", result2["Division"]);
		}

		#endregion

		#region TestHeight

		public void TestHeight()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Height = 2.3m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Height = 0.8m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(2.3m, result1["Height"]);
			AssertEquals(0.8m, result2["Height"]);
		}

		#endregion

		#region TestWidth

		public void TestWidth()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Width = 3.4m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Width = 0.7m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(3.4m, result1["Width"]);
			AssertEquals(0.7m, result2["Width"]);
		}

		#endregion

		#region TestDepth

		public void TestDepth()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Depth = 4.5m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Depth = 0.6m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(4.5m, result1["Depth"]);
			AssertEquals(0.6m, result2["Depth"]);
		}

		#endregion

		#region TestMeasureUQ

		public void TestMeasureUQ()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_MeasureUQ = "PK";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_MeasureUQ = "CT";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("PK", result1["MeasureUQ"]);
			AssertEquals("CT", result2["MeasureUQ"]);
		}

		#endregion

		#region TestOrderQty

		public void TestOrderQty()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_OrderMultipleQty = 5.6m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_OrderMultipleQty = 0.5m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(5.6m, result1["OrderQty"]);
			AssertEquals(0.5m, result2["OrderQty"]);
		}

		#endregion

		#region TestOrderQtyUnit

		public void TestOrderQtyUnit()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_OrderMultipleUnit = "NO";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_OrderMultipleUnit = "YES";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("NO", result1["OrderQtyUnit"]);
			AssertEquals("YES", result2["OrderQtyUnit"]);
		}

		#endregion

		#region TestStockQty

		public void TestStockQty()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_QtyInStock = 6.7m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_QtyInStock = 0.4m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(6.7m, result1["StockQty"]);
			AssertEquals(0.4m, result2["StockQty"]);
		}

		#endregion

		#region TestStockQtyUnit

		public void TestStockQtyUnit()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_StockKeepingUnit = "BAG";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_StockKeepingUnit = "CTN";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("BAG", result1["StockQtyUnit"]);
			AssertEquals("CTN", result2["StockQtyUnit"]);
		}

		#endregion

		#region TestVendorPackQty

		public void TestVendorPackQty()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_VendorPackQty = 7.8m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_VendorPackQty = 0.3m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(7.8m, result1["VendorPackQty"]);
			AssertEquals(0.3m, result2["VendorPackQty"]);
		}

		#endregion

		#region TestVendorPackUnit

		public void TestVendorPackUnit()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_F3_NKPackType = "CTN";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_F3_NKPackType = "BOX";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("CTN", result1["VendorPackUnit"]);
			AssertEquals("BOX", result2["VendorPackUnit"]);
		}

		#endregion

		#region TestProductWeight

		public void TestProductWeight()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_Weight = 8.9m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_Weight = 0.2m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(8.9m, result1["ProductWeight"]);
			AssertEquals(0.2m, result2["ProductWeight"]);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_WeightUQ = "KT";
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_WeightUQ = "KG";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("KT", result1["WeightUQ"]);
			AssertEquals("KG", result2["WeightUQ"]);
		}

		#endregion

		#region TestWeightedCost

		public void TestWeightedCost()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_WeightedCost = 9.1m;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_WeightedCost = 0.1m;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(9.1m, result1["WeightedCost"]);
			AssertEquals(0.1m, result2["WeightedCost"]);
		}

		#endregion

		#region TestCommodityPK

		public void TestCommodityPK()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product3 = Helper.CreateProduct(client3, "P3", OrgPartRelation.RelationshipTypes.Owner);

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			product1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			product2.OP_RH_NKCommodityCode = commodity[1].RH_Code;

			Factory.Save();

			var results = LoadView();

			AssertEquals(3, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			var result3 = results[2];
			AssertEquals(commodity[0].PK, result1["CommodityPK"]);
			AssertEquals(commodity[1].PK, result2["CommodityPK"]);
			AssertEquals(ZGuid.Empty, result3["CommodityPK"]);
		}

		#endregion

		#region TestCommodityCode

		public void TestCommodityCode()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product3 = Helper.CreateProduct(client3, "P3", OrgPartRelation.RelationshipTypes.Owner);

			var commodity = new RefCommodityCode[2]
			{
				Factory.NewWithValidTestData<RefCommodityCode>(), Factory.NewWithValidTestData<RefCommodityCode>()
			};

			product1.OP_RH_NKCommodityCode = commodity[0].RH_Code;
			product2.OP_RH_NKCommodityCode = commodity[1].RH_Code;

			Factory.Save();

			var results = LoadView();

			AssertEquals(3, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			var result3 = results[2];
			AssertEquals(commodity[0].RH_Code, result1["CommodityCode"]);
			AssertEquals(commodity[1].RH_Code, result2["CommodityCode"]);
			AssertEquals("", result3["CommodityCode"]);
		}

		#endregion

		#region TestUNDGFields

		public void TestUNDGFields()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product3 = Helper.CreateProduct(client3, "P3", OrgPartRelation.RelationshipTypes.Owner);

			AddUNDGToProduct(product1, "D11-", "1", "PSN-1", "Cl-1", "SL1-1", "SL2-1", "P-1");
			AddUNDGToProduct(product1, "D11-", "2", "PSN-2", "Cl-2", "SL1-2", "SL2-2", "P-2");
			AddUNDGToProduct(product1, "D11-", "3", "PSN-3", "Cl-3", "SL1-3", "SL2-3", "P-3");

			AddUNDGToProduct(product2, "D22", "", "PSN-4", "Cl-4", "SL1-4", "SL2-4", "P-4");

			Factory.Save();

			var results = LoadView();

			AssertEquals(3, results.Count);

			AssertUNDGItem(
				results[0],
				"D11-1\r\nD11-2\r\nD11-3",
				"PSN-1\r\nPSN-2\r\nPSN-3",
				"Cl-1\r\nCl-2\r\nCl-3",
				"SL1-1\r\nSL1-2\r\nSL1-3",
				"SL2-1\r\nSL2-2\r\nSL2-3",
				"P-1\r\nP-2\r\nP-3");

			AssertUNDGItem(
				results[1],
				"D22",
				"PSN-4",
				"Cl-4",
				"SL1-4",
				"SL2-4",
				"P-4");

			AssertUNDGItem(results[2], "", "", "", "", "", "");
		}

		public void TestView_ProductWithHugeUNDGCollection()
		{
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P2", OrgPartRelation.RelationshipTypes.Both);
			for (var i = 0; i < 2001; i++)
			{
				AddUNDGToProduct(product, "DG1", "1", "SHP1", "CLS1", "SR12", "SR22", "PK1");
			}
			Factory.Save();

			DynamicBusinessObjectCollection result = null;
			AssertNoExceptionThrown(() => result = LoadView());
			AssertEquals(1, result.Count);
			Assert("UNDGNumber is not empty", !((ZString)(result[0]["HazMat"])).IsEmpty);
		}

		void AddUNDGToProduct(
			OrgSupplierPart product,
			string undgUNNO,
			string undgVariant,
			string shippingName,
			string imoClass,
			string subRisk1,
			string subRisk2,
			string packingGroup)
		{
			var undgNumber = undgUNNO + undgVariant;

			var query = new ZQuery(UNDGSubstanceSchema.DG_Code, undgNumber);
			var substance = Factory.LoadTop1<UNDGSubstance>(query) ?? Factory.New<UNDGSubstance>();
			if (substance.DG_Code.IsEmpty)
			{
				substance.DG_UNNO = undgUNNO;
				substance.DG_Variant = undgVariant;
				substance.DG_Code = undgNumber;
				substance.DG_PSN = shippingName;
				substance.DG_Class = imoClass;
				substance.DG_SubLabel1 = subRisk1;
				substance.DG_SubLabel2 = subRisk2;
				substance.DG_PG = packingGroup;
			}

			var undg = product.UNDGs.AddNew();

			var subsPivot = Factory.New<UNDGSubstancePivot>();
			subsPivot.DP_UNNO = substance.DG_UNNO;
			subsPivot.DP_Variant = substance.DG_Variant;
			subsPivot.DP_ParentId = undg.PK;
			subsPivot.DP_ParentTableCode = undg.TablePrefix;
			subsPivot.DP_Standard = "IMO";
			subsPivot.DP_IsDefault = true;
		}

		void AssertUNDGItem(
			DynamicBusinessObject result,
			string expectedUNDGNumber,
			string expectedShippingName,
			string expectedClass,
			string expectedSubRisk1,
			string expectedSubRisk2,
			string expectedPackGroup)
		{
			AssertEquals("HazMat", expectedUNDGNumber, result["HazMat"]);
			AssertEquals("UNDGProperShippingName", expectedShippingName, result["UNDGProperShippingName"]);
			AssertEquals("UNDGIMOClass", expectedClass, result["UNDGIMOClass"]);
			AssertEquals("UNDGSubRisk1", expectedSubRisk1, result["UNDGSubRisk1"]);
			AssertEquals("UNDGSubRisk2", expectedSubRisk2, result["UNDGSubRisk2"]);
			AssertEquals("UNDGPackingGroup", expectedPackGroup, result["UNDGPackingGroup"]);
		}

		#endregion

		#region TestUnitConversions

		public void TestUnitConversions()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product3 = Helper.CreateProduct(client3, "P3", OrgPartRelation.RelationshipTypes.Owner);

			product2.OP_Cubic = 10.1m;
			product2.OP_CubicUQ = "M3";
			product2.OP_StockKeepingUnit = "BAG";
			product2.OP_Weight = 8.9m;
			product2.OP_WeightUQ = "KT";

			var product3Unit = product3.PartUnits.AddNew();
			product3Unit.OF_QuantityInParent = 10;
			product3Unit.OF_PackType = "UNT";
			product3Unit.OF_ParentPackType = "PLT";

			Factory.Save();

			var results = LoadView();

			AssertEquals(3, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			var result3 = results[2];
			AssertEquals("12 x UNT = CTN\r\n2 x KG = UNT\r\n0.02 x M3 = UNT", result1["UnitConversions"]);
			AssertEquals("12 x UNT = CTN\r\n8.9 x KT = BAG\r\n10.1 x M3 = BAG", result2["UnitConversions"]);
			AssertEquals("12 x UNT = CTN\r\n10 x UNT = PLT\r\n2 x KG = UNT\r\n0.02 x M3 = UNT", result3["UnitConversions"]);
		}

		#endregion

		#region TestABCCategory

		public void TestABCCategory()
		{
			var year = ZDateTime.Now.Year;
			var warehouse = Helper.CreateWarehouse("1", "2", "A");
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product3 = Helper.CreateProduct(client3, "P3", OrgPartRelation.RelationshipTypes.Owner);

			var oldABCCategoryProduct1 = Factory.New<WhsABCCategory>();
			oldABCCategoryProduct1.WJ_OP_Product = product1.PK;
			oldABCCategoryProduct1.WJ_OH_Client = client1.PK;
			oldABCCategoryProduct1.WJ_WW_Warehouse = warehouse.PK;
			oldABCCategoryProduct1.WJ_Category = "A";
			oldABCCategoryProduct1.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 1, 1);
			oldABCCategoryProduct1.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 1, 1);

			var currentABCCategoryProduct1 = Factory.New<WhsABCCategory>();
			currentABCCategoryProduct1.WJ_OP_Product = product1.PK;
			currentABCCategoryProduct1.WJ_OH_Client = client1.PK;
			currentABCCategoryProduct1.WJ_WW_Warehouse = warehouse.PK;
			currentABCCategoryProduct1.WJ_Category = "B";
			currentABCCategoryProduct1.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 2, 2);
			currentABCCategoryProduct1.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 2, 2);

			var oldABCCategoryProduct2 = Factory.New<WhsABCCategory>();
			oldABCCategoryProduct2.WJ_OP_Product = product2.PK;
			oldABCCategoryProduct2.WJ_OH_Client = client2.PK;
			oldABCCategoryProduct2.WJ_WW_Warehouse = warehouse.PK;
			oldABCCategoryProduct2.WJ_Category = "D";
			oldABCCategoryProduct2.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 1, 1);
			oldABCCategoryProduct2.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 1, 1);

			var currentABCCategoryProduct2 = Factory.New<WhsABCCategory>();
			currentABCCategoryProduct2.WJ_OP_Product = product2.PK;
			currentABCCategoryProduct2.WJ_OH_Client = client2.PK;
			currentABCCategoryProduct2.WJ_WW_Warehouse = warehouse.PK;
			currentABCCategoryProduct2.WJ_Category = "C";
			currentABCCategoryProduct2.WJ_AnalysisDateFrom = new ZDateTimeOffset(year, 1, 1);
			currentABCCategoryProduct2.WJ_AnalysisDateTo = new ZDateTimeOffset(year, 2, 2);

			Factory.Save();

			var results = LoadView();

			AssertEquals(3, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			var result3 = results[2];
			AssertEquals("A", result1["PreviousABCCategory"]);
			AssertEquals("B", result1["ABCCategoryName"]);
			AssertEquals("C", result2["PreviousABCCategory"]);
			AssertEquals("C", result2["ABCCategoryName"]);
			AssertEquals("", result3["PreviousABCCategory"]);
			AssertEquals("", result3["ABCCategoryName"]);
		}

		#endregion

		#region TestLocalPartDescription

		public void TestLocalPartDescription()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_LocalPartDescription = "Test Part1 Local Description";
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_LocalPartDescription = "Test Part2 Local Description";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("Test Part1 Local Description", result1["LocalPartDescription"]);
			AssertEquals("Test Part2 Local Description", result2["LocalPartDescription"]);
		}

		#endregion

		#region TestLocalPartCode

		public void TestLocalPartCode()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_LocalPartNumber = "A-234";
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_LocalPartNumber = "Local Part Number of P2";
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals("A-234", result1["LocalPartCode"]);
			AssertEquals("Local Part Number of P2", result2["LocalPartCode"]);
		}

		#endregion

		#region TestIsActive

		public void TestIsActive()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			product1.OP_IsActive = true;
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			product2.OP_IsActive = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["IsActive"]);
			AssertEquals(false, result2["IsActive"]);
		}

		#endregion

		#region TestUsePartAttrib1

		public void TestUsePartAttrib1()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_UsePartAttrib1 = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_UsePartAttrib1 = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["UsePartAttrib1"]);
			AssertEquals(false, result2["UsePartAttrib1"]);
		}

		#endregion

		#region TestUsePartAttrib2

		public void TestUsePartAttrib2()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_UsePartAttrib2 = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_UsePartAttrib2 = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["UsePartAttrib2"]);
			AssertEquals(false, result2["UsePartAttrib2"]);
		}

		#endregion

		#region TestUsePartAttrib3

		public void TestUsePartAttrib3()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_UsePartAttrib3 = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_UsePartAttrib3 = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["UsePartAttrib3"]);
			AssertEquals(false, result2["UsePartAttrib3"]);
		}

		#endregion

		#region TestUseSerialNumber

		public void TestUseSerialNumber()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_UseSerialNumber = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_UseSerialNumber = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["UseSerialNumber"]);
			AssertEquals(false, result2["UseSerialNumber"]);
		}

		#endregion

		#region TestUsePackingDate

		public void TestUsePackingDate()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_UsePackingDate = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_UsePackingDate = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["UsePackingDate"]);
			AssertEquals(false, result2["UsePackingDate"]);
		}

		#endregion

		#region TestUseExpiryDate

		public void TestUseExpiryDate()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_UseExpiryDate = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_UseExpiryDate = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["UseExpiryDate"]);
			AssertEquals(false, result2["UseExpiryDate"]);
		}

		#endregion

		#region TestCompletePalletPicking

		public void TestCompletePalletPicking()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_CompletePalletPicking = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_CompletePalletPicking = false;
			Factory.Save();

			var results = LoadView();
			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["CompletePalletPicking"]);
			AssertEquals(false, result2["CompletePalletPicking"]);
		}

		#endregion

		#region TestIsPartAttrib1ReleaseCaptured

		public void TestIsPartAttrib1ReleaseCaptured()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_IsPartAttrib1ReleaseCaptured = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_IsPartAttrib1ReleaseCaptured = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["IsPartAttrib1ReleaseCaptured"]);
			AssertEquals(false, result2["IsPartAttrib1ReleaseCaptured"]);
		}

		#endregion

		#region TestIsPartAttrib2ReleaseCaptured

		public void TestIsPartAttrib2ReleaseCaptured()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_IsPartAttrib2ReleaseCaptured = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_IsPartAttrib2ReleaseCaptured = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["IsPartAttrib2ReleaseCaptured"]);
			AssertEquals(false, result2["IsPartAttrib2ReleaseCaptured"]);
		}

		#endregion

		#region TestIsPartAttrib3ReleaseCaptured

		public void TestIsPartAttrib3ReleaseCaptured()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_IsPartAttrib3ReleaseCaptured = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_IsPartAttrib3ReleaseCaptured = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["IsPartAttrib3ReleaseCaptured"]);
			AssertEquals(false, result2["IsPartAttrib3ReleaseCaptured"]);
		}

		#endregion

		#region TestIsSerialNumberReleaseCaptured

		public void TestIsSerialNumberReleaseCaptured()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var product1 = Helper.CreateProduct(client1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var product2 = Helper.CreateProduct(client2, "P2", OrgPartRelation.RelationshipTypes.Both);
			var product1OrganisationRelation = Helper.CreateProductClientRelationShip(client1, product1, OrgPartRelation.RelationshipTypes.Owner);
			product1OrganisationRelation.OU_IsSerialNumberReleaseCaptured = true;
			var product2OrganisationRelation = Helper.CreateProductClientRelationShip(client2, product2, OrgPartRelation.RelationshipTypes.Both);
			product2OrganisationRelation.OU_IsSerialNumberReleaseCaptured = false;
			Factory.Save();

			var results = LoadView();

			AssertEquals(2, results.Count);
			var result1 = results[0];
			var result2 = results[1];
			AssertEquals(true, result1["IsSerialNumberReleaseCaptured"]);
			AssertEquals(false, result2["IsSerialNumberReleaseCaptured"]);
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"select *
						   from WhsProductListingReport(null)
						   order by Client, Product, HazMat";

			var sqlParams = new ZSqlParameterCollection();
			result.Load(sql, sqlParams);
			return result;
		}

		#endregion
	}
}
