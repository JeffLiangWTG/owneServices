using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefAirlineFilterBusinessObject : FilterStripBusinessObject
	{
		public RefAirlineFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddCodeTextFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Address", RefAirlineSchema.RM_AddressLine1).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|Address", "Address");
			filters.AddTextFilter("City", RefAirlineSchema.RM_AirlineCity).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|City", "City");
			filters.AddTextFilter("Country", RefAirlineSchema.RM_AirlineCountry).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|Country", "Country/Region");
			filters.AddTextFilter("Name", RefAirlineSchema.RM_AirlineName1).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|Name", "Name");
		}

		void AddCodeTextFilters(ModuleFilterCollection filters)
		{
			FilterCategory codeCategory = new FilterCategory(ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|AirlineCodes", "Airline Codes"));

			ModuleFilter filter = filters.AddTextFilter("Airline Numeric Code", RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode);
			filter.Category = codeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|AirlineNumericCode", "Airline Numeric Code");

			filter = filters.AddTextFilter("Two Char Code", RefAirlineSchema.RM_TwoCharacterCode);
			filter.Category = codeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|TwoCharCode", "Two Char Code");

			filter = filters.AddTextFilter("Three Char Code", RefAirlineSchema.RM_ThreeLetterCode);
			filter.Category = codeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefAirlineFilter|ThreeCharCode", "Three Char Code");
		}

		#endregion

		#endregion
	}
}
