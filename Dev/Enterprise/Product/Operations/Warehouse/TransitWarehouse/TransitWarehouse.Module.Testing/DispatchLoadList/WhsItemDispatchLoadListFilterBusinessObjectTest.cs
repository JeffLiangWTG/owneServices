using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListFilterBusinessObject))]
	public class WhsItemDispatchLoadListFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse1.PK);
			var dll3 = Helper.CreateDispatchLoadList("DLL3", warehouse2.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dll1, dll2, dll3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemDispatchLoadListFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return load lists for the given warehouse", filters.Filter, dll1, dll2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return load lists for the given warehouse", filters.Filter, dll3);
			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Must only return load lists for the given warehouse", filters.Filter);
		}

		public void TestReferenceNumber()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse1.PK);
			dll2.WDL_ReferenceNumber = "reference2";
			var dll3 = Helper.CreateDispatchLoadList("DLL3", warehouse2.PK);
			dll3.WDL_ReferenceNumber = "reference3";

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dll1, dll2, dll3);
			var filter = (ModuleNumberFilter)filters.ModuleFilters[WhsItemDispatchLoadListFilterBusinessObject.Schema.ReferenceNumber];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "DLL1";
			Asserter.AssertMatches("Must only return load lists for the given ReferenceNumber", filter, dll1);
			filter.Property = "reference2";
			Asserter.AssertMatches("Must only return load lists for the given ReferenceNumber", filter, dll2);
			filter.Property = "reference4";
			Asserter.AssertMatches("Must only return load lists for the given ReferenceNumber", filter);
		}

		public void TestJobID()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse1.PK);
			var dll3 = Helper.CreateDispatchLoadList("DLL3", warehouse2.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dll1, dll2, dll3);
			var filter = (ModuleNumberFilter)filters.ModuleFilters[WhsItemDispatchLoadListFilterBusinessObject.Schema.JobID];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "DLL1";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter, dll1);
			filter.Property = "DLL2";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter, dll2);
			filter.Property = "DLL4";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter);
		}

		public void TestCreditor()
		{
			var warehouse = Helper.CreateTRWWarehouse();

			var creditor1 = Helper.CreateClient("CDT1");
			var creditor2 = Helper.CreateClient("CDT2");
			var creditor3 = Helper.CreateClient("CDT3");

			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, creditor: creditor1);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK, creditor: creditor1);
			var dll3 = Helper.CreateDispatchLoadList("DLL3", warehouse.PK, creditor: creditor2);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dll1, dll2, dll3);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsItemDispatchLoadListFilterBusinessObject.Schema.Creditor];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Organisations, filter.Category);

			filter.Property = "CDT1";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter, dll1, dll2);
			filter.Property = "CDT2";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter, dll3);
			filter.Property = "CDT3";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter);
			filter.Property = "";
			Asserter.AssertMatches("Must only return load lists for the given JobID", filter, dll1, dll2, dll3);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new WhsItemDispatchLoadListFilterBusinessObject();

		protected FilterStripAsserter<WhsItemDispatchLoadList> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemDispatchLoadList>(Factory, dll => dll.WDL_JobID)); }
		}
		FilterStripAsserter<WhsItemDispatchLoadList> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
