using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(StatusIndexFilter))]
	class StatusFilterTest : IndexSearchModuleFilterTestCase<StatusIndexFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new StatusIndexFilter(field, new ProcessTaskFilterBusinessObject().Statuses);
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = new StatusIndexFilter(SearchField.Create("moo"), new ProcessTaskFilterBusinessObject().Statuses);
			filter.Property = "ASN";

			filter.ComparisonOperator = "any starts with";
			AssertEquals("(startswith(moo,'ASN'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "NCM";
			AssertEquals("((moo eq 'OPN') or (moo eq 'ASN') or (moo eq 'WRK') or (moo eq 'SUS'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "A+S";
			AssertEquals("((moo eq 'ASN') or (moo eq 'SUS'))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "W+S";
			AssertEquals("((moo eq 'WRK') or (moo eq 'SUS'))", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}
}
