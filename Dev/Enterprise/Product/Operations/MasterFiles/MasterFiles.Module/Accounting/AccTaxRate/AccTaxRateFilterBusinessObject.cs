using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxRateFilterBusinessObject : FilterStripBusinessObject
	{
		public AccTaxRateFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddPostingGroupFilter(result);
			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", AccTaxRateSchema.AT_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccTaxRateFilter|Code", "Code");
			filters.AddTextFilter("Description", AccTaxRateSchema.AT_Description).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccTaxRateFilter|Description", "Description");
		}

		void AddPostingGroupFilter(ModuleFilterCollection filters)
		{
			if (AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				filters.AddNumberFilter("Posting Group", GetPostingGroupQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccTaxRateFilter|Posting Group", "Posting Group");
			}
		}

		ZQuery GetPostingGroupQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var querySql = new ZQuery();
			querySql.AddToFilter_PossiblyCommaSeparated(AccTaxRateSchema.AT_Description, comparisonOperator, value);

			var sqlText = querySql.LiteralTextSqlFormatted.Replace(AccTaxRateSchema.Constants.AT_Description,
				ZString.Format(" CAST({0} AS VARCHAR(10)) ", AccTaxRateSchema.Constants.AT_PostingGroupId));

			var query = new ZDBOnlyQuery(typeof(AccTaxRate));
			query.AddFilterAndZSQLParameterCollection(sqlText, null, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion
	}
}
