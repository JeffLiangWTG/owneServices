using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Module
{
	public class UniversalCommodityCodeFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			var filter = filters.AddTextFilter("Universal Group", UniversalCommodityCodeSchema.RH_UniversalCommodityGroup);
			filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalCommodityCodeFilter|UniversalGroup", "Universal Group");
			filter.Visibility = FilterVisibility.AlwaysVisible;

			filter = filters.AddTextFilter("Universal Group Description", UniversalCommodityCodeSchema.RH_UniversalCommodityGroupDescription);
			filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalCommodityCodeFilter|UniversalGroupDescription", "Universal Group Description");
			filter.Visibility = FilterVisibility.AlwaysVisible;

			filter = filters.AddTextFilter("Commodity Code", UniversalCommodityCodeSchema.RH_Code);
			filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalCommodityCodeFilter|CommodityCode", "Commodity Code");

			filter = filters.AddTextFilter("Commodity Description", UniversalCommodityCodeSchema.RH_Description);
			filter.MultilingualDescription = ResString.GetMultilingualString("Rating|UniversalCommodityCodeFilter|CommodityDescription", "Commodity Description");

			return filters;
		}

		protected override bool ShouldAddCustomSqlFilter => false;
	}
}
