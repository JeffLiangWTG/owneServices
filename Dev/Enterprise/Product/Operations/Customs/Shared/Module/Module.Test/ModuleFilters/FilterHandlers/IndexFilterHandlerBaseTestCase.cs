using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestsSubclassesOf(typeof(IndexFilterHandlerBase))]
	abstract class IndexFilterHandlerBaseTestCase<TIndexFilterHandler> : TestCaseWithFactory where TIndexFilterHandler : IndexFilterHandlerBase
	{
		public void TestApplicableTypes_MatchesExpected()
		{
			AssertContainsExactElementsInAnyOrder("Applicable types should match the expected.", ExpectedApplicableTypes, FilterHandlerToTest.ApplicableTypes);
		}

		public void TestApplicableTypes_AreChildrenOfFilterStripBusinessObject()
		{
			CombineAssertions(() =>
			{
				foreach (var applicableType in FilterHandlerToTest.ApplicableTypes)
				{
					Assert($"{applicableType.Name} should be a child of FilterStripBusinessObject.", applicableType.IsSubclassOf(typeof(FilterStripBusinessObject)));
				}
			});
		}

		public void TestIsApplicable_ReturnsTrue_ForApplicableTypes()
		{
			CombineAssertions(() =>
			{
				foreach (var applicableType in FilterHandlerToTest.ApplicableTypes)
				{
					var applicableObject = (FilterStripBusinessObject)Activator.CreateInstance(applicableType);
					var filterHandler = GetNewIndexFilterHandler(applicableObject);
					Assert($"{applicableType.Name} should be applicable", filterHandler.IsApplicable());
				}
			});
		}

		public void TestAdd_WithNoIndexSearchFields_ShouldReturnSameFilters()
		{
			var searchFieldCollection = new SearchFieldCollection(null, Array.Empty<SearchField>());
			FilterHandlerToTest.Parent.IndexSearchFields = searchFieldCollection;

			var filters = new ModuleFilterCollection();
			var result = FilterHandlerToTest.Add(filters);

			AssertEquals(filters, result);
			AssertContainsExactElementsInAnyOrder(filters, result);
			AssertEquals(0, result.Count());
		}

		public void TestAdd_WithNoValidIndexSearchFields_ShouldMarkItAsReturnNoResultsQuery()
		{
			FilterHandlerToTest.Parent.ReturnNoResultsQuery = false;

			FilterHandlerToTest.Parent.IndexSearchFields = FilterStripBusinessObjectForTesting.GetNewDummySearchFieldCollection();
			Assert("[PreCondition]: FilterStripBusinessObject should not set as ReturnNoResultsQuery", !FilterHandlerToTest.Parent.ReturnNoResultsQuery);

			var filters = new ModuleFilterCollection();
			var result = FilterHandlerToTest.Add(filters);

			SimulateFilterAdded(FilterHandlerToTest.Parent);

			AssertEquals(filters, result);
			AssertContainsExactElementsInAnyOrder(filters, result);
			AssertEquals(0, result.Count());
			Assert("FilterStripBusinessObject should be set as ReturnNoResultsQuery", FilterHandlerToTest.Parent.ReturnNoResultsQuery);
		}

		public void TestAdd_WithApplicableHandler_ShouldAddFilters()
		{
			var filters = new ModuleFilterCollection();
			var result = FilterHandlerToTest.Add(filters);

			AssertEquals(filters, result);
			Assert(result.Any());

			SimulateFilterAdded(FilterStripBO);
			AssertSequencesEqual(ExpectedQueries, FilterStripBO.GetActiveFiltersQueries().Select(m => m.ToUrlComponent()));
		}

		public void TestAdd_WithNonApplicableHandler_ShouldReturnSameFilters()
		{
			var parent = new FilterStripBusinessObjectForTesting();
			var handler = GetNewIndexFilterHandler(parent);
			var filters = new ModuleFilterCollection();

			var result = handler.Add(filters);

			AssertEquals(filters, result);
			AssertEquals(0, result.Count());
		}

		#region Implementation

		protected abstract IReadOnlyCollection<Type> ExpectedApplicableTypes { get; }

		protected abstract IReadOnlyCollection<string> ExpectedQueries { get; }

		protected virtual SearchFieldCollection GetSearchFields()
		{
			var searchFields = new List<SearchField>();
			foreach (var requiredIndexSearchField in FilterHandlerToTest.RequiredIndexSearchFields)
			{
				var sf = SearchField.Create(requiredIndexSearchField, requiredIndexSearchField);
				searchFields.Add(sf);
			}
			return new SearchFieldCollection(string.Empty, searchFields.ToArray());
		}

		protected abstract FilterStripBusinessObject GetNewFilterStripBizO();

		protected abstract ZModule GetParentModule();

		protected virtual TIndexFilterHandler GetNewIndexFilterHandler(FilterStripBusinessObject parent)
		{
			return (TIndexFilterHandler)Activator.CreateInstance(typeof(TIndexFilterHandler), parent);
		}

		protected virtual string[] GlowUseIndexingForModuleList => new[] { "CusDec", };

		protected virtual void SimulateFilterAdded(FilterStripBusinessObject filterStripBusinessObject)
		{ }

		protected override void SetUp()
		{
			base.SetUp();

			tempEnableIndexingService = GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(default, default, default, true);
			tempEnableModuleForIndexing = GlowRegistry.Instance.GlowUseIndexingForModuleList.SetTemporaryValue(default, default, default, GlowUseIndexingForModuleList);

			parentModule = GetParentModule();

			FilterStripBO = GetNewFilterStripBizO();
			FilterStripBO.ParentModule = parentModule;

			FilterHandlerToTest = GetNewIndexFilterHandler(FilterStripBO);

			FilterStripBO.IndexSearchFields = GetSearchFields();
			FilterStripBO.SearchType = SearchType.Index;

			AssertEquals(SearchType.Index, FilterStripBO.SearchType);
		}

		protected override void TearDown()
		{
			tempEnableModuleForIndexing?.Dispose();
			tempEnableIndexingService?.Dispose();

			parentModule?.Dispose();

			base.TearDown();
		}

		protected FilterStripBusinessObject FilterStripBO { get; set; }
		protected TIndexFilterHandler FilterHandlerToTest { get; set; }

		IDisposable tempEnableIndexingService;
		IDisposable tempEnableModuleForIndexing;

		ZModule parentModule;

		#endregion
	}

	sealed class FilterStripBusinessObjectForTesting : FilterStripBusinessObject
	{
		public FilterStripBusinessObjectForTesting()
		{
			var searchFieldCollection = GetNewDummySearchFieldCollection();
			IndexSearchFields = searchFieldCollection;
		}

		#region Overrides of FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersCore() => new();

		#endregion

		public static SearchFieldCollection GetNewDummySearchFieldCollection() => new(null, new SearchField[] { SearchField.Create("AnUnusableTestField", "") });
	}
}
