using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(PickFacesFilterBusinessObject))]
	class PickFacesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		#region TestFilter_Warehouse

		public void TestFilter_Warehouse()
		{
			var data = new PickFaceViewTestData(Factory, warehouses: 2);
			var i = 1;

			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], data.Locations[data.Warehouses[0]][0], i++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], data.Locations[data.Warehouses[1]][0], i++);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);
			AssertGuidFilter(PickFacesFilterBusinessObject.Schema.Warehouse, p => p.WPV_WW_Whs, result);
		}

		public void TestFilter_Warehouse_IsMandatory()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterStrip[PickFacesFilterBusinessObject.Schema.Warehouse];
			AssertEquals("Filter must always be visible", FilterVisibility.AlwaysVisible, filter.Visibility);

			filter.Property = data.Whs1.PK; // need to double set so validation will be run on empty value
			filter.Property = ZGuid.Empty;
			AssertHasError(filter.PropertyInfo, "Warehouse is required.");

			filter.Property = data.Whs1.PK;
			AssertNoError(filter.PropertyInfo, "Warehouse is required.");
		}

		public void TestFilter_Warehouse_ClearLocationWhenChanged()
		{
			// See `ClearProductOnChangeClient` explanation. Same, but applies to warehouse and location.
			var filter = GetNewFilterStripBusinessObject();

			var warehouseFilter = (ModuleGuidFilter)filter.ModuleFilters[PickFacesFilterBusinessObject.Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)filter.ModuleFilters[PickFacesFilterBusinessObject.Schema.Location];
			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;
			warehouseFilter.Property = ZGuid.BrettsGuid;
			locationFilter.Property = ZGuid.Missing;

			AssertNotEquals(ZGuid.Empty, locationFilter.Property);
			warehouseFilter.Property = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, locationFilter.Property);
		}

		#endregion

		#region TestFilter_Client

		public void TestFilter_Client()
		{
			var data = new PickFaceViewTestData(Factory, clients: 2, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var i = 1;

			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], i++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[1], data.Clients[1], locations[1], i++);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);
			AssertGuidFilter(PickFacesFilterBusinessObject.Schema.Client, p => p.WPV_OH, result);
		}

		public void TestFilter_Client_ClearProductWhenChanged()
		{
			// There can be two different products with the same code, each assigned to a different client
			// When client changes, we clear product, to avoid this confusion.
			// Otherwise the 'correct' code could be displayed with an error, as the backing product PK is wrong.
			var filter = GetNewFilterStripBusinessObject();

			var clientFilter = (ModuleGuidFilter)filter.ModuleFilters[PickFacesFilterBusinessObject.Schema.Client];
			var productFilter = (ModuleGuidFilter)filter.ModuleFilters[PickFacesFilterBusinessObject.Schema.Product];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;
			clientFilter.Property = ZGuid.BrettsGuid; // arbitrary
			productFilter.Property = ZGuid.Missing; // arbitrary

			AssertNotEquals(ZGuid.Empty, productFilter.Property);
			clientFilter.Property = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, productFilter.Property);
		}

		#endregion

		#region TestFilter_Product

		public void TestFilter_Product()
		{
			var data = new PickFaceViewTestData(Factory, clients: 2, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var i = 1;

			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], i++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[1], data.Clients[1], locations[1], i++);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);
			AssertGuidFilter(PickFacesFilterBusinessObject.Schema.Product, p => p.WPV_OP, result);
		}

		public void TestFilter_Product_RequiresClient()
		{
			var client = Helper.CreateClient("DATBOI");
			var part = Helper.CreateProduct(client, "P4");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var productFilter = (ModuleGuidFilter)filter[PickFacesFilterBusinessObject.Schema.Product];
			productFilter.IsActive = true;
			productFilter.Property = part.PK; // need to double set so validation will be run on empty value
			productFilter.Property = ZGuid.Empty;
			AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");

			productFilter.Property = part.PK;
			AssertHasError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
			productFilter.Property = ZGuid.Empty; // clean up

			var clientFilter = (ModuleGuidFilter)filter[PickFacesFilterBusinessObject.Schema.Client];
			clientFilter.IsActive = true;
			clientFilter.Property = client.PK;
			productFilter.Property = part.PK;
			AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
		}

		#endregion

		#region TestFilter_Location

		public void TestFilter_Location()
		{
			var data = new PickFaceViewTestData(Factory, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var i = 1;

			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], i++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], i++);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);
			AssertGuidFilter(PickFacesFilterBusinessObject.Schema.Location, p => p.WPV_WL, result);
		}

		public void TestFilter_Location_RequiresWarehouse()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);

			var filter = GetNewFilterStripBusinessObject();
			var locationFilter = (ModuleGuidFilter)filter[PickFacesFilterBusinessObject.Schema.Location];
			locationFilter.IsActive = true;
			locationFilter.Property = data.Whs1.FindLocation("A").PK; // need to double set so validation will be run on empty value
			locationFilter.Property = ZGuid.Empty;
			AssertNoError(locationFilter.PropertyInfo, "Location can not be entered without a Warehouse Code.");

			locationFilter.Property = data.Whs1.FindLocation("A").PK;
			AssertHasError(locationFilter.PropertyInfo, "Location can not be entered without a Warehouse Code.");
			locationFilter.Property = ZGuid.Empty; // clean up

			var warehouseFilter = (ModuleGuidFilter)filter[PickFacesFilterBusinessObject.Schema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			locationFilter.Property = data.Whs1.FindLocation("A").PK;
			AssertNoError(locationFilter.PropertyInfo, "Location can not be entered without a Warehouse Code.");
		}

		#endregion

		#region TestFilter_AssignmentStatus

		public void TestFilter_AssignmentStatus_Unassigned()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("222", "222");
			Helper.CreateProductClientRelationShip(org2, data.Part2);

			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;

			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, fixLocation1, 50m, 200m, 10m);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);
			AssertEquals("Precondition: no pickface", ZGuid.Empty, result[0].WPV_WF);
			AssertNotNull("Precondition: pickface exists", result[1].WPV_WF);
			Asserter.AddToScope(result);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizO[PickFacesFilterBusinessObject.Schema.UnassignedPickfaces];
			AssertEquals("The category should be Status and Flags", FilterCategories.StatusAndFlags, filter.Category);

			// Default case
			AssertEquals("Expect default case all", PickFaceStatuses.Codes.All, filter.Property);
			Asserter.AssertMatches("Expect all results", filter, result);

			filter.Property = PickFaceStatuses.Codes.Assigned;
			Asserter.AssertMatches("Expect only locations with pickfaces", filter, result[1]);

			filter.Property = PickFaceStatuses.Codes.Unassigned;
			Asserter.AssertMatches("Expect only unassigned locations", filter, result[0]);

			filter.Property = PickFaceStatuses.Codes.UnassignedWithStock;
			Asserter.AssertMatches("Expected not to show for Un-assigned with stock filter.", filter, Array.Empty<WhsPickFaceView>());
		}

		public void TestFilter_AssignmentStatus_Unassigned_WithStock()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var client = Helper.CreateClient("C1", "C1");
			var product = Helper.CreateProduct(client, "P1");
			var locationTypeFix = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			var locationFixed1 = whs.FindLocation("A-1");
			var locationFixed2 = whs.FindLocation("A-2");
			locationFixed1.WLV_WLT_LocationType = locationTypeFix.PK;
			locationFixed2.WLV_WLT_LocationType = locationTypeFix.PK;
			Factory.Save();

			// Add inventory for location "A-1" so that it is shown on view.
			var receivePK = iHelper.CreateWhsReceive(client.PK, whs.PK, "R1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 10m, locationFixed1.PK);
			Factory.Save();

			var pickViewQuery = new ZQuery();
			pickViewQuery.OrderBy = WhsPickFaceViewSchema.Constants.WPV_TotalQuantity + OrderByClause.Descending;

			var pickViewList = Factory.Load<WhsPickFaceView>(pickViewQuery);
			AssertEquals("Precondition: Length", 2, pickViewList.Length);
			AssertEquals("Precondition: Must not have a pick face assigned.", ZGuid.Empty, pickViewList[0].WPV_WF);
			AssertEquals("Precondition: First location should have stock.", 10m, pickViewList[0].WPV_TotalQuantity);
			AssertEquals("Precondition: Must not have a pick face assigned.", ZGuid.Empty, pickViewList[1].WPV_WF);

			Asserter.AddToScope(pickViewList);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizO[PickFacesFilterBusinessObject.Schema.UnassignedPickfaces];

			filter.Property = PickFaceStatuses.Codes.Assigned;
			Asserter.AssertMatches("Expected not to show for Assigned filter.", filter, Array.Empty<WhsPickFaceView>());

			filter.Property = PickFaceStatuses.Codes.Unassigned;
			Asserter.AssertMatches("Expected to show unassigned location without stock.", filter, pickViewList[1]);

			filter.Property = PickFaceStatuses.Codes.UnassignedWithStock;
			Asserter.AssertMatches("Expected to show only unassigned location with stock.", filter, pickViewList[0]);
		}

		public void TestFilter_AssignmentStatus_Unassigned_WithOpenTransactions()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = Helper.CreateWarehouse("WH1", "A", 3, 1);
			var client = Helper.CreateClient("C1", "C1");
			var product = Helper.CreateProduct(client, "P1");
			var locationTypeFix = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			var locationForInventory = whs.FindLocation("A-1");
			var locationFixed1 = whs.FindLocation("A-2");
			locationFixed1.WLV_WLT_LocationType = locationTypeFix.PK;

			var receivePK = iHelper.CreateWhsReceive(client.PK, whs.PK, "R1", Notify);
			var receiveLinePK = iHelper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 10m, locationForInventory.PK);

			var locationFixed2 = whs.FindLocation("A-3");
			var transferPK = iHelper.CreateWhsTransfer(client.PK, whs.PK, "T1", Notify);
			var transferLine = iHelper.CreateWhsTransferLine(transferPK, product.PK, 10m, locationForInventory.PK, locationFixed2.PK);
			var pickLine = Factory.New<IWhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLinePK;
			pickLine.WZ_WE_TransactionLine = transferLine;
			pickLine.WZ_Units = 10m;
			locationFixed2.WLV_WLT_LocationType = locationTypeFix.PK;

			Factory.Save();

			var pickViewQuery = new ZQuery();
			pickViewQuery.OrderBy = WhsPickFaceViewSchema.Constants.WPV_Incoming + OrderByClause.Descending;

			var pickViewList = Factory.Load<WhsPickFaceView>(pickViewQuery);
			AssertEquals("Precondition: Length", 2, pickViewList.Length);
			AssertEquals("Precondition: Must not have a pick face assigned.", ZGuid.Empty, pickViewList[0].WPV_WF);
			AssertEquals("Precondition: Must not have a pick face assigned.", ZGuid.Empty, pickViewList[1].WPV_WF);
			AssertEquals("Precondition: Location should have incoming stock.", 10m, pickViewList[0].WPV_Incoming);

			Asserter.AddToScope(pickViewList);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizO[PickFacesFilterBusinessObject.Schema.UnassignedPickfaces];

			filter.Property = PickFaceStatuses.Codes.Assigned;
			Asserter.AssertMatches("Expected not to show for Assigned filter.", filter, Array.Empty<WhsPickFaceView>());

			filter.Property = PickFaceStatuses.Codes.Unassigned;
			Asserter.AssertMatches("Expected to show unassigned location without stock.", filter, pickViewList[1]);

			filter.Property = PickFaceStatuses.Codes.UnassignedWithStock;
			Asserter.AssertMatches("Expected to show only unassigned location with open transaction.", filter, pickViewList[0]);
		}

		public void TestFilter_AssignmentStatus_Assigned_WithOpenTransactions()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = Helper.CreateWarehouse("WH1", "A", 3, 1);
			var client = Helper.CreateClient("C1", "C1");
			var product = Helper.CreateProduct(client, "P1");
			var locationTypeFix = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			var locationForInventory = whs.FindLocation("A-1");
			var receivePK = iHelper.CreateWhsReceive(client.PK, whs.PK, "R1", Notify);
			var receiveLinePK = iHelper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 10m, locationForInventory.PK);

			var locationFixed = whs.FindLocation("A-2");
			var transferPK = iHelper.CreateWhsTransfer(client.PK, whs.PK, "T1", Notify);
			var transferLine = iHelper.CreateWhsTransferLine(transferPK, product.PK, 10m, locationForInventory.PK, locationFixed.PK);
			var pickLine = Factory.New<IWhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLinePK;
			pickLine.WZ_WE_TransactionLine = transferLine;
			pickLine.WZ_Units = 10m;

			var pickFace = Helper.CreateProductPickFace(product, client, locationFixed);
			locationFixed.WLV_WLT_LocationType = locationTypeFix.PK;

			Factory.Save();

			var pickViewQuery = new ZQuery();
			pickViewQuery.OrderBy = WhsPickFaceViewSchema.Constants.WPV_Incoming + OrderByClause.Descending;

			var pickView = Factory.Load<WhsPickFaceView>(pickViewQuery).Single();
			AssertEquals("Precondition: Must have a pick face assigned.", pickFace.PK, pickView.WPV_WF);
			AssertEquals("Precondition: Location should have incoming stock.", 10m, pickView.WPV_Incoming);

			Asserter.AddToScope(pickView);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizO[PickFacesFilterBusinessObject.Schema.UnassignedPickfaces];

			filter.Property = PickFaceStatuses.Codes.Assigned;
			Asserter.AssertMatches("Expected to show for Assigned filter.", filter, pickView);

			filter.Property = PickFaceStatuses.Codes.Unassigned;
			Asserter.AssertMatches("Expected not to show for Un-assigned filter.", filter, Array.Empty<WhsPickFaceView>());

			filter.Property = PickFaceStatuses.Codes.UnassignedWithStock;
			Asserter.AssertMatches("Expected not to show for Un-assigned with stock filter.", filter, Array.Empty<WhsPickFaceView>());
		}

		#endregion

		#region TestFilter_ABCCategory

		public void TestFilter_ABCCategory()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var i = 1;
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1, i++);
			var abcCategory1 = Factory.New(ObjectFactory.GetType<IWhsABCCategory>());
			abcCategory1[WhsABCCategorySchema.WJ_OP_Product] = data.Part1.PK;
			abcCategory1[WhsABCCategorySchema.WJ_OH_Client] = data.Org1.PK;
			abcCategory1[WhsABCCategorySchema.WJ_WW_Warehouse] = data.Whs1.PK;
			abcCategory1[WhsABCCategorySchema.WJ_Category] = "A";
			abcCategory1[WhsABCCategorySchema.WJ_AnalysisDateTo] = ZDateTime.Today;
			abcCategory1[WhsABCCategorySchema.WJ_AnalysisDateFrom] = ZDateTime.Today;

			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part2, data.Org1, location2, i++);
			var abcCategory2 = Factory.New(ObjectFactory.GetType<IWhsABCCategory>());
			abcCategory2[WhsABCCategorySchema.WJ_OP_Product] = data.Part2.PK;
			abcCategory2[WhsABCCategorySchema.WJ_OH_Client] = data.Org1.PK;
			abcCategory2[WhsABCCategorySchema.WJ_WW_Warehouse] = data.Whs1.PK;
			abcCategory2[WhsABCCategorySchema.WJ_Category] = "B";
			abcCategory2[WhsABCCategorySchema.WJ_AnalysisDateTo] = ZDateTime.Today;
			abcCategory2[WhsABCCategorySchema.WJ_AnalysisDateFrom] = ZDateTime.Today;

			var product3 = Helper.CreateProduct(data.Org1, "Part3");
			Helper.CreateProductPickFace(product3, data.Org1, location2, i++);

			Factory.Save();

			var pickViewQuery = new ZQuery
			{
				OrderBy = WhsPickFaceViewSchema.Constants.WPV_ReplenishMinimum + OrderByClause.Ascending
			};

			var pickViewList = Factory.Load<WhsPickFaceView>(pickViewQuery);
			AssertEquals("Precondition: Length", 3, pickViewList.Length);
			AssertEquals("Precondition: First pick face is product1.", data.Part1.PK, pickViewList[0].WPV_OP);
			AssertEquals("Precondition: First pick face ABC Category is A.", "A", pickViewList[0].WPV_ABCCategory);
			AssertEquals("Precondition: Second pick face is product2.", data.Part2.PK, pickViewList[1].WPV_OP);
			AssertEquals("Precondition: Second pick face ABC Category is B.", "B", pickViewList[1].WPV_ABCCategory);
			AssertEquals("Precondition: Third pick face is product3.", product3.PK, pickViewList[2].WPV_OP);
			AssertEquals("Precondition: Third pick face ABC Category is blank.", "", pickViewList[2].WPV_ABCCategory);

			Asserter.AddToScope(pickViewList);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizO[PickFacesFilterBusinessObject.Schema.ABCCategory];

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Expected pickFace3.", filter, pickViewList[2]);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("Expected pickFace3.", filter, pickViewList[0], pickViewList[1]);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "A";
			Asserter.AssertMatches("Expected pickFace1.", filter, pickViewList[0]);

			filter.Property = "B";
			Asserter.AssertMatches("Expected pickFace2.", filter, pickViewList[1]);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("Expected pickFace1.", filter, pickViewList[0], pickViewList[2]);
		}

		#endregion

		#region TestFilter_PercentageFull

		public void TestFilter_PercentageFull()
		{
			var percentageFull = new[] { 0m, 5m, 100m, 98m, 7m, 50m, 60m, 49m, 51m };
			var replenishMax = 100m;
			var locations = new WhsLocation[9];

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = Helper.CreateWarehouse("WH1", "A", 10, 1);
			var client = Helper.CreateClient("C1", "C1");
			var product = Helper.CreateProduct(client, "P1");
			var fixedLocationType = helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);

			Factory.Save();

			for (var i = 0; i < locations.Length; i++)
			{
				locations[i] = whs.FindLocation(String.Format("A-{0}", i + 2));
				locations[i].WLV_WLT_LocationType = fixedLocationType.PK;
				Helper.CreateProductPickFace(product, client, locations[i], i, replenishMax);

				var receivePK = iHelper.CreateWhsReceive(client.PK, whs.PK, String.Format("R{0}", i + 1), Notify);
				iHelper.CreateWhsReceiveInventoryLine(receivePK, product.PK, percentageFull[i], locations[i].PK);
				iHelper.FinaliseDocket(receivePK);
			}

			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 9);
			AssertContainsExactElementsInAnyOrder("Pre-condition: PercentageFull for each view should equal expected values", result.Select(wpv => wpv.WPV_PercentageFull).ToArray(), percentageFull);
			Asserter.AddToScope(result);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)filterStripBizO[PickFacesFilterBusinessObject.Schema.PercentageFull];
			filter.IsActive = true;

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 7m;
			Asserter.AssertMatches("Expected PickFaceView with PercentageFull of 7", filter, result[4]);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 50m;
			filter.Property2 = 60m;
			Asserter.AssertMatches("Expected PickFaceViews with PercentageFull values of 50, 51, 60", filter, new[] { result[5], result[6], result[8] });

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = 55m;
			Asserter.AssertMatches("Expected PickFaceViews with PercentageFull values greater than 55", filter, new[] { result[2], result[3], result[6] });

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 50m;
			Asserter.AssertMatches("Expected PickFaceViews with PercentageFull values less than 50", filter, new[] { result[0], result[1], result[4], result[5], result[7] });
		}

		#endregion

		#region TestFilter_Pick

		public void TestFilter_Pick()
		{
			var data = new PickFaceViewTestData(Factory, clients: 2, locationsPerWarehouse: 3);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocation = Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocation.PK;

			int replenishMin = 1;
			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[2], replenishMin++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[1], data.Clients[1], locations[1], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);
			Factory.Save();

			var receive2PK = transactionHelper.CreateWhsReceive(data.Clients[1].PK, data.Warehouses[0].PK, "R2", Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive2PK, data.Parts[1].PK, 50m, locations[1].PK);
			transactionHelper.FinaliseDocket(receive2PK);
			Factory.Save();

			var transfer = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T1", Notify);
			var transferLine = transactionHelper.CreateWhsTransferLine(transfer, data.Parts[0].PK, 100m, locations[0].PK, locations[2].PK);
			var transferLineBizO = Factory.GetBizOsForPK(transferLine.ToGuid());
			transferLineBizO[0].RunPreSaveValidation();
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 100m);
			var pick1 = transactionHelper.CreatePickNew(false, false, order1.PK);

			var order2 = transactionHelper.CreateWhsOrder(data.Clients[1].PK, data.Warehouses[0].PK, data.Clients[1].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[1].PK, 50m);
			var pick2 = transactionHelper.CreatePickNew(false, false, order2.PK);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);
			Asserter.AddToScope(result);

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO[PickFacesFilterBusinessObject.Schema.PickNumber];
			filter.IsActive = true;

			filter.Property = pick1.PK;
			Asserter.AssertMatches("Expect the first pick face", filter, result[0]);

			filter.Property = pick2.PK;
			Asserter.AssertMatches("Expect the second pick face", filter, result[1]);

			filter.Property = ZGuid.BrettsGuid;
			Asserter.AssertMatches("Expect no results for Brett's GUID.", filterBizO.Filter);

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Expect all results for empty filter.", filterBizO.Filter, result);
		}

		public void TestFilter_Pick_ComparisonOperators()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO[PickFacesFilterBusinessObject.Schema.PickNumber];
			AssertEquals("Should show comparison operators.", true, filter.HasComparisonOperator);
			AssertContainsExactElementsInAnyOrder(new[] { "exact", "not equal", "filters match" }, filter.ComparisonOperator_List.GetAllCodes());
		}

		#endregion

		#endregion

		#region Test scaffolding

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		FilterStripAsserter<WhsPickFaceView> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsPickFaceView>(Factory, (p) => p.PK.ToString()));
		FilterStripAsserter<WhsPickFaceView> asserter;

		protected TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		WhsPickFaceView[] LoadDataFromViewAndAssertCount(int expectedCount)
		{
			var query = new ZQuery();
			query.OrderBy = WhsPickFaceViewSchema.Constants.WPV_ReplenishMinimum;

			var result = Factory.Load<WhsPickFaceView>(query);
			AssertEquals("Precondition: Length", expectedCount, result.Length);
			Assert("Precondition: Expect \"WPV_ReplenishMinimum\" to be unique in results.", !result.GroupBy(r => r.WPV_ReplenishMinimum).Any(g => g.IsCountMoreThan(1))); // Ensure sort is determinstic
			return result;
		}

		void AssertGuidFilter(string filterName, Func<WhsPickFaceView, ZGuid> guidSelector, WhsPickFaceView[] expected)
		{
			var cases = expected.Select(guidSelector).ToArray();

			Asserter.AddToScope(expected);

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO[filterName];
			filter.IsActive = true;

			for (int i = 0; i < expected.Length; i++)
			{
				filter.Property = cases[i];
				Asserter.AssertMatches("Expect one result", filterBizO.Filter, expected[i]);
			}

			filter.Property = ZGuid.BrettsGuid;
			Asserter.AssertMatches("Expect no results for Brett's GUID.", filterBizO.Filter);

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Expect all results for empty filter.", filterBizO.Filter, expected);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PickFacesFilterBusinessObject();
		}

		#endregion
	}
}
