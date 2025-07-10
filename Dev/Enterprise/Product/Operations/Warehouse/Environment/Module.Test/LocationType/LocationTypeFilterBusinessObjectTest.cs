using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(LocationTypeFilterBusinessObject))]
	class LocationTypeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestCode

		public void TestCodeFilter()
		{
			var locationType1 = Helper.CreateLocationType("CD1", "TestCD1", false, 1, CodeLists.LocationClasses.Codes.FIX);
			var locationType2 = Helper.CreateLocationType("CD2", "TestCD2", false, 1, CodeLists.LocationClasses.Codes.FIX);
			Factory.Save();
			Asserter.AddToScope(locationType1, locationType2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleTextFilter)filterBizO1[LocationTypeFilterBusinessObject.Schema.Code];
			moduleFilter1.Property = "CD1";
			moduleFilter1.IsActive = true;
			Asserter.AssertMatches("Shoud return only locationType1", filterBizO1.Filter, locationType1);

			moduleFilter1.Property = "CD2";
			Asserter.AssertMatches("Shoud return only locationType2", filterBizO1.Filter, locationType2);
		}

		#endregion

		#region TestDescription

		public void TestDescriptionFilter()
		{
			var locationType1 = Helper.CreateLocationType("DD1", "TestDD1", false, 1, CodeLists.LocationClasses.Codes.FIX);
			var locationType2 = Helper.CreateLocationType("DD2", "TestDD2", false, 1, CodeLists.LocationClasses.Codes.FIX);
			Factory.Save();
			Asserter.AddToScope(locationType1, locationType2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleTextFilter)filterBizO1[LocationTypeFilterBusinessObject.Schema.Description];
			moduleFilter1.Property = "TestDD1";
			moduleFilter1.IsActive = true;
			Asserter.AssertMatches("Shoud return only locationType1", filterBizO1.Filter, locationType1);

			moduleFilter1.Property = "TestDD2";
			Asserter.AssertMatches("Shoud return only locationType2", filterBizO1.Filter, locationType2);
		}

		#endregion

		#region TestLocationClass

		public void TestLocationClassFilter()
		{
			var locationType1 = Helper.CreateLocationType("LD1", "TestLD1", false, 1, CodeLists.LocationClasses.Codes.FIX);
			var locationType2 = Helper.CreateLocationType("LD2", "TestLD2", false, 0, CodeLists.LocationClasses.Codes.DDL);
			Factory.Save();
			Asserter.AddToScope(locationType1, locationType2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var locationTypeFilter = (ModuleTextFilter)filterBizO1[LocationTypeFilterBusinessObject.Schema.LocationClass];
			AssertContainsExactElementsInAnyOrder(locationTypeFilter.ComparisonOperator_List.GetAllCodes(),
				new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual });

			locationTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			locationTypeFilter.Property = CodeLists.LocationClasses.Codes.FIX;
			locationTypeFilter.IsActive = true;
			Asserter.AssertMatches("Shoud return only locationType1", filterBizO1.Filter, locationType1);

			locationTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("Shoud return only locationType2", filterBizO1.Filter, locationType2);
		}

		public void TestLocationClassFilter_List()
		{
			var expectedLocationClasses = new LocationClasses();
			var filterBizO1 = GetNewFilterStripBusinessObject();
			var locationTypeFilter = (ModuleTextFilter)filterBizO1[LocationTypeFilterBusinessObject.Schema.LocationClass];
			AssertContainsExactElementsInAnyOrder(expectedLocationClasses, locationTypeFilter.List);
		}

		#endregion

		#region TestDefaultGranularity

		public void TestDefaultGranularityFilter()
		{
			var locationType1 = Helper.CreateLocationType("LD1", "TestLD1", false, 1, CodeLists.LocationClasses.Codes.FIX);
			locationType1.WLT_DefaultCycleCountGranularity = CodeLists.CycleCountGranularities.Codes.PID;
			var locationType2 = Helper.CreateLocationType("LD2", "TestLD2", false, 0, CodeLists.LocationClasses.Codes.DDL);
			Factory.Save();
			Asserter.AddToScope(locationType1, locationType2);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var defaultGranularityFilter = (ModuleTextFilter)filterBizO1[LocationTypeFilterBusinessObject.Schema.DefaultGranularity];
			AssertContainsExactElementsInAnyOrder(defaultGranularityFilter.ComparisonOperator_List.GetAllCodes(),
				new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual });

			defaultGranularityFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			defaultGranularityFilter.Property = CodeLists.CycleCountGranularities.Codes.PID;
			defaultGranularityFilter.IsActive = true;
			Asserter.AssertMatches("Shoud return only locationType1", filterBizO1.Filter, locationType1);

			defaultGranularityFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("Shoud return only locationType2", filterBizO1.Filter, locationType2);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LocationTypeFilterBusinessObject();
		}

		WhsTestHelperFunctionsEnv Helper => (helper = helper ?? new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		FilterStripAsserter<WhsLocationType> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsLocationType>(Factory, c => c.WLT_Code));
		FilterStripAsserter<WhsLocationType> asserter;

		#endregion
	}
}
