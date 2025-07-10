using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ConsolidateChargeCodeFilterBusinessObject : AccChargeCodeFilterBusinessObject
	{
		public ConsolidateChargeCodeFilterBusinessObject()
		{
		}

		#region Filters

		protected override void AddAdditionalFilters(ModuleFilterCollection filters)
		{
			base.AddAdditionalFilters(filters);
			AddCompanyFilters(filters);
		}

		#endregion

		#region Company

		protected void AddCompanyFilters(ModuleFilterCollection filters)
		{
			var subgroup = new CompanySubGroup();

			var filter = filters.AddTextFilter("CompanyCode", GlbCompanySchema.GC_Code);
			filter.Category = FilterCategories.TextSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ConsolidateChargeCodeFilter|CompanyCode", "Company Code");
			filter.SubGroup = subgroup;

			filter = filters.AddTextFilter("CompanyName", GlbCompanySchema.GC_Name);
			filter.Category = FilterCategories.TextSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ConsolidateChargeCodeFilter|CompanyName", "Company Name");
			filter.SubGroup = subgroup;
		}

		class CompanySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(GlbCompany), AccChargeCodeSchema.AC_GC);
				subQuery.AddToFilter(filter);
				var query = new ZDBOnlyQuery(typeof(AccChargeCode));
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion
	}
}
