using System;
using System.Collections;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class ValueAnalysisFilterStripsHelper : FilterStripsHelper
	{
		public ValueAnalysisFilterStripsHelper(string productCode, string moduleContext, BusinessObjectFactory factory) : base(typeof(ViewValueAnalysis), factory)
		{
			ProductCode = productCode;
			ModuleContext = moduleContext;
		}

		public override bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return true;
		}

		public override bool CanAddFilters()
		{
			return base.CanAddFilters() && !string.IsNullOrEmpty(ProductCode) && !string.IsNullOrEmpty(ModuleContext);
		}

		protected override void AddFilterStrips(ModuleFilterCollection filters)
		{
			if (!CanAddFilters())
			{
				return;
			}

			AddTradeDetailsFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddDatesFilters(filters);
			AddOrganisationFilters(filters);
			AddAggregatedTradeLaneFilters(filters);
		}

		#region Implementation
		#region SuppressResourceStringsCheckRegion 
		public static class Description
		{
			public const string SalesProduct = "Sales Product";
			public const string TransportMode = "Transport Mode";
			public const string WarehouseName = "Warehouse Name";
			public const string WarehouseLocation = "Warehouse Location";
			public const string WarehouseService = "Warehouse Service";
			public const string CargoType = "Cargo Type";
			public const string CargoType_Air = "Cargo Type - Air";
			public const string CargoType_SeaRail = "Cargo Type - Sea & Rail";
			public const string CargoType_Road = "Cargo Type - Road";
			public const string ClearanceType = "Clearance Type";
			public const string TradeStatus = "Trade Status";
			public const string ProspectStatus = "Prospect Status";
			public const string OriginDestination = "Origin / Destination";
			public const string TradedOriginDestination = "Traded Origin / Destination";
			public const string Certainty = "Certainty";
			public const string VerticalMarket = "Vertical Market";
			public const string PeriodActivity = "Period of Activity";
			public const string AnalysisPeriod = "Analysis Period";
			public const string ExpectedTradeDate = "Expected Trade Date";
			public const string LastProspectDate = "Last Prospect Date";
			public const string OriginCity = "Origin - City";
			public const string OriginState = "Origin - State";
			public const string DestinationCity = "Destination - City";
			public const string DestinationState = "Destination - State";
			public const string ProspectiveBrokerageLocation = "Prospective Brokerage Location";
			public const string ProspectiveWarehouseLocation = "Prospective Warehouse Location";
			public const string TradedSinceProspectDate = "Traded since Prospect date";
			public const string OrgBuyer = "Buyer";
			public const string OrgSupplier = "Supplier";
			public const string OrgCompetitor = "Competitor";
			public const string OrgControllingAgent = "Controlling Agent";
			public const string Carrier = "Carrier";
			public const string Organization = "Organization";
			public const string TradedJobCount = "Traded Job Count";
			public const string TradedTEUCount = "Traded TEU Count";
			public const string TradedVolume = "Traded Volume";
			public const string TradedJobProfit = "Traded Job Profit";
			public const string TradedJobRevenue = "Traded Job Revenue";
			public const string TradedRevenue = "Traded Revenue";
			public const string TradedJobCost = "Traded Job Cost";
			public const string EstimateTEUCountPA = "Estimate TEU Count (p.a)";
			public const string EstimateWeightPA = "Estimate Weight (p.a)";
			public const string EstimateChargeablePA = "Estimate Chargeable (p.a)";
			public const string EstimateVolumePA = "Estimate Volume (p.a)";
			public const string EstimateJobCountPA = "Estimate Job Count (p.a)";
			public const string PipelineValuePA = "Pipeline Value (p.a)";
			public const string UnsuccessfulValuePA = "Unsuccessful Value (p.a)";
			public const string CommittedValue = "Committed Value";
			public const string CommittedAndForecastValue = "Committed And Forecast Value";
		}
		#endregion

		readonly string ProductCode;
		readonly string ModuleContext;

		#region Trade Details

		LocationCollection Locations => fLocations ?? (fLocations = new LocationCollection(Factory));
		LocationCollection fLocations;

		ViewLocationCollection ViewLocations => fViewLocations ?? (fViewLocations = new ViewLocationCollection(Factory));
		ViewLocationCollection fViewLocations;

		void AddTradeDetailsFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|", "Trade Details"));

			AddTradeProductFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|SalesProduct", Description.SalesProduct);

			if (ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment || ProductCode == SystemDefinedSalesProductList.Codes.LinerAgency || ProductCode == SystemDefinedSalesProductList.Codes.Transport)
			{
				AddOriginDestinationFilter(filters, category, Description.OriginDestination).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|OriginDestination", Description.OriginDestination);

				if (ProductCode == SystemDefinedSalesProductList.Codes.Transport)
				{
					AddLocationFilter(filters, category, ViewValueAnalysisSchema.VVA_Origin, ViewLocationSchema.VLO_Description, Description.OriginCity).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|OriginCity", Description.OriginCity);
					AddLocationFilter(filters, category, ViewValueAnalysisSchema.VVA_Origin, ViewLocationSchema.VLO_StateDescription, Description.OriginState).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|OriginState", Description.OriginState);
					AddLocationFilter(filters, category, ViewValueAnalysisSchema.VVA_Destination, ViewLocationSchema.VLO_Description, Description.DestinationCity).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|DestinationCity", Description.DestinationCity);
					AddLocationFilter(filters, category, ViewValueAnalysisSchema.VVA_Destination, ViewLocationSchema.VLO_StateDescription, Description.DestinationState).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|DestinationState", Description.DestinationState);
				}
			}

			if (ProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				if (ModuleContext == OrgHeaderSchema.PK.Name || ModuleContext == ViewCampaignContactSchema.VCC_OH.Name)
				{
					AddOriginDestinationFilter(filters, category, Description.TradedOriginDestination).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|TradedOriginDestination", Description.TradedOriginDestination);
				}

				AddProspectiveOriginFilter(filters, category, Description.ProspectiveBrokerageLocation).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|ProspectiveBrokerageLocation", Description.ProspectiveBrokerageLocation);
			}

			if (ProductCode == SystemDefinedSalesProductList.Codes.Warehouse && ModuleContext == OrgOpportunitySchema.PK.Name)
			{
				AddProspectiveOriginFilter(filters, category, Description.ProspectiveWarehouseLocation).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|ProspectiveWarehouseLocation", Description.ProspectiveWarehouseLocation);
			}

			if (ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment || ProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				AddTradeModeFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|TransportMode", Description.TransportMode);
			}

			if (ProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				AddWarehouseNameFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|WarehouseName", Description.WarehouseName);
				AddWarehouseLocationFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|WarehouseLocation", Description.WarehouseLocation);
				AddServiceFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|WarehouseService", Description.WarehouseService);
			}

			if (ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment)
			{
				AddTradeTypeFilter(filters, category, Description.CargoType_Air, Constants.TransportModes.Air).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|CargoType_Air", Description.CargoType_Air);
				AddTradeTypeFilter(filters, category, Description.CargoType_SeaRail, Constants.TransportModes.Sea).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|CargoType_SeaRail", Description.CargoType_SeaRail);
				AddTradeTypeFilter(filters, category, Description.CargoType_Road, Constants.TransportModes.Road).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|CargoType_Road", Description.CargoType_Road);
			}
			else if (ProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				AddTradeTypeFilter(filters, category, Description.ClearanceType, ZString.Empty).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|ClearanceType", Description.ClearanceType);
			}
			else if (ProductCode == SystemDefinedSalesProductList.Codes.LinerAgency || ProductCode == SystemDefinedSalesProductList.Codes.Transport)
			{
				AddTradeTypeFilter(filters, category, Description.CargoType, ZString.Empty).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|TradeDetails|CargoType", Description.CargoType);
			}
		}

		ModuleFilter AddTradeProductFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var codes = Factory.GetCachedValue("MarketingManager|ValueAnalysisFilter|SalesProduct", () => new SystemDefinedSalesProductList());
			var filter = filters.AddTextFilter(Description.SalesProduct, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_MP_Product, Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, value)).PK), codes);
			filter.Category = category;
			filter.IsPublishedOnWeb = false;
			filter.GroupOrCategory = FilterOrCategory.None;
			filter.IsGroupOrCategoryReadOnly = true;
			filter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
			filter.ReadOnly = true;
			filter.OrCategory = FilterOrCategory.MandatoryFilterOrCategory;
			filter.IsOrCategoryReadOnly = true;
			filter.DefaultProperty = ProductCode;
			return filter;
		}

		ModuleFilter AddProspectiveOriginFilter(ModuleFilterCollection filters, FilterCategory category, string description)
		{
			var filter = description == Description.ProspectiveWarehouseLocation
				? filters.AddGuidFilter(description, ModuleIDs.ViewLocation, ViewValueAnalysisSchema.VVA_Origin, ViewLocations)
				: filters.AddGuidFilter(description, ModuleIDs.ViewLocation, (g) =>
				{
					var query = new ZQuery(ViewValueAnalysisSchema.VVA_Origin, g);
					query.AddToFilter(new ZQuery(
						new ZQuery(ViewValueAnalysisSchema.VVA_TradeType, OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import), JoinCondition.Or,
						new ZQuery(ViewValueAnalysisSchema.VVA_TradeType, OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Export)));
					return query;
				}, ViewLocations);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddOriginDestinationFilter(ModuleFilterCollection filters, FilterCategory category, string description)
		{
			var filter = description == Description.OriginDestination
				? AddLocationFilter(filters, description, GetOriginDestinationQuery, Locations, Locations)
				: AddLocationFilter(filters, description, (o, d) =>
				{
					var query = GetOriginDestinationQuery(o, d);
					query.AddToFilter(ViewValueAnalysisSchema.VVA_IsTraded, SQLComparisonOperator.Equal, true);
					return query;
				}, Locations, Locations);
			filter.SetItemDescriptions(Res.GetData("MarketingManager|ValueAnalysisFilter|TradeDetails|Origin", "Origin"), Res.GetData("MarketingManager|ValueAnalysisFilter|TradeDetails|Destination", "Destination"));
			filter.Category = category;
			return filter;
		}

		#region Location Filter

		ModuleLocationFilter AddLocationFilter(ModuleFilterCollection filters, ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection location1List, IBusinessObjectCollection location2List)
		{
			ValueAnalysisLocationFilter result = new ValueAnalysisLocationFilter(description, queryDelegate, location1List, location2List);
			filters.AddFilter(result);

			return result;
		}

		#endregion

		ModuleFilter AddTradeTypeFilter(ModuleFilterCollection filters, FilterCategory category, ZString description, ZString mode)
		{
			var key = category.Description + description + ProductCode + mode;
			var codes = Factory.GetCachedValue(key, () => OrgTradeDetailLookups.GetTradeTypes(ProductCode, mode));
			var filter = filters.AddTextFilter(description, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_TradeType, value), codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddTradeModeFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var key = category.Description + Description.TransportMode + ProductCode;
			var codes = Factory.GetCachedValue(key, () => OrgTradeDetailLookups.GetTradeModes(ProductCode));
			var filter = filters.AddTextFilter(Description.TransportMode, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_TradeMode, value), codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddWarehouseNameFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var filter = filters.AddTextFilter(Description.WarehouseName, ViewValueAnalysisSchema.VVA_Warehouse);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddWarehouseLocationFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var filter = filters.AddNkFilter(Description.WarehouseLocation, GetWarehouseLocationQuery, ModuleIDs.Location, Locations);
			filter.Category = category;
			return filter;
		}

		ZQuery GetWarehouseLocationQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ViewValueAnalysis));
			query.AddToFilter(LocationHelper.GetLocationFilter(Factory, value, ViewValueAnalysisSchema.VVA_WarehouseLocation, typeof(ViewValueAnalysis)));
			return query;
		}

		ModuleFilter AddServiceFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var key = category.Description + Description.WarehouseService;
			var codes = Factory.GetCachedValue(key, () => new OrgSalesWarehouseServiceTypesList());
			var filter = filters.AddTextFilter(Description.WarehouseService, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_Service, value), codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddLocationFilter(ModuleFilterCollection filters, FilterCategory category, SchemaGuidColumn pkColumn, SchemaStringColumn searchColumn, string description)
		{
			GetTextQueryWithOperator query = (op, value) => GetLocationQuery(new ZDBOnlyQuery(typeof(ViewValueAnalysis)), pkColumn, searchColumn, value, op);
			var filter = filters.AddTextFilter(description, query);
			filter.Category = category;
			return filter;
		}

		ZDBOnlyQuery GetLocationQuery(ZDBOnlyQuery query, SchemaGuidColumn pkColumn, SchemaStringColumn searchColumn, ZString value, SQLComparisonOperator comparison)
		{
			if (!value.IsEmpty)
			{
				var locationsubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), pkColumn);
				locationsubQuery.AddToFilter(searchColumn, comparison, value);

				var zoneSubQuery = GetZoneQuery(pkColumn, searchColumn, value, comparison);
				var zoneUnlocoSubQurey = GetZoneUNLOCOSubQuery(pkColumn, searchColumn, value, comparison);

				query.AddSubQuery(locationsubQuery, JoinCondition.And);
				query.AddSubQuery(zoneSubQuery, JoinCondition.Or);
				query.AddSubQuery(zoneUnlocoSubQurey, JoinCondition.Or);
			}
			return query;
		}
		#region LocationFilterQuery
		ZDBOnlySubQuery GetZoneQuery(SchemaGuidColumn pkColumn, SchemaStringColumn searchColumn, ZString value, SQLComparisonOperator comparison)
		{
			var zoneQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), pkColumn, RefZonePivotSchema.F2_ParentID);
			var zoneSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), RefZonePivotSchema.F2_FZ);
			zoneSubQuery.AddToFilter(searchColumn, comparison, value);

			zoneQuery.AddSubQuery(zoneSubQuery, JoinCondition.And);
			return zoneQuery;
		}

		ZDBOnlySubQuery GetZoneUNLOCOSubQuery(SchemaGuidColumn pkColumn, SchemaStringColumn searchColumn, ZString value, SQLComparisonOperator comparison)
		{
			var resultQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), pkColumn);
			var countrySubQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefUNLOCOSchema.RL_RN_NKCountryCode, RefCountrySchema.RN_Code);
			var zonePivotSubQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefCountrySchema.PK, RefZonePivotSchema.F2_ParentID);
			var viewLocationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), RefZonePivotSchema.F2_FZ);
			viewLocationSubQuery.AddToFilter(searchColumn, comparison, value);

			zonePivotSubQuery.AddSubQuery(viewLocationSubQuery, JoinCondition.And);
			countrySubQuery.AddSubQuery(zonePivotSubQuery, JoinCondition.And);
			resultQuery.AddSubQuery(countrySubQuery, JoinCondition.And);

			return resultQuery;
		}

		ZQuery GetOriginDestinationQuery(ZString origin, ZString destination)
		{
			var result = new ZDBOnlyQuery(typeof(ViewValueAnalysis));
			GetLocationQuery(result, ViewValueAnalysisSchema.VVA_Origin, ViewLocationSchema.VLO_Code, origin, SQLComparisonOperator.StartsWith);
			GetLocationQuery(result, ViewValueAnalysisSchema.VVA_Destination, ViewLocationSchema.VLO_Code, destination, SQLComparisonOperator.StartsWith);
			return result;
		}
		#endregion

		#endregion

		#region Status and Flags

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|", "Status and Flags"));

			if (ModuleContext == OrgHeaderSchema.PK.Name || ModuleContext == ViewCampaignContactSchema.VCC_OH.Name)
			{
				AddTradeStatusFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|TradeStatus", Description.TradeStatus);
			}

			AddCertaintyFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|Certainty", Description.Certainty);
			AddVerticalMarketFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|VerticalMarket", Description.VerticalMarket);

			AddPeriodOfActivityFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|PeriodActivity", Description.PeriodActivity);

			if (ModuleContext == OrgOpportunitySchema.PK.Name)
			{
				AddProspectStatusFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|ProspectStatus", Description.ProspectStatus);
				AddTradedSinceProspectDateFilter(filters, category).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|StatusAndFlags|TradedSinceProspectDate", Description.TradedSinceProspectDate);
			}
		}

		ModuleFilter AddProspectStatusFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var codes = new ValueAnalysisTradeStatusList();

			codes.RemoveCode(ValueAnalysisTradeStatusList.Codes.TradedIs_Traded);

			var filter = filters.AddTextFilter(Description.TradeStatus, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_Status, value), codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddTradeStatusFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var key = category.Description + Description.TradeStatus;
			var codes = Factory.GetCachedValue(key, () => new ValueAnalysisTradeStatusList());
			var filter = filters.AddTextFilter(Description.TradeStatus, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_Status, value), codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddCertaintyFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var key = category.Description + Description.Certainty;
			var codes = Factory.GetCachedValue(key, () => new CertaintyLikertItemList());
			var filter = filters.AddTextFilter(Description.Certainty, GetCertaintyQuery, codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddVerticalMarketFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var key = category.Description + Description.VerticalMarket;
			var codes = Factory.GetCachedValue(key, () =>
			{
				var result = OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetActiveCodeDescriptionPairList();
				result.SortByDescription();
				return result;
			});
			var filter = filters.AddTextFilter(Description.VerticalMarket, (value) => new ZDBOnlyQuery(typeof(ViewValueAnalysis)).AddFilterAndZSQLParameterCollection(FormattableString.Invariant(
				$@"(SELECT 
CASE 
WHEN (CONVERT(VARCHAR(5), VVA_IndustryVertical) = '') 
THEN (SELECT OM_CMIndustryVertical FROM dbo.OrgMiscServ WHERE OM_OH = VVA_OH_Primary AND OM_CMIndustryVertical = '{value}') 
WHEN (CONVERT(VARCHAR(5), VVA_IndustryVertical) = '{value}') 
THEN VVA_IndustryVertical 
ELSE null 
END) is not null "), null), codes);

			filter.Category = category;
			return filter;
		}

		ModuleFilter AddPeriodOfActivityFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var key = category.Description + Description.PeriodActivity;
			var codes = Factory.GetCachedValue(key, () =>
			{
				var result = OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetActiveCodeDescriptionPairList();
				result.SortByDescription();
				return result;
			});
			var filter = filters.AddTextFilter(Description.PeriodActivity, (value) => new ZQuery(ViewValueAnalysisSchema.VVA_PeriodOfActivity, value), codes);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddTradedSinceProspectDateFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var filter = filters.AddFlagsFilter(Description.TradedSinceProspectDate, new[] { Description.TradedSinceProspectDate }, new GetFlagsQuery[] { GetTradedSinceProspectDateQuery });
			filter.Category = category;
			return filter;
		}

		ZQuery GetCertaintyQuery(ZString value)
		{
			var result = new ZQuery();
			if (value == CertaintyLikertItemList.Codes._5ExtremelyLikely)
			{
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.GreaterThanOrEqualTo, (byte)80);
			}
			else if (value == CertaintyLikertItemList.Codes._4Likely)
			{
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.GreaterThanOrEqualTo, (byte)60);
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.LessThan, (byte)80);
			}
			else if (value == CertaintyLikertItemList.Codes._3Neutral)
			{
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.GreaterThanOrEqualTo, (byte)40);
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.LessThan, (byte)60);
			}
			else if (value == CertaintyLikertItemList.Codes._2Unlikely)
			{
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.GreaterThanOrEqualTo, (byte)20);
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.LessThan, (byte)40);
			}
			else
			{
				result.AddToFilter(ViewValueAnalysisSchema.VVA_ConversionCertainty, SQLComparisonOperator.LessThan, (byte)20);
			}
			return result;
		}

		ZQuery GetTradedSinceProspectDateQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(ViewValueAnalysis));
			string subQuery =
				@"
(
	SELECT 1
	FROM
		dbo.ViewSalesProspectToActualPivot
		JOIN dbo.OrgTradeDetail ON PA_OW = VOW_OW_Actual
		JOIN dbo.OrgTradePeriod ON PAS_PA = PA_PK AND VOW_ProspectPrimary = PAS_OH_Client
	WHERE
		VOW_OW_Prospect = VVA_OW
		AND VVA_OH_Primary = VOW_ProspectPrimary
		AND PAS_Period >= VVA_LatestProspectDate
)
";
			var sql = value ? FormattableString.Invariant($"VVA_IsTraded = 0 AND EXISTS {subQuery}") : FormattableString.Invariant($"VVA_IsTraded = 0 AND NOT EXISTS {subQuery}");

			result.AddFilterAndZSQLParameterCollection(sql, null);
			return result;
		}

		#endregion

		#region Dates

		SalesAnalysisPeriodList PeriodList => fPeriodList ?? (fPeriodList = new SalesAnalysisPeriodList());
		SalesAnalysisPeriodList fPeriodList;

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Dates|", "Dates"));

			if (ModuleContext == OrgHeaderSchema.PK.Name || ModuleContext == ViewCampaignContactSchema.VCC_OH.Name)
			{
				AddAnalysisPeriodFilter(filters, category, Description.AnalysisPeriod).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Dates|AnalysisPeriod", Description.AnalysisPeriod);
				AddDateFilter(filters, category, ViewValueAnalysisSchema.VVA_LatestProspectDate, Description.LastProspectDate).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Dates|LastProspectDate", Description.LastProspectDate);
			}

			AddDateFilter(filters, category, ViewValueAnalysisSchema.VVA_ExpectedTradeStartDate, Description.ExpectedTradeDate).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Dates|ExpectedTradeDate", Description.ExpectedTradeDate);
		}

		ModuleFilter AddDateFilter(ModuleFilterCollection filters, FilterCategory category, SchemaDateColumn column, ZString description)
		{
			var filter = filters.AddDateFilter(description, (GetDateQuery)((c, v1, v2) => new DateQueryBuilder().CreateDateRange(c, column, v1.Date, v2.Date)));
			filter.Category = category;
			return filter;
		}

		#region Analysis Period

		ModuleFilter AddAnalysisPeriodFilter(ModuleFilterCollection filters, FilterCategory category, ZString description)
		{
			var periodDescriptionList = SalesAnalysisPeriodListUtils.GetPeriodDescriptionList(PeriodList);
			var filter = new ValueAnalysisPeriodFilter(description, GetAnalysisPeriodFilter, periodDescriptionList);
			filters.AddCustomFilter(filter);
			filter.Category = category;
			return filter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it is sql statement")]
		protected ZQuery GetAnalysisPeriodFilter(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(ViewValueAnalysis));
			var parameters = new ZSqlParameterCollection();

			var tables = "dbo.OrgTradePeriod";
			var periodCode = PeriodList.GetCodeFromDescription(value);

			if (periodCode != SalesAnalysisPeriodList.Codes.TotalTradingLifetime)
			{
				var firstDayOfTheMonth = new ZDate(ZDate.Today.Year, ZDate.Today.Month, 1);
				var startDate = SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(firstDayOfTheMonth, periodCode);
				parameters.Add("@startDate", startDate, OrgTradePeriodSchema.PAS_Period);
				tables += " WHERE PAS_Period>=@startDate";

				var endDate = SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(firstDayOfTheMonth, periodCode);
				if (!endDate.IsEmpty)
				{
					parameters.Add("@endDate", endDate, OrgTradePeriodSchema.PAS_Period);
					tables += " AND PAS_Period<=@endDate";
				}
			}

			string queryString = string.Format(CultureInfo.InvariantCulture, "VVA_PK IN (SELECT PAS_PK FROM {0})", tables);
			string sql = string.Format(CultureInfo.InvariantCulture, queryString);
			result.AddFilterAndZSQLParameterCollection(sql, parameters);
			return result;
		}

		public class ValueAnalysisPeriodFilter : ModuleTextFilter
		{
			public ValueAnalysisPeriodFilter(ZString description, GetTextQuery tradePeriodFilter, IList periodDescriptionList)
				: base(description, tradePeriodFilter, periodDescriptionList)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				DefaultProperty = SalesAnalysisPeriodList.Descriptions.Trailing12Months;
			}
		}

		#endregion
		#endregion
		#region Organisation
		public OrganisationsFindBoxCollection Consignees
		{
			get
			{
				return new ConsigneeCollection(Factory);
			}
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|", "Organizations"));

			AddOrganisationFilter(filters, category, ViewValueAnalysisSchema.VVA_OH_Buyer, Description.OrgBuyer).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|Buyer", Description.OrgBuyer);
			AddOrganisationFilter(filters, category, ViewValueAnalysisSchema.VVA_OH_Supplier, Description.OrgSupplier).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|Supplier", Description.OrgSupplier);
			AddOrganisationFilter(filters, category, ViewValueAnalysisSchema.VVA_OH_Competitor, Description.OrgCompetitor).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|Competitor", Description.OrgCompetitor);
			AddOrganisationFilter(filters, category, ViewValueAnalysisSchema.VVA_OH_ControllingAgent, Description.OrgControllingAgent).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|ControllingAgent", Description.OrgControllingAgent);
			AddOrganisationFilter(filters, category, ViewValueAnalysisSchema.VVA_OH_ServiceProvider, Description.Carrier).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|Carrier", Description.Carrier);
			AddOrganisationFilter(filters, category, ViewValueAnalysisSchema.VVA_OH_Primary, Description.Organization).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|Organizations|Organization", Description.Organization);
		}

		ModuleFilter AddOrganisationFilter(ModuleFilterCollection filters, FilterCategory category, SchemaGuidColumn column, ZString description)
		{
			IBusinessObjectCollection collectionForFilter;
			switch (description)
			{
				case (Description.OrgBuyer):
					collectionForFilter = new ConsigneeCollection(Factory);
					break;
				case (Description.OrgSupplier):
					collectionForFilter = new ConsignorCollection(Factory);
					break;
				case (Description.OrgCompetitor):
					collectionForFilter = new CompetitorCollection(Factory);
					break;
				case (Description.OrgControllingAgent):
					collectionForFilter = new ControllingAgentCollection(Factory);
					break;
				case (Description.Carrier):
					collectionForFilter = new ShippingProviderCollection(Factory);
					break;
				default:
					collectionForFilter = new OrgHeaderCollection(Factory);
					break;
			}

			var filter = filters.AddGuidFilter(description, ModuleIDs.Organisation, column, collectionForFilter);
			filter.Category = category;
			return filter;
		}

		#endregion

		#region Aggregated Values per Trade lane, Mode and Type

		void AddAggregatedTradeLaneFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|", "Aggregated Values per Trade lane, Mode and Type"));

			if (ModuleContext == OrgHeaderSchema.PK.Name || ModuleContext == ViewCampaignContactSchema.VCC_OH.Name)
			{
				AddQuantityFilter(filters, category, OrgTradeValueSchema.PAV_Revenue, Description.TradedJobRevenue, true, ValueAnalysisQuantityFilter.Context.JobRevenue).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedJobRevenue", Description.TradedJobRevenue);
				AddQuantityFilter(filters, category, OrgTradeValueSchema.PAV_Revenue, Description.TradedRevenue, true, ValueAnalysisQuantityFilter.Context.Revenue).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedRevenue", Description.TradedRevenue);
				AddQuantityFilter(filters, category, OrgTradeValueSchema.PAV_Cost, Description.TradedJobCost, true, ValueAnalysisQuantityFilter.Context.JobCost).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedJobCost", Description.TradedJobCost);
				AddQuantityFilter(filters, category, OrgTradeValueSchema.PAV_Revenue, Description.TradedJobProfit, true, ValueAnalysisQuantityFilter.Context.JobProfit).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedJobProfit", Description.TradedJobProfit);

				AddQuantityFilter(filters, category, OrgTradePeriodSchema.PAS_RepeatsMnth, Description.TradedJobCount, true).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedJobCount", Description.TradedJobCount);
				AddQuantityFilter(filters, category, OrgTradePeriodSchema.PAS_TEUQuantity, Description.TradedTEUCount, true).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedTEUCount", Description.TradedTEUCount);
				AddQuantityFilter(filters, category, OrgTradePeriodSchema.PAS_Volume, Description.TradedVolume, true).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|TradedVolume", Description.TradedVolume);
			}

			if (ModuleContext == OrgOpportunitySchema.PK.Name)
			{
				if (ProductCode == SystemDefinedSalesProductList.Codes.ForwardingShipment || ProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage || ProductCode == SystemDefinedSalesProductList.Codes.LinerAgency)
				{
					AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_TEUQuantity, Description.EstimateTEUCountPA).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|EstimateTEUCountPA", Description.EstimateTEUCountPA);
				}

				AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_EstimatedProfit, Description.PipelineValuePA, ValueAnalysisPipelineFilter.Context.PipelineValue).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|PipelineValuePA", Description.PipelineValuePA);
				AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_EstimatedProfit, Description.UnsuccessfulValuePA, ValueAnalysisPipelineFilter.Context.UnsuccessfulValue).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|UnsuccessfulValuePA", Description.UnsuccessfulValuePA);
				AddQuantityFilter(filters, category, OrgTradePeriodSchema.PAS_EstimatedProfit, Description.CommittedValue, false, ValueAnalysisQuantityFilter.Context.Committed).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|CommittedValue", Description.CommittedValue);
				AddQuantityFilter(filters, category, OrgTradePeriodSchema.PAS_EstimatedProfit, Description.CommittedAndForecastValue, false).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|CommittedAndForecastValue", Description.CommittedAndForecastValue);

				AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_Weight, Description.EstimateWeightPA).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|EstimateWeightPA", Description.EstimateWeightPA);
				AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_Chargeable, Description.EstimateChargeablePA).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|EstimateChargeablePA", Description.EstimateChargeablePA);
				AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_Volume, Description.EstimateVolumePA).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|EstimateVolumePA", Description.EstimateVolumePA);
				AddPipelineFilter(filters, category, OrgTradePeriodSchema.PAS_RepeatsMnth, Description.EstimateJobCountPA).MultilingualDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisFilter|AggregatedTradeLane|EstimateJobCountPA", Description.EstimateJobCountPA);
			}
		}

		ModuleFilter AddQuantityFilter(ModuleFilterCollection filters, FilterCategory category, SchemaNumericColumn column, ZString description, bool isTradedFilter, ValueAnalysisQuantityFilter.Context context = ValueAnalysisQuantityFilter.Context.Any)
		{
			var filter = new ValueAnalysisQuantityFilter(description, column, isTradedFilter, context);
			filters.AddCustomFilter(filter);
			filter.Category = category;
			return filter;
		}

		ModuleFilter AddPipelineFilter(ModuleFilterCollection filters, FilterCategory category, SchemaNumericColumn column, ZString description, ValueAnalysisPipelineFilter.Context context = ValueAnalysisPipelineFilter.Context.Any)
		{
			var filter = new ValueAnalysisPipelineFilter(description, column, context);
			filters.AddCustomFilter(filter);
			filter.Category = category;
			return filter;
		}

		#endregion

#if DEBUG
		public override string GetAutomaticFilterTestCaseName_ForObjectFactory() => "ValueAnalysisFilterStripsHelperAutomaticFilterTest";

#endif
		#endregion
	}
}
