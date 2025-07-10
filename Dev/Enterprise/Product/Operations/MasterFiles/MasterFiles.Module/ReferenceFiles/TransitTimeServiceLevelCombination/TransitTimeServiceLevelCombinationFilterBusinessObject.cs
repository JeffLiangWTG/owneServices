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
	class TransitTimeServiceLevelCombinationFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddTextFilters(filters);
			AddZoneFilters(filters);
			AddOrgFilters(filters);
			AddCustomFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var modeFilter = filters.AddTextFilter("Mode", GetModeFilter, Modes);
			modeFilter.MultilingualDescription = ResString.GetMultilingualString("3c423f98-4543-4814-82ba-188c9d79f36d", "Mode");

			var serviceLevelFilter = filters.AddTextFilter("Service Level", TransitTimeServiceLevelCombinationViewSchema.TSC_Code, ServiceLevelCollection);
			serviceLevelFilter.MultilingualDescription = ServiceLevelFilterDescription;
		}

		void AddZoneFilters(ModuleFilterCollection filters)
		{
			var originFilter = filters.AddGuidFilter("Origin Zone", ModuleIDs.RateTransportZone, GetOriginZoneFilter, RateTransportZonesCollection);
			originFilter.MultilingualDescription = OriginFilterDescription;

			var destinationFilter = filters.AddGuidFilter("Destination Zone", ModuleIDs.RateTransportZone, GetDestinationZoneFilter, RateTransportZonesCollection);
			destinationFilter.MultilingualDescription = DestinationFilterDescription;
		}

		protected readonly ResourceString OriginFilterDescription = ResString.GetMultilingualString("f04b14b4-348b-46f3-95ac-5bcaa9f5b1a4", "Origin Zone");
		protected readonly ResourceString DestinationFilterDescription = ResString.GetMultilingualString("7842ec82-7973-4e28-b008-10fbb77ca3a6", "Destination Zone");
		protected ResourceString ServiceLevelFilterDescription => ResString.GetMultilingualString("017267f6-7f8a-4b7d-8ad1-6683ff0f182d", "Code");

		void AddOrgFilters(ModuleFilterCollection filters)
		{
			var originRelatedOrgFilter = filters.AddGuidFilter("Origin Zone Owner/Carrier", ModuleIDs.Organisation, GetOriginRelatedOrgFilter, OrgHeaderCollection);
			originRelatedOrgFilter.MultilingualDescription = ResString.GetMultilingualString("8d7d8fd9-628c-4908-8b73-765d855914cd", "Origin Zone Owner/Carrier");

			var destinationRelatedOrgFilter = filters.AddGuidFilter("Destination Zone Owner/Carrier", ModuleIDs.Organisation, GetDestinationRelatedOrgFilter, OrgHeaderCollection);
			destinationRelatedOrgFilter.MultilingualDescription = ResString.GetMultilingualString("33637bcb-0073-4a11-810a-f0ddcdfc305d", "Destination Zone Owner/Carrier");
		}

		void AddCustomFilters(ModuleFilterCollection filters)
		{
			var transitTimeFilter = new TransitTimeModuleFilter("Transit Time", TransitTimeServiceLevelCombinationViewSchema.TSC_TransitHours);
			transitTimeFilter.MultilingualDescription = ResString.GetMultilingualString("d5bafb48-6d70-446b-b32c-300d18e67c0a", "Transit Time");
			filters.AddCustomFilter(transitTimeFilter);
		}

		#region Queries

		ZQuery GetOriginZoneFilter(ZGuid zonePK)
		{
			return AddTransitHoursSubQuery(GetZoneFilter(zonePK, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_OriginDomesticZone));
		}

		ZQuery GetDestinationZoneFilter(ZGuid zonePK)
		{
			return AddTransitHoursSubQuery(GetZoneFilter(zonePK, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_DestinationDomesticZone));
		}

		ZQuery GetZoneFilter(ZGuid zonePK, SchemaGuidColumn domesticColumn)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(TransitTimeServiceLevelCombinationView));
			mainQuery.AddToFilter(domesticColumn, zonePK);
			return mainQuery;
		}

		ZQuery GetModeFilter(ZString transportMode)
		{
			var ratingConstantsHelper = ObjectFactory.Get<IRatingConstantsHelper>();
			var applicableModes = ratingConstantsHelper.GetRateModes(transportMode);

			var query = new ZDBOnlyQuery(typeof(TransitTimeServiceLevelCombinationView));
			query.AddToFilter(TransitTimeServiceLevelCombinationViewSchema.TSC_Mode, applicableModes);
			return AddTransitHoursSubQuery(query);
		}

		ZQuery GetOriginRelatedOrgFilter(ZGuid orgPK)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(TransitTimeServiceLevelCombinationView));
			return AddTransitHoursSubQuery(GetRelatedOrgFilter(orgPK, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_OriginDomesticZone, null, mainQuery));
		}

		ZQuery GetDestinationRelatedOrgFilter(ZGuid orgPK)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(TransitTimeServiceLevelCombinationView));
			return AddTransitHoursSubQuery(GetRelatedOrgFilter(orgPK, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_DestinationDomesticZone, null, mainQuery));
		}

		ZQuery GetRelatedOrgFilter(ZGuid orgPK, SchemaGuidColumn domesticColumn, SchemaGuidColumn internationalColumn, ZDBOnlyQuery mainQuery)
		{
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

		ZQuery DomesticOriginQuery(ZBool isDomestic)
		{
			return new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_OriginDomesticZone, isDomestic ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);
		}

		ZQuery DomesticDestinationQuery(ZBool isDomestic)
		{
			return new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_DestinationDomesticZone, isDomestic ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, null);
		}

		ZQuery AddTransitHoursSubQuery(ZQuery query)
		{
			var transitTimeSubQuery = new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, null);
			transitTimeSubQuery.AddToFilter(JoinCondition.And, TransitTimeServiceLevelCombinationViewSchema.TSC_TransitHours, SQLComparisonOperator.GreaterThan, 0);
			query.AddToFilter(transitTimeSubQuery, JoinCondition.Or);
			return query;
		}

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

		IRateTransportZonesCollection RateTransportZonesCollection => ObjectFactory.Get<IRateTransportZonesCollection>(nameof(IRateTransportZonesCollection), new object[1] { Factory });

		#endregion

		#region Lookups

		public CodeDescriptionPairList Modes => new RefTransitTimeLookups(null).Modes;

		#endregion
	}
}
