using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccWithholdingFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", AccWithholdingSchema.AW_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccWithholdingFilter|Code", "Code");
			filters.AddTextFilter("Description", AccWithholdingSchema.AW_Description).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccWithholdingFilter|Description", "Description");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter rateFilter = filters.AddTextFilter("Rate", GetRateFilter);
			rateFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			rateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccWithholdingFilter|Rate", "Rate");
		}

		ZQuery GetRateFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDecimal result;
			if (ZDecimal.TryParse(value, out result))
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(AccWithholdingSchema.AW_Rate, SQLComparisonOperator.Equal, result);

				return query;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region ActiveStatus List

		public CodeDescriptionPairList ActiveStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.AllClients, OrgConstants.FilterControl.ActiveStatus.Description.AllClients);
				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients, OrgConstants.FilterControl.ActiveStatus.Description.ActiveClients);
				list.AddPair(OrgConstants.FilterControl.ActiveStatus.Code.InactiveClients, OrgConstants.FilterControl.ActiveStatus.Description.InactiveClients);

				return list;
			}
		}

		#endregion

		#endregion
	}
}
