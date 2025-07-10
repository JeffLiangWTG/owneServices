using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefDomesticCartageZoneFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("City/Town", RefDomesticCartageZoneSchema.F1_CityTown).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|CityTown", "City/Town");
			filters.AddTextFilter("Postcode", RefDomesticCartageZoneSchema.F1_CityTownPostCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|Postcode", "Postcode");
			filters.AddTextFilter("Airport City/Town", RefDomesticCartageZoneSchema.F1_AirportCity).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|AirportCityTown", "Airport City/Town");
			filters.AddTextFilter("Airport Postcode", RefDomesticCartageZoneSchema.F1_AirportPostcode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|AirportPostcode", "Airport Postcode");
			filters.AddTextFilter("Zone Identifier", RefDomesticCartageZoneSchema.F1_Zone).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|ZoneIdentifier", "Zone Identifier");

			ModuleFilter filter = filters.AddNkFilter("UNLOCO", RefDomesticCartageZoneSchema.F1_RL_NKLoco, ModuleIDs.RefUNLOCO, UNLOCOs);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|UNLOCO", "UNLOCO");

			filter = filters.AddTextFilter("IATA Code", RefDomesticCartageZoneSchema.F1_PortCode);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|IATACode", "IATA Code");

			filter = filters.AddNkFilter("State", RefDomesticCartageZoneSchema.F1_RW_NKState, ModuleIDs.RefCountryStates, States);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|State", "State");

			filter = filters.AddFlagsFilter("Is Beyond", new string[] { Res.GetString("4e58a2d5-c8d1-428c-8bfa-ef4b5889bef0", "Show Marked As Beyond") },
					new SchemaBoolColumn[] { RefDomesticCartageZoneSchema.F1_IsBeyond });
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefDomesticCartageZoneFilter|IsBeyond", "Is Beyond");

			return filters;
		}

		#endregion

		#region Lookups

		RefUNLOCOCollection UNLOCOs
		{
			get { return fUNLOCOs ?? (fUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}

		RefUNLOCOCollection fUNLOCOs;

		RefCountryStatesCollection States
		{
			get { return fStates ?? (fStates = new RefCountryStatesCollection(Factory)); }
		}

		RefCountryStatesCollection fStates;

		#endregion
	}
}
