using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.Universal.CusRefTradeGroupCollection.FilterConstants;

namespace Enterprise.Customs.Module
{
	class TradeGroupsFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var countryCodeFilter = result.AddNkFilter(Constants.CountryCode, CusRefTradeGroupSchema.CR9_RN_NKCountryCode, ModuleIDs.RefCountry, Lookups.CountryCodeLookup);
			countryCodeFilter.Category = FilterCategories.Locations;
			countryCodeFilter.MultilingualDescription = ResString.GetMultilingualString("f7359156-d1bc-453e-85dc-e58e186555b1", Constants.CountryCode);
			countryCodeFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryCodeFilter.ReadOnly = true;

			var tradeGroupFilter = result.AddTextFilter(Constants.TradeGroup, GetTradeGroupFilter);
			tradeGroupFilter.Category = FilterCategories.TextSearch;
			tradeGroupFilter.MaxLength = CusRefTradeGroupSchema.CR9_TradeGroup.MaxLength;
			tradeGroupFilter.MultilingualDescription = ResString.GetMultilingualString("f94c7f1d-0765-4657-a376-94703567d597", Constants.TradeGroup);

			var descriptionFilter = result.AddTextFilter(Constants.Description, GetDescriptionQuery);
			descriptionFilter.Category = FilterCategories.TextSearch;
			descriptionFilter.MaxLength = CusRefTradeGroupSchema.CR9_Description.MaxLength;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("1c2cfd99-de5b-49fb-b431-a12b25c5c525", Constants.Description);

			var startDateFilter = result.AddDateFilter(Constants.StartDate, GetStartDateQuery);
			startDateFilter.Category = FilterCategories.Dates;
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("f945179d-98fb-4f7b-afdb-29e9453042e3", Constants.StartDate);

			var endDateFilter = result.AddDateFilter(Constants.EndDate, GetEndDateFilter);
			endDateFilter.Category = FilterCategories.Dates;
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("b91973ea-c4c5-4695-9403-b08114e37cc1", Constants.EndDate);

			return result;
		}

		ZQuery GetTradeGroupFilter(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(CusRefTradeGroupSchema.CR9_TradeGroup, comparisonOperator, value);

		ZQuery GetDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(CusRefTradeGroupSchema.CR9_Description, comparisonOperator, value);

		ZQuery GetStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime from, ZDateTime to)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusRefTradeGroupSchema.CR9_StartDate, from, to);
			return result;
		}

		ZQuery GetEndDateFilter(DateComparisonOperator comparisonOperator, ZDateTime from, ZDateTime to)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusRefTradeGroupSchema.CR9_EndDate, from, to);
			result.AddToFilter(JoinCondition.Or, CusRefTradeGroupSchema.CR9_EndDate, null);
			return result;
		}

		TradeGroupsFilterLookups Lookups => lookups ?? (lookups = new TradeGroupsFilterLookups(this));
		TradeGroupsFilterLookups lookups;
	}
}
