using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class ProcessingManagerTest : WhsTestCaseWithFactory
	{
		#region TestProcessingManager_CategorisesProductsWithABCAnalysisCorrectly

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_CategoryNameNotInAlphabeticOrder()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 97m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 3m) }, ZDateTimeOffset.Today.AddDays(-2));

			WarehouseDataRegistry.Instance.ABCAnalysisCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ABCAnalysisCategoryCollection
			{
				new ABCAnalysisCategory { CategoryName = "Grt", PercentageOfTotal = 80 },
				new ABCAnalysisCategory { CategoryName = "Ord", PercentageOfTotal = 15 },
				new ABCAnalysisCategory { CategoryName = "Bad", PercentageOfTotal = 5 },
				new ABCAnalysisCategory { CategoryName = "Wst", PercentageOfTotal = 5 }
			});

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::Grt 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::Bad 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2020, 1, 2, 3, 4, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_AnalysisDate_TimeCheck()
		{
			TestDateAttribute.UseUNLOCO = true;
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product, 97m) }, ZDateTimeOffset.Today.AddDays(-2));

			WarehouseDataRegistry.Instance.ABCAnalysisCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ABCAnalysisCategoryCollection
			{
				new ABCAnalysisCategory { CategoryName = "Grt", PercentageOfTotal = 80 },
				new ABCAnalysisCategory { CategoryName = "Ord", PercentageOfTotal = 15 },
				new ABCAnalysisCategory { CategoryName = "Bad", PercentageOfTotal = 5 },
				new ABCAnalysisCategory { CategoryName = "Wst", PercentageOfTotal = 5 }
			});

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var category = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery()).Single();
			AssertEquals(new ZDateTimeOffset(2020, 1, 2, 14, 4, 0), category.WJ_AnalysisDateFrom);
			AssertEquals(new ZDateTimeOffset(2020, 1, 2, 14, 4, 0), category.WJ_AnalysisDateTo);
			AssertEquals(new ZDateTime(2020, 1, 2, 3, 4, 0), category.WJ_SystemCreateTimeUtc);
			AssertEquals(new ZDateTime(2020, 1, 2, 3, 4, 0), category.WJ_SystemLastEditTimeUtc);
		}

		[TestTimeZoneUNLOCO("CNBJS")]
		[TestDate(2020, 1, 2, 3, 4, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_AnalysisDate_TimeCheck_differentTimeZone()
		{
			TestDateAttribute.UseUNLOCO = true;
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product, 97m) }, ZDateTimeOffset.Today.AddDays(-2));

			WarehouseDataRegistry.Instance.ABCAnalysisCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ABCAnalysisCategoryCollection
			{
				new ABCAnalysisCategory { CategoryName = "Grt", PercentageOfTotal = 80 },
				new ABCAnalysisCategory { CategoryName = "Ord", PercentageOfTotal = 15 },
				new ABCAnalysisCategory { CategoryName = "Bad", PercentageOfTotal = 5 },
				new ABCAnalysisCategory { CategoryName = "Wst", PercentageOfTotal = 5 }
			});

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var category = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery()).Single();
			AssertEquals(new ZDateTimeOffset(2020, 1, 2, 11, 4, 0), category.WJ_AnalysisDateFrom);
			AssertEquals(new ZDateTimeOffset(2020, 1, 2, 11, 4, 0), category.WJ_AnalysisDateTo);
			AssertEquals(new ZDateTime(2020, 1, 2, 3, 4, 0), category.WJ_SystemCreateTimeUtc);
			AssertEquals(new ZDateTime(2020, 1, 2, 3, 4, 0), category.WJ_SystemLastEditTimeUtc);
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_OverwriteExistingCategoryGeneratedOnSameDay()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("TRANS", "TRANSLOGIC", "QTC", "QLY");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R01", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R02", product2, 1000m);

			var abcCategoryWithSameDayAndSameKey = CreateABCCategory("A", "QLY", "0 80 QTC", product1.PK, client.PK, warehouse.PK, ZDateTimeOffset.Now, ZDateTimeOffset.Now);

			// Orders
			var x2_product1 = new OrderLineDetail(product1, 2m);
			var x2_product2 = new OrderLineDetail(product2, 2m);
			var x4_product1 = new OrderLineDetail(product1, 4m);
			CreateWhsOrderWithOrderLines(client, warehouse, "Order1", new[] { x2_product1 }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse, "Order2", new[] { x2_product2, x2_product1, x4_product1 }, ZDateTimeOffset.Today.AddDays(-16));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 QTC QLY::19-Nov-11 to 16-Feb-12
WHS::P2::B 80 95 QTC QLY::19-Nov-11 to 16-Feb-12
");
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_DoNotCreateCategoryForWarehouseHasNoTransactions()
		{
			var warehouse1 = CreateWarehouse("WHS", "Big Warehouse");
			var warehouse2 = CreateWarehouse("ABC", "ABC Warehouse");

			var client = CreateClient("TRANS", "TRANSLOGIC", "QTC", "QLY");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");

			// Data
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R01", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R02", product2, 1000m);

			Helper.CreateProductParamsByWhsAndClient(product2, client, warehouse1, 0);

			// Orders
			var x2_product1 = new OrderLineDetail(product1, 2m);
			var x2_product2 = new OrderLineDetail(product2, 2m);
			var x4_product1 = new OrderLineDetail(product1, 4m);
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order1", new[] { x2_product1 }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order2", new[] { x2_product2, x2_product1, x4_product1 }, ZDateTimeOffset.Today.AddDays(-16));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var warehouse2Categories = categories.Where(c => c.WJ_OH_Client == client.PK && c.WJ_WW_Warehouse == warehouse2.PK);
			AssertEquals(0, warehouse2Categories.Count());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_QuantityConsumedRatio()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("TRANS", "TRANSLOGIC", "QTC", "QLY");

			var product1 = Helper.CreateProduct(client, "BATS");
			var product2 = Helper.CreateProduct(client, "CATS");

			// Data
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R01", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R02", product2, 1000m);

			Helper.CreateProductParamsByWhsAndClient(product2, client, warehouse, 0);

			// Orders
			var x2_product1 = new OrderLineDetail(product1, 2m);
			var x2_product2 = new OrderLineDetail(product2, 2m);
			var x4_product1 = new OrderLineDetail(product1, 4m);
			CreateWhsOrderWithOrderLines(client, warehouse, "Order1", new[] { x2_product1 }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse, "Order2", new[] { x2_product2, x2_product1, x4_product1 }, ZDateTimeOffset.Today.AddDays(-16));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::BATS::A 0 80 QTC QLY::19-Nov-11 to 16-Feb-12
WHS::CATS::B 80 95 QTC QLY::19-Nov-11 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals("Product Params", 2, productParams.Length);
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExistDoNotCheckAuditInfo(productParams, client, warehouse, product2);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
TRANSLOGIC (TRANS)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 2, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
TRANSLOGIC (TRANS)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_VelocityRatio()
		{
			var warehouse = CreateWarehouse("BEC", "Beach Warehouse");
			var client = CreateClient("ATM", "ATMOS", "VLC", "DEF");

			var product1 = Helper.CreateProduct(client, "FATS");
			var product2 = Helper.CreateProduct(client, "LATS");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R01", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R02", product2, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "Order1", new[] { new OrderLineDetail(product1, 2m) }, ZDateTimeOffset.Today.AddDays(-2));
			for (int i = 2; i <= 25; i++)
			{
				CreateWhsOrderWithOrderLines(client, warehouse, $"Order{i}", new[] { new OrderLineDetail(product2, 2m) }, ZDateTimeOffset.Today.AddDays(-2));
			}

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
BEC::LATS::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
BEC::FATS::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals("Product Params", 2, productParams.Length);
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
ATMOS (ATM)
Warehouses:
Beach Warehouse (BEC)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 2, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
ATMOS (ATM)
Warehouses:
Beach Warehouse (BEC)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysisCorrectly_DefaultToRegistry()
		{
			var warehouse1 = CreateWarehouse("WHS", "Big Warehouse");
			var warehouse2 = CreateWarehouse("ABC", "ABC Warehouse");
			var warehouse3 = CreateWarehouse("BEC", "Beach Warehouse");

			var client = CreateClient("MAG", "MAGIC", "DEF", "MLY");
			var product1 = Helper.CreateProduct(client, "HATS");
			var product2 = Helper.CreateProduct(client, "RATS");

			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R01", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R02", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R03", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R04", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse3, "R05", product2, 1000m);

			Helper.CreateProductParamsByWhsAndClient(product1, client, warehouse1, 0);
			Helper.CreateProductParamsByWhsAndClient(product1, client, warehouse2, 0);

			//	Client:		ABC Analysis Method: DEF	-- VLC

			// existing ABCCategory
			//	warehouse1
			//		product1	A		2011-01-01
			//		product2	B		2010-12-01
			//		product2	C		2011-01-01
			//	warehouse2
			//		product1	A		2010-12-01

			var abcCategory1 = CreateABCCategory("A", "MLY", "0 80 VLC", product1.PK, client.PK, warehouse1.PK, ZDateTimeOffset.Today.AddYears(-1), ZDateTimeOffset.Today.AddYears(-1));
			var oldABCCategory = CreateABCCategory("B", "MLY", "80 95 VLC", product2.PK, client.PK, warehouse1.PK, ZDateTimeOffset.Today.AddYears(-2).AddDays(-16), ZDateTimeOffset.Today.AddYears(-2).AddDays(-16));
			var abcCategory2 = CreateABCCategory("C", "MLY", "95 100 VLC", product2.PK, client.PK, warehouse1.PK, ZDateTimeOffset.Today.AddYears(-1), ZDateTimeOffset.Today.AddYears(-1));
			var abcCategoryWithDifferentPeriod = CreateABCCategory("A", "QLY", "0 80 VLC", product1.PK, client.PK, warehouse2.PK, ZDateTimeOffset.Today.AddYears(-2).AddDays(-16), ZDateTimeOffset.Today.AddYears(-2).AddDays(-16));

			// orders
			//		warehouse1
			//			product1:	dockets 1 (order1), 1 (order3), 1 (order5), 1 (order6), 1 (order7)		5 dockets		A
			//			product2:	dockets	1 (order1)														1 docket		C
			//		warehouse2
			//			product1:	1 docket		A
			//			product2:	1 docket		A
			//		warehouse3
			//			product1:	1 docket		A

			CreateWhsOrderWithOrderLines(client, warehouse1, "Order1", new[] { new OrderLineDetail(product1, 2m), new OrderLineDetail(product2, 2m), new OrderLineDetail(product2, 4m), new OrderLineDetail(product2, 6m) }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse2, "Order2", new[] { new OrderLineDetail(product1, 2m), new OrderLineDetail(product2, 2m) }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order3", new[] { new OrderLineDetail(product1, 2m) }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse3, "Order4", new[] { new OrderLineDetail(product2, 2m) }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order5", new[] { new OrderLineDetail(product1, 1m) }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order6", new[] { new OrderLineDetail(product1, 1m) }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order7", new[] { new OrderLineDetail(product1, 1m) }, ZDateTimeOffset.Today.AddDays(-16));
			var order8 = Helper.CreateWhsOrder(client, warehouse1, "Order8");
			order8.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-16);
			Helper.CreateWhsOrderLine(order8, product1, 2m);

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 7, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
ABC::HATS::A 0 80 VLC QLY::03-Nov-09 to 31-Jan-10
ABC::HATS::A 0 80 VLC MLY::18-Jan-12 to 16-Feb-12
ABC::RATS::A 0 80 VLC MLY::18-Jan-12 to 16-Feb-12
BEC::RATS::A 0 80 VLC MLY::18-Jan-12 to 16-Feb-12
WHS::HATS::A 0 80 VLC MLY::18-Jan-12 to 16-Feb-12
WHS::RATS::B 80 95 VLC MLY::02-Jan-10 to 31-Jan-10
WHS::RATS::C 95 100 VLC MLY::18-Jan-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals("Product Params", 5, productParams.Length);
			AssertProductParamsExistDoNotCheckAuditInfo(productParams, client, warehouse1, product1);
			AssertProductParamsExist(productParams, client, warehouse1, product2);
			AssertProductParamsExistDoNotCheckAuditInfo(productParams, client, warehouse2, product1);
			AssertProductParamsExist(productParams, client, warehouse2, product2);
			AssertProductParamsExist(productParams, client, warehouse3, product2);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
MAGIC (MAG)
Warehouses:
ABC Warehouse (ABC)
Beach Warehouse (BEC)
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 7, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
MAGIC (MAG)
Warehouses:
ABC Warehouse (ABC)
Beach Warehouse (BEC)
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_AmnestyFailure()
		{
			// As part of WI00668321, this test was introduced to reproduce amnesty failure consistently
			// The amnesty was a result of batching by warehouse, and looping non-deterministically over the loaded warehouses
			var warehouse1 = CreateWarehouse("WH1", "Big Warehouse");
			var warehouse2 = CreateWarehouse("WH2", "Small Warehouse");

			var client = CreateClient("MAG", "MAGIC", "DEF", "MLY");
			var product1 = Helper.CreateProduct(client, "HATS");
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "R03", product1, 1000m);
			Helper.CreateProductParamsByWhsAndClient(product1, client, warehouse2, 0);
			var abcCategoryWithDifferentPeriod = CreateABCCategory("A", "QLY", "0 80 VLC", product1.PK, client.PK, warehouse2.PK, ZDateTimeOffset.Today.AddYears(-2).AddDays(-16), ZDateTimeOffset.Today.AddYears(-2).AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse2, "Order2", new[] { new OrderLineDetail(product1, 2m) }, ZDateTimeOffset.Today.AddDays(-16));
			Factory.Save();
			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();
			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WH2::HATS::A 0 80 VLC QLY::03-Nov-09 to 31-Jan-10
WH2::HATS::A 0 80 VLC MLY::18-Jan-12 to 16-Feb-12
");
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_AmnestyInvestigation()
		{
			var warehouse1 = CreateWarehouse("ABC", "ABC Warehouse");
			var warehouse2 = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");
			var product5 = Helper.CreateProduct(client, "P5");
			var product6 = Helper.CreateProduct(client, "P6");
			var product7 = Helper.CreateProduct(client, "P7");

			// Warehouse 1
			// 80%		(product1 79%)
			// 15%		(product2 14%)
			// 5%		(product3 4% product4 3%)
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "Receive4", product4, 1000m);

			CreateMultipleOrders(79, client, warehouse1, product1, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(14, client, warehouse1, product2, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(4, client, warehouse1, product3, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(3, client, warehouse1, product4, ZDateTimeOffset.Today.AddDays(-2));

			// Warehouse 2
			// 80%		(product4 79%)
			// 15%		(product5 14%)
			// 5%		(product6 4%, product7 3%)
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "Receive5", product4, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "Receive6", product5, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "Receive7", product6, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "Receive8", product7, 1000m);

			CreateMultipleOrders(79, client, warehouse2, product4, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(14, client, warehouse2, product5, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(4, client, warehouse2, product6, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(3, client, warehouse2, product7, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 8, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
ABC::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
ABC::P2::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
ABC::P3::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
ABC::P4::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P4::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P5::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P6::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P7::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysis_Exclude_WarehouseIsNotABCAnalysisEnabled()
		{
			var warehouseNonABC = CreateWarehouse("NON", "NonABC", false);
			var client = CreateClient("MAG", "MAGIC", "DEF", "MLY");
			var product = Helper.CreateProduct(client, "HATS");

			Helper.CreateWhsReceiveWithInventory(client, warehouseNonABC, "R01", product, 1000m);
			CreateWhsOrderWithOrderLines(client, warehouseNonABC, "Client1Order1", new[] { new OrderLineDetail(product, 2m) }, ZDateTimeOffset.Today.AddDays(-16));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			var client1warehouseNonABCcategories = categories.Where(c => c.WJ_OH_Client == client.PK && c.WJ_WW_Warehouse == warehouseNonABC.PK);
			AssertEquals(0, client1warehouseNonABCcategories.Count());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysis_Exclude_ClientIsNotABCAnalysisEnabled()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var clientNonABC = CreateClient("NON", "NonABC");
			var product = Helper.CreateProduct(clientNonABC, "ZZZZ");

			Helper.CreateWhsReceiveWithInventory(clientNonABC, warehouse, "R01", product, 1000m);
			CreateWhsOrderWithOrderLines(clientNonABC, warehouse, "Order1", new[] { new OrderLineDetail(product, 2m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());

			var clientNonABCcategories = categories.Where(c => c.WJ_OH_Client == clientNonABC.PK);
			AssertEquals(0, clientNonABCcategories.Count());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysis_Exclude_AnalysisDateIsOutOfAnalysisPeriod()
		{
			var warehouse = CreateWarehouse("ABC", "ABC Warehouse");
			var clientLCW = CreateClient("WEIRD", "WEIRDCO", "QTC", "LCW");
			var product = Helper.CreateProduct(clientLCW, "VATS");

			Helper.CreateWhsReceiveWithInventory(clientLCW, warehouse, "R01", product, 1000m);
			CreateWhsOrderWithOrderLines(clientLCW, warehouse, "ClientLCWOrder1", new[] { new OrderLineDetail(product, 2m), new OrderLineDetail(product, 4m), new OrderLineDetail(product, 6m) }, ZDateTimeOffset.Today.AddDays(-5));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();
			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());

			var clientLCWcategories = categories.Where(c => c.WJ_OH_Client == clientLCW.PK);
			AssertEquals(0, clientLCWcategories.Count());
		}

		#endregion

		#region TestABCAnalysisCalculation

		#region Method: VelocityRatio

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_VelocityRatio_AllProductRankSame()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive4", product4, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 2m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP3", new[] { new OrderLineDetail(product3, 3m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP4", new[] { new OrderLineDetail(product4, 4m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 4, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P4::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);
			AssertProductParamsExist(productParams, client, warehouse, product4);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 4, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_VelocityRatio_DecimalPercentage()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateMultipleOrders(170, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-3));
			CreateMultipleOrders(30, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-3));
			CreateMultipleOrders(10, client, warehouse, product3, ZDateTimeOffset.Today.AddDays(-3));

			//	P1:	170		start: 1 to 170/210 = 80.95%			A
			//	P2:	30		start:	80.95% to 200/210 = 95.23%		B
			//	P3:	10		start:	95.23% to 100%					C
			WarehouseDataRegistry.Instance.ABCAnalysisCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ABCAnalysisCategoryCollection
			{
				new ABCAnalysisCategory { CategoryName = "A", PercentageOfTotal = 80 },
				new ABCAnalysisCategory { CategoryName = "B", PercentageOfTotal = 15 },
				new ABCAnalysisCategory { CategoryName = "C", PercentageOfTotal = 5 },
				new ABCAnalysisCategory { CategoryName = "D", PercentageOfTotal = 5 },
			});

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_VelocityRatio_TwoProducts_OneDominating()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);

			CreateMultipleOrders(30, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2-1", new[] { new OrderLineDetail(product2, 40m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");
			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 2, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_VelocityRatio_OneProductOnly()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 1, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 1, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_VelocityRatio_ProductMatchRankCutOffPointsPrecisely()
		{
			// 80%
			// 15%
			// 5%
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateMultipleOrders(16, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));   // 16/20 = 80%
			CreateMultipleOrders(3, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-2));    // 3/20 = 15%
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP31", new[] { new OrderLineDetail(product3, 1) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 3, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_VelocityRatio_ProductsOverlapCutOffPoints()
		{
			// 80%		(product1 45%	product2 36%)
			// 15%		(product3 9%	product4 6%)
			// 5%		(product5 4%)
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CVLC", "VLC Client", "VLC", "DEF");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");
			var product5 = Helper.CreateProduct(client, "P5");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive4", product4, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive5", product5, 1000m);

			CreateMultipleOrders(45, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(36, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(9, client, warehouse, product3, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(6, client, warehouse, product4, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(4, client, warehouse, product5, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 5, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P4::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P5::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);
			AssertProductParamsExist(productParams, client, warehouse, product4);
			AssertProductParamsExist(productParams, client, warehouse, product5);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 5, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_VelocityRatio_NoProductMovedDuringTimePeriod()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var clientVLC = CreateClient("CVLC", "VLC Client", "VLC", "DEF");
			var product = Helper.CreateProduct(clientVLC, "P1");
			Helper.CreateWhsReceiveWithInventory(clientVLC, warehouse, "Receive1", product, 1000m);

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 0, categories.Length);

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals(0, productParams.Length);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 0, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client (CVLC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_VelocityRatio_MultipleWarehousesMultipleClients()
		{
			var warehouse1 = CreateWarehouse("WH1", "Warehouse1");
			var warehouse2 = CreateWarehouse("WH2", "Warehouse2");

			var client1 = CreateClient("CVLC1", "VLC Client 1", "VLC", "DEF");
			var client1Product1 = Helper.CreateProduct(client1, "P11");
			var client1Product2 = Helper.CreateProduct(client1, "P12");
			var client1Product3 = Helper.CreateProduct(client1, "P13");

			var client2 = CreateClient("CVLC2", "VLC Client 2", "VLC", "DEF");
			var client2Product1 = Helper.CreateProduct(client2, "P21");
			var client2Product2 = Helper.CreateProduct(client2, "P22");
			var client2Product3 = Helper.CreateProduct(client2, "P23");

			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R111", client1Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R121", client1Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R112", client1Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R122", client1Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R113", client1Product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R123", client1Product3, 1000m);

			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R211", client2Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R221", client2Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R212", client2Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R222", client2Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R213", client2Product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R223", client2Product3, 1000m);

			CreateMultipleOrders(55, client1, warehouse1, client1Product1, ZDateTimeOffset.Today.AddDays(-2));    //	P1: A
			CreateMultipleOrders(35, client1, warehouse1, client1Product2, ZDateTimeOffset.Today.AddDays(-2));    //	P2: A
			CreateMultipleOrders(10, client1, warehouse1, client1Product3, ZDateTimeOffset.Today.AddDays(-2));    //	P3:	B

			CreateMultipleOrders(85, client1, warehouse2, client1Product2, ZDateTimeOffset.Today.AddDays(-2));    //	P2: A
			CreateMultipleOrders(11, client1, warehouse2, client1Product3, ZDateTimeOffset.Today.AddDays(-2));    //	P3: B
			CreateMultipleOrders(4, client1, warehouse2, client1Product1, ZDateTimeOffset.Today.AddDays(-2));      //	P1: C

			CreateMultipleOrders(35, client2, warehouse1, client2Product1, ZDateTimeOffset.Today.AddDays(-2));     //	P1: A
			CreateMultipleOrders(35, client2, warehouse1, client2Product2, ZDateTimeOffset.Today.AddDays(-2));     //	P2: A
			CreateMultipleOrders(30, client2, warehouse1, client2Product3, ZDateTimeOffset.Today.AddDays(-2));     //	P3:	A

			CreateMultipleOrders(100, client2, warehouse2, client2Product1, ZDateTimeOffset.Today.AddDays(-2)); //	P1: A
			CreateMultipleOrders(1, client2, warehouse2, client2Product3, ZDateTimeOffset.Today.AddDays(-2));   //	P3:	C

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 11, categories.Length);

			var client1Categories = categories.Where(c => c.WJ_OH_Client == client1.PK).ToArray();
			AssertABCCategories(client1Categories, @"
WH1::P11::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH1::P12::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH1::P13::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WH2::P12::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH2::P13::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WH2::P11::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");

			var client2Categories = categories.Where(c => c.WJ_OH_Client == client2.PK).ToArray();
			AssertABCCategories(client2Categories, @"
WH1::P21::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH1::P22::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH1::P23::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH2::P21::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WH2::P23::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals("Product Params", 11, productParams.Length); // 6 + 5
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product1);
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product2);
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product3);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product1);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product2);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product3);

			AssertProductParamsExist(productParams, client2, warehouse1, client2Product1);
			AssertProductParamsExist(productParams, client2, warehouse1, client2Product2);
			AssertProductParamsExist(productParams, client2, warehouse1, client2Product3);
			AssertProductParamsExist(productParams, client2, warehouse2, client2Product1);
			AssertProductParamsExist(productParams, client2, warehouse2, client2Product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client 1 (CVLC1)
VLC Client 2 (CVLC2)
Warehouses:
Warehouse1 (WH1)
Warehouse2 (WH2)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 11, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
VLC Client 1 (CVLC1)
VLC Client 2 (CVLC2)
Warehouses:
Warehouse1 (WH1)
Warehouse2 (WH2)
".Trim(), Logger.ToString());
		}

		void CreateMultipleOrders(int countOfOrders, OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDateTimeOffset finalisedDate, bool finaliseOrder = true, bool finalisePick = false)
		{
			var orderReference = "O" + client.OH_Code.Trim() + warehouse.WW_WarehouseCode.Trim() + product.OP_PartNum.Trim();
			for (int i = 1; i <= countOfOrders; i++)
			{
				CreateWhsOrderWithOrderLines(client, warehouse, orderReference + i.ToString(), new[] { new OrderLineDetail(product, i) }, finalisedDate, finaliseOrder, finalisePick);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisMethod_DefaultClient_DefaultRegistry()
		{
			var expectedResult = @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12";

			TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: "DEF", expectedResult);
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisMethod_DefaultClient_QuantityConsumedFromRegistry()
		{
			using (WarehouseDataRegistry.Instance.ABCAnalysisMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsABCAnalysisMethodCodeList.Codes.QuantityConsumed))
			{
				var expectedResult = @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12";

				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: "DEF", expectedResult);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisMethod_NotDefaultClient_WithRegistrySetting()
		{
			using (WarehouseDataRegistry.Instance.ABCAnalysisMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsABCAnalysisMethodCodeList.Codes.QuantityConsumed))
			{
				var expectedResult = @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12";
				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "VLC", clientPeriod: "DEF", expectedResult);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisMethod_DefaultClient_RegistryCurrentCompany()
		{
			using (WarehouseDataRegistry.Instance.ABCAnalysisMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, WhsABCAnalysisMethodCodeList.Codes.QuantityConsumed))
			{
				var expectedResult = @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12";

				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: "DEF", expectedResult);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisMethod_DefaultClient_RegistryNotCurrentCompany()
		{
			var otherClient = Factory.NewWithValidTestData<OrgHeader>();
			using (WarehouseDataRegistry.Instance.ABCAnalysisMethod.SetTemporaryValue(otherClient.PK.ToGuid(), Guid.Empty, Guid.Empty, WhsABCAnalysisMethodCodeList.Codes.QuantityConsumed))
			{
				var expectedResult = @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12";

				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: "DEF", expectedResult);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisPriod_DefaultClient_MonthlyFromRegistry()
		{
			using (WarehouseDataRegistry.Instance.ABCAnalysisPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsABCAnalysisPeriodCodeList.Codes.Monthly))
			{
				var expectedResult = @"
WHS::P1::A 0 80 VLC MLY::18-Jan-12 to 16-Feb-12
WHS::P2::B 80 95 VLC MLY::18-Jan-12 to 16-Feb-12
WHS::P3::C 95 100 VLC MLY::18-Jan-12 to 16-Feb-12";

				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: "DEF", expectedResult);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisPriod_DefaultClient_RegistryNotCurrentCompany()
		{
			var otherClient = Factory.NewWithValidTestData<OrgHeader>();
			using (WarehouseDataRegistry.Instance.ABCAnalysisPeriod.SetTemporaryValue(otherClient.PK.ToGuid(), Guid.Empty, Guid.Empty, WhsABCAnalysisPeriodCodeList.Codes.Monthly))
			{
				var expectedResult = @"
WHS::P1::A 0 80 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 VLC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 VLC WKY::10-Feb-12 to 16-Feb-12";

				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: "DEF", expectedResult);
			}
		}

		[TestDate(2012, 2, 17)]
		public void TestABCAnalysis_ABCAnalysisPriod_NotDefaultClient_WithRegistrySetting()
		{
			using (WarehouseDataRegistry.Instance.ABCAnalysisPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsABCAnalysisPeriodCodeList.Codes.Monthly))
			{
				var expectedResult = @"
WHS::P1::A 0 80 VLC QLY::19-Nov-11 to 16-Feb-12
WHS::P2::B 80 95 VLC QLY::19-Nov-11 to 16-Feb-12
WHS::P3::C 95 100 VLC QLY::19-Nov-11 to 16-Feb-12";

				TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(clientMethod: "DEF", clientPeriod: WhsABCAnalysisPeriodCodeList.Codes.Quarterly, expectedResult);
			}
		}

		void TestABCAnalysis_ABCAnalysisMethod_ABCAnalysisPeriod_Core(string clientMethod, string clientPeriod, string expectedResult)
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("C01", "Test Client", clientMethod, clientPeriod);

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateMultipleOrders(16, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));   // 16/20 = 80%
			CreateMultipleOrders(3, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-2));    // 3/20 = 15%
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP31", new[] { new OrderLineDetail(product3, 1) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, expectedResult);

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
Test Client (C01)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 3, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
Test Client (C01)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		#endregion

		#region Method: QuantityConsumedRatio

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_AllProductRankSame()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive4", product4, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP3", new[] { new OrderLineDetail(product3, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP4", new[] { new OrderLineDetail(product4, 1m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 4, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P4::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);
			AssertProductParamsExist(productParams, client, warehouse, product4);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 4, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_DecimalPercentage()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1-1", new[] { new OrderLineDetail(product1, 10m) }, ZDateTimeOffset.Today.AddDays(-3));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2-1", new[] { new OrderLineDetail(product2, 10m) }, ZDateTimeOffset.Today.AddDays(-3));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP3-1", new[] { new OrderLineDetail(product3, 10m) }, ZDateTimeOffset.Today.AddDays(-3));

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 160m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 20m) }, ZDateTimeOffset.Today.AddDays(-2));

			//	P1:	170m		start: 1 to 170/210 = 80.95%			A
			//	P2:	30m			start:	80.95% to 200/210 = 95.23%		B
			//	P3:	10m			start:	95.23% to 100%					C
			WarehouseDataRegistry.Instance.ABCAnalysisCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ABCAnalysisCategoryCollection
			{
				new ABCAnalysisCategory { CategoryName = "A", PercentageOfTotal = 80 },
				new ABCAnalysisCategory { CategoryName = "B", PercentageOfTotal = 15 },
				new ABCAnalysisCategory { CategoryName = "C", PercentageOfTotal = 5 },
				new ABCAnalysisCategory { CategoryName = "D", PercentageOfTotal = 5 },
			});

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_TwoProducts_OneDominating()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 97m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 3m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 2, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_OneProductOnly()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 1, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 1, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_ProductMatchRankCutOffPointsPrecisely()
		{
			// 80%
			// 15%
			// 5%
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 80m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 15m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP3", new[] { new OrderLineDetail(product3, 5m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::B 80 95 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 3, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_ProductsOverlapCutOffPoints()
		{
			// 80%		(product1 45%	product2 36%)
			// 15%		(product3 9%	product4 6%)
			// 5%		(product5 4%)
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CQTC", "QTC Client", "QTC", "WKY");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");
			var product5 = Helper.CreateProduct(client, "P5");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive4", product4, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive5", product5, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 45m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 36m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP3", new[] { new OrderLineDetail(product3, 9m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP4", new[] { new OrderLineDetail(product4, 6m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP5", new[] { new OrderLineDetail(product5, 4m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 5, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P2::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P3::B 80 95 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P4::B 80 95 QTC WKY::10-Feb-12 to 16-Feb-12
WHS::P5::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);
			AssertProductParamsExist(productParams, client, warehouse, product4);
			AssertProductParamsExist(productParams, client, warehouse, product5);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 5, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_NoProductMovedDuringTimePeriod()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var clientQTC = CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(clientQTC, "P1");
			Helper.CreateWhsReceiveWithInventory(clientQTC, warehouse, "Receive1", product, 1000m);

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 0, categories.Length);

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals(0, productParams.Length);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 0, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client (CQTC)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_QuantityConsumedRatio_MultipleWarehousesMultipleClients()
		{
			var warehouse1 = CreateWarehouse("WH1", "Warehouse1");
			var warehouse2 = CreateWarehouse("WH2", "Warehouse2");

			var client1 = CreateClient("CQTC1", "QTC Client 1", "QTC", "DEF");
			var client1Product1 = Helper.CreateProduct(client1, "P11");
			var client1Product2 = Helper.CreateProduct(client1, "P12");
			var client1Product3 = Helper.CreateProduct(client1, "P13");

			var client2 = CreateClient("CQTC2", "QTC Client 2", "QTC", "DEF");
			var client2Product1 = Helper.CreateProduct(client2, "P21");
			var client2Product2 = Helper.CreateProduct(client2, "P22");
			var client2Product3 = Helper.CreateProduct(client2, "P23");

			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R111", client1Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R121", client1Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R112", client1Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R122", client1Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R113", client1Product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R123", client1Product3, 1000m);

			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R211", client2Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R221", client2Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R212", client2Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R222", client2Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R213", client2Product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R223", client2Product3, 1000m);

			CreateWhsOrderWithOrderLines(client1, warehouse1, "OrderC1W1P1", new[] { new OrderLineDetail(client1Product1, 55m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P1: A
			CreateWhsOrderWithOrderLines(client1, warehouse1, "OrderC1W1P2", new[] { new OrderLineDetail(client1Product2, 35m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P2: A
			CreateWhsOrderWithOrderLines(client1, warehouse1, "OrderC1W1P3", new[] { new OrderLineDetail(client1Product3, 10m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P3:	B

			CreateWhsOrderWithOrderLines(client1, warehouse2, "OrderC1W2P2", new[] { new OrderLineDetail(client1Product2, 85m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P2: A
			CreateWhsOrderWithOrderLines(client1, warehouse2, "OrderC1W2P3", new[] { new OrderLineDetail(client1Product3, 11m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P3: B
			CreateWhsOrderWithOrderLines(client1, warehouse2, "OrderC1W2P1", new[] { new OrderLineDetail(client1Product1, 4m) }, ZDateTimeOffset.Today.AddDays(-2));   //	P1: C

			CreateWhsOrderWithOrderLines(client2, warehouse1, "OrderC2W1P1", new[] { new OrderLineDetail(client2Product1, 35m) }, ZDateTimeOffset.Today.AddDays(-2));  //	P1: A
			CreateWhsOrderWithOrderLines(client2, warehouse1, "OrderC2W1P2", new[] { new OrderLineDetail(client2Product2, 35m) }, ZDateTimeOffset.Today.AddDays(-2));  //	P2: A
			CreateWhsOrderWithOrderLines(client2, warehouse1, "OrderC2W1P3", new[] { new OrderLineDetail(client2Product3, 30m) }, ZDateTimeOffset.Today.AddDays(-2));  //	P3:	A

			CreateWhsOrderWithOrderLines(client2, warehouse2, "OrderC2W2P1", new[] { new OrderLineDetail(client2Product1, 100m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P1: A
			CreateWhsOrderWithOrderLines(client2, warehouse2, "OrderC2W2P3", new[] { new OrderLineDetail(client2Product3, 1m) }, ZDateTimeOffset.Today.AddDays(-2));  //	P3: C

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 11, categories.Length);

			var client1Categories = categories.Where(c => c.WJ_OH_Client == client1.PK).ToArray();
			AssertABCCategories(client1Categories, @"
WH1::P11::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH1::P12::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH1::P13::B 80 95 QTC WKY::10-Feb-12 to 16-Feb-12
WH2::P12::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH2::P13::B 80 95 QTC WKY::10-Feb-12 to 16-Feb-12
WH2::P11::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");

			var client2Categories = categories.Where(c => c.WJ_OH_Client == client2.PK).ToArray();
			AssertABCCategories(client2Categories, @"
WH1::P21::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH1::P22::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH1::P23::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH2::P21::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH2::P23::C 95 100 QTC WKY::10-Feb-12 to 16-Feb-12
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals("Product Params", 11, productParams.Length);   // 6 + 5
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product1);
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product2);
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product3);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product1);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product2);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product3);

			AssertProductParamsExist(productParams, client2, warehouse1, client2Product1);
			AssertProductParamsExist(productParams, client2, warehouse1, client2Product2);
			AssertProductParamsExist(productParams, client2, warehouse1, client2Product3);
			AssertProductParamsExist(productParams, client2, warehouse2, client2Product1);
			AssertProductParamsExist(productParams, client2, warehouse2, client2Product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client 1 (CQTC1)
QTC Client 2 (CQTC2)
Warehouses:
Warehouse1 (WH1)
Warehouse2 (WH2)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 11, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
QTC Client 1 (CQTC1)
QTC Client 2 (CQTC2)
Warehouses:
Warehouse1 (WH1)
Warehouse2 (WH2)
".Trim(), Logger.ToString());
		}

		#endregion

		#region Method: Both

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_AllProductRankSame()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CBTH", "BTH Client", "BTH", "DEF");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive4", product4, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product1, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2", new[] { new OrderLineDetail(product2, 2m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP3", new[] { new OrderLineDetail(product3, 3m) }, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP4", new[] { new OrderLineDetail(product4, 4m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 4, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P2::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P3::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P4::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P1::A B 0 80 80 95 BTH WKY::23-Jan-24 to 29-Jan-24
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);
			AssertProductParamsExist(productParams, client, warehouse, product4);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 4, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_DecimalPercentage()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CBTH", "BTH Client", "BTH", "DEF");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateMultipleOrders(170, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-3));
			CreateMultipleOrders(30, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-3));
			CreateMultipleOrders(10, client, warehouse, product3, ZDateTimeOffset.Today.AddDays(-3));

			//	P1:	170		start: 1 to 170/210 = 80.95%			A
			//	P2:	30		start:	80.95% to 200/210 = 95.23%		B
			//	P3:	10		start:	95.23% to 100%					C
			WarehouseDataRegistry.Instance.ABCAnalysisCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ABCAnalysisCategoryCollection
			{
				new ABCAnalysisCategory { CategoryName = "A", PercentageOfTotal = 80 },
				new ABCAnalysisCategory { CategoryName = "B", PercentageOfTotal = 15 },
				new ABCAnalysisCategory { CategoryName = "C", PercentageOfTotal = 5 },
				new ABCAnalysisCategory { CategoryName = "D", PercentageOfTotal = 5 },
			});

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P2::B C 80 95 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P3::C C 95 100 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
");
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_TwoProducts_OneDominating()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CBTH", "BTH Client", "BTH", "DEF");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);

			CreateMultipleOrders(30, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP2-1", new[] { new OrderLineDetail(product2, 40m) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P2::C B 95 100 80 95 BTH WKY::23-Jan-24 to 29-Jan-24
");
			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 2, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_OneProductOnly()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var client = CreateClient("CBTH", "BTH Client", "BTH", "DEF");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product, 1000m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP1", new[] { new OrderLineDetail(product, 1m) }, ZDateTimeOffset.Today.AddDays(-2));
			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 1, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"WHS::P1::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 1, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_ProductMatchRankCutOffPointsPrecisely()
		{
			// 80%
			// 15%
			// 5%
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CBTH", "BTH Client", "BTH", "DEF");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);

			CreateMultipleOrders(16, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));   // 16/20 = 80%
			CreateMultipleOrders(3, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-2));    // 3/20 = 15%
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderP31", new[] { new OrderLineDetail(product3, 1) }, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 3, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P2::B C 80 95 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P3::C C 95 100 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 3, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_ProductsOverlapCutOffPoints()
		{
			// 80%		(product1 45%	product2 36%)
			// 15%		(product3 9%	product4 6%)
			// 5%		(product5 4%)
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");
			var client = CreateClient("CBTH", "BTH Client", "BTH", "DEF");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var product3 = Helper.CreateProduct(client, "P3");
			var product4 = Helper.CreateProduct(client, "P4");
			var product5 = Helper.CreateProduct(client, "P5");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive1", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive2", product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive3", product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive4", product4, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "Receive5", product5, 1000m);

			CreateMultipleOrders(45, client, warehouse, product1, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(36, client, warehouse, product2, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(9, client, warehouse, product3, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(6, client, warehouse, product4, ZDateTimeOffset.Today.AddDays(-2));
			CreateMultipleOrders(4, client, warehouse, product5, ZDateTimeOffset.Today.AddDays(-2));

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 5, categories.Length);

			var clientCategories = categories.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WHS::P1::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P2::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P3::B C 80 95 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P4::B C 80 95 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
WHS::P5::C C 95 100 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertProductParamsExist(productParams, client, warehouse, product1);
			AssertProductParamsExist(productParams, client, warehouse, product2);
			AssertProductParamsExist(productParams, client, warehouse, product3);
			AssertProductParamsExist(productParams, client, warehouse, product4);
			AssertProductParamsExist(productParams, client, warehouse, product5);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 5, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_NoProductMovedDuringTimePeriod()
		{
			var warehouse = CreateWarehouse("WHS", "Big Warehouse");

			var clientVLC = CreateClient("CBTH", "BTH Client", "BTH", "DEF");
			var product = Helper.CreateProduct(clientVLC, "P1");
			Helper.CreateWhsReceiveWithInventory(clientVLC, warehouse, "Receive1", product, 1000m);

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 0, categories.Length);

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals(0, productParams.Length);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 0, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client (CBTH)
Warehouses:
Big Warehouse (WHS)
".Trim(), Logger.ToString());
		}

		[TestDate(2024, 1, 30)]
		public void TestABCAnalysis_BothVelocityAndQuantityConsume_MultipleWarehousesMultipleClients()
		{
			var warehouse1 = CreateWarehouse("WH1", "Warehouse1");
			var warehouse2 = CreateWarehouse("WH2", "Warehouse2");

			var client1 = CreateClient("CBTH1", "BTH Client 1", "BTH", "DEF");
			var client1Product1 = Helper.CreateProduct(client1, "P11");
			var client1Product2 = Helper.CreateProduct(client1, "P12");
			var client1Product3 = Helper.CreateProduct(client1, "P13");

			var client2 = CreateClient("CBTH2", "BTH Client 2", "BTH", "DEF");
			var client2Product1 = Helper.CreateProduct(client2, "P21");
			var client2Product2 = Helper.CreateProduct(client2, "P22");
			var client2Product3 = Helper.CreateProduct(client2, "P23");

			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R111", client1Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R121", client1Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R112", client1Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R122", client1Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R113", client1Product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R123", client1Product3, 1000m);

			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R211", client2Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R221", client2Product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R212", client2Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R222", client2Product2, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R213", client2Product3, 1000m);
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "R223", client2Product3, 1000m);

			CreateMultipleOrders(55, client1, warehouse1, client1Product1, ZDateTimeOffset.Today.AddDays(-2));    //	P1: A
			CreateMultipleOrders(35, client1, warehouse1, client1Product2, ZDateTimeOffset.Today.AddDays(-2));    //	P2: A
			CreateMultipleOrders(10, client1, warehouse1, client1Product3, ZDateTimeOffset.Today.AddDays(-2));    //	P3:	B

			CreateMultipleOrders(85, client1, warehouse2, client1Product2, ZDateTimeOffset.Today.AddDays(-2));    //	P2: A
			CreateMultipleOrders(11, client1, warehouse2, client1Product3, ZDateTimeOffset.Today.AddDays(-2));    //	P3: B
			CreateMultipleOrders(4, client1, warehouse2, client1Product1, ZDateTimeOffset.Today.AddDays(-2));      //	P1: C

			CreateMultipleOrders(35, client2, warehouse1, client2Product1, ZDateTimeOffset.Today.AddDays(-2));     //	P1: A
			CreateMultipleOrders(35, client2, warehouse1, client2Product2, ZDateTimeOffset.Today.AddDays(-2));     //	P2: A
			CreateMultipleOrders(30, client2, warehouse1, client2Product3, ZDateTimeOffset.Today.AddDays(-2));     //	P3:	A

			CreateMultipleOrders(100, client2, warehouse2, client2Product1, ZDateTimeOffset.Today.AddDays(-2)); //	P1: A
			CreateMultipleOrders(1, client2, warehouse2, client2Product3, ZDateTimeOffset.Today.AddDays(-2));   //	P3:	C

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 11, categories.Length);

			var client1Categories = categories.Where(c => c.WJ_OH_Client == client1.PK).ToArray();
			AssertABCCategories(client1Categories, @"
WH1::P11::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH1::P12::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH1::P13::B C 80 95 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
WH2::P12::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH2::P13::B C 80 95 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
WH2::P11::C C 95 100 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
");

			var client2Categories = categories.Where(c => c.WJ_OH_Client == client2.PK).ToArray();
			AssertABCCategories(client2Categories, @"
WH1::P21::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH1::P22::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH1::P23::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH2::P21::A A 0 80 0 80 BTH WKY::23-Jan-24 to 29-Jan-24
WH2::P23::C C 95 100 95 100 BTH WKY::23-Jan-24 to 29-Jan-24
");

			var productParams = new BusinessObjectFactory().Load<WhsProductParamsByWhsAndClient>(new ZQuery());
			AssertEquals("Product Params", 11, productParams.Length); // 6 + 5
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product1);
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product2);
			AssertProductParamsExist(productParams, client1, warehouse1, client1Product3);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product1);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product2);
			AssertProductParamsExist(productParams, client1, warehouse2, client1Product3);

			AssertProductParamsExist(productParams, client2, warehouse1, client2Product1);
			AssertProductParamsExist(productParams, client2, warehouse1, client2Product2);
			AssertProductParamsExist(productParams, client2, warehouse1, client2Product3);
			AssertProductParamsExist(productParams, client2, warehouse2, client2Product1);
			AssertProductParamsExist(productParams, client2, warehouse2, client2Product3);

			AssertMultilineASCIIEquals("logged Messages", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client 1 (CBTH1)
BTH Client 2 (CBTH2)
Warehouses:
Warehouse1 (WH1)
Warehouse2 (WH2)
".Trim(), Logger.ToString());

			Logger.ClearLog();
			processingManager.DoABCAnalysis();

			categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("Running Analysis again with no changes should achieve same result", 11, categories.Length);
			AssertMultilineASCIIEquals("Temporary Tables were dropped and thus Analysis works properly again.", @"
Information|ABC Analysis completed successfully.
Information|Clients and warehouses included in this Analysis were:
Clients:
BTH Client 1 (CBTH1)
BTH Client 2 (CBTH2)
Warehouses:
Warehouse1 (WH1)
Warehouse2 (WH2)
".Trim(), Logger.ToString());
		}

		#endregion

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2012, 2, 17, 1, 0, 0)]
		public void TestABCAnalysis_AuditFields()
		{
			TestDateAttribute.UseUNLOCO = true;
			var warehouse = CreateWarehouse("WH", "Warehouse");

			var client = CreateClient("CQTC1", "QTC Client 1", "QTC", "DEF");
			var client1Product1 = Helper.CreateProduct(client, "P11");
			var client1Product2 = Helper.CreateProduct(client, "P12");

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R111", client1Product1, 500m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R112", client1Product2, 300m);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderC1W1P1", new[] { new OrderLineDetail(client1Product1, 55m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P1: A
			CreateWhsOrderWithOrderLines(client, warehouse, "OrderC1W1P2", new[] { new OrderLineDetail(client1Product2, 35m) }, ZDateTimeOffset.Today.AddDays(-2)); //	P2: A

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			var categories1 = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories1.Length);

			var clientCategories = categories1.Where(c => c.WJ_OH_Client == client.PK).ToArray();
			AssertABCCategories(clientCategories, @"
WH::P11::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
WH::P12::A 0 80 QTC WKY::10-Feb-12 to 16-Feb-12
");
			var now = ZDateTime.UtcNow;
			AssertEquals("WJ_SystemCreateTimeUtc should set value.", true, categories1.All(c => c.WJ_SystemCreateTimeUtc == now));
			AssertEquals("WJ_SystemLastEditTimeUtc should set value.", true, categories1.All(c => c.WJ_SystemLastEditTimeUtc == now));
			AssertEquals("WJ_SystemCreateUser should set value.", true, categories1.All(c => c.WJ_SystemCreateUser == "~BP"));
			AssertEquals("WJ_SystemLastEditUser should set value.", true, categories1.All(c => c.WJ_SystemLastEditUser == "~BP"));

			TestDateAttribute.AddDays(1);

			CreateWhsOrderWithOrderLines(client, warehouse, "OrderC1W1P1_2", new[] { new OrderLineDetail(client1Product1, 44m) }, ZDateTimeOffset.Today.AddDays(-1));
			Factory.Save();

			processingManager.DoABCAnalysis();

			var categories2 = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
			AssertEquals("ABC Categories", 2, categories2.Length);

			AssertEquals("WJ_SystemCreateTimeUtc should not change.", true, categories2.All(c => c.WJ_SystemCreateTimeUtc == now));
			AssertEquals("Should update WJ_SystemLastEditTimeUtc.", true, categories2.All(c => c.WJ_SystemLastEditTimeUtc == now.AddDays(1)));
			AssertEquals("WJ_SystemCreateUser should not change.", true, categories2.All(c => c.WJ_SystemCreateUser == "~BP"));
			AssertEquals("WJ_SystemLastEditUser should not change.", true, categories2.All(c => c.WJ_SystemLastEditUser == "~BP"));
		}

		#endregion

		#region TestProcessingManagerLogsErrors

		public void TestProcessingManagerLogsErrors()
		{
			var warehouse = CreateWarehouse("XXX", "XXX");
			Db.Connection.ExecuteNonQuery("DROP FUNCTION WhsProductListingReport DROP VIEW WhsPickFaceView DROP VIEW WhsDynamicPickFaceView DROP TABLE WhsABCCategory"); // This is a test!!

			var client = CreateClient("XXX", "XXX", "XXX", "XXX");
			var product1 = Helper.CreateProduct(client, "HATS");

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1");
			Helper.CreateWhsOrderLine(order1, product1, 6m);

			Factory.Save();

			var processingManager = GetNewProcessingManager();
			processingManager.DoABCAnalysis();

			AssertMultilineASCIIEquals("Logger has errors",
@"Error|Invalid object name 'dbo.WhsABCCategory'.
Information|ABC Analysis failed.
".Trim(), Logger.ToString());
		}

		#endregion

		#region TestProcessingManager_ABCAnalysis_SetsUserContext

		[TestDate(2023, 10, 5, 1, 0, 0)]
		public void TestProcessingManager_ABCAnalysis_SetsUserContext()
		{
			var warehouse1 = CreateWarehouse("WHS", "Big Warehouse");
			var warehouse2 = CreateWarehouse("ABC", "ABC Warehouse");
			var warehouse3 = CreateWarehouse("DEF", "ABC Warehouse");
			warehouse3.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse3.WW_GB_RelatedCompanyBranch = warehouse2.WW_GB_RelatedCompanyBranch;

			var client = CreateClient("TRANS", "TRANSLOGIC", "QTC", "QLY");

			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");

			// Data
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R01", product1, 1000m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R02", product2, 1000m);

			Helper.CreateProductParamsByWhsAndClient(product2, client, warehouse1, 0);

			// Orders
			var x2_product1 = new OrderLineDetail(product1, 2m);
			var x2_product2 = new OrderLineDetail(product2, 2m);
			var x4_product1 = new OrderLineDetail(product1, 4m);
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order1", new[] { x2_product1 }, ZDateTimeOffset.Today.AddDays(-16));
			CreateWhsOrderWithOrderLines(client, warehouse1, "Order2", new[] { x2_product2, x2_product1, x4_product1 }, ZDateTimeOffset.Today.AddDays(-16));

			Factory.Save();

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;
				var processingManager = GetNewProcessingManager();
				processingManager.DoABCAnalysis();

				var categories = new BusinessObjectFactory().Load<WhsABCCategory>(new ZQuery());
				AssertEquals("ABC Categories", 2, categories.Length);

				var warehouse2Categories = categories.Where(c => c.WJ_OH_Client == client.PK && c.WJ_WW_Warehouse == warehouse2.PK);
				AssertEquals(0, warehouse2Categories.Count());

				var warehouse3Categories = categories.Where(c => c.WJ_OH_Client == client.PK && c.WJ_WW_Warehouse == warehouse3.PK);
				AssertEquals(0, warehouse3Categories.Count());

				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}
		}

		#endregion

		#region Implementation

		ABCAnalysisProcessingManager GetNewProcessingManager()
		{
			return new ABCAnalysisProcessingManager(Logger);
		}

		readonly TestServiceLogger Logger = new TestServiceLogger();

		OrgHeader CreateClient(string code, string name, string abcAnalysisMethod = "", string abcAnalysisPeriod = "")
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = code;
			client.OH_FullName = name;
			client.OH_IsWarehouseClient = true;

			if (!string.IsNullOrEmpty(abcAnalysisMethod + abcAnalysisPeriod))
			{
				client.MiscServ.OM_WhsABCAnalysisEnabled = true;
				client.MiscServ.OM_WhsABCAnalysisMethod = abcAnalysisMethod;
				client.MiscServ.OM_WhsABCAnalysisPeriod = abcAnalysisPeriod;
			}

			return client;
		}

		WhsWarehouse CreateWarehouse(string code, string name, bool includeInABCAnalysis = true)
		{
			var warehouse = Helper.CreateWarehouse(name, code, "A");
			warehouse.WW_ABCAnalysisEnabled = includeInABCAnalysis;
			return warehouse;
		}

		WhsABCCategory CreateABCCategory(string category, string analysisPeriod, string key, ZGuid productPK, ZGuid clientPK, ZGuid warehousePK, ZDateTimeOffset dateFrom, ZDateTimeOffset dateTo)
		{
			var abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_Category = category;
			abcCategory.WJ_AnalysisPeriod = analysisPeriod;
			abcCategory.WJ_Key = key;
			abcCategory.WJ_OP_Product = productPK;
			abcCategory.WJ_OH_Client = clientPK;
			abcCategory.WJ_WW_Warehouse = warehousePK;
			abcCategory.WJ_AnalysisDateFrom = dateFrom;
			abcCategory.WJ_AnalysisDateTo = dateTo;
			abcCategory.WJ_SystemCreateUser = "~BP";
			abcCategory.WJ_SystemLastEditUser = "~BP";

			return abcCategory;
		}

		WhsOrder CreateWhsOrderWithOrderLines(OrgHeader client, WhsWarehouse warehouse, ZString reference, OrderLineDetail[] lineDetails, ZDateTimeOffset finalisedDate, bool finaliseOrder = true, bool finalisePick = false)
		{
			var order = Helper.CreateWhsOrder(client, warehouse, reference);
			foreach (var line in lineDetails)
			{
				Helper.CreateWhsOrderLine(order, line.Product, line.Units);
			}
			Helper.CreatePickNew(finaliseOrders: finaliseOrder, finalisePick: finalisePick, pickableDockets: order);
			order.WD_FinalisedDate = finalisedDate;

			return order;
		}

		class OrderLineDetail
		{
			public OrderLineDetail(OrgSupplierPart product, ZDecimal units)
			{
				this.Product = product;
				this.Units = units;
			}

			public OrgSupplierPart Product { get; set; }
			public ZDecimal Units { get; set; }
		}

		void AssertABCCategories(WhsABCCategory[] clientABCCategories, string expectedCategoryString)
		{
			var categories = clientABCCategories.OrderBy(c => c.Warehouse.WW_WarehouseCode).ThenBy(c => c.WJ_Key).ThenBy(c => c.Product.OP_PartNum).ThenBy(c => c.WJ_AnalysisDateTo);

			var sb = new ZStringBuilder();
			foreach (WhsABCCategory category in categories)
			{
				sb.AppendLine($"{category.Warehouse.WW_WarehouseCode}::{category.Product.OP_PartNum}::{category.WJ_Category} {category.WJ_Key} {category.WJ_AnalysisPeriod}::{category.ABCAnalysisPeriodAsString}");
			}
			var actualCategoryString = sb.ToString();

			AssertEquals(expectedCategoryString.Trim(), actualCategoryString.Trim());
		}

		WhsProductParamsByWhsAndClient AssertProductParamsExistDoNotCheckAuditInfo(WhsProductParamsByWhsAndClient[] productParams, OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product)
		{
			var productParam = productParams.Single(p => p.W3_OH == client.PK && p.W3_WW == warehouse.PK && p.W3_OP == product.PK);
			AssertNotNull(productParam);

			return productParam;
		}

		void AssertProductParamsExist(WhsProductParamsByWhsAndClient[] productParams, OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product)
		{
			var productParam = AssertProductParamsExistDoNotCheckAuditInfo(productParams, client, warehouse, product);
			Assert(productParam.W3_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1));
			AssertEquals("~BP", productParam.W3_SystemCreateUser);
			Assert(productParam.W3_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1));
			AssertEquals("~BP", productParam.W3_SystemLastEditUser);
		}

		#endregion
	}
}
