using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using FilterName = Enterprise.Customs.Universal.RefCusTradeGroupCollection.FilterName;

namespace Enterprise.Customs.Universal.Module
{
	public class RefCusTradeGroupFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		static void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddFilter(new ModuleTextFilter(FilterName.EconomicGroup, RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, new EconomicGroupList())
			{
				Category = FilterCategories.Organisations,
				MultilingualDescription = ResString.GetMultilingualString("RefCusTradeGroupFilter|EconomicGroup", FilterName.EconomicGroup),
			});

			filters.AddFilter(new ModuleTextFilter(FilterName.Code, RefCusTradeGroupSchema.ZZA_TradeGroup)
			{
				Category = FilterCategories.AttributeSearch,
				MultilingualDescription = ResString.GetMultilingualString("RefCusTradeGroupFilter|Code", FilterName.Code),
			});

			filters.AddFilter(new ModuleTextFilter(FilterName.Name, RefCusTradeGroupSchema.ZZA_Description)
			{
				Category = FilterCategories.AttributeSearch,
				MultilingualDescription = ResString.GetMultilingualString("RefCusTradeGroupFilter|Name", FilterName.Name),
			});
		}
	}
}
