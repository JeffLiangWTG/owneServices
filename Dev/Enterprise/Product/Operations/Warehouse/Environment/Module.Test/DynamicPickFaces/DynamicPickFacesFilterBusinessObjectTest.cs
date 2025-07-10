using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Schema = Enterprise.Warehouse.Environment.Module.DynamicPickFacesFilterBusinessObject.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(DynamicPickFacesFilterBusinessObject))]
	class DynamicPickFacesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Overrides

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DynamicPickFacesFilterBusinessObject();
		}

		#endregion

		#region TestFilter_Warehouse

		public void TestFilter_Warehouse()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			Factory.Save();
			var dynamicPF1 = Helper.CreateDynamicPF(data.Whs1, data.Whs1.FindLocation("A"), "DPFArea1");
			var dynamicPF2 = Helper.CreateDynamicPF(data.Whs2, data.Whs2.FindLocation("A"), "DPFArea2");
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);

			AssertGuidFilterReturnsUniqueCode(Schema.Warehouse, view => view.WDP_WarehouseCode, result, filterCase => filterCase.WW_WarehouseCode, data.Whs1, data.Whs2);
		}

		public void TestFilter_Warehouse_IsMandatory()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterStrip[Schema.Warehouse];
			AssertEquals("Filter must always be visible", FilterVisibility.AlwaysVisible, filter.Visibility);

			filter.Property = ZGuid.BrettsGuid;
			filter.Property = ZGuid.Empty;
			AssertHasError(filter.PropertyInfo, "Warehouse is required.");

			filter.Property = ZGuid.BrettsGuid;
			AssertNoError(filter.PropertyInfo, "Warehouse is required.");
		}

		public void TestFilter_Warehouse_ClearAreaAndLocationWhenChanged()
		{
			var filter = GetNewFilterStripBusinessObject();

			var warehouseFilter = (ModuleGuidFilter)filter.ModuleFilters[Schema.Warehouse];
			var areaFilter = (ModuleGuidFilter)filter.ModuleFilters[Schema.Area];
			var locationFilter = (ModuleGuidFilter)filter.ModuleFilters[Schema.Location];
			warehouseFilter.IsActive = true;
			areaFilter.IsActive = true;
			locationFilter.IsActive = true;

			warehouseFilter.Property = ZGuid.BrettsGuid;
			areaFilter.Property = ZGuid.Missing;
			locationFilter.Property = ZGuid.Missing;

			AssertNotEquals(ZGuid.Empty, locationFilter.Property);
			AssertNotEquals(ZGuid.Empty, areaFilter.Property);

			warehouseFilter.Property = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, locationFilter.Property);
			AssertEquals(ZGuid.Empty, areaFilter.Property);
		}

		#endregion

		#region TestFilter_Area

		public void TestFilter_Area()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			Helper.CreateRow(data.Whs1, "B");
			Factory.Save();
			var dynamicPF1 = Helper.CreateDynamicPF(data.Whs1, data.Whs1.FindLocation("A"), "DPFArea1");
			var dynamicPF2 = Helper.CreateDynamicPF(data.Whs1, data.Whs1.FindLocation("B"), "DPFArea2");
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);

			AssertGuidFilterReturnsUniqueCode(Schema.Area, view => view.WDP_AreaName, result, filterCase => filterCase.WA_Name, dynamicPF1, dynamicPF2);
		}

		public void TestFilter_Area_RequiresWarehouse()
		{
			var filter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filter[Schema.Warehouse];
			var areaFilter = (ModuleGuidFilter)filter[Schema.Area];
			warehouseFilter.IsActive = true;
			areaFilter.IsActive = true;

			warehouseFilter.Property = ZGuid.Empty;
			areaFilter.Property = ZGuid.Missing;
			AssertHasError(areaFilter.PropertyInfo, "Area can not be entered without a Warehouse Code.");

			warehouseFilter.Property = ZGuid.BrettsGuid;
			areaFilter.Property = ZGuid.Missing;
			AssertNoError(areaFilter.PropertyInfo, "Area can not be entered without a Warehouse Code.");
		}

		public void TestFilter_Area_WithinWarehouse()
		{
			var errorMessage = "Enter a valid selection.";
			var data = new DynamicPickFaceViewTestData(Factory);
			var location = data.Whs1.FindLocation("A");
			var wrongLocation = data.Whs2.FindLocation("A");

			var area = Helper.CreateDynamicPF(data.Whs1, location);
			var wrongArea = Helper.CreateDynamicPF(data.Whs2, wrongLocation);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filter[Schema.Warehouse];
			var areaFilter = (ModuleGuidFilter)filter[Schema.Area];
			warehouseFilter.IsActive = true;
			areaFilter.IsActive = true;

			warehouseFilter.Property = data.Whs1.PK;
			areaFilter.Property = wrongArea.PK;
			AssertHasError(areaFilter.PropertyInfo, errorMessage);

			areaFilter.Property = area.PK;
			AssertNoError(areaFilter.PropertyInfo, errorMessage);

			warehouseFilter.Property = data.Whs2.PK;
			areaFilter.Property = area.PK;
			AssertHasError(areaFilter.PropertyInfo, errorMessage);
		}

		#endregion

		#region TestFilter_Location

		public void TestFilter_Location()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			Helper.CreateRow(data.Whs1, "B");
			Factory.Save();
			var locationA = data.Whs1.FindLocation("A");
			var locationB = data.Whs1.FindLocation("B");
			var dynamicPF1 = Helper.CreateDynamicPF(data.Whs1, locationA, "DPFArea1");
			data.AssignDynamicLocationAndSave(locationB, dynamicPF1);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);

			AssertGuidFilterReturnsUniqueCode(Schema.Location, view => view.WDP_LocationString, result, filterCase => filterCase.WLV_LocationString, locationA, locationB);
		}

		public void TestFilter_Location_RequiresWarehouse()
		{
			var filter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filter[Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)filter[Schema.Location];
			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;

			warehouseFilter.Property = ZGuid.Empty;
			locationFilter.Property = ZGuid.Missing;
			AssertHasError(locationFilter.PropertyInfo, "Location can not be entered without a Warehouse Code.");

			warehouseFilter.Property = ZGuid.BrettsGuid;
			locationFilter.Property = ZGuid.Missing;
			AssertNoError(locationFilter.PropertyInfo, "Location can not be entered without a Warehouse Code.");
		}

		public void TestFilter_Location_WithinWarehouse()
		{
			string errorMessage = "Enter a valid selection.";
			var data = new DynamicPickFaceViewTestData(Factory);
			var location = data.Whs1.FindLocation("A");
			var wrongLocation = data.Whs2.FindLocation("A");

			var filter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filter[Schema.Warehouse];
			var locationFilter = (ModuleGuidFilter)filter[Schema.Location];
			warehouseFilter.IsActive = true;
			locationFilter.IsActive = true;

			warehouseFilter.Property = data.Whs1.PK;
			locationFilter.Property = wrongLocation.PK;
			AssertHasError(locationFilter.PropertyInfo, errorMessage);

			locationFilter.Property = location.PK;
			AssertNoError(locationFilter.PropertyInfo, errorMessage);

			warehouseFilter.Property = data.Whs2.PK;
			locationFilter.Property = location.PK;
			AssertHasError(locationFilter.PropertyInfo, errorMessage);
		}

		#endregion

		#region TestFilter_Client

		public void TestFilter_Client()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			var location = data.Whs1.FindLocation("A");
			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, location);
			data.AssignDynamicProductAndSave(data.Org1, data.Part1, dynamicPFArea);
			data.AssignDynamicProductAndSave(data.Org2, data.Part1, dynamicPFArea);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);

			AssertGuidFilterReturnsUniqueCode(Schema.Client, view => view.WDP_ClientCode, result, filterCase => filterCase.OH_Code, data.Org1, data.Org2);
		}

		public void TestFilter_Client_ClearProductWhenChanged()
		{
			var filter = GetNewFilterStripBusinessObject();

			var clientFilter = (ModuleGuidFilter)filter.ModuleFilters[Schema.Client];
			var productFilter = (ModuleGuidFilter)filter.ModuleFilters[Schema.Product];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;

			clientFilter.Property = ZGuid.BrettsGuid;
			productFilter.Property = ZGuid.Missing;

			AssertNotEquals(ZGuid.Empty, clientFilter.Property);
			AssertNotEquals(ZGuid.Empty, productFilter.Property);

			clientFilter.Property = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, clientFilter.Property);
			AssertEquals(ZGuid.Empty, productFilter.Property);
		}

		#endregion

		#region TestFilter_Product

		public void TestFilter_Product()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			var location = data.Whs1.FindLocation("A");
			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, location);
			data.AssignDynamicProductAndSave(data.Org1, data.Part1, dynamicPFArea);
			data.AssignDynamicProductAndSave(data.Org1, data.Part2, dynamicPFArea);
			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(expectedCount: 2);

			AssertGuidFilterReturnsUniqueCode(Schema.Product, view => view.WDP_ProductCode, result, filterCase => filterCase.OP_PartNum, data.Part1, data.Part2);
		}

		public void TestFilter_Product_RequiresClient()
		{
			var errorMessage = "Product Code can not be entered without a Client Code.";

			var filter = GetNewFilterStripBusinessObject();
			var clientFilter = (ModuleGuidFilter)filter[Schema.Client];
			var productFilter = (ModuleGuidFilter)filter[Schema.Product];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;

			clientFilter.Property = ZGuid.Empty;
			productFilter.Property = ZGuid.Missing;
			AssertHasError(productFilter.PropertyInfo, errorMessage);

			clientFilter.Property = ZGuid.BrettsGuid;
			productFilter.Property = ZGuid.Missing;
			AssertNoError(productFilter.PropertyInfo, errorMessage);
		}

		public void TestFilter_Product_OwnedByClient()
		{
			var errorMessage = "Enter a valid selection.";

			var data = new DynamicPickFaceViewTestData(Factory);
			var org3 = Helper.CreateClient("Org3");
			var org3Product = Helper.CreateProduct("Org3Product", org3);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var clientFilter = (ModuleGuidFilter)filter[Schema.Client];
			var productFilter = (ModuleGuidFilter)filter[Schema.Product];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;

			clientFilter.Property = data.Org1.PK;
			productFilter.Property = org3Product.PK;
			AssertHasError(productFilter.PropertyInfo, errorMessage);

			productFilter.Property = data.Part1.PK;
			AssertNoError(productFilter.PropertyInfo, errorMessage);

			clientFilter.Property = org3.PK;
			productFilter.Property = data.Part1.PK;
			AssertHasError(productFilter.PropertyInfo, errorMessage);
		}

		#endregion

		#region TestFilter_AssignmentStatus

		public void TestFilter_AssignmentStatus()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			Helper.CreateRow(data.Whs1, "B");
			Helper.CreateRow(data.Whs1, "C");
			Helper.CreateRow(data.Whs1, "D");
			Factory.Save();

			var locationA = data.Whs1.FindLocation("A");    // Assigned Part1 with stock
			var locationB = data.Whs1.FindLocation("B");    // Assigned Part1 without stock
			var locationC = data.Whs1.FindLocation("C");    // Assigned Part2, unassigned Part1 with stock
			var locationD = data.Whs1.FindLocation("D");  // EmptyLocationNoAssignedProduct

			var dynamicPFAreaA = Helper.CreateDynamicPF(data.Whs1, locationA, "dynamicPFAreaA");
			data.AssignDynamicLocationAndSave(locationB, dynamicPFAreaA);
			var dynamicPFAreaC = Helper.CreateDynamicPF(data.Whs1, locationC, "dynamicPFAreaC");
			var dynamicPFAreaD = Helper.CreateDynamicPF(data.Whs1, locationD, "dynamicPFAreaD");

			data.AssignDynamicProductAndSave(data.Org1, data.Part1, dynamicPFAreaA);
			data.AssignDynamicProductAndSave(data.Org1, data.Part2, dynamicPFAreaC);

			data.CreateStockAndSave(data.Whs1, data.Org1, data.Part1, locationA, "ReceiveA", 10M);
			data.CreateStockAndSave(data.Whs1, data.Org1, data.Part1, locationC, "ReceiveC", 10M);

			var result = LoadDataFromViewAndAssertCount(5)
				.Where(x => x.WDP_ProductCode != data.Part2.OP_PartNum) // Get rid of LocationC - Part2 assignment
				.OrderBy(x => x[WhsDynamicPickFaceViewSchema.WDP_LocationString]).ToArray();

			AssertEquals("Should have 4 rows", 4, result.Length);
			Asserter.AddToScope(result);

			var filterBizO = GetNewFilterStripBusinessObject();
			var assignmentFilter = (ModuleTextFilter)filterBizO[Schema.Assignment];

			AssertEquals("Precondition: Location A", "A", result[0].WDP_LocationString);
			AssertEquals("Precondition: Location B", "B", result[1].WDP_LocationString);
			AssertEquals("Precondition: Location C", "C", result[2].WDP_LocationString);
			AssertEquals("Precondition: Location D", "D", result[3].WDP_LocationString);

			Assert("Precondition: Location A Assigned", result[0].WDP_IsAssigned);
			Assert("Precondition: Location B Assigned", result[1].WDP_IsAssigned);
			Assert("Precondition: Location C Not assinged", !result[2].WDP_IsAssigned);
			Assert("Precondition: Location D Not assinged", !result[3].WDP_IsAssigned);

			AssertEquals("Precondition: Location A With stock", expected: 10M, result[0].WDP_TotalQuantity);
			AssertEquals("Precondition: Location B Without stock", expected: 0M, result[1].WDP_TotalQuantity);
			AssertEquals("Precondition: Location C With stock", expected: 10M, result[2].WDP_TotalQuantity);
			AssertEquals("Precondition: Location D Without stock", expected: 0M, result[3].WDP_TotalQuantity);

			assignmentFilter.Property = DynamicPickFaceAssignmentStatus.Codes.Assigned;
			Asserter.AssertMatches("Should return assigned, with or without stock", assignmentFilter, result[0], result[1]);

			assignmentFilter.Property = DynamicPickFaceAssignmentStatus.Codes.Unassigned;
			Asserter.AssertMatches("Should return unassigned, with or without stock", assignmentFilter, result[2], result[3]);

			assignmentFilter.Property = DynamicPickFaceAssignmentStatus.Codes.All;
			Asserter.AssertMatches("Should return all", assignmentFilter, result[0], result[1], result[2], result[3]);
		}

		#endregion

		#region ABCCategory

		public void TestFilter_ABCCategory()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			Helper.CreateRow(data.Whs1, "B");
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Factory.Save();

			var locationA = data.Whs1.FindLocation("A");    // Category A/B/Blank
			var locationB = data.Whs1.FindLocation("B");  // Unassigned

			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, locationA);
			Helper.CreateDynamicPF(data.Whs1, locationB, "AREA_NO_ASSIGNED_PRODUCT");

			data.CreateABCCategoryAndSave(data.Whs1, data.Org1, data.Part1, "A");
			data.CreateABCCategoryAndSave(data.Whs1, data.Org1, data.Part2, "B");

			data.AssignDynamicProductAndSave(data.Org1, data.Part1, dynamicPFArea); // Category A
			data.AssignDynamicProductAndSave(data.Org1, data.Part2, dynamicPFArea); // Category B
			data.AssignDynamicProductAndSave(data.Org1, part3, dynamicPFArea);         // Category Blank

			var result = LoadDataFromViewAndAssertCount(4)
				.OrderBy(x => x.WDP_LocationString)
				.ThenBy(x => x.WDP_ProductCode)
				.ToArray();

			AssertEquals("Precondition: Location A Part1 Category A", "A P1 A",
				$"{result[0].WDP_LocationString} {result[0].WDP_ProductCode} {result[0].WDP_ABCCategory}");

			AssertEquals("Precondition: Location A Part2 Category B", "A P2 B",
				$"{result[1].WDP_LocationString} {result[1].WDP_ProductCode} {result[1].WDP_ABCCategory}");

			AssertEquals("Precondition: Location A Part3 Category Blank", "A P3 ",
				$"{result[2].WDP_LocationString} {result[2].WDP_ProductCode} {result[2].WDP_ABCCategory}");

			AssertEquals("Precondition: Location B Unassigned Category Blank", "B  ",
				$"{result[3].WDP_LocationString} {result[3].WDP_ProductCode} {result[3].WDP_ABCCategory}");

			Asserter.AddToScope(result);

			var filterBizO = GetNewFilterStripBusinessObject();
			var assignmentFilter = (ModuleTextFilter)filterBizO[Schema.ABCCategory];

			// Equal
			assignmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			assignmentFilter.Property = "A";
			Asserter.AssertMatches("Expect Category A", assignmentFilter, result[0]);

			assignmentFilter.Property = "B";
			Asserter.AssertMatches("Expect Category B", assignmentFilter, result[1]);

			assignmentFilter.Property = "";
			Asserter.AssertMatches("Empty filter - Expect returns all", assignmentFilter, result);

			// Not Equal
			assignmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			assignmentFilter.Property = "A";
			Asserter.AssertMatches("Expect all but A", assignmentFilter, result[1], result[2], result[3]);

			// Not blank
			assignmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("Expect non-blank category", assignmentFilter, result[0], result[1]);

			// Blank
			assignmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Expect blank category", assignmentFilter, result[2], result[3]);
		}

		#endregion

		#region TestFilter_LocationType

		public void TestFilter_LocationType()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			Helper.CreateRow(data.Whs1, "B");
			Factory.Save();

			var locationA = data.Whs1.FindLocation("A");
			var locationB = data.Whs1.FindLocation("B");
			var dynamicLocaitonTypeA = Helper.CreateLocationType("DPA", LocationClasses.Codes.DPF);
			var dynamicLocaitonTypeB = Helper.CreateLocationType("DPB", LocationClasses.Codes.DPF);

			var dynamicPFArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, isPickingArea: true, isPutawayArea: false);

			locationA.WLV_WLT_LocationType = dynamicLocaitonTypeA.PK;
			locationA.WLV_WA_PickingArea = dynamicPFArea.PK;
			locationB.WLV_WLT_LocationType = dynamicLocaitonTypeB.PK;
			locationB.WLV_WA_PickingArea = dynamicPFArea.PK;

			Factory.Save();

			var result = LoadDataFromViewAndAssertCount(2);
			AssertGuidFilterReturnsUniqueCode(Schema.LocationType,
				view => view.WDP_LocationType, result,
				type => type.WLT_Code, dynamicLocaitonTypeA, dynamicLocaitonTypeB);
		}

		#endregion

		#region Test Scaffolding

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		FilterStripAsserter<WhsDynamicPickFaceView> Asserter => asserter ??
			(asserter = new FilterStripAsserter<WhsDynamicPickFaceView>(Factory, (p) => $"{p.PK} {p.WDP_LocationString} {p.WDP_ProductCode} {p.WDP_ClientCode}"));
		FilterStripAsserter<WhsDynamicPickFaceView> asserter;

		WhsDynamicPickFaceView[] LoadDataFromViewAndAssertCount(int expectedCount)
		{
			var query = new ZQuery();

			var result = Factory.Load<WhsDynamicPickFaceView>(query);
			AssertEquals("Precondition: row count", expectedCount, result.Length);
			return result;
		}

		void AssertGuidFilterReturnsUniqueCode<T>(string filterName, Func<WhsDynamicPickFaceView, ZString> resultCodeSelector, WhsDynamicPickFaceView[] expected, Func<T, ZString> caseCodeSelector, params T[] cases)
			where T : BusinessObject
		{
			AssertContainsExactElementsInAnyOrder($"Precondition: cases have no duplicate code for filter {filterName}",
				Array.Empty<ZString>(),
				cases.Select(caseCodeSelector).GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key));
			var codeToGuidDictionary = cases.ToDictionary(x => caseCodeSelector(x), x => x.PK);

			var codeCases = expected.Select(resultCodeSelector).ToArray();
			AssertContainsExactElementsInAnyOrder("Each code cases should have one Guid case", codeCases, codeToGuidDictionary.Keys);

			Asserter.AddToScope(expected);

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO[filterName];
			filter.IsActive = true;

			CombineAssertions(() =>
			{
				for (int i = 0; i < expected.Length; i++)
				{
					filter.Property = codeToGuidDictionary[codeCases[i]];
					Asserter.AssertMatches($"Expect one result on filter {filterName} = {codeCases[i]}", filterBizO.Filter, expected[i]);
				}

				filter.Property = ZGuid.BrettsGuid;
				Asserter.AssertMatches("Expect no results for Brett's GUID.", filterBizO.Filter);

				filter.Property = ZGuid.Empty;
				Asserter.AssertMatches("Expect all results for empty filter.", filterBizO.Filter, expected);
			});
		}

		#endregion
	}
}
