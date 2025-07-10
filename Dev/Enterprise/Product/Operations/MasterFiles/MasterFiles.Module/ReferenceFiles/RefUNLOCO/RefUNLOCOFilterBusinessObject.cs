using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefUNLOCOFilterBusinessObject : FilterStripBusinessObject
	{
		public RefUNLOCOFilterBusinessObject()
		{
			countryCode = GlbCompany.CurrentCompany.Country.Code;
			countryName = GlbCompany.CurrentCompany.Country.RN_DescMultilingual;
		}

		#region Filters

		#region Country/State Filter

		void AddCustomCountryStateFilter(ModuleFilterCollection filters)
		{
			var stateModuleFilter = new RefUNLOCOCountryStateModuleFilter("CountryState", RefUNLOCOSchema.RL_RN_NKCountryCode, RefUNLOCOSchema.RL_RW);
			stateModuleFilter.MultilingualDescription = ResString.GetMultilingualString("B994416E-0444-44EF-8A84-8BAF888B1B8D", "Country/Region/State");
			filters.AddCustomFilter(stateModuleFilter);
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddCustomCountryStateFilter(filters);
			var colName = ResString.GetMultilingualString("1d33f8b6-b1e8-4cb9-bb2e-3eab607342d9", "Is in {0}?", countryName);
			ModuleFilter filter = filters.AddFlagsFilter(colName.GetUnresolvedString(), new string[] { Res.GetString("827f0f39-5763-498c-b1bb-f3b9d72cbc5e", "Ticked for yes, unticked for no") }, new GetFlagsQuery[] { GetCountryQuery });
			filter.MultilingualDescription = colName;
			filter = filters.AddTextFilter("Economic Group", GetEconomicGroupQuery, new EconomicGroupList());
			filter.MultilingualDescription = ResString.GetMultilingualString("23ecbdae-301d-4919-9da5-f0ce32657090", "Economic Group");
			filter.SubGroup = new EconomicGroupSubGroup();

			var isSystem = ResString.GetMultilingualString("649C235C-835C-46C2-9F08-D5A4601F710F", "Is System Updatable");
			ModuleFilter filter2 = filters.AddFlagsFilter(isSystem.GetUnresolvedString(), new string[] { Res.GetString("827f0f39-5763-498c-b1bb-f3b9d72cbc5e", "Ticked for yes, unticked for no") }, new GetFlagsQuery[] { GetIsSystemUpdatableQuery });
			filter2.MultilingualDescription = isSystem;

			var hasAirport = ResString.GetMultilingualString("96333ACD-5A71-4126-B9BC-A783C4EC48FC", "Has Airport");
			ModuleFilter filter3 = filters.AddFlagsFilter(hasAirport.GetUnresolvedString(), new string[] { Res.GetString("827f0f39-5763-498c-b1bb-f3b9d72cbc5e", "Ticked for yes, unticked for no") }, new GetFlagsQuery[] { GetHasAirportQuery });
			filter3.MultilingualDescription = hasAirport;

			var hasSeaport = ResString.GetMultilingualString("B21CAF33-4CAC-4F3D-8B02-A8BE4A1C35A0", "Has Seaport");
			ModuleFilter filter4 = filters.AddFlagsFilter(hasSeaport.GetUnresolvedString(), new string[] { Res.GetString("827f0f39-5763-498c-b1bb-f3b9d72cbc5e", "Ticked for yes, unticked for no") }, new GetFlagsQuery[] { GetHasSeaportQuery });
			filter4.MultilingualDescription = hasSeaport;

			UNLCOIdentifierModuleFilter identifierFilter = new UNLCOIdentifierModuleFilter();
			identifierFilter.Category = new FilterCategory(ResString.GetMultilingualString("A0F4B889-2B24-4A52-AE6A-DD7447834264", "UNLOCO Identifiers"));
			identifierFilter.ShowAddOrRadioBox = true;
			identifierFilter.AndJoinCondition = false;
			identifierFilter.OrJoinCondition = true;
			filters.AddCustomFilter(identifierFilter);
			identifierFilter.MultilingualDescription = ResString.GetMultilingualString("A0F4B889-2B24-4A52-AE6A-DD7447834264", "UNLOCO Identifiers");

			return filters;
		}

		ZQuery GetIsSystemUpdatableQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.RL_IsUpdatable, value);
			return query;
		}

		ZQuery GetHasAirportQuery(ZBool value)
		{
			return new ZQuery(RefUNLOCOSchema.RL_HasAirport, value);
		}

		ZQuery GetHasSeaportQuery(ZBool value)
		{
			return new ZQuery(RefUNLOCOSchema.RL_HasSeaport, value);
		}

		readonly ZString countryCode;
		readonly MultilingualString countryName;
		ZQuery GetCountryQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.RL_Code, value ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith, countryCode);
			return query;
		}

		class EconomicGroupSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(RefUNLOCO));
				var sub = new ZDBOnlySubQuery(typeof(RefCountry), RefCountrySchema.RN_Code);
				sub.AddToFilter(filter);
				query.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, sub, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetEconomicGroupQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(RefCountrySchema.RN_EconomicGrouping, comparisonOperator, value.SubstringSafe(0, RefCountrySchema.RN_EconomicGrouping.MaxLength));
			return query;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Port Name", RefUNLOCOSchema.RL_PortName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|PortName", "Port Name");
			filters.AddTextFilter("Code", RefUNLOCOSchema.RL_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|Code", "Code");
			filters.AddTextFilter("IATA Code", RefUNLOCOSchema.RL_IATA).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|IATACode", "IATA Code");
			filters.AddTextFilter("IATA Region Code", RefUNLOCOSchema.RL_IATARegionCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|IATARegionCode", "IATA Region Code"); //IATA CITY
			filters.AddTextFilter("Proper Name", RefUNLOCOSchema.RL_NameWithDiacriticals).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|ProperName", "Proper Name");
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
				|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				FilterCategory category = new FilterCategory(ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|DomesticCartageZonesACI", "Domestic Port Transport Zones (ACI)"));
				ModuleTextFilter postCodeFilter = filters.AddTextFilter("Servicing Postal Code", GetUNLOCOByPostalCode);
				postCodeFilter.MaxLength = RefDomesticCartageZoneSchema.F1_CityTownPostCode.MaxLength;
				postCodeFilter.Category = category;
				postCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|ServicingPostalCode", "Servicing Postal Code");
				ModuleTextFilter filter = filters.AddTextFilter("Airport Radius Search Distance", GetDistanceFromAirportQuery);
				filter.Category = category;
				filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefUNLOCOFilter|AirportRadiusSearchDistance", "Airport Radius Search Distance");
			}
		}

		#region Delegates

		ZQuery GetDistanceFromAirportQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefUNLOCO));
			ZDecimal distance;
			if (ZDecimal.TryParse(value, out distance))
			{
				ZDBOnlySubQuery domesticZoneSubQuery = new ZDBOnlySubQuery(typeof(RefDomesticCartageZone), RefDomesticCartageZoneSchema.F1_RL_NKLoco);
				domesticZoneSubQuery.AddToFilter(RefDomesticCartageZoneSchema.F1_Distance, SQLComparisonOperator.LessThanOrEqualTo, distance);
				query.AddSubQuery(RefUNLOCOSchema.RL_Code, domesticZoneSubQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetUNLOCOByPostalCode(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefUNLOCO));
			ZDBOnlySubQuery domesticZoneSubQuery = new ZDBOnlySubQuery(typeof(RefDomesticCartageZone), RefDomesticCartageZoneSchema.F1_RL_NKLoco);
			domesticZoneSubQuery.AddToFilter(RefDomesticCartageZoneSchema.F1_CityTownPostCode, comparisonOperator, value);
			query.AddSubQuery(RefUNLOCOSchema.RL_Code, domesticZoneSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#endregion
	}
}
