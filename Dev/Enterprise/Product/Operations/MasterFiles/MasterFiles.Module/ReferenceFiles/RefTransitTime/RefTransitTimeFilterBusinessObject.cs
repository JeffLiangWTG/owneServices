using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class RefTransitTimeFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public class FilterConstants
		{
			public const string Mode = "Mode";
			public const string ServiceLevel = "Service Level";
			public const string OriginZone = "Origin Zone";
			public const string DestinationZone = "Destination Zone";
			public const string IsOriginDomestic = "Is Origin Domestic";
			public const string IsDestinationDomestic = "Is Destination Domestic";
			public const string OriginZoneOwner = "Origin Zone Owner/Carrier";
			public const string DestinationZoneOwner = "Destination Zone Owner/Carrier";
			public const string OriginCountry = "Domestic Origin Zone Hub Country/Region";
			public const string DestinationCountry = "Domestic Destination Zone Hub Country/Region";
			public const string TransitTime = "Transit Time";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddTextFilters(filters);
			AddZoneFilters(filters);
			AddFlagsFilters(filters);
			AddOrgFilters(filters);
			AddCustomFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var modeFilter = filters.AddTextFilter(FilterConstants.Mode, GetModeFilter, Modes);
			modeFilter.MultilingualDescription = ResString.GetMultilingualString("3c423f98-4543-4814-82ba-188c9d79f36d", FilterConstants.Mode);

			var serviceLevelFilter = filters.AddTextFilter(FilterConstants.ServiceLevel, RefTransitTimeSchema.RTT_RS_NKServiceLevel, ServiceLevelCollection);
			serviceLevelFilter.MultilingualDescription = ServiceLevelFilterDescription;
		}

		protected virtual ResourceString ServiceLevelFilterDescription => ResString.GetMultilingualString("7e3194f4-c8cc-40b5-bc9a-1ab8dbf065ba", FilterConstants.ServiceLevel);

		void AddZoneFilters(ModuleFilterCollection filters)
		{
			var originFilter = filters.AddTextFilter(FilterConstants.OriginZone, GetOriginZoneFilter);
			originFilter.MultilingualDescription = OriginFilterDescription;

			var destinationFilter = filters.AddTextFilter(FilterConstants.DestinationZone, GetDestinationZoneFilter);
			destinationFilter.MultilingualDescription = DestinationFilterDescription;
		}

		protected readonly ResourceString OriginFilterDescription = ResString.GetMultilingualString("e9937640-6bb7-4d59-b4d4-016ed8f267f4", FilterConstants.OriginZone);
		protected readonly ResourceString DestinationFilterDescription = ResString.GetMultilingualString("88b99454-4dbb-4a6f-b770-aac5e3571f9a", FilterConstants.DestinationZone);

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var isDomesticLabel = Res.GetString("4ab3364e-2e3c-4b5d-ae4d-000f42fdd2ef", "Is Domestic");

			var isOriginDomesticFilter = filters.AddFlagsFilter(FilterConstants.IsOriginDomestic, new[] { isDomesticLabel }, new GetFlagsQuery[] { DomesticOriginQuery });
			isOriginDomesticFilter.MultilingualDescription = ResString.GetMultilingualString("604abfa7-d2a0-4274-9d9e-2b257a72ef48", FilterConstants.IsOriginDomestic);

			var isDestinationDomesticFilter = filters.AddFlagsFilter(FilterConstants.IsDestinationDomestic, new[] { isDomesticLabel }, new GetFlagsQuery[] { DomesticDestinationQuery });
			isDestinationDomesticFilter.MultilingualDescription = ResString.GetMultilingualString("a19db5fa-20e0-400f-9316-fdbb31861547", FilterConstants.IsDestinationDomestic);
		}

		void AddOrgFilters(ModuleFilterCollection filters)
		{
			var originRelatedOrgFilter = filters.AddGuidFilter(FilterConstants.OriginZoneOwner, ModuleIDs.Organisation, GetOriginZoneOwnerFilter, OrgHeaderCollection);
			originRelatedOrgFilter.MultilingualDescription = ResString.GetMultilingualString("8d7d8fd9-628c-4908-8b73-765d855914cd", FilterConstants.OriginZoneOwner);

			var destinationRelatedOrgFilter = filters.AddGuidFilter(FilterConstants.DestinationZoneOwner, ModuleIDs.Organisation, GetDestinationZoneOwnerFilter, OrgHeaderCollection);
			destinationRelatedOrgFilter.MultilingualDescription = ResString.GetMultilingualString("33637bcb-0073-4a11-810a-f0ddcdfc305d", FilterConstants.DestinationZoneOwner);

			var originCountryFilter = filters.AddNkFilter(FilterConstants.OriginCountry, GetDomesticOriginZoneCountryFilter, ModuleIDs.RefCountry, Countries)
				.WithMaxLengthOf<ModuleNkFilter>(RateTransportProviderSchema.TP_RN_NKCountry)
				.WithMaxLengthOf<ModuleNkFilter>(RefCityTownSchema.R9_RN_NKCountry);
			originCountryFilter.MultilingualDescription = ResString.GetMultilingualString("babc0455-c725-4682-bd22-770c0adad2db", FilterConstants.OriginCountry);
			originCountryFilter.Category = FilterCategories.Locations;

			var destCountryFilter = filters.AddNkFilter(FilterConstants.DestinationCountry, GetDomesticDestinationZoneCountryFilter, ModuleIDs.RefCountry, Countries)
				.WithMaxLengthOf<ModuleNkFilter>(RateTransportProviderSchema.TP_RN_NKCountry)
				.WithMaxLengthOf<ModuleNkFilter>(RefCityTownSchema.R9_RN_NKCountry);
			destCountryFilter.MultilingualDescription = ResString.GetMultilingualString("7d4eb710-e4f5-44eb-b538-11cd41bd66f8", FilterConstants.DestinationCountry);
			destCountryFilter.Category = FilterCategories.Locations;
		}

		void AddCustomFilters(ModuleFilterCollection filters)
		{
			var transitTimeFilter = new TransitTimeModuleFilter(FilterConstants.TransitTime, RefTransitTimeSchema.RTT_TransitHours);
			transitTimeFilter.MultilingualDescription = ResString.GetMultilingualString("25415e42-6e24-421f-ab16-fa049ef3f975", FilterConstants.TransitTime);
			filters.AddCustomFilter(transitTimeFilter);
		}

		#region Queries

		ZQuery GetDomesticOriginZoneCountryFilter(SQLComparisonOperator comparisonOperator, ZString country)
		{
			return GetDomesticZoneCountryQuery(comparisonOperator, RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, country);
		}

		ZQuery GetDomesticDestinationZoneCountryFilter(SQLComparisonOperator comparisonOperator, ZString country)
		{
			return GetDomesticZoneCountryQuery(comparisonOperator, RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, country);
		}

		ZQuery GetOriginZoneFilter(SQLComparisonOperator comparisonOperator, ZString zoneCode)
		{
			return GetZoneFilter(comparisonOperator, zoneCode, RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, RefTransitTimeSchema.RTT_FZ_OriginInternationalZone);
		}

		ZQuery GetDestinationZoneFilter(SQLComparisonOperator comparisonOperator, ZString zoneCode)
		{
			return GetZoneFilter(comparisonOperator, zoneCode, RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, RefTransitTimeSchema.RTT_FZ_DestinationInternationalZone);
		}

		ZQuery GetZoneFilter(SQLComparisonOperator comparisonOperator, ZString zoneCode, SchemaGuidColumn domesticColumn, SchemaGuidColumn internationalColumn)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(RefTransitTime));

			if (zoneCode.Length <= RateTransportZonesSchema.TZ_ZoneName.MaxLength)
			{
				var domesticQuery = new ZDBOnlySubQuery(typeof(IRateTransportZone), RateTransportZonesSchema.PK);
				domesticQuery.AddToFilter(RateTransportZonesSchema.TZ_ZoneName, comparisonOperator, zoneCode);
				mainQuery.AddSubQuery(domesticColumn, domesticQuery, JoinCondition.Or);
			}

			if (zoneCode.Length <= RefZoneHeaderSchema.FZ_Code.MaxLength)
			{
				var internationalQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.PK);
				internationalQuery.AddToFilter(RefZoneHeaderSchema.FZ_Code, comparisonOperator, zoneCode);
				mainQuery.AddSubQuery(internationalColumn, internationalQuery, JoinCondition.Or);
			}

			return mainQuery;
		}

		protected virtual ZQuery GetOriginZoneOwnerFilter(ZGuid orgPK)
		{
			return GetZoneOwnerFilter(orgPK, RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, RefTransitTimeSchema.RTT_FZ_OriginInternationalZone);
		}

		protected virtual ZQuery GetDestinationZoneOwnerFilter(ZGuid orgPK)
		{
			return GetZoneOwnerFilter(orgPK, RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, RefTransitTimeSchema.RTT_FZ_DestinationInternationalZone);
		}

		protected ZQuery GetZoneOwnerFilter(ZGuid orgPK, SchemaGuidColumn domesticColumn, SchemaGuidColumn internationalColumn)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(RefTransitTime));
			if (domesticColumn != null)
			{
				var transportProviderSubQuery = new ZDBOnlySubQuery(typeof(IRateTransportProvider), RateTransportProviderSchema.PK);
				transportProviderSubQuery.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, orgPK);

				var domesticQuery = new ZDBOnlySubQuery(typeof(IRateTransportZone), RateTransportZonesSchema.PK);
				domesticQuery.AddSubQuery(RateTransportZonesSchema.TZ_TP, transportProviderSubQuery, JoinCondition.And);
				mainQuery.AddSubQuery(domesticColumn, domesticQuery, JoinCondition.Or);
			}

			if (internationalColumn != null)
			{
				var internationalQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.PK);
				internationalQuery.AddToFilter(RefZoneHeaderSchema.FZ_OH_RelatedParty, orgPK);
				mainQuery.AddSubQuery(internationalColumn, internationalQuery, JoinCondition.Or);
			}

			return mainQuery;
		}

		ZQuery GetModeFilter(ZString transportMode)
		{
			var ratingConstantsHelper = ObjectFactory.Get<IRatingConstantsHelper>();
			var applicableModes = ratingConstantsHelper.GetRateModes(transportMode);

			var query = new ZDBOnlyQuery(typeof(RefTransitTime));
			query.AddToFilter(RefTransitTimeSchema.RTT_Mode, applicableModes);
			return query;
		}

		ZQuery DomesticOriginQuery(ZBool isDomestic)
		{
			return new ZQuery(RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, isDomestic ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);
		}

		ZQuery DomesticDestinationQuery(ZBool isDomestic)
		{
			return new ZQuery(RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, isDomestic ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);
		}

		#region Country

		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(Factory)); }
		}
		RefCountryCollection countries;

		ZQuery GetDomesticZoneCountryQuery(SQLComparisonOperator sqlOperator, SchemaGuidColumn domesticColumn, ZString value)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(RefTransitTime));

			var query = new ZDBOnlySubQuery(typeof(IRateTransportZone), RateTransportZonesSchema.PK);

			var sub = new ZDBOnlySubQuery(typeof(IRateTransportProvider), RateTransportZonesSchema.TZ_TP);
			sub.AddToFilter(RateTransportProviderSchema.TP_RN_NKCountry, sqlOperator, value);
			sub.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_R9_ZoneHubLocation, null);

			var cityTownCountrySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RateTransportProviderSchema.TP_R9_ZoneHubLocation);
			cityTownCountrySubQuery.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, sqlOperator, value);
			sub.AddSubQuery(cityTownCountrySubQuery, JoinCondition.Or);

			query.AddSubQuery(sub, JoinCondition.And);

			mainQuery.AddSubQuery(domesticColumn, query, JoinCondition.And);
			return mainQuery;
		}

		#endregion

		#endregion

		#region Collections

		protected RefServiceLevelCollection ServiceLevelCollection
		{
			get { return new RefServiceLevelCollection(Factory); }
		}

		OrgHeaderCollection OrgHeaderCollection
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList Modes => new RefTransitTimeLookups(null).Modes;

		#endregion
	}
}
