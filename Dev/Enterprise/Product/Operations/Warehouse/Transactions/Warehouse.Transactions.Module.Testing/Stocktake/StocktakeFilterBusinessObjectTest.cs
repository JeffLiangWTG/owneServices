using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	#region StocktakeFilterBusinessObjectTest

	[TestedType(typeof(StocktakeFilterBusinessObject))]
	public class StocktakeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filter Warehouse

		public virtual void TestFilterByStocktakeNumber()
		{
			SetupTestData();
			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Stocktake No", (ZString)"SK00000001");
			StocktakeAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Stocktake No", (ZString)"00000002");
			StocktakeAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Stocktake No", (ZString)"3");
			StocktakeAssert(false, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Stocktake No", (ZString)"0004");
			StocktakeAssert(false, false, false, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Stocktake No", ZString.Empty);
			StocktakeAssert(true, true, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Stocktake No", (ZString)"0005");
			StocktakeAssert(false, false, false, false);
		}

		public virtual void TestFilterWarehouse()
		{
			SetupTestData();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Warehouse", Whs1.PK);
			StocktakeAssert(true, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Warehouse", Whs2.PK);
			StocktakeAssert(false, true, false, true);
		}

		public virtual void TestFilterWarehouseVisibility()
		{
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			var filter = (ModuleGuidFilter)StocktakeFilter["Warehouse"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				var stocktakeFilter1 = new StocktakeFilterBusinessObject();
				var filter1 = (ModuleGuidFilter)stocktakeFilter1["Warehouse"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					var stocktakeFilter2 = new StocktakeFilterBusinessObject();
					var filter2 = (ModuleGuidFilter)stocktakeFilter2["Warehouse"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			}
		}

		#endregion

		#region TestFilterClient

		public virtual void TestFilterClient()
		{
			SetupTestData();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", Org1.PK);
			StocktakeAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", Org2.PK);
			StocktakeAssert(false, false, true, true);
		}

		public virtual void TestFilterClientVisibility()
		{
			AssertEquals("Precondition: WhsAllowedClients", true, Env.Security.WhsAllowedClients.IsAllowed);
			var filter = (ModuleGuidFilter)StocktakeFilter["Client"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedClients.IsAllowed = false;
			try
			{
				var stocktakeFilter1 = new StocktakeFilterBusinessObject();
				var filter1 = (ModuleGuidFilter)stocktakeFilter1["Client"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					var stocktakeFilter2 = new StocktakeFilterBusinessObject();
					var filter2 = (ModuleGuidFilter)stocktakeFilter2["Client"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedClients.IsAllowed = true;
			}
		}

		#endregion

		#region TestStatusFilter

		public void TestStatusFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktakes with different status

			var stocktakeWithStatusNew = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);
			var stocktakewithStatusLoaded = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var stocktakeWithStatusFinalised = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Finalised);

			Asserter.AddStocktakesToScope(stocktakeWithStatusNew, stocktakewithStatusLoaded, stocktakeWithStatusFinalised);

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["Status"];
			AssertEquals("Status filter category should be StatusAndFlags.", filter.Category, FilterCategories.StatusAndFlags);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithStatusNew, stocktakewithStatusLoaded, stocktakeWithStatusFinalised);

			filter.Property = StocktakeStatus.Codes.New;
			Asserter.AssertMatches("Since the filter value is new it should return only stockTake1.", filter, stocktakeWithStatusNew);

			filter.Property = StocktakeStatus.Codes.Loaded;
			Asserter.AssertMatches("Since the filter value is Loaded it should return only stockTake2.", filter, stocktakewithStatusLoaded);

			filter.Property = StocktakeStatus.Codes.Finalised;
			Asserter.AssertMatches("Since the filter value is Loaded it should return only stockTake3.", filter, stocktakeWithStatusFinalised);
		}

		#endregion

		#region TestDateOpenedFilter

		public void TestDateOpenedFilter()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes with stocktake dates

			var stocktakeOpenedTwoDayAgo = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeOpenedFiveDaysAgo = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeOpenedTwentyDaysAgo = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoOpenedDate = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktakeOpenedTwoDayAgo.WS_StocktakeDate = now.AddDays(-2);
			stocktakeOpenedFiveDaysAgo.WS_StocktakeDate = now.AddDays(-5);
			stocktakeOpenedTwentyDaysAgo.WS_StocktakeDate = now.AddDays(-20);
			//stockTake4.WS_StocktakeDate has empty datetime.

			Asserter.AddStocktakesToScope(stocktakeOpenedTwoDayAgo, stocktakeOpenedFiveDaysAgo, stocktakeOpenedTwentyDaysAgo, stocktakeWithNoOpenedDate);

			// test the filter

			var filter = (ModuleDateFilter)StocktakeFilter["DateOpened"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("Date Opened filter category should be Dates.", filter.Category, FilterCategories.Dates);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeOpenedTwoDayAgo, stocktakeOpenedFiveDaysAgo, stocktakeOpenedTwentyDaysAgo, stocktakeWithNoOpenedDate);

			filter.Property1 = now.AddDays(-2);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("Since the filter value is two days ahead it should return stockTake1 only.", filter, stocktakeOpenedTwoDayAgo);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(-4);
			Asserter.AssertMatches("Since the filter value is in a range it should return stockTake2 only.", filter, stocktakeOpenedFiveDaysAgo);

			filter.Property1 = now.AddDays(-10);
			filter.Property2 = now.AddDays(-10);
			Asserter.AssertMatches("There are no stocktake items associated with this DateOpened value.", filter);
		}

		#endregion

		#region TestCycleFilter

		public void TestCycleFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes with Cycle

			var stocktakeWithCycle1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithCycle2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoCycle = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktakeWithCycle1.WS_StocktakeCycle = "C1";
			stocktakeWithCycle2.WS_StocktakeCycle = "C2";
			stocktakeWithNoCycle.WS_StocktakeCycle = "";

			Asserter.AddStocktakesToScope(stocktakeWithCycle1, stocktakeWithCycle2, stocktakeWithNoCycle);

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["Cycle"];
			AssertEquals("Cycle filter category should be StatusAndFlags.", filter.Category, FilterCategories.StatusAndFlags);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithCycle1, stocktakeWithCycle2, stocktakeWithNoCycle);

			filter.Property = "C1";
			Asserter.AssertMatches("Since the filter value is C1 it should return stockTake1 only.", filter, stocktakeWithCycle1);

			filter.Property = "C3";
			Asserter.AssertMatches("There are no stocktake items associated with this Cycle.", filter);
		}

		#endregion

		#region TestProductFilter

		#region TestProductFilter

		public void TestProductFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2", "O2");
			Helper.CreateProductClientRelationShip(org2, data.Part2);
			var part3 = Helper.CreateProduct(org2, "T1");

			// Setup stocktakes with two different products

			var stocktakeWithClient1Part1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithClient1Part2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part2);
			var stocktakeWithClient2Part2 = Helper.CreateWhsStocktake(org2, data.Whs1, data.Part2);
			var stocktakeWithNoProductAndClient = Helper.CreateWhsStocktake(org2, data.Whs1); //Stocktake4 doesn't have a product

			Asserter.AddStocktakesToScope(stocktakeWithClient1Part1, stocktakeWithClient1Part2, stocktakeWithClient2Part2, stocktakeWithNoProductAndClient);

			Factory.Save();

			// test the filter

			var filter = (ModuleGuidFilter)StocktakeFilter["Product"];

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product");
			AssertEquals("Product filter category should be Other.", filter.Category, FilterCategories.Other);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", StocktakeFilter.Filter, stocktakeWithClient1Part1, stocktakeWithClient1Part2, stocktakeWithClient2Part2, stocktakeWithNoProductAndClient);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part1.PK);
			Asserter.AssertMatches("Since the filter value is Part1 it should return stockTake1 only.", StocktakeFilter.Filter, stocktakeWithClient1Part1);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part2.PK);
			Asserter.AssertMatches("Since the filter value is Part2 it should return stocktake2 only.", StocktakeFilter.Filter, stocktakeWithClient1Part2);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", org2.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part2.PK);
			Asserter.AssertMatches("Since the filter value is Part2 it should return stockTake3 only.", StocktakeFilter.Filter, stocktakeWithClient2Part2);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", org2.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", part3.PK);
			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertEquals("product part3 doesn't have any stocktakes.", 0, StocktakeCollection.Count);
		}

		#endregion

		#region TestProductFilter_Validation

		public void TestProductFilter_Validation()
		{
			var client = Helper.CreateClient("CLIENT1");
			var part = Helper.CreateProduct(client, "P1");

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var productFilter = (ModuleGuidFilter)filter["Product"];
			productFilter.IsActive = true;
			productFilter.Property = part.PK; // need to double set so validation will be run on empty value
			productFilter.Property = ZGuid.Empty;
			AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			AssertNoError(productFilter.PropertyInfo, "Enter a valid selection.");

			productFilter.Property = part.PK;
			AssertHasError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			AssertHasError(productFilter.PropertyInfo, "Enter a valid selection.");
			productFilter.Property = ZGuid.Empty; // clean up

			var clientFilter = (ModuleGuidFilter)filter["Client"];
			clientFilter.IsActive = true;
			clientFilter.Property = client.PK;
			productFilter.Property = part.PK;
			AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			AssertNoError(productFilter.PropertyInfo, "Enter a valid selection.");
		}

		#endregion

		#region TestProductFilter_ReValidation

		public void TestProductFilter_ReValidation()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var clientFilter = (ModuleGuidFilter)filter["Client"];
			var productFilter = (ModuleGuidFilter)filter["Product"];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;
			FieldInvalidTextMemory.SetInvalidText(productFilter, productFilter.PropertyInfo.Name, "AA");
			productFilter.Property = ZGuid.Missing;
			AssertNoExceptionThrown("No Exception should be thrown for invalid product code.", () => clientFilter.Property = client.PK);
			AssertEquals("Product PK should not be changed.", ZGuid.Missing, productFilter.Property);
			AssertEquals("Invalid Product code should not be cleared out.", "AA", FieldInvalidTextMemory.GetInvalidText(productFilter, productFilter.PropertyInfo.Name));
		}

		#endregion

		#region TestProductFilter_WithStocktakeLines

		public void TestProductFilter_WithStocktakeLines()
		{
			// Setup test data

			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "T3");
			var part4 = Helper.CreateProduct(data.Org1, "T4");

			var stocktakeWithMultipleTypeOfProducts = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithSingleTypeOfProduct = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithoutMatchingProduct = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoLines = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			Helper.CreateWhsStocktakeLine(stocktakeWithMultipleTypeOfProducts, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			Helper.CreateWhsStocktakeLine(stocktakeWithMultipleTypeOfProducts, data.Org1, data.Part2, data.Whs1.DefaultLocation);
			Helper.CreateWhsStocktakeLine(stocktakeWithSingleTypeOfProduct, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			Helper.CreateWhsStocktakeLine(stocktakeWithoutMatchingProduct, data.Org1, part3, data.Whs1.DefaultLocation);

			Asserter.AddStocktakesToScope(stocktakeWithMultipleTypeOfProducts, stocktakeWithSingleTypeOfProduct, stocktakeWithoutMatchingProduct, stocktakeWithNoLines);

			Factory.Save();

			// test the filter

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product");
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", StocktakeFilter.Filter, stocktakeWithMultipleTypeOfProducts, stocktakeWithSingleTypeOfProduct, stocktakeWithoutMatchingProduct, stocktakeWithNoLines);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part1.PK);
			Asserter.AssertMatches("Since the filter value is Part1 it should return stocktakeWithMultipleTypeOfProducts and stocktakeWithSingleTypeOfProduct only.", StocktakeFilter.Filter, stocktakeWithMultipleTypeOfProducts, stocktakeWithSingleTypeOfProduct);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part2.PK);
			Asserter.AssertMatches("Since the filter value is Part2 it should return stocktakeWithMultipleTypeOfProducts only.", StocktakeFilter.Filter, stocktakeWithMultipleTypeOfProducts);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", part4.PK);
			Asserter.AssertMatches("Since the filter value is Part4 it should not return any lines.", StocktakeFilter.Filter);
		}

		#endregion

		#region TestProductFilterWithManyProductsInStocktake

		public void TestProductFilterWithManyProductsInStocktake()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktakeWithOneProduct = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithTwoProducts = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Helper.CreateWhsStocktakeProductFilter(stocktakeWithTwoProducts, data.Part1);
			Helper.CreateWhsStocktakeProductFilter(stocktakeWithTwoProducts, data.Part2);
			Asserter.AddStocktakesToScope(stocktakeWithOneProduct, stocktakeWithTwoProducts);

			Factory.Save();

			var filter = (ModuleGuidFilter)StocktakeFilter["Product"];
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product");
			AssertEquals("Product filter category should be Other.", filter.Category, FilterCategories.Other);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", StocktakeFilter.Filter, stocktakeWithOneProduct, stocktakeWithTwoProducts);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part1.PK);
			Asserter.AssertMatches("Since the filter value is Part1 it should return both stocktakes.", StocktakeFilter.Filter, stocktakeWithOneProduct, stocktakeWithTwoProducts);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Client", data.Org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product", data.Part2.PK);
			Asserter.AssertMatches("Since the filter value is Part2 it should return only second stocktake.", StocktakeFilter.Filter, stocktakeWithTwoProducts);
		}

		#endregion

		#endregion

		#region TestProductCategoryFilter

		public void TestProductCategoryFilter()
		{
			var warehouse = Helper.CreateWarehouse("Warehouse", "A");
			var client = Helper.CreateClient("Client");

			// product categories and products
			var categoryBeverage = Helper.CreateProductCategory("BEV", "Beverages");
			var categorySoftdrink = helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage);
			var categoryBeer = helper.CreateProductCategory("BEER", "All Beers", categoryBeverage);
			var categoryDarkBeer = helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer);

			var productCoke = Helper.CreateProduct(client, "Coke");
			var relationCoke = productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationCoke.OU_OPC_Category = categorySoftdrink.PK;

			var productTea = Helper.CreateProduct(client, "Tea");
			var relationTea = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationTea.OU_OPC_Category = categorySoftdrink.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var relationVB = productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationVB.OU_OPC_Category = categoryBeer.PK;

			var productGuinness = Helper.CreateProduct(client, "Guinness");
			var relationGuinness = productGuinness.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationGuinness.OU_OPC_Category = categoryDarkBeer.PK;

			var stocktakeCoke = Helper.CreateWhsStocktake(client, warehouse, productCoke);
			var stocktakeTea = Helper.CreateWhsStocktake(client, warehouse, productTea);
			var stocktakeVB = Helper.CreateWhsStocktake(client, warehouse, productVB);
			var stocktakeGuinness = Helper.CreateWhsStocktake(client, warehouse, productGuinness);

			Asserter.AddStocktakesToScope(stocktakeCoke, stocktakeTea, stocktakeVB, stocktakeGuinness);

			Factory.Save();

			// test the filter
			var filter = (ModuleGuidFilter)StocktakeFilter["Product Category"];

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product Category");
			AssertEquals("Product Category filter's category should be Other.", filter.Category, FilterCategories.Other);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", StocktakeFilter.Filter, stocktakeCoke, stocktakeTea, stocktakeVB, stocktakeGuinness);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product Category", categorySoftdrink.PK);
			Asserter.AssertMatches("Since the filter value is Soft-Drink should return stocktakeCoke and stocktakeTea.", StocktakeFilter.Filter, stocktakeCoke, stocktakeTea);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product Category", categoryBeer.PK);
			Asserter.AssertMatches("Since the filter value is Beers it should return stocktakeVB and stocktakeGuinness.", StocktakeFilter.Filter, stocktakeVB, stocktakeGuinness);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product Category", categoryDarkBeer.PK);
			Asserter.AssertMatches("Since the filter value is Dark-Beer it should return stocktakeGuinness only.", StocktakeFilter.Filter, stocktakeGuinness);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "Product Category", categoryBeverage.PK);
			Asserter.AssertMatches("Since the filter value is All Beverage it should return all of them.", StocktakeFilter.Filter, stocktakeCoke, stocktakeTea, stocktakeVB, stocktakeGuinness);
		}

		#endregion

		#region TestCommodityFilter

		public void TestCommodityFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup stocktakes with and without commodity codes (one stocktake and lines)

			var commodityCode1 = Factory.New<RefCommodityCode>();
			var commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "1";
			commodityCode2.RH_Code = "2";

			var stocktakeWithCCAndNoLinesWithCC = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithCCAndLinesWithCC = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoCCAndNoLinesWithCC = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoCCAndLinesWithCC = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktakeWithCCAndNoLinesWithCC.WS_RH_NKCommodityCode = commodityCode1.RH_Code;
			stocktakeWithCCAndLinesWithCC.WS_RH_NKCommodityCode = commodityCode1.RH_Code;
			stocktakeWithNoCCAndNoLinesWithCC.WS_RH_NKCommodityCode = "";
			stocktakeWithNoCCAndLinesWithCC.WS_RH_NKCommodityCode = "";

			Asserter.AddStocktakesToScope(stocktakeWithCCAndNoLinesWithCC, stocktakeWithCCAndLinesWithCC, stocktakeWithNoCCAndNoLinesWithCC, stocktakeWithNoCCAndLinesWithCC);

			data.Part1.OP_RH_NKCommodityCode = commodityCode1.RH_Code;
			//part2 doesn't have a comodity code

			Helper.CreateWhsStocktakeLine(stocktakeWithCCAndNoLinesWithCC, data.Org1, data.Part2, data.Whs1.DefaultLocation);
			Helper.CreateWhsStocktakeLine(stocktakeWithCCAndLinesWithCC, data.Org1, data.Part1, data.Whs1.DefaultLocation); // commodity code 1 on line
			Helper.CreateWhsStocktakeLine(stocktakeWithNoCCAndNoLinesWithCC, data.Org1, data.Part2, data.Whs1.DefaultLocation);
			Helper.CreateWhsStocktakeLine(stocktakeWithNoCCAndLinesWithCC, data.Org1, data.Part1, data.Whs1.DefaultLocation); // commodity code 1 on line

			Factory.Save();

			// test the filter

			var filter = (ModuleNkFilter)StocktakeFilter["Commodity"];
			AssertEquals("Commodity filter category should be Other.", filter.Category, FilterCategories.Other);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithCCAndNoLinesWithCC, stocktakeWithCCAndLinesWithCC, stocktakeWithNoCCAndNoLinesWithCC, stocktakeWithNoCCAndLinesWithCC);

			filter.Property = "1";
			Asserter.AssertMatches("Since the filter value is 1 it should return stocktakes which have commodity code 1 on stocktake or lines.", filter, stocktakeWithCCAndNoLinesWithCC, stocktakeWithCCAndLinesWithCC, stocktakeWithNoCCAndLinesWithCC);

			filter.Property = "2";
			Asserter.AssertMatches("There are no stocktake items associated with this commodity code.", filter);
		}

		#endregion

		#region TestRowFilter

		public void TestRowFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whsRow1 = data.Whs1.Rows.Single(r => r.WR_Name == "A");
			var whsRow2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "ROW1", 2, 2);

			// setup stocktakes with and without Row (one stocktake and lines)

			var stocktakeWithRowAndLinesWithNoRow = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithRowAndLinesWithRow = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoRowAndLinesWithRow = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoRowAndLinesWithNoRow = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			stocktakeWithRowAndLinesWithNoRow.WS_WR_Row = whsRow2.PK;
			stocktakeWithRowAndLinesWithRow.WS_WR_Row = whsRow2.PK;
			//stocktakeWithNoRowButLinesWithRow doesn't have a row.
			//stocktakeWithNoRowAndLinesWithNoRow doesn't have a row.

			Asserter.AddStocktakesToScope(stocktakeWithRowAndLinesWithNoRow, stocktakeWithRowAndLinesWithRow, stocktakeWithNoRowAndLinesWithRow, stocktakeWithNoRowAndLinesWithNoRow);

			Helper.CreateWhsStocktakeLine(stocktakeWithRowAndLinesWithNoRow, data.Org1, data.Part1, whsRow1.Locations[0]);
			Helper.CreateWhsStocktakeLine(stocktakeWithRowAndLinesWithRow, data.Org1, data.Part1, whsRow2.Locations[0]); // row1 on line
			Helper.CreateWhsStocktakeLine(stocktakeWithNoRowAndLinesWithRow, data.Org1, data.Part1, whsRow2.Locations[0]); // row1 on line
			Helper.CreateWhsStocktakeLine(stocktakeWithNoRowAndLinesWithNoRow, data.Org1, data.Part1, whsRow1.Locations[0]);

			Factory.Save();

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["Row"];
			AssertEquals("Row filter category should be Locations.", filter.Category, FilterCategories.Locations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithRowAndLinesWithNoRow, stocktakeWithRowAndLinesWithRow, stocktakeWithNoRowAndLinesWithRow, stocktakeWithNoRowAndLinesWithNoRow);

			filter.Property = "ROW";
			Asserter.AssertMatches("Since the filter value is ROW it should return stocktakes which are in a row starts with ROW.", filter, stocktakeWithRowAndLinesWithNoRow, stocktakeWithRowAndLinesWithRow, stocktakeWithNoRowAndLinesWithRow);

			filter.Property = "ROW1";
			Asserter.AssertMatches("Since the filter value is Row1 it should return stocktakes which are in ROW1.", filter, stocktakeWithRowAndLinesWithNoRow, stocktakeWithRowAndLinesWithRow, stocktakeWithNoRowAndLinesWithRow);

			filter.Property = "ROW2";
			Asserter.AssertMatches("There are no stocktake items associated with ROW2.", filter);
		}

		#endregion

		#region TestAreaNameFilter

		public void TestAreaNameFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var whsLocations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var area = Helper.CreateArea(data.Whs1, "A1", AreaTypes.Codes.FreeStore);
			whsLocations[0].WLV_WA_PickingArea = area.PK;
			//whsLocations[1] area is default area.

			// setup stocktakes with and without Row (one stocktake and lines)

			var stocktakeWithAreaAndLinesWithNoArea = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithAreaAndLinesWithArea = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoAreaAndLinesWithArea = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoAreaAndLinesWithNoArea = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Asserter.AddStocktakesToScope(stocktakeWithAreaAndLinesWithNoArea, stocktakeWithAreaAndLinesWithArea, stocktakeWithNoAreaAndLinesWithArea, stocktakeWithNoAreaAndLinesWithNoArea);

			stocktakeWithAreaAndLinesWithNoArea.WS_WA_Area = area.PK;
			stocktakeWithAreaAndLinesWithArea.WS_WA_Area = area.PK;
			//stocktakeWithNoAreaButLinesWithArea doesn't have a row.
			//stocktakeWithNoAreaAndLinesWithNoArea doesn't have a row.

			Helper.CreateWhsStocktakeLine(stocktakeWithAreaAndLinesWithNoArea, data.Org1, data.Part1, whsLocations[1]);
			Helper.CreateWhsStocktakeLine(stocktakeWithAreaAndLinesWithArea, data.Org1, data.Part1, area.PickLocations[0]); // A1 on line
			Helper.CreateWhsStocktakeLine(stocktakeWithNoAreaAndLinesWithArea, data.Org1, data.Part1, area.PickLocations[0]); // A1 on line
			Helper.CreateWhsStocktakeLine(stocktakeWithNoAreaAndLinesWithNoArea, data.Org1, data.Part1, whsLocations[1]);

			Factory.Save();

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["PickAreaName"];
			AssertEquals("AreaName filter category should be Locations.", filter.Category, FilterCategories.Locations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithAreaAndLinesWithNoArea, stocktakeWithAreaAndLinesWithArea, stocktakeWithNoAreaAndLinesWithArea, stocktakeWithNoAreaAndLinesWithNoArea);

			filter.Property = "A";
			Asserter.AssertMatches("Since the filter value is A it should return stocktakes which have Area starts with A.", filter, stocktakeWithAreaAndLinesWithNoArea, stocktakeWithAreaAndLinesWithArea, stocktakeWithNoAreaAndLinesWithArea);

			filter.Property = "A1";
			Asserter.AssertMatches("Since the filter value is A1 it should return stocktakes which are in A1.", filter, stocktakeWithAreaAndLinesWithNoArea, stocktakeWithAreaAndLinesWithArea, stocktakeWithNoAreaAndLinesWithArea);

			filter.Property = "A2";
			Asserter.AssertMatches("There are no stocktake items associated with A2.", filter);
		}

		#endregion

		#region TestExpiryDateFilter

		public void TestExpiryDateFilter()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines with Expiry dates

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake3 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Asserter.AddStocktakesToScope(stocktake1, stocktake2, stocktake3);

			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(4), ZDate.Empty, "", "", ""); // Expires within 4 days
			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(6), ZDate.Empty, "", "", ""); // Expires within 6 days
			Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(6), ZDate.Empty, "", "", ""); // Expires within 6 days
			Helper.CreateWhsStocktakeLine(stocktake3, data.Org1, data.Part1, data.Whs1.DefaultLocation);  //WU_ExpiryDate has an empty date.

			Factory.Save();

			// test the filter

			var filter = (ModuleDateFilter)StocktakeFilter["ExpiryDate"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("ExpiryDate filter category should be AttributeSearch.", filter.Category, FilterCategories.AttributeSearch);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2, stocktake3);

			filter.Property1 = today.AddDays(4);
			filter.Property2 = today.AddDays(4);
			Asserter.AssertMatches("Since the filter value is 4 days ahead it should return stockTake1 only.", filter, stocktake1);

			filter.Property1 = today.AddDays(5);
			filter.Property2 = today.AddDays(8);
			Asserter.AssertMatches("Since the filter value is a date range it should return stockTake1 and stockTake2 only.", filter, stocktake1, stocktake2);

			filter.Property1 = today.AddDays(30);
			filter.Property2 = today.AddDays(30);
			Asserter.AssertMatches("There are no stocktake items expired in 30 days.", filter);
		}

		#endregion

		#region TestPackingDateFilter

		public void TestPackingDateFilter()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake3 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Asserter.AddStocktakesToScope(stocktake1, stocktake2, stocktake3);

			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, today.AddDays(-4), "", "", ""); // Packed 4 days ago
			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, today.AddDays(-6), "", "", ""); // Packed 6 days ago
			Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, today.AddDays(-6), "", "", ""); // packed 6 days ago
			Helper.CreateWhsStocktakeLine(stocktake3, data.Org1, data.Part1, data.Whs1.DefaultLocation); // line doesn't have a packing date

			Factory.Save();

			// test the filter

			var filter = (ModuleDateFilter)StocktakeFilter["PackingDate"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("PackingDate filter category should be AttributeSearch.", filter.Category, FilterCategories.AttributeSearch);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2, stocktake3);

			filter.Property1 = today.AddDays(-4);
			filter.Property2 = today.AddDays(-4);
			Asserter.AssertMatches("Since the filter value is 4 day ago it should return stockTake1 only.", filter, stocktake1);

			filter.Property1 = today.AddDays(-8);
			filter.Property2 = today.AddDays(-5);
			Asserter.AssertMatches("Since the filter value is a date range it should return stockTake2 only.", filter, stocktake1, stocktake2);

			filter.Property1 = today.AddDays(-30);
			filter.Property2 = today.AddDays(-30);
			Asserter.AssertMatches("There are no stocktake items which are having packing date 30 days ago.", filter);
		}

		#endregion

		#region TestPartAttribute1Filter

		public void TestPartAttribute1Filter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake3 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Asserter.AddStocktakesToScope(stocktake1, stocktake2, stocktake3);

			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "value1", "", ""); // Atribute1 is Value1
			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "value2", "", ""); // Atribute1 is Value2
			Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "value2", "", ""); // Atribute1 is Value2
			Helper.CreateWhsStocktakeLine(stocktake3, data.Org1, data.Part1, data.Whs1.DefaultLocation); // Line doesn't have an attribute1 value.

			Factory.Save();

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["PartAttribute1"];
			AssertEquals("PartAttribute1 filter category should be AttributeSearch.", filter.Category, FilterCategories.AttributeSearch);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2, stocktake3);

			filter.Property = "val";
			Asserter.AssertMatches("Since the filter value is value1 it should return stockTake1 and stocktake2 only.", filter, stocktake1, stocktake2);

			filter.Property = "value1";
			Asserter.AssertMatches("Since the filter value is value2 it should return stockTake1 only.", filter, stocktake1);

			filter.Property = "value3";
			Asserter.AssertMatches("There are no stocktake items for value3.", filter);
		}

		#endregion

		#region TestPartAttribute2Filter

		public void TestPartAttribute2Filter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake3 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Asserter.AddStocktakesToScope(stocktake1, stocktake2, stocktake3);

			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "value1", ""); // Atribute2 is value1
			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "value2", ""); // Atribute2 is value2
			Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "value2", ""); // Atribute2 is value2
			Helper.CreateWhsStocktakeLine(stocktake3, data.Org1, data.Part1, data.Whs1.DefaultLocation); // Attribute2 doesn't have a value

			Factory.Save();

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["PartAttribute2"];
			AssertEquals("PartAttribute2 filter category should be AttributeSearch.", filter.Category, FilterCategories.AttributeSearch);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2, stocktake3);

			filter.Property = "val";
			Asserter.AssertMatches("Since the filter value is value1 it should return stockTake1 and stocktake2 only.", filter, stocktake1, stocktake2);

			filter.Property = "value1";
			Asserter.AssertMatches("Since the filter value is value2 it should return stockTake1 only.", filter, stocktake1);

			filter.Property = "value3";
			Asserter.AssertMatches("There are no stocktake items for value3.", filter);
		}

		#endregion

		#region TestPartAttribute3Filter

		public void TestPartAttribute3Filter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//setup stocktakes and lines

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake3 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "value1"); // Attribute3 is value1
			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "value2"); // Attribute3 is value2
			Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "value2"); // Attribute3 is value2
			Helper.CreateWhsStocktakeLine(stocktake3, data.Org1, data.Part1, data.Whs1.DefaultLocation); // Attribute3 doesn't have a value.

			Factory.Save();

			// test the filter

			Asserter.AddStocktakesToScope(stocktake1, stocktake2, stocktake3);

			var filter = (ModuleTextFilter)StocktakeFilter["PartAttribute3"];
			AssertEquals("PartAttribute3 filter category should be AttributeSearch.", filter.Category, FilterCategories.AttributeSearch);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2, stocktake3);

			filter.Property = "val";
			Asserter.AssertMatches("Since the filter value is value1 it should return stockTake1 and stockTake2 only.", filter, stocktake1, stocktake2);

			filter.Property = "value1";
			Asserter.AssertMatches("Since the filter value is value2 it should return stockTake1 only.", filter, stocktake1);

			filter.Property = "value3";
			Asserter.AssertMatches("There are no stocktake items for value3.", filter);
		}

		#endregion

		#region TestSerialNumberFilter

		public void TestSerialNumberFilter_RegistryOn()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines
			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Asserter.AddStocktakesToScope(stocktake1, stocktake2);

			var stockTake1Line = Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			stockTake1Line.WU_SerialNumber = "SN11";
			var stockTake2Line = Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			stockTake2Line.WU_SerialNumber = "SN12";

			Factory.Save();

			// test the filter
			var filter = (ModuleTextFilter)StocktakeFilter["SerialNumber"];
			AssertEquals("Serial Number filter category should be AttributeSearch.", filter.Category, FilterCategories.AttributeSearch);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2);

			// equal
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SN11";
			Asserter.AssertMatches("Equals 'SN11' should return only stocktake1.", filter, stocktake1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Equals 'SN12' should return only stocktake2.", filter, stocktake2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Equals 'SN1' should return no stocktakes.", filter);

			// starts with
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "SN11";
			Asserter.AssertMatches("Starts with 'SN11' should return only stocktake1.", filter, stocktake1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Starts with 'SN12' should return only stocktake2.", filter, stocktake2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Starts with 'SN1' should return all stocktakes.", filter, stocktake1, stocktake2);
			filter.Property = "SN2";
			Asserter.AssertMatches("Starts with 'SN2' should return no stocktakes.", filter);

			// contains
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "SN11";
			Asserter.AssertMatches("Contains 'SN11' should return only stocktake1.", filter, stocktake1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Contains 'SN12' should return only stocktake2.", filter, stocktake2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Contains 'SN1' should return all stocktakes.", filter, stocktake1, stocktake2);
			filter.Property = "SN2";
			Asserter.AssertMatches("Contains 'SN2' should return no stocktakes.", filter);

			// not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "SN11";
			Asserter.AssertMatches("Not contains 'SN11' should return only stocktake2.", filter, stocktake2);
			filter.Property = "SN12";
			Asserter.AssertMatches("Not contains 'SN12' should return only stocktake1.", filter, stocktake1);
			filter.Property = "SN1";
			Asserter.AssertMatches("Not contains 'SN1' should return no stocktakes.", filter);
			filter.Property = "SN2";
			Asserter.AssertMatches("Not contains 'SN2' should return all stocktakes.", filter, stocktake1, stocktake2);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			// setup stocktake lines

			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			stocktakeLine1.WU_PalletID = "ID123";

			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			stocktakeLine2.WU_PalletID = "ID456";

			Asserter.AddStocktakesToScope(stocktake1, stocktake2);

			Factory.Save(); // for DBOnly query

			// test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["Pallet ID"];
			AssertEquals("Filter category should be NumbersAndReferences.", filter.Category, FilterCategories.NumbersAndReferences);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2);

			filter.Property = "ID123";
			Asserter.AssertMatches("Since the filter value is 'ID123' it should return only stockTake1.", filter, stocktake1);

			filter.Property = "ID456";
			Asserter.AssertMatches("Since the filter value is 'ID456' it should return only stocktake2.", filter, stocktake2);
		}

		#endregion

		#region TestDateClosedFilter

		public void TestDateClosedFilter()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			//setup stocktakes and lines

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktake3 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, now.AddDays(-1)); // closed 1 day ago
			Helper.CreateWhsStocktakeLine(stocktake1, data.Org1, data.Part1, data.Whs1.DefaultLocation, now.AddDays(-3)); // closed 3 days ago
			Helper.CreateWhsStocktakeLine(stocktake2, data.Org1, data.Part1, data.Whs1.DefaultLocation, now.AddDays(-3)); // closed 3 days ago
			Helper.CreateWhsStocktakeLine(stocktake3, data.Org1, data.Part1, data.Whs1.DefaultLocation); // No closed yet 

			Factory.Save();

			// test the filter

			Asserter.AddStocktakesToScope(stocktake1, stocktake2, stocktake3);

			var filter = (ModuleDateFilter)StocktakeFilter["DateClosed"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("DateClosed filter category should be AttributeSearch.", filter.Category, FilterCategories.Dates);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktake1, stocktake2, stocktake3);

			filter.Property1 = now.AddDays(-1);
			filter.Property2 = now.AddDays(-1);
			Asserter.AssertMatches("Since the filter value is 1 day ago it should return stockTake1 only.", filter, stocktake1);

			filter.Property1 = now.AddDays(-4);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("Since the filter value is in the date range it should return stockTake1 and stockTake2 only.", filter, stocktake1, stocktake2);

			filter.Property1 = now.AddDays(-10);
			filter.Property2 = now.AddDays(-10);
			Asserter.AssertMatches("There are no stocktake items which are closed 10 days ago.", filter);
		}

		#endregion

		#region TestLastVerifiedByFilter

		public void TestLastVerifiedByFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktakeWithCountOneVerifiedBy = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithCountTwoVerifiedBy = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithCountThreeVerifiedBy = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithOutVerifiedBy = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var staff1 = Helper.CreateGlbStaff("C1", "Login1");
			var staff2 = Helper.CreateGlbStaff("C2", "Login2");
			var staff3 = Helper.CreateGlbStaff("C3", "Login3");

			var lineWithFirstVerifiedBy = Helper.CreateWhsStocktakeLine(stocktakeWithCountOneVerifiedBy, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, staff1, (ZByte)1, StocktakeLineStatus.Codes.Open);
			var lineWithSecondVerifiedBy = Helper.CreateWhsStocktakeLine(stocktakeWithCountTwoVerifiedBy, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, staff2, (ZByte)2, StocktakeLineStatus.Codes.Open);
			var lineWithThirdVerifiedBy = Helper.CreateWhsStocktakeLine(stocktakeWithCountThreeVerifiedBy, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, staff2, (ZByte)3, StocktakeLineStatus.Codes.Open);
			var lineWithoutVerifiedBy = Helper.CreateWhsStocktakeLine(stocktakeWithOutVerifiedBy, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			lineWithSecondVerifiedBy.WU_GS_NKVerifiedBy = staff1.GS_Code; // To make sure only it's current verified by is staff2
			lineWithThirdVerifiedBy.WU_GS_NKVerifiedBy = staff1.GS_Code; // To make sure only it's current verified by is staff2
			lineWithThirdVerifiedBy.WU_Count2VerifiedBy = staff1.GS_Code; // To make sure only it's current verified by is staff2

			Factory.Save();

			// test the filter

			Asserter.AddStocktakesToScope(stocktakeWithCountOneVerifiedBy, stocktakeWithCountTwoVerifiedBy, stocktakeWithCountThreeVerifiedBy, stocktakeWithOutVerifiedBy);
			var filter = (ModuleGuidFilter)StocktakeFilter["LastVerifiedBy"];
			AssertEquals("Last Verified By filter category should be AttributeSearch.", filter.Category, FilterCategories.Organisations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithCountOneVerifiedBy, stocktakeWithCountTwoVerifiedBy, stocktakeWithCountThreeVerifiedBy, stocktakeWithOutVerifiedBy);

			filter.Property = staff1.PK;
			Asserter.AssertMatches("Since the filter value is Staff1 it should return stockTake1 only.", filter, stocktakeWithCountOneVerifiedBy, stocktakeWithCountTwoVerifiedBy, stocktakeWithCountThreeVerifiedBy);

			filter.Property = staff2.PK;
			Asserter.AssertMatches("Since the filter value is Staff2 it should return stockTake1 and stockTake2 only.", filter, stocktakeWithCountTwoVerifiedBy, stocktakeWithCountThreeVerifiedBy);

			filter.Property = staff3.PK;
			Asserter.AssertMatches("There are no stocktake items verified by Staff3.", filter);
		}

		#endregion

		#region TestLastVerifiedDateFilter

		public void TestLastVerifiedDateFilter()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktakeWithCountOneDateVerified = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithCountTwoDateVerified = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithCountThreeDateVerified = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithOutDateVerified = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var lineWithFirstDateVerified = Helper.CreateWhsStocktakeLine(stocktakeWithCountOneDateVerified, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, now.AddDays(-1), (ZByte)1, StocktakeLineStatus.Codes.Open);
			var lineWithSecondDateVerified = Helper.CreateWhsStocktakeLine(stocktakeWithCountTwoDateVerified, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, now.AddDays(-4), (ZByte)2, StocktakeLineStatus.Codes.Open);
			var lineWithThirdDateVerified = Helper.CreateWhsStocktakeLine(stocktakeWithCountThreeDateVerified, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, now.AddDays(-4), (ZByte)3, StocktakeLineStatus.Codes.Open);
			var lineWithoutDateVerified = Helper.CreateWhsStocktakeLine(stocktakeWithOutDateVerified, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			lineWithSecondDateVerified.WU_DateVerified = now.AddDays(-1); // To make sure only it's current Date Verified
			lineWithThirdDateVerified.WU_DateVerified = now.AddDays(-1); // To make sure only it's current Date Verified
			lineWithThirdDateVerified.WU_Count2DateVerified = now.AddDays(-1); // To make sure only it's current Date Verified

			Factory.Save();

			// test the filter

			Asserter.AddStocktakesToScope(stocktakeWithCountOneDateVerified, stocktakeWithCountTwoDateVerified, stocktakeWithCountThreeDateVerified, stocktakeWithOutDateVerified);
			var filter = (ModuleDateFilter)StocktakeFilter["LastVerifiedDate"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertEquals("LastDateVerified filter category should be AttributeSearch.", filter.Category, FilterCategories.Dates);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktakes.", filter, stocktakeWithCountOneDateVerified, stocktakeWithCountTwoDateVerified, stocktakeWithCountThreeDateVerified, stocktakeWithOutDateVerified);

			filter.Property1 = now.AddDays(-1);
			filter.Property2 = now.AddDays(-1);
			Asserter.AssertMatches("Since the filter value is 1 day ago it should return stockTake1 only.", filter, stocktakeWithCountOneDateVerified, stocktakeWithCountTwoDateVerified, stocktakeWithCountThreeDateVerified);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("Since the filter value is in the range it should return stockTake1 and stockTake2 only.", filter, stocktakeWithCountTwoDateVerified, stocktakeWithCountThreeDateVerified);

			filter.Property1 = now.AddDays(-10);
			filter.Property2 = now.AddDays(-10);
			Asserter.AssertMatches("There are no stocktake items which are verified 10 days ago.", filter);
		}

		#endregion

		#region TestZeroCountOnlyFilter

		#region TestZeroCountOnlyFilter_Type

		public void TestZeroCountOnlyFilter_Type()
		{
			var filter = (ModuleFlagsFilter)StocktakeFilter["ZeroCountOnlyFilter"];
			AssertEquals("ZeroCountOnly filter category should be NumbersAndReferences.", filter.Category, FilterCategories.NumbersAndReferences);
		}

		#endregion

		#region TestZeroCountOnlyFilter_CountOne

		public void TestZeroCountOnlyFilter_CountOne()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktakeCountOneWithOpenLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeCountOneWithCloseLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeCountOneWithEmptyLine = Helper.CreateWhsStocktake(data.Whs1, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);

			var openLineWithZeroCountOne = Helper.CreateWhsStocktakeLine(stocktakeCountOneWithOpenLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 1, StocktakeLineStatus.Codes.Open); //stocktakeCountOneWithOpenLine.WU_LastCount = 0
			var closeLineWithZeroCountOne = Helper.CreateWhsStocktakeLine(stocktakeCountOneWithCloseLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 1, StocktakeLineStatus.Codes.Closed); //stocktakeCountOneWithCloseLine.WU_LastCount = 0
			var emptyLineWithZeroCountOne = Helper.CreateEmptyWhsStocktakeLine(stocktakeCountOneWithEmptyLine, data.Whs1.DefaultLocation); // WU_LastCount = 0

			Factory.Save();

			// test the filter

			var filter = (ModuleFlagsFilter)StocktakeFilter["ZeroCountOnlyFilter"];
			Asserter.AddStocktakesToScope(stocktakeCountOneWithOpenLine, stocktakeCountOneWithCloseLine, stocktakeCountOneWithEmptyLine);

			filter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is true it should return stocktakeCountOneWithOpenLine only.", filter, stocktakeCountOneWithOpenLine);

			filter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value false it should return all stocktakes.", filter, stocktakeCountOneWithOpenLine, stocktakeCountOneWithCloseLine, stocktakeCountOneWithEmptyLine);
		}

		#endregion

		#region TestZeroCountOnlyFilter_CountTwo

		public void TestZeroCountOnlyFilter_CountTwo()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktakeWithWithNonZeroCountOneAndZeroCountTwoLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithZeroCountOneAndNonZeroCountTwoLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithCloseLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithNonZeroCountOneAndZeroCountTwo = Helper.CreateWhsStocktakeLine(stocktakeWithWithNonZeroCountOneAndZeroCountTwoLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, StocktakeLineStatus.Codes.Open); //stocktakeCountOneWithOpenLine.WU_Count2 = 0
			var openLineWithZeroCountOneAndNonZeroCountTwo = Helper.CreateWhsStocktakeLine(stocktakeWithZeroCountOneAndNonZeroCountTwoLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 2, StocktakeLineStatus.Codes.Open); //stocktakeCountOneWithOpenLine.WU_Count2 = 1
			var closeLineWithCountTwo = Helper.CreateWhsStocktakeLine(stocktakeWithCloseLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, StocktakeLineStatus.Codes.Closed); //stocktakeCountOneWithCloseLine.WU_Count2 = 0
			openLineWithNonZeroCountOneAndZeroCountTwo.WU_LastCount = 1; // To make sure only it's current count is zero.
			openLineWithZeroCountOneAndNonZeroCountTwo.WU_LastCount = 0; // First count is zero
			closeLineWithCountTwo.WU_LastCount = 1; // To make sure only it's current count is zero.

			Factory.Save();

			// test the filter

			var filter = (ModuleFlagsFilter)StocktakeFilter["ZeroCountOnlyFilter"];
			Asserter.AddStocktakesToScope(stocktakeWithWithNonZeroCountOneAndZeroCountTwoLine, stocktakeWithZeroCountOneAndNonZeroCountTwoLine, stocktakeWithCloseLine);

			filter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is true it should return stocktakeCountTwoWithOpenLine only.", filter, stocktakeWithWithNonZeroCountOneAndZeroCountTwoLine, stocktakeWithZeroCountOneAndNonZeroCountTwoLine);

			filter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value false it should return all stocktakes.", filter, stocktakeWithWithNonZeroCountOneAndZeroCountTwoLine, stocktakeWithZeroCountOneAndNonZeroCountTwoLine, stocktakeWithCloseLine);
		}

		#endregion

		#region TestZeroCountOnlyFilter_CountThree

		public void TestZeroCountOnlyFilter_CountThree()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktakeWithZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThreeLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndZeroCountThreeLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithZeroCountThreeCloseLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithZeroCountOneAndNonZeroCountTwoAndThree = Helper.CreateWhsStocktakeLine(stocktakeWithZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open); //openLineWithZeroCountOneAndNonZeroCountTwoAndThree.WU_Count3 = 1;
			openLineWithZeroCountOneAndNonZeroCountTwoAndThree.WU_LastCount = 0;
			openLineWithZeroCountOneAndNonZeroCountTwoAndThree.WU_Count2 = 1;

			var openLineWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktakeWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThreeLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open); //openLineWithZeroCountOneAndNonZeroCountTwoAndThree.WU_Count3 = 1;
			openLineWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThree.WU_LastCount = 1;
			openLineWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThree.WU_Count2 = 0;

			var openLineWithZeroCountThree = Helper.CreateWhsStocktakeLine(stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndZeroCountThreeLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 3, StocktakeLineStatus.Codes.Open); //stocktakeCountOneWithOpenLine.WU_Count3 = 0
			openLineWithZeroCountThree.WU_LastCount = 1; // To make sure only it's current count is zero.
			openLineWithZeroCountThree.WU_Count2 = 1; // To make sure only it's current count is zero.

			var openLineWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open); //openLineWithZeroCountOneAndNonZeroCountTwoAndThree.WU_Count3 = 1;
			openLineWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThree.WU_LastCount = 1;
			openLineWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThree.WU_Count2 = 1;

			var closeLineWithCountThree = Helper.CreateWhsStocktakeLine(stocktakeWithZeroCountThreeCloseLine, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 3, StocktakeLineStatus.Codes.Closed); //stocktakeCountOneWithCloseLine.WU_Count3 = 0
			closeLineWithCountThree.WU_LastCount = 1; // To make sure only it's current count is zero.
			closeLineWithCountThree.WU_Count2 = 1; // To make sure only it's current count is zero.

			Factory.Save();

			// test the filter

			var filter = (ModuleFlagsFilter)StocktakeFilter["ZeroCountOnlyFilter"];
			Asserter.AddStocktakesToScope(stocktakeWithZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithZeroCountThreeCloseLine);

			filter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is true it should return stocktakeCountThreeWithOpenLine only.", filter,
				stocktakeWithZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndZeroCountThreeLine);

			filter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value false it should return all stocktakes.", filter,
				stocktakeWithZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndZeroCountThreeLine,
				stocktakeWithNonZeroCountOneAndNonZeroCountTwoAndNonZeroCountThreeLine,
				stocktakeWithZeroCountThreeCloseLine);
		}

		#endregion

		#endregion

		#region BillingFiltersTest

		public void TestJobInvoicingStatusFilter()
		{
			SetupTestData();
			AssertNotNull(StocktakeFilter["Invoicing Job Status"]);
			var jobstatusFilter = (ModuleTextFilter)StocktakeFilter["Invoicing Job Status"];

			var job = new JobHeader.Loader(Stocktake11).TryLoadOrCreate();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			jobstatusFilter.Property = JobHeaderStatus.Working.Code;
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobstatusFilter.IsActive = true;

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertCollectionContains(Stocktake11, StocktakeCollection);

			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			StocktakeCollection.Load(StocktakeFilter.Filter);

			AssertCollectionNotContains(Stocktake11, StocktakeCollection);
		}

		public void TestAPInvoiceNumberFilter()
		{
			SetupTestData();

			AssertNotNull(StocktakeFilter["AP Invoice #"]);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = Stocktake11.PK;
			job.JH_ParentTableCode = WhsStocktakeSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = Stocktake11;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "AP Invoice #", (ZString)"00001001");
			StocktakeAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "AP Invoice #", (ZString)"00001002");
			StocktakeAssert(false, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "AP Invoice #", (ZString)"");
			StocktakeAssert(true, true, true, true);
		}

		public void TestARTransactionNumberFilter()
		{
			SetupTestData();

			AssertNotNull(StocktakeFilter["AR Transaction #"]);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = Stocktake11.PK;
			job.JH_ParentTableCode = WhsStocktakeSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = Stocktake11;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "AR Transaction #", (ZString)"00001001");
			StocktakeAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "AR Transaction #", (ZString)"00001002");
			StocktakeAssert(false, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(StocktakeFilter, "AR Transaction #", (ZString)"");
			StocktakeAssert(true, true, true, true);
		}

		#endregion

		#region TestStocktakeTypeFilter

		public void TestStocktakeTypeFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes with Type

			var stocktakeWithType1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithType2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeWithNoType = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktakeWithType1.WS_StocktakeType = "T1";
			stocktakeWithType2.WS_StocktakeType = "T2";
			stocktakeWithNoType.WS_StocktakeType = "";

			Asserter.AddStocktakesToScope(stocktakeWithType1, stocktakeWithType2, stocktakeWithNoType);

			// Test the filter

			var filter = (ModuleTextFilter)StocktakeFilter["StocktakeType"];
			AssertEquals("Stocktake Type filter category should be StatusAndFlags.", filter.Category, FilterCategories.StatusAndFlags);
			Asserter.AssertMatches("Since the filter is empty, it should return all stocktakes.", filter, stocktakeWithType1, stocktakeWithType2, stocktakeWithNoType);

			filter.Property = "T1";
			Asserter.AssertMatches("Since the filter value is T1 it should return stocktakeWithType1 only.", filter, stocktakeWithType1);

			filter.Property = "T3";
			Asserter.AssertMatches("There are no stocktake items associated with this Stocktake Type.", filter);
		}

		#endregion

		#region Test Filter Max Length

		public void TestFilterMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Pick Area Name should be set correctly.", WhsAreaSchema.WA_Name.MaxLength, StocktakeFilter["PickAreaName"].MaxLength);
				AssertEquals("MaxLength of Row Filter should be set correctly.", WhsRowSchema.WR_Name.MaxLength, StocktakeFilter["Row"].MaxLength);
				AssertEquals("MaxLength of Pallet ID should be set correctly.", WhsStocktakeLineSchema.WU_PalletID.MaxLength, StocktakeFilter["Pallet ID"].MaxLength);
				AssertEquals("MaxLength of Part Attribute 1 should be set correctly.", WhsStocktakeLineSchema.WU_PartAttrib1.MaxLength, StocktakeFilter["PartAttribute1"].MaxLength);
				AssertEquals("MaxLength of Part Attribute 2 should be set correctly.", WhsStocktakeLineSchema.WU_PartAttrib2.MaxLength, StocktakeFilter["PartAttribute2"].MaxLength);
				AssertEquals("MaxLength of Part Attribute 3 should be set correctly.", WhsStocktakeLineSchema.WU_PartAttrib3.MaxLength, StocktakeFilter["PartAttribute3"].MaxLength);
			});
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			SetupTestData();
			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = Stocktake11.PK;
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = Stocktake12.PK;
			job2.JH_ProfitLossReasonCode = "CD1";

			Factory.Save();

			AssertNotNull(StocktakeFilter["Profit/Loss Reason"]);
			var profitLossReasonFilter = (ModuleTextFilter)StocktakeFilter["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake11 }, StocktakeCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake11 }, StocktakeCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake11, Stocktake12 }, StocktakeCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake12 }, StocktakeCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake12 }, StocktakeCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake12 }, StocktakeCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertEquals(0, StocktakeCollection.Count);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Stocktake11, Stocktake12 }, StocktakeCollection);
		}

		#region Implementation

		protected virtual void StocktakeAssert(bool d11, bool d12, bool d21, bool d22)
		{
			StocktakeCollection.Load(StocktakeFilter.Filter);
			AssertEquals("Docket11", d11, StocktakeCollection.Contains(Stocktake11.PK));
			AssertEquals("Docket12", d12, StocktakeCollection.Contains(Stocktake12.PK));
			AssertEquals("Docket21", d21, StocktakeCollection.Contains(Stocktake21.PK));
			AssertEquals("Docket22", d22, StocktakeCollection.Contains(Stocktake22.PK));
		}

		protected void SetupTestData()
		{
			Whs1 = Helper.CreateWarehouse("1", "A");
			Whs2 = Helper.CreateWarehouse("2", "A");
			Org1 = Helper.CreateClient("O1", "O1");
			Org2 = Helper.CreateClient("O2", "O2");
			Stocktake11 = Helper.CreateWhsStocktake(Org1, Whs1);
			Stocktake12 = Helper.CreateWhsStocktake(Org1, Whs2);
			Stocktake21 = Helper.CreateWhsStocktake(Org2, Whs1);
			Stocktake22 = Helper.CreateWhsStocktake(Org2, Whs2);
		}

		protected WhsTestHelperFunctions Helper => helper = helper ?? new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StocktakeFilterBusinessObject();
		}

		protected WhsStocktakeCollection GetStocktakeCollection()
		{
			return new WhsStocktakeCollection(Factory);
		}

		protected StocktakeFilterBusinessObject StocktakeFilter
		{
			get => stocktakeFilter ?? (stocktakeFilter = new StocktakeFilterBusinessObject());
			set => stocktakeFilter = value;
		}
		StocktakeFilterBusinessObject stocktakeFilter;

		FilterStripAsserter<WhsStocktake> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsStocktake>(Factory, (s) => s.WS_StocktakeNumber));
		FilterStripAsserter<WhsStocktake> asserter;

		protected WhsWarehouse Whs1;
		protected WhsWarehouse Whs2;
		protected WhsWarehouse Whs3;
		protected OrgHeader Org1;
		protected OrgHeader Org2;
		protected OrgHeader Org3;
		protected WhsStocktake Stocktake11;
		protected WhsStocktake Stocktake12;
		protected WhsStocktake Stocktake21;
		protected WhsStocktake Stocktake22;

		WhsStocktakeCollection StocktakeCollection => stocktakeCollection ?? (stocktakeCollection = GetStocktakeCollection());
		WhsStocktakeCollection stocktakeCollection;

		#endregion
	}

	#endregion

	#region AccountingFilterStripTest

	public class StocktakeFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsStocktake>
	{
		protected override WhsStocktake GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<WhsStocktake>();
		}

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.WhsStocktake;
	}

	#endregion

	#region AsserterExtension

	static class AsserterExtension
	{
		public static void AddStocktakesToScope(this FilterStripAsserter<WhsStocktake> whsStocktake, params WhsStocktake[] stocktakes)
		{
			foreach (var stocktake in stocktakes)
			{
				whsStocktake.AddToScope(stocktake);
			}
		}
	}

	#endregion
}
