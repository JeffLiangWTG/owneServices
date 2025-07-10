using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	public class RateTransportZoneFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class FilterConstants
		{
			public const string ZoneName = "Zone Name";
			public const string ZoneOwner = "Zone Owner";
			public const string Country = "Country";
			public const string ZoneType = "Zone Type";
			public const string ZoneMode = "Zone Mode";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter(FilterConstants.ZoneName, RateTransportZonesSchema.TZ_ZoneName)
				.MultilingualDescription = ResString.GetMultilingualString("Rating|RateTransportZoneFilterBusinessObject|ZoneName", FilterConstants.ZoneName);

			filters.AddGuidFilter(FilterConstants.ZoneOwner, ModuleIDs.Organisation, GetZoneOwnerQuery, new LocalTransportCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("Rating|RateTransportZoneFilterBusinessObject|ZoneOwner", FilterConstants.ZoneOwner);

			var countryFilter = filters.AddNkFilter(FilterConstants.Country, GetCountryQuery, ModuleIDs.RefCountry, Countries)
				.WithMaxLengthOf<ModuleNkFilter>(RateTransportProviderSchema.TP_RN_NKCountry)
				.WithMaxLengthOf<ModuleNkFilter>(RefCityTownSchema.R9_RN_NKCountry);
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("de37450d-c543-47d3-b516-9b55101474bc", "Zone Hub Country/Region");
			countryFilter.Category = FilterCategories.Locations;

			filters.AddFlagsFilter(FilterConstants.ZoneType, ZoneTypeFlagDescriptions, ZoneTypeFlagsQueries, JoinCondition.Or)
				.MultilingualDescription = ResString.GetMultilingualString("Rating|RateTransportZoneFilterBusinessObject|ZoneType", FilterConstants.ZoneType);

			filters.AddTextFilter(FilterConstants.ZoneMode, GetZoneModeQuery, ZoneModeList)
				.MultilingualDescription = ResString.GetMultilingualString("Rating|RateTransportZoneFilterBusinessObject|ZoneMode", FilterConstants.ZoneMode);

			return filters;
		}

		#region Zone Owner (Transport Provider) Filter

		ZQuery GetZoneOwnerQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(RateTransportZone));
			var sub = new ZDBOnlySubQuery(typeof(RateTransportProvider), RateTransportZonesSchema.TZ_TP);
			sub.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, comparisonOperator, value);
			query.AddSubQuery(sub, JoinCondition.And);

			return query;
		}

		#endregion

		#region Country

		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(Factory)); }
		}
		RefCountryCollection countries;

		ZQuery GetCountryQuery(SQLComparisonOperator sqlOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RateTransportZone));

			var sub = new ZDBOnlySubQuery(typeof(RateTransportProvider), RateTransportZonesSchema.TZ_TP);
			sub.AddToFilter(RateTransportProviderSchema.TP_RN_NKCountry, sqlOperator, value);
			sub.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_R9_ZoneHubLocation, null);

			var cityTownCountrySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RateTransportProviderSchema.TP_R9_ZoneHubLocation);
			cityTownCountrySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, sqlOperator, value);
			sub.AddSubQuery(cityTownCountrySubQuery, JoinCondition.Or);

			query.AddSubQuery(sub, JoinCondition.And);

			return query;
		}

		#endregion

		#region Zone Type Filter

		string[] ZoneTypeFlagDescriptions
		{
			get { return zoneTypeFlagDescriptions ?? (zoneTypeFlagDescriptions = GetZoneTypeDescriptions()); }
		}

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

		string[] zoneTypeFlagDescriptions;

		GetFlagsQuery[] ZoneTypeFlagsQueries
		{
			get { return zoneTypeFlagsQueries ?? (zoneTypeFlagsQueries = GetFlagsQueries()); }
		}

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

		GetFlagsQuery[] zoneTypeFlagsQueries;

		ZQuery GetZoneTypeQuery(bool value, ZString zoneType)
		{
			if (!value)
			{
				return new ZQuery();
			}

			var query = new ZDBOnlyQuery(typeof(RateTransportZone));
			var providerZoneQuery = new ZDBOnlySubQuery(typeof(RateTransportProvider), RateTransportZonesSchema.TZ_TP);
			providerZoneQuery.AddToFilter(RateTransportProviderSchema.TP_ZoneType, zoneType);
			query.AddSubQuery(providerZoneQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Zone Mode Filter

		CodeDescriptionPairList ZoneModeList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.RateModes); }
		}

		ZQuery GetZoneModeQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RateTransportZone));
			var sub = new ZDBOnlySubQuery(typeof(RateTransportProvider), RateTransportZonesSchema.TZ_TP);
			sub.AddToFilter(RateTransportProviderSchema.TP_ZoneMode, value);
			query.AddSubQuery(sub, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion
	}
}

