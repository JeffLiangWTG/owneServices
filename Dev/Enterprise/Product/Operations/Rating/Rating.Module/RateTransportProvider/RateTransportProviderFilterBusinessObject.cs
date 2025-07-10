namespace Enterprise.Rating.Module
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	public class RateTransportProviderFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddGuidFilter("Related Organisation", ModuleIDs.Organisation, RateTransportProviderSchema.TP_OH_RelatedParty, TransportProviders)
				.MultilingualDescription = ResString.GetMultilingualString("1eb931e4-4571-4204-b355-2816d22106c7", "Zone Owner");

			var countryFilter = filters.AddNkFilter("Country", GetCountryQuery, ModuleIDs.RefCountry, Countries)
				.WithMaxLengthOf<ModuleNkFilter>(RateTransportProviderSchema.TP_RN_NKCountry)
				.WithMaxLengthOf<ModuleNkFilter>(RefCityTownSchema.R9_RN_NKCountry);
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("de37450d-c543-47d3-b516-9b55101474bc", "Zone Hub Country/Region");
			countryFilter.Category = FilterCategories.Locations;

			var zoneHubLocationFilter = filters.AddGuidFilter("Zone Hub", ModuleIDs.RefCityTown, RateTransportProviderSchema.TP_R9_ZoneHubLocation, ZoneHubLocations);
			zoneHubLocationFilter.MultilingualDescription = ResString.GetMultilingualString("d5c8ae72-1295-4b5a-990d-aaaee0f40bf1", "Zone Hub Location");
			zoneHubLocationFilter.Category = FilterCategories.Locations;

			filters.AddFlagsFilter("Zone Types", ZoneTypeFlagDescriptions, ZoneTypeFlagsQueries, JoinCondition.Or)
				.MultilingualDescription = ResString.GetMultilingualString("Rating|RateTransportProviderFilterBusinessObject|ZoneType", "Zone Type");

			filters.AddTextFilter("Zone Modes", RateTransportProviderSchema.TP_ZoneMode, ZoneModeList).MultilingualDescription = ResString.GetMultilingualString("Rating|RateTransportZoneFilterBusinessObject|ZoneMode", "Zone Mode");

			return filters;
		}

		#endregion

		#region Lookups

		public OrganisationsFindBoxCollection TransportProviders
		{
			get { return fTransportProviders ?? (fTransportProviders = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection fTransportProviders;

		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(Factory)); }
		}
		RefCountryCollection countries;

		ZQuery GetCountryQuery(SQLComparisonOperator sqlOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(RateTransportProvider));
			result.AddToFilter(RateTransportProviderSchema.TP_RN_NKCountry, sqlOperator, value);
			result.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_R9_ZoneHubLocation, null);

			var cityTownCountrySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RateTransportProviderSchema.TP_R9_ZoneHubLocation);
			cityTownCountrySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, sqlOperator, value);

			result.AddSubQuery(cityTownCountrySubQuery, JoinCondition.Or);

			return result;
		}

		public RefCityTownCollection ZoneHubLocations
		{
			get { return cityTownCollection ?? (cityTownCollection = new RefCityTownCollection(Factory)); }
		}
		RefCityTownCollection cityTownCollection;

		#region Zone Type

		string[] ZoneTypeFlagDescriptions
		{
			get { return zoneTypeFlagDescriptions ?? (zoneTypeFlagDescriptions = GetZoneTypeDescriptions()); }
		}

		string[] zoneTypeFlagDescriptions;

		static string[] GetZoneTypeDescriptions()
		{
			var result = new List<string>
			{
				RatingConstants.RatingZoneTypes.Descriptions.All,
				RatingConstants.RatingZoneTypes.Descriptions.Operations,
				RatingConstants.RatingZoneTypes.Descriptions.Rating,
				RatingConstants.RatingZoneTypes.Descriptions.Reporting
			};

			return result.ToArray();
		}

		GetFlagsQuery[] ZoneTypeFlagsQueries
		{
			get { return zoneTypeFlagsQueries ?? (zoneTypeFlagsQueries = GetFlagsQueries()); }
		}

		GetFlagsQuery[] zoneTypeFlagsQueries;

		GetFlagsQuery[] GetFlagsQueries()
		{
			var result = new List<GetFlagsQuery>
			{
				value => GetZoneTypeQuery(value, RatingConstants.RatingZoneTypes.All),
				value => GetZoneTypeQuery(value, RatingConstants.RatingZoneTypes.Operations),
				value => GetZoneTypeQuery(value, RatingConstants.RatingZoneTypes.Rating),
				value => GetZoneTypeQuery(value, RatingConstants.RatingZoneTypes.Reporting)
			};

			return result.ToArray();
		}

		ZQuery GetZoneTypeQuery(bool hasValueSelected, ZString zoneType)
		{
			var result = new ZDBOnlyQuery(typeof(RateTransportZone));
			if (hasValueSelected)
			{
				result.AddToFilter(RateTransportProviderSchema.TP_ZoneType, zoneType);
			}

			return result;
		}

		#endregion

		CodeDescriptionPairList ZoneModeList
		{
			get { return zoneModeList ?? (zoneModeList = new CodeDescriptionPairList(OLookUpEditType.RateModes)); }
		}
		CodeDescriptionPairList zoneModeList;

		#endregion
	}
}

