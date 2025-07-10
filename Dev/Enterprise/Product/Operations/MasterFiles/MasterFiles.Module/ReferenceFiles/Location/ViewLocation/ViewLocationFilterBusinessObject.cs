using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ViewLocationFilterBusinessObject : FilterStripBusinessObject
	{
		public ViewLocationFilterBusinessObject(ViewLocationCollection collection)
		{
			this.collection = collection;
		}
		readonly ViewLocationCollection collection;

		public ViewLocationFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddFilters(filters);
			return filters;
		}

		void AddFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Location Type", GetLocationTypeQuery, LocationTypeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ViewLocationFilterBusinessObject|LocationType", "Location Type");
			filters.AddTextFilter("Code", ViewLocationSchema.VLO_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ViewLocationFilterBusinessObject|Code", "Code");
			filters.AddTextFilter("Description", ViewLocationSchema.VLO_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ViewLocationFilterBusinessObject|Description", "Description");

			var countryFilter = filters.AddNkFilter("Country", ViewLocationSchema.VLO_CountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ViewLocationFilterBusinessObject|CountryCode", "Country/Region");
		}

		ZQuery GetLocationTypeQuery(ZString code)
		{
			string tableCode = "";
			switch (code)
			{
				case ViewLocationTypeList.Codes.UNLOCO:
					tableCode = RefUNLOCOSchema.Constants.Prefix;
					break;
				case ViewLocationTypeList.Codes.Country:
					tableCode = RefCountrySchema.Constants.Prefix;
					break;
				case ViewLocationTypeList.Codes.State:
					tableCode = RefCountryStatesSchema.Constants.Prefix;
					break;
				case ViewLocationTypeList.Codes.City:
					tableCode = RefCityTownSchema.Constants.Prefix;
					break;
				case ViewLocationTypeList.Codes.InternationalZone:
					tableCode = RefZoneHeaderSchema.Constants.Prefix;
					break;
				case ViewLocationTypeList.Codes.TransportZone:
					tableCode = RateTransportZonesSchema.Constants.Prefix;
					break;
			}

			return new ZQuery(ViewLocationSchema.VLO_TableCode, tableCode);
		}

		CodeDescriptionPairList LocationTypeList
		{
			get
			{
				var result = new ViewLocationTypeList();

				if (!collection.LocationsTypesToInclude.HasFlag(ViewLocationType.UNLOCO))
				{ result.RemoveCode(ViewLocationTypeList.Codes.UNLOCO); }
				if (!collection.LocationsTypesToInclude.HasFlag(ViewLocationType.Country))
				{ result.RemoveCode(ViewLocationTypeList.Codes.Country); }
				if (!collection.LocationsTypesToInclude.HasFlag(ViewLocationType.State))
				{ result.RemoveCode(ViewLocationTypeList.Codes.State); }
				if (!collection.LocationsTypesToInclude.HasFlag(ViewLocationType.City))
				{ result.RemoveCode(ViewLocationTypeList.Codes.City); }
				if (!collection.LocationsTypesToInclude.HasFlag(ViewLocationType.InternationalZone))
				{ result.RemoveCode(ViewLocationTypeList.Codes.InternationalZone); }
				if (!collection.LocationsTypesToInclude.HasFlag(ViewLocationType.TransportZone))
				{ result.RemoveCode(ViewLocationTypeList.Codes.TransportZone); }

				return result;
			}
		}
	}
}
