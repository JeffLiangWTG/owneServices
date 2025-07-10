using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbCapabilityFilterBusinessObject))]
	sealed class GlbCapabilityFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCapabilityScopeFilter()
		{
			var bizo = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizo.ModuleFilters["Capacity Scope"];
			filter.Property = GlbCapabilityScopeList.Codes.GlobalScope;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			AssertEquals("G4_CapacityScope = 'GLB'", filter.Query.LiteralTextADO);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbCapabilityFilterBusinessObject();
		}

		public void TestResourcesWithCapabilityFilter()
		{
			var staff1 = MasterFilesTestHelper.CreateStaff(Factory, "YES");
			var staff2 = MasterFilesTestHelper.CreateStaff(Factory, "NO");
			var staff3 = MasterFilesTestHelper.CreateStaff(Factory, "YES");
			var staff4 = MasterFilesTestHelper.CreateStaff(Factory, "NO");

			var capability1 = MasterFilesTestHelper.CreateCapability(Factory, "ANY", "Should match on Any Match only.", staff1, staff2);
			var capability2 = MasterFilesTestHelper.CreateCapability(Factory, "NON", "Should match on None Match only.", staff2, staff4);
			var capability3 = MasterFilesTestHelper.CreateCapability(Factory, "ALY", "Should match on Any Match and All Match.", staff1, staff3);
			var capability4 = MasterFilesTestHelper.CreateCapability(Factory, "ALN", "Should match on All Match and None Match.");

			Factory.Save();

			var filterBizo = new GlbCapabilityFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<ModuleGuidPivotFilter>("Resources with Capability");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip("Email Address", "YES");

			var query = filterBizo.Filter;
			var result = Factory.Load<GlbCapability>(query);
			AssertContainsExactElementsInAnyOrder("Any match: " + query.LiteralTextSqlFormatted, new[] { "Should match on Any Match only.", "Should match on Any Match and All Match." }, result.Select(x => x.G4_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			query = filterBizo.Filter;
			result = Factory.Load<GlbCapability>(query);
			AssertContainsExactElementsInAnyOrder("None match: " + query.LiteralTextSqlFormatted, new[] { "Should match on None Match only.", "Should match on All Match and None Match." }, result.Select(x => x.G4_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			query = filterBizo.Filter;
			result = Factory.Load<GlbCapability>(query);
			AssertContainsExactElementsInAnyOrder("All match: " + query.LiteralTextSqlFormatted, new[] { "Should match on Any Match and All Match.", "Should match on All Match and None Match." }, result.Select(x => x.G4_Description));
		}

		public void TestIndexSearchCapabilityScopeFilter()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbCapability))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IGlbCapability" });
				}))
			{
				var scopeFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["CapacityScope"];
				AssertNotNull(scopeFilter);
				AssertEquals(FilterCategories.StatusAndFlags, scopeFilter.Category);
				AssertEquals(GlbCapabilityScopeList.Codes.GlobalScope, scopeFilter.DefaultProperty);
				AssertEquals("Capacity Scope", scopeFilter.MultilingualDescription.ToString());
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("CapacityScope", "Capacity Scope");
			var ret = new SearchFieldCollection("IGlbCapability", new SearchField[] { field1 });
			return ret;
		}
	}
}
