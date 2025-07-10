using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using GlowIndexQueryService.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class IndexFilterHandlerBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetFilters_WhenRequiredIndexSearchFieldsIsEmpty()
		{
			var parent = new FilterStripBusinessObjectForTesting();
			var filter = new DummyIndexFilterForTest(parent);
			filter.RequiredIndexSearchFields_Exposed = Array.Empty<string>();

			var generatedFilters = filter.GetFilters().ToArray();

			AssertEquals(1, generatedFilters.Length);
			Assert(!parent.ReturnNoResultsQuery);
			Assert(generatedFilters[0].Description == "DummyModuleFilter");
		}

		public void TestGetFilters_WhenRequiredIndexSearchFieldsPresent()
		{
			var parent = new FilterStripBusinessObjectForTesting();
			parent.IndexSearchFields = new(null, new SearchField[] { SearchField.Create("ExistentDummyField", "") });

			var filter = new DummyIndexFilterForTest(parent);
			filter.RequiredIndexSearchFields_Exposed = new[] { "ExistentDummyField" };

			var generatedFilters = filter.GetFilters().ToArray();

			AssertEquals(1, generatedFilters.Length);
			Assert(!parent.ReturnNoResultsQuery);
			Assert(generatedFilters[0].Description == "DummyModuleFilter");
		}

		public void TestGetFilters_WhenRequiredIndexSearchFieldsNotPresent()
		{
			var parent = new FilterStripBusinessObjectForTesting();
			parent.IndexSearchFields = new(null, new SearchField[] { SearchField.Create("ExistentDummyField", "") });

			var filter = new DummyIndexFilterForTest(parent);
			filter.RequiredIndexSearchFields_Exposed = new[] { "NonExistentDummyField" };

			var generatedFilters = filter.GetFilters().ToArray();

			AssertEquals(0, generatedFilters.Length);
			Assert("It should not return any results", parent.ReturnNoResultsQuery);
		}

		class DummyIndexFilterForTest : IndexFilterHandlerBase
		{
			public DummyIndexFilterForTest(FilterStripBusinessObject parent) : base(parent)
			{
			}

			public override IReadOnlyCollection<Type> ApplicableTypes { get; }

			public override string[] RequiredIndexSearchFields => RequiredIndexSearchFields_Exposed;

			public string[] RequiredIndexSearchFields_Exposed { get; set; }

			protected override IEnumerable<ModuleFilter> GetFiltersCore()
			{
				var mf = new ModuleTextFilter("DummyModuleFilter", () => { });
				return new[] { mf };
			}
		}
	}
}
