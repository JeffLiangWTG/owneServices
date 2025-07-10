using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	public class RateEntryFilterProvider
	{
		public RateEntryFilterProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public ModuleFilterCollection GetRateEntryFilters(string rateType, string filterCategory, bool includeAllFilters = true)
		{
			var filters = new ModuleFilterCollection();

			AddDateFilters(filters);
			AddOrganisationFilters(rateType, filterCategory, filters);
			AddLocationFilters(filters, includeAllFilters);
			AddContractNumberFilters(rateType, filters, includeAllFilters);
			AddTextFilters(rateType, filters);
			AddModesAndTypesFilters(rateType, filters, filterCategory);

			return filters;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			RateEntryFilterUtility.AddStartDateFilter(filters);
			RateEntryFilterUtility.AddEndDateFilter(filters);
			RateEntryFilterUtility.AddEffectiveOnFilter(filters);
		}

		void AddOrganisationFilters(string rateType, string filterCategory, ModuleFilterCollection filters)
		{
			RateEntryFilterUtility.AddControllingCustomerFilter(filters, OrgHeaderList);

			RateEntryFilterUtility.AddConsigneeFilter(filters, OrgHeaderList);

			RateEntryFilterUtility.AddConsignorFilter(filters, OrgHeaderList);

			RateEntryFilterUtility.AddCarrierTransportProviderFilter(filters, OrgHeaderList);

			if (RateTypesWithSupplierFilter.Contains(rateType) && !CategoriesWithoutSupplierColumn.Contains(filterCategory))
			{
				RateEntryFilterUtility.AddSupplierFilter(filters, OrgHeaderList);
			}

			RateEntryFilterUtility.AddProductWarehouseFilter(filters, RateEntryLookups.GetWarehouses(factory, WarehouseCollectionType.ProductWarehouse));

			RateEntryFilterUtility.AddTransitWarehouseFilter(filters, RateEntryLookups.GetWarehouses(factory, WarehouseCollectionType.TransitWarehouse));

			RateEntryFilterUtility.AddFromToOrganizationFilter(filters, OrgHeaderList, OrgHeaderList);
		}

		public static string[] RateTypesWithSupplierFilter =>
			new[]
			{
				RatingConstants.RatingHeaderTypes.ClientRate,
				RatingConstants.RatingHeaderTypes.Tariff,
				RatingConstants.RatingHeaderTypes.Quote,
			};

		public static string[] CategoriesWithoutSupplierColumn =>
			new[]
			{
				RatingConstants.RateCategory.TRW,
				RatingConstants.RateCategory.TWU,
				RatingConstants.RateCategory.CYD,
				RatingConstants.RateCategory.CYU,
				RatingConstants.RateCategory.CYM,
			};

		void AddLocationFilters(ModuleFilterCollection filters, bool includeAllFilters)
		{
			RateEntryFilterUtility.AddOriginDestinationFilter(filters, LocationList);

			RateEntryFilterUtility.AddFirstLoadFilter(filters, LocationList);
			RateEntryFilterUtility.AddLastDischargeFilter(filters, LocationList);
			RateEntryFilterUtility.AddFirstRouteSetLoadFilter(filters, LocationList);
			RateEntryFilterUtility.AddLastRouteSetDischargeFilter(filters, LocationList);

			if (includeAllFilters)
			{
				RateEntryFilterUtility.AddViaFilter(filters, LocationList);

				RateEntryFilterUtility.AddCrossTradeFilter(filters);
			}

			RateEntryFilterUtility.AddTransitTimeFilter(filters);

			RateEntryFilterUtility.AddFromToSuburbFilter(filters, SuburbsList, SuburbsList);

			RateEntryFilterUtility.AddFromPostCodeFilter(filters);

			RateEntryFilterUtility.AddToPostCodeFilter(filters);

			RateEntryFilterUtility.AddFromToZoneFilter(filters, ZonesList, ZonesList);

			RateEntryFilterUtility.AddFromLocationDescriptionFilter(filters);

			RateEntryFilterUtility.AddToLocationDescriptionFilter(filters);
		}

		void AddContractNumberFilters(string rateType, ModuleFilterCollection filters, bool includeAllFilters)
		{
			if (includeAllFilters)
			{
				if (rateType != RatingConstants.RatingHeaderTypes.Costing)
				{
					RateEntryFilterUtility.AddClientContractNumberFilter(filters);
				}
				else
				{
					RateEntryFilterUtility.AddCarrierContractNumberFilter(filters);
					RateEntryFilterUtility.AddContractNumberLinkedFilter(filters);
				}
			}
		}

		void AddTextFilters(string rateType, ModuleFilterCollection filters)
		{
			RateEntryFilterUtility.AddCommodityCodeFilter(filters, CommodityCodesList);
			RateEntryFilterUtility.AddCarrierServiceLevelFilter(filters);
			RateEntryFilterUtility.AddCurrencyFilter(filters, new RefCurrencyCollection(factory));
			if (RatingHeader.IsFMCTariffAllowed(rateType))
			{
				RateEntryFilterUtility.AddFMCTariffIDFilter(filters);
			}
		}

		void AddModesAndTypesFilters(string rateType, ModuleFilterCollection filters, string filterCategory)
		{
			if (CategoriesWithTransportModeColumn.Contains(filterCategory))
			{
				RateEntryFilterUtility.AddTransportModeFilter(filters, TransportModeList);
			}

			RateEntryFilterUtility.AddContainterTypeFilter(filters, ContainerTypeList);

			RateEntryFilterUtility.AddServiceLevelFilter(filters, ServiceLevelList);

			RateEntryFilterUtility.AddAircraftTypeFilter(filters, AircraftTypeList);

			if (rateType == RatingConstants.RatingHeaderTypes.IntercompanyTariff)
			{
				RateEntryFilterUtility.AddGatewayAgentTypeTypeFilter(filters, GatewayAgentTypeList);
				RateEntryFilterUtility.AddGatewayServiceLevelFilter(filters, ServiceLevelList);
				RateEntryFilterUtility.AddShipmentGatewayServiceLevelFilter(filters, GatewayServiceLevelList);
			}

			if (rateType == RatingConstants.RatingHeaderTypes.Costing)
			{
				RateEntryFilterUtility.AddShipmentConsolidationStatusFilter(filters, ShipmentConsolidationStatusList);
			}

			if (rateType == RatingConstants.RatingHeaderTypes.ClientRate || rateType == RatingConstants.RatingHeaderTypes.Tariff || rateType == RatingConstants.RatingHeaderTypes.Quote)
			{
				RateEntryFilterUtility.AddHBLDeliveryModeFilter(filters, HBLDeliveryModeList);
			}

			RateEntryFilterUtility.AddIsNonOperatingReeferFilter(filters, IsNonOperatingReeferList);
		}

		public static string[] CategoriesWithTransportModeColumn =>
			new[]
			{
				// Summary
				string.Empty,                                      // Used for CollectionInfo & Module filters
				RatingConstants.RateCategory.SummaryRatesCategory, // Used for TabPage
				// Forwarding
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.FCL,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,
				// Customs
				RatingConstants.RateCategory.CAI,
				RatingConstants.RateCategory.CFC,
				RatingConstants.RateCategory.CLC,
				RatingConstants.RateCategory.COR,
				RatingConstants.RateCategory.CDS,
				// Shipping
				RatingConstants.RateCategory.SOR,
				RatingConstants.RateCategory.SDE,
				// CFS
				RatingConstants.RateCategory.PAC,
				RatingConstants.RateCategory.UNP,
				RatingConstants.RateCategory.CST,
				// Transport
				RatingConstants.RateCategory.TRN,
				RatingConstants.RateCategory.TBC,
				// Warehouse
				RatingConstants.RateCategory.TWU,
				RatingConstants.RateCategory.CYD,
				RatingConstants.RateCategory.CYU,
			};

		#region Implementation

		#region Org Header

		OrgHeaderCollection OrgHeaderList
		{
			get { return orgHeaderList ?? (orgHeaderList = new OrgHeaderCollection(factory)); }
		}

		OrgHeaderCollection orgHeaderList;

		#endregion

		#region Service Level

		RefServiceLevelCollection ServiceLevelList
		{
			get { return new RefServiceLevelCollection(factory); }
		}

		RefServiceLevelCollection GatewayServiceLevelList =>
			new GatewayServiceLevelCollection(factory);

		#endregion

		#region Container Type

		RatingRefContainerCollection ContainerTypeList
		{
			get { return new RatingRefContainerCollection(factory); }
		}

		#endregion

		#region Location

		RatingLocationCollection LocationList
		{
			get { return locationList ?? (locationList = new RatingLocationCollection(factory)); }
		}

		RatingLocationCollection locationList;

		RefCityTownCollection SuburbsList
		{
			get { return suburbsList ?? (suburbsList = new RefCityTownCollection(factory)); }
		}

		RefCityTownCollection suburbsList;

		RateTransportZonesCollection ZonesList
		{
			get { return zonesList ?? (zonesList = new RateTransportZonesCollection(factory)); }
		}

		RateTransportZonesCollection zonesList;

		#endregion

		#region Transport Mode

		CodeDescriptionPairList TransportModeList
		{
			get { return transportModeList ?? (transportModeList = RateEntryFilterUtility.GetTransportModeList()); }
		}

		CodeDescriptionPairList transportModeList;

		#endregion

		#region Aircraft Type

		CodeDescriptionPairList AircraftTypeList => aircraftTypeList ?? (aircraftTypeList = RateEntryFilterUtility.GetAircraftTypeList());

		CodeDescriptionPairList aircraftTypeList;

		#endregion

		#region Gateway Agent Type

		CodeDescriptionPairList GatewayAgentTypeList => gatewayAgentTypeList ?? (gatewayAgentTypeList = RateEntryFilterUtility.GetGatewayAgentTypeList());

		CodeDescriptionPairList gatewayAgentTypeList;

		#endregion

		#region Shipment Consolidation Status

		CodeDescriptionPairList ShipmentConsolidationStatusList => shipmentConsolidationStatusList ?? (shipmentConsolidationStatusList = RateEntryLookups.GetShipmentConsolidationStatusList());

		CodeDescriptionPairList shipmentConsolidationStatusList;

		#endregion

		#region HBL Delivery Mode

		CodeDescriptionPairList HBLDeliveryModeList => hblDeliveryModeList ?? (hblDeliveryModeList = RateEntryLookups.GetHBLDeliveryModeList(string.Empty, string.Empty));

		CodeDescriptionPairList hblDeliveryModeList;

		#endregion

		RefCommodityCodeCollection CommodityCodesList => new RefCommodityCodeCollection(factory);

		#endregion

		#region IsNonOperatedReefer

		CodeDescriptionPairList IsNonOperatingReeferList => isNonOperatingReeferList ?? (isNonOperatingReeferList = RateEntryLookups.GetIsNonOperatingReeferList(includeAny: false));

		CodeDescriptionPairList isNonOperatingReeferList;

		#endregion
	}
}
