using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefAirlineCommodityCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public RefAirlineCommodityCodeFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", RefAirlineCommodityCodeSchema.RAC_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineCommodityCodeFilter|Code", "Code");
			filters.AddTextFilter("Description", RefAirlineCommodityCodeSchema.RAC_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineCommodityCodeFilter|Description", "Description");
			filters.AddTextFilter("Airline Numeric Code", RefAirlineCommodityCodeSchema.RAC_AirlineID).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineCommodityCodeFilter|AirlineNumericCode", "Airline Numeric Code");
		}

		#endregion
	}
}
