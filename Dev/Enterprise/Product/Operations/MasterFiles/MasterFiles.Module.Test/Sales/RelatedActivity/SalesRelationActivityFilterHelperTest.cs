using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class SalesRelationActivityFilterHelperTest : TestCaseWithFactory
	{
		#region Module Filters

		public void TestAddAllModuleFilters()
		{
			var filters = new ModuleFilterCollection();
			SalesRelationActivityFilterHelper.AddAllModuleFilters(Factory, filters, OrgOpportunitySchema.Instance, OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var recentActivityDatefilter = filters[SalesRelationActivityFilterHelper.FilterDescription.RecentActivityDate];
			AssertNotNull(recentActivityDatefilter);
			AssertType(typeof(RecentActivityDateFilter), recentActivityDatefilter);
			AssertEquals("Sales Relations Last Edit Time", recentActivityDatefilter.MultilingualDescription);

			var hasSalesRelationfilter = filters[SalesRelationActivityFilterHelper.FilterDescription.HasSalesRelation];
			AssertNotNull(hasSalesRelationfilter);
			AssertType(typeof(ActivityHasSalesRelationFilter), hasSalesRelationfilter);
			AssertEquals("Has Sales Relation", hasSalesRelationfilter.MultilingualDescription);
		}

		#endregion

		#region Lists

		public void TestGetSalesRelationTypeList()
		{
			var expectedCodes = new[] { SalesRelationActivityFilterHelper.AnySalesRelationTypeCode }.Union(SalesRelationTypeList.New().GetCodes());
			var result = SalesRelationActivityFilterHelper.GetSalesRelationTypeList();
			var actualCodes = result.Cast<ICodeDescription>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder("Filter list should include all sales relation type codes", expectedCodes, actualCodes);
		}

		#endregion
	}
}
