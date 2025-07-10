using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefCityTown;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefCityTownFilterBusinessObject : FilterStripBusinessObject
	{
		public RefCityTownFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddCustomCountryStateFilter(filters);
			AddGuidFilter(filters);
			return filters;
		}

		#region Filters

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("International Name", RefCityTownSchema.R9_InternationalName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCityTownFilter|InternationalName", "International Name");
			filters.AddTextFilter("Local Language Name", RefCityTownSchema.R9_LocalLanguageName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCityTownFilter|LocalLanguageName", "Local Language Name");
		}

		#endregion

		#region Country/State Filter

		void AddCustomCountryStateFilter(ModuleFilterCollection filters)
		{
			RefCityTownCountryStateModuleFilter stateModuleFilter = new RefCityTownCountryStateModuleFilter("CountryState", RefCityTownSchema.R9_RN_NKCountry, RefCityTownSchema.R9_RW_NKState);
			stateModuleFilter.MultilingualDescription = ResString.GetMultilingualString("B994416E-0444-44EF-8A84-8BAF888B1B8D", "Country/Region/State");
			filters.AddCustomFilter(stateModuleFilter);
		}

		#endregion

		#region Guid

		void AddGuidFilter(ModuleFilterCollection filters)
		{
			var postCodeFilter = filters.AddGuidFilter("Postcode", ModuleIDs.RefPostCode, RefPostCodeSchema.PK, new RefPostCodeCollection(Factory));
			postCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCityTownFilter|PostcodeFilter", "Postcode");
			postCodeFilter.Category = FilterCategories.Locations;
			postCodeFilter.SubGroup = new PostCodeSubGroup();

			var zoneSetFilter = filters.AddGuidFilter("Transport Zone Membership", ModuleIDs.RateTransportProvider, GetZoneSetQuery, ObjectFactory.New<IRateTransportProviderCollection>(Factory));
			zoneSetFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCityTownFilter|ZoneSetFilter", "Transport Zone Membership");
			zoneSetFilter.Category = FilterCategories.Locations;
		}

		class PostCodeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(RefCityTown));

				var postcodeSubQuery = new ZDBOnlySubQuery(typeof(RefPostCode), RefPostCodeSchema.PK);
				postcodeSubQuery.AddToFilter(filter);

				var postcodeLinkQuery = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_R9);
				postcodeLinkQuery.AddSubQuery(RefCityPCodePivotSchema.R0_RK, postcodeSubQuery, JoinCondition.And);

				query.AddSubQuery(postcodeLinkQuery, JoinCondition.And);

				return query;
			}
		}

		ZQuery GetZoneSetQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var query = new ZDBOnlyQuery(typeof(RefCityTown));
			var zoneSetPK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out zoneSetPK);
			bool notIn = comparisonOperator == SQLComparisonOperator.NotEqual || comparisonOperator == SpecialComparisonOperator.IsBlank;

			var typeOfZone = ObjectFactory.GetType<IRateTransportZone>();
			var typeOfZoneItem = ObjectFactory.GetType<IRateTransportZoneItem>();
			var zoneSetLinkQuery = new ZDBOnlySubQuery(typeOfZoneItem, RateTransportZoneItemSchema.TQ_R9_CityTown, notIn);
			if (!zoneSetPK.IsEmpty)
			{
				var zoneSetSubQuery = new ZDBOnlySubQuery(typeOfZone, ZArchitecture.Schema.RateTransportZonesSchema.PK);
				zoneSetSubQuery.AddToFilter(RateTransportZonesSchema.TZ_TP, zoneSetPK);
				zoneSetLinkQuery.AddSubQuery(RateTransportZoneItemSchema.TQ_TZ_DomesticZone, zoneSetSubQuery, JoinCondition.And);
			}
			else
			{
				zoneSetLinkQuery.AddToFilter(RateTransportZoneItemSchema.TQ_R9_CityTown, SQLComparisonOperator.NotEqual, null);
			}

			query.AddSubQuery(zoneSetLinkQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion
	}
}
