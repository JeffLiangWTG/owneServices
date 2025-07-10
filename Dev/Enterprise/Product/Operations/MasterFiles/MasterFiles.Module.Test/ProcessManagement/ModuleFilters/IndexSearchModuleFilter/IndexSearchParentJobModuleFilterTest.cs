using System;
using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(IndexSearchParentJobModuleFilter))]
	sealed class IndexSearchParentJobModuleFilterTest : IndexSearchModuleFilterTestCase<IndexSearchParentJobModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestGetGlowIndexQuery()
		{
			var field = SearchField.Create("moo");
			var filter = new IndexSearchParentJobModuleFilter(field, ProcessTasksSchema.P9_ParentID, Factory);
			filter.IsActive = true;
			var guid = Guid.NewGuid();

			filter.ComparisonOperator = "exact";
			filter.SelectedModule = "OrgHeader";
			filter.Property = guid;
			AssertEquals($"(moo eq {guid})", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "not equal";
			filter.SelectedModule = "OrgHeader";
			filter.Property = guid;
			AssertEquals($"(moo ne {guid})", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is blank";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = "is not blank";
			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());
		}

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexSearchParentJobModuleFilter(field, ProcessTasksSchema.P9_ParentID, Factory);
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleFilter modulefilter)
		{
			var filter = modulefilter as IndexSearchParentJobModuleFilter;
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "SelectedModule" }).ToArray();
		}
	}
}
