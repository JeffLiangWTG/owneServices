using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefPostCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public RefPostCodeFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddNKFilters(filters);
			AddGuidFilters(filters);

			return filters;
		}

		#region Filters

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Postcode", RefPostCodeSchema.RK_CityTownPostCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPostCodeFilter|CityTownPostCode", "Postcode");
		}

		#endregion

		#region Country

		void AddNKFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter("Country", RefPostCodeSchema.RK_RN_NKCountry, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPostCodeFilter|Country", "Country/Region");
			filter.Category = FilterCategories.Locations;
		}

		#endregion

		#region Guid

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var cityTownFilter = filters.AddGuidFilter("City/Town", ModuleIDs.RefCityTown, RefCityTownSchema.PK, new RefCityTownCollection(Factory));
			cityTownFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPostCodeFilter|CityTownFilter", "City/Town");
			cityTownFilter.Category = FilterCategories.Locations;
			cityTownFilter.SubGroup = new CityTownSubGroup();
			var zoneSetfilter = filters.AddGuidFilter("Transport Zone Membership", ModuleIDs.RateTransportProvider, GetZoneSetQuery, ObjectFactory.New<IRateTransportProviderCollection>(Factory));
			zoneSetfilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPostCodeFilter|ZoneSetFilter", "Transport Zone Membership");
			zoneSetfilter.Category = FilterCategories.Locations;
		}

		class CityTownSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(RefPostCode));

				var cityTownSubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
				cityTownSubQuery.AddToFilter(filter);

				var cityTownLinkQuery = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_RK);
				cityTownLinkQuery.AddSubQuery(RefCityPCodePivotSchema.R0_R9, cityTownSubQuery, JoinCondition.And);

				query.AddSubQuery(cityTownLinkQuery, JoinCondition.And);

				return query;
			}
		}

		/// <summary>
		/// Query to find post codes that belong/don't belong to a RateTransportZones record
		/// </summary>
		/// <param name="comparisonOperator"></param>
		/// <param name="pkValue">RateTransportZones PK</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetZoneSetQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var query = new ZDBOnlyQuery(typeof(RefPostCode));
			var zoneSetPK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out zoneSetPK);
			bool inZoneSet = comparisonOperator != SpecialComparisonOperator.IsBlank && comparisonOperator != SQLComparisonOperator.NotEqual;

			// Load all the zone items for the zone set.
			var typeOfZone = ObjectFactory.GetType<IRateTransportZone>();
			var typeOfZoneItem = ObjectFactory.GetType<IRateTransportZoneItem>();
			var zoneItemQuery = new ZDBOnlyQuery(typeOfZoneItem);
			if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				var zoneSetSubQuery = new ZDBOnlySubQuery(typeOfZone, ZArchitecture.Schema.RateTransportZonesSchema.PK);
				zoneSetSubQuery.AddToFilter(RateTransportZonesSchema.TZ_TP, zoneSetPK);
				zoneItemQuery.AddSubQuery(RateTransportZoneItemSchema.TQ_TZ_DomesticZone, zoneSetSubQuery, JoinCondition.And);
			}
			var zoneItems = Factory.Load(typeOfZoneItem, (zoneItemQuery));

			// Add filters for the post codes that match any zone item.
			// E.g. for finding post codes that belong to the zone set
			//		RK_CityTownPostCode {in zoneItem 1 PostCode range}
			//		OR
			//		RK_CityTownPostCode {in zoneItem 2 PostCode range}
			//		...
			bool hasPostCode = false;
			foreach (IRateTransportZoneItem zoneItem in zoneItems)
			{
				string fromPostCode = "";
				string toPostCode = "";
				if (!zoneItem.TQ_FromPostCode.IsEmpty)
				{
					fromPostCode = zoneItem.TQ_FromPostCode;
				}
				if (!zoneItem.TQ_ToPostCode.IsEmpty)
				{
					toPostCode = zoneItem.TQ_ToPostCode;
				}
				if (!string.IsNullOrEmpty(fromPostCode))
				{
					hasPostCode = true;
					var subQuery = new ZQuery();

					if (!string.IsNullOrEmpty(toPostCode))
					{
						if (inZoneSet)
						{
							subQuery.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, SQLComparisonOperator.GreaterThanOrEqualTo, fromPostCode);
							subQuery.AddToFilter(JoinCondition.And, RefPostCodeSchema.RK_CityTownPostCode, SQLComparisonOperator.LessThanOrEqualTo, toPostCode);
						}
						else
						{
							subQuery.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, SQLComparisonOperator.LessThan, fromPostCode);
							subQuery.AddToFilter(JoinCondition.Or, RefPostCodeSchema.RK_CityTownPostCode, SQLComparisonOperator.GreaterThan, toPostCode);
						}
					}
					else
					{
						if (inZoneSet)
						{
							subQuery.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, SQLComparisonOperator.Equal, fromPostCode);
						}
						else
						{
							subQuery.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, SQLComparisonOperator.NotEqual, fromPostCode);
						}
					}

					if (inZoneSet)
					{
						subQuery.AddToFilter(RefPostCodeSchema.RK_RN_NKCountry, zoneItem.TQ_RN_NKCountry);
						query.AddToFilter(subQuery, JoinCondition.Or);
					}
					else
					{
						query.AddToFilter(subQuery, JoinCondition.And);
					}
				}
			}
			if (!hasPostCode)
			{
				query.IsNoResultQuery = true;
			}

			return query;
		}

		#endregion

		#region IsSystemDefinedDefaultProperty

		protected override string IsSystemDefinedDefaultProperty => DefinedStatusAllCode;

		#endregion

		#endregion
	}
}
