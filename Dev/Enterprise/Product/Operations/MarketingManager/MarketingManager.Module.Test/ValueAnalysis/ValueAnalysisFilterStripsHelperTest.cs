using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class ValueAnalysisFilterStripsHelperTest : TestCaseWithFactory
	{
		public void TestValueAnalysisFilterStripsAdded()
		{
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg,
				ModuleIDs.ValueAnalysisCustomsBrokerageOpp,
				ModuleIDs.ValueAnalysisForwardingOpp,
				ModuleIDs.ValueAnalysisLinerAgencyOpp,
				ModuleIDs.ValueAnalysisTransportOpp,
				ModuleIDs.ValueAnalysisWarehouseOpp
			};

			var orgModules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};

			var oppModules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOpp,
				ModuleIDs.ValueAnalysisForwardingOpp,
				ModuleIDs.ValueAnalysisLinerAgencyOpp,
				ModuleIDs.ValueAnalysisTransportOpp,
				ModuleIDs.ValueAnalysisWarehouseOpp
			};

			var schemas = new[]
			{
				OrgHeaderSchema.PK,
				OrgOpportunitySchema.PK
			};

			#region expected filters

			var expectedFilters = new Dictionary<string, string[]>
			{
				{
					OrgHeaderSchema.PK.Name + ModuleIDs.ValueAnalysisCustomsBrokerageOrg.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.TransportMode,
						ValueAnalysisFilterStripsHelper.Description.ClearanceType,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.TradedOriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.LastProspectDate,
						ValueAnalysisFilterStripsHelper.Description.ProspectiveBrokerageLocation,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCount,
						ValueAnalysisFilterStripsHelper.Description.TradedTEUCount,
						ValueAnalysisFilterStripsHelper.Description.TradedVolume,
						ValueAnalysisFilterStripsHelper.Description.TradedJobProfit,
						ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCost,
					}
				},
				{
					OrgHeaderSchema.PK.Name + ModuleIDs.ValueAnalysisForwardingOrg.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.TransportMode,
						ValueAnalysisFilterStripsHelper.Description.CargoType_Air,
						ValueAnalysisFilterStripsHelper.Description.CargoType_SeaRail,
						ValueAnalysisFilterStripsHelper.Description.CargoType_Road,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.OriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.LastProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCount,
						ValueAnalysisFilterStripsHelper.Description.TradedTEUCount,
						ValueAnalysisFilterStripsHelper.Description.TradedVolume,
						ValueAnalysisFilterStripsHelper.Description.TradedJobProfit,
						ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCost,
					}
				},
				{
					OrgHeaderSchema.PK.Name + ModuleIDs.ValueAnalysisLinerAgencyOrg.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.CargoType,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.OriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.LastProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCount,
						ValueAnalysisFilterStripsHelper.Description.TradedTEUCount,
						ValueAnalysisFilterStripsHelper.Description.TradedVolume,
						ValueAnalysisFilterStripsHelper.Description.TradedJobProfit,
						ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCost,
					}
				},
				{
					OrgHeaderSchema.PK.Name + ModuleIDs.ValueAnalysisTransportOrg.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.CargoType,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.OriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.LastProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OriginCity,
						ValueAnalysisFilterStripsHelper.Description.OriginState,
						ValueAnalysisFilterStripsHelper.Description.DestinationCity,
						ValueAnalysisFilterStripsHelper.Description.DestinationState,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCount,
						ValueAnalysisFilterStripsHelper.Description.TradedTEUCount,
						ValueAnalysisFilterStripsHelper.Description.TradedVolume,
						ValueAnalysisFilterStripsHelper.Description.TradedJobProfit,
						ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCost,
					}
				},
				{
					OrgHeaderSchema.PK.Name + ModuleIDs.ValueAnalysisWarehouseOrg.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.WarehouseName,
						ValueAnalysisFilterStripsHelper.Description.WarehouseLocation,
						ValueAnalysisFilterStripsHelper.Description.WarehouseService,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.LastProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCount,
						ValueAnalysisFilterStripsHelper.Description.TradedTEUCount,
						ValueAnalysisFilterStripsHelper.Description.TradedVolume,
						ValueAnalysisFilterStripsHelper.Description.TradedJobProfit,
						ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedRevenue,
						ValueAnalysisFilterStripsHelper.Description.TradedJobCost,
					}
				},
				{
					OrgOpportunitySchema.PK.Name + ModuleIDs.ValueAnalysisCustomsBrokerageOpp.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.TransportMode,
						ValueAnalysisFilterStripsHelper.Description.ClearanceType,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.ProspectiveBrokerageLocation,
						ValueAnalysisFilterStripsHelper.Description.TradedSinceProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.EstimateTEUCountPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateChargeablePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateJobCountPA,
						ValueAnalysisFilterStripsHelper.Description.PipelineValuePA,
						ValueAnalysisFilterStripsHelper.Description.UnsuccessfulValuePA,
						ValueAnalysisFilterStripsHelper.Description.CommittedValue,
						ValueAnalysisFilterStripsHelper.Description.CommittedAndForecastValue
					}
				},
				{
					OrgOpportunitySchema.PK.Name + ModuleIDs.ValueAnalysisForwardingOpp.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.TransportMode,
						ValueAnalysisFilterStripsHelper.Description.CargoType_Air,
						ValueAnalysisFilterStripsHelper.Description.CargoType_SeaRail,
						ValueAnalysisFilterStripsHelper.Description.CargoType_Road,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.OriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.TradedSinceProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.EstimateTEUCountPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateChargeablePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateJobCountPA,
						ValueAnalysisFilterStripsHelper.Description.PipelineValuePA,
						ValueAnalysisFilterStripsHelper.Description.UnsuccessfulValuePA,
						ValueAnalysisFilterStripsHelper.Description.CommittedValue,
						ValueAnalysisFilterStripsHelper.Description.CommittedAndForecastValue
					}
				},
				{
					OrgOpportunitySchema.PK.Name + ModuleIDs.ValueAnalysisLinerAgencyOpp.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.CargoType,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.OriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.TradedSinceProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.EstimateTEUCountPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateChargeablePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateJobCountPA,
						ValueAnalysisFilterStripsHelper.Description.PipelineValuePA,
						ValueAnalysisFilterStripsHelper.Description.UnsuccessfulValuePA,
						ValueAnalysisFilterStripsHelper.Description.CommittedValue,
						ValueAnalysisFilterStripsHelper.Description.CommittedAndForecastValue
					}
				},
				{
					OrgOpportunitySchema.PK.Name + ModuleIDs.ValueAnalysisTransportOpp.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.CargoType,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.OriginDestination,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.OriginCity,
						ValueAnalysisFilterStripsHelper.Description.OriginState,
						ValueAnalysisFilterStripsHelper.Description.DestinationCity,
						ValueAnalysisFilterStripsHelper.Description.DestinationState,
						ValueAnalysisFilterStripsHelper.Description.TradedSinceProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateChargeablePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateJobCountPA,
						ValueAnalysisFilterStripsHelper.Description.PipelineValuePA,
						ValueAnalysisFilterStripsHelper.Description.UnsuccessfulValuePA,
						ValueAnalysisFilterStripsHelper.Description.CommittedValue,
						ValueAnalysisFilterStripsHelper.Description.CommittedAndForecastValue
					}
				},
				{
					OrgOpportunitySchema.PK.Name + ModuleIDs.ValueAnalysisWarehouseOpp.Name,
					new[]
					{
						ValueAnalysisFilterStripsHelper.Description.SalesProduct,
						ValueAnalysisFilterStripsHelper.Description.WarehouseName,
						ValueAnalysisFilterStripsHelper.Description.WarehouseLocation,
						ValueAnalysisFilterStripsHelper.Description.WarehouseService,
						ValueAnalysisFilterStripsHelper.Description.TradeStatus,
						ValueAnalysisFilterStripsHelper.Description.Certainty,
						ValueAnalysisFilterStripsHelper.Description.VerticalMarket,
						ValueAnalysisFilterStripsHelper.Description.PeriodActivity,
						ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate,
						ValueAnalysisFilterStripsHelper.Description.ProspectiveWarehouseLocation,
						ValueAnalysisFilterStripsHelper.Description.TradedSinceProspectDate,
						ValueAnalysisFilterStripsHelper.Description.OrgBuyer,
						ValueAnalysisFilterStripsHelper.Description.OrgSupplier,
						ValueAnalysisFilterStripsHelper.Description.OrgCompetitor,
						ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent,
						ValueAnalysisFilterStripsHelper.Description.Carrier,
						ValueAnalysisFilterStripsHelper.Description.Organization,
						ValueAnalysisFilterStripsHelper.Description.EstimateWeightPA,
						ValueAnalysisFilterStripsHelper.Description.EstimateChargeablePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateVolumePA,
						ValueAnalysisFilterStripsHelper.Description.EstimateJobCountPA,
						ValueAnalysisFilterStripsHelper.Description.PipelineValuePA,
						ValueAnalysisFilterStripsHelper.Description.UnsuccessfulValuePA,
						ValueAnalysisFilterStripsHelper.Description.CommittedValue,
						ValueAnalysisFilterStripsHelper.Description.CommittedAndForecastValue
					}
				},
			};

			#endregion

			foreach (var schema in schemas)
			{
				foreach (var module in modules)
				{
					if ((schema == OrgHeaderSchema.PK && orgModules.Contains(module)) || (schema == OrgOpportunitySchema.PK && oppModules.Contains(module)))
					{
						var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
						var filters = new ModuleFilterCollection();
						helper.AddFilterStrips(filters);
						var key = schema.Name + module.Name;
						var expected = expectedFilters[key];
						foreach (var description in expected)
						{
							var filter = filters[description];
							AssertNotNull(key + " -> " + description, filter);
						}
						AssertEquals(key, expected.Length, filters.Count());
					}
				}
			}
		}

		[TestDate(2018, 1, 1)]
		public void TestValueAnalysisFilterStripsQuery()
		{
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg,
				ModuleIDs.ValueAnalysisCustomsBrokerageOpp,
				ModuleIDs.ValueAnalysisForwardingOpp,
				ModuleIDs.ValueAnalysisLinerAgencyOpp,
				ModuleIDs.ValueAnalysisTransportOpp,
				ModuleIDs.ValueAnalysisWarehouseOpp
			};

			var orgModules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};

			var oppModules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOpp,
				ModuleIDs.ValueAnalysisForwardingOpp,
				ModuleIDs.ValueAnalysisLinerAgencyOpp,
				ModuleIDs.ValueAnalysisTransportOpp,
				ModuleIDs.ValueAnalysisWarehouseOpp
			};

			var schemas = new[]
			{
				OrgHeaderSchema.PK,
				OrgOpportunitySchema.PK
			};

			var list = new Dictionary<string, String>
			{
				{ ValueAnalysisFilterStripsHelper.Description.TransportMode, "VVA_TradeMode = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.WarehouseService, "VVA_Service = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.CargoType, "VVA_TradeType = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.CargoType_Air, "VVA_TradeType = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.CargoType_SeaRail, "VVA_TradeType = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.CargoType_Road, "VVA_TradeType = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.ClearanceType, "VVA_TradeType = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.TradeStatus, "VVA_Status = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.OriginDestination, "(VVA_Origin IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'here%') or VVA_Origin IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'here%')) or VVA_Origin IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'here%'))))) and VVA_Destination IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'there%') or VVA_Destination IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'there%')) or VVA_Destination IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'there%'))))" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedOriginDestination, "((VVA_Origin IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'here%') or VVA_Origin IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'here%')) or VVA_Origin IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'here%'))))) and VVA_Destination IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'there%') or VVA_Destination IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'there%')) or VVA_Destination IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Code like 'there%'))))) and VVA_IsTraded = 1" },
				{ ValueAnalysisFilterStripsHelper.Description.Certainty, "VVA_ConversionCertainty < '20'" },
				{ ValueAnalysisFilterStripsHelper.Description.VerticalMarket, "(SELECT \r\nCASE \r\nWHEN (CONVERT(VARCHAR(5), VVA_IndustryVertical) = '') \r\nTHEN (SELECT OM_CMIndustryVertical FROM dbo.OrgMiscServ WHERE OM_OH = VVA_OH_Primary AND OM_CMIndustryVertical = 'test') \r\nWHEN (CONVERT(VARCHAR(5), VVA_IndustryVertical) = 'test') \r\nTHEN VVA_IndustryVertical \r\nELSE null \r\nEND) is not null " },
				{ ValueAnalysisFilterStripsHelper.Description.PeriodActivity, "VVA_PeriodOfActivity = 'test'" },
				{ ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod, "VVA_PK IN (SELECT PAS_PK FROM dbo.OrgTradePeriod WHERE PAS_Period>=#2017-07-01 00:00:00.000# AND PAS_Period<=#2018-01-01 00:00:00.000#)" },
				{ ValueAnalysisFilterStripsHelper.Description.ExpectedTradeDate, "VVA_ExpectedTradeStartDate >= #2017-01-01 00:00:00.000#" },
				{ ValueAnalysisFilterStripsHelper.Description.LastProspectDate, "VVA_LatestProspectDate >= #2017-01-01 00:00:00.000#" },
				{ ValueAnalysisFilterStripsHelper.Description.OriginCity, "VVA_Origin IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Description like 'test%') or VVA_Origin IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Description like 'test%')) or VVA_Origin IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Description like 'test%'))))" },
				{ ValueAnalysisFilterStripsHelper.Description.OriginState, "VVA_Origin IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_StateDescription like 'test%') or VVA_Origin IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_StateDescription like 'test%')) or VVA_Origin IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_StateDescription like 'test%'))))" },
				{ ValueAnalysisFilterStripsHelper.Description.DestinationCity, "VVA_Destination IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Description like 'test%') or VVA_Destination IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Description like 'test%')) or VVA_Destination IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_Description like 'test%'))))" },
				{ ValueAnalysisFilterStripsHelper.Description.DestinationState, "VVA_Destination IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_StateDescription like 'test%') or VVA_Destination IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_StateDescription like 'test%')) or VVA_Destination IN (SELECT RL_PK FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode IN (SELECT RN_Code FROM dbo.RefCountry WHERE RN_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ IN (SELECT VLO_PK FROM dbo.ViewLocation WHERE VLO_StateDescription like 'test%'))))" },
				{ ValueAnalysisFilterStripsHelper.Description.ProspectiveBrokerageLocation, "VVA_Origin = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid') and (VVA_TradeType = 'IMP' or VVA_TradeType = 'EXP')" },
				{ ValueAnalysisFilterStripsHelper.Description.ProspectiveWarehouseLocation, "VVA_Origin = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.OrgBuyer, "VVA_OH_Buyer = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.OrgSupplier, "VVA_OH_Supplier = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.OrgCompetitor, "VVA_OH_Competitor = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.OrgControllingAgent, "VVA_OH_ControllingAgent = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.Carrier, "VVA_OH_ServiceProvider = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.Organization, "VVA_OH_Primary = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedJobCount, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAS_IsJobValue = 0 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_RepeatsMnth)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedTEUCount, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAS_IsJobValue = 0 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_TEUQuantity)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedVolume, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAS_IsJobValue = 0 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_Volume)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedJobProfit, $"EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAV_GC = '{Env.CurrentCompanyPK}' AND PAS_IsJobValue = 1 GROUP BY PA_PK, PAS_OH_Client HAVING (SUM(PAV_Revenue)-SUM(PAV_Cost))=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedJobRevenue, $"EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAV_GC = '{Env.CurrentCompanyPK}' AND PAS_IsJobValue = 1 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAV_Revenue)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedRevenue, $"EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAV_GC = '{Env.CurrentCompanyPK}' AND PAS_IsJobValue = 0 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAV_Revenue)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.TradedJobCost, $"EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAV_GC = '{Env.CurrentCompanyPK}' AND PAS_IsJobValue = 1 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAV_Cost)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.CommittedValue, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 0 AND PA_Status = 'SUC' AND PAS_IsForecast = 0 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_EstimatedProfit)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" },
				{ ValueAnalysisFilterStripsHelper.Description.CommittedAndForecastValue, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 0 AND PA_Status = 'SUC' GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_EstimatedProfit)=7) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)" }
			};

			int count = 0;
			foreach (var schema in schemas)
			{
				foreach (var module in modules)
				{
					if ((schema == OrgHeaderSchema.PK && orgModules.Contains(module)) || (schema == OrgOpportunitySchema.PK && oppModules.Contains(module)))
					{
						var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
						var filters = new ModuleFilterCollection();
						helper.AddFilterStrips(filters);
						foreach (var item in list)
						{
							var filter = filters[item.Key];
							if (filter != null)
							{
								var key = schema.Name + module.Name + " -> " + item.Key;
								if (filter is ModuleTextFilter filterText)
								{
									if (filter is ValueAnalysisFilterStripsHelper.ValueAnalysisPeriodFilter filterPeriod)
									{
										filterPeriod.Property = SalesAnalysisPeriodList.Descriptions.Trailing6Months;
									}
									else
									{
										filterText.Property = "test";
									}
								}
								else if (filter is ModuleDateFilter filterDate)
								{
									filterDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
									filterDate.Property1 = new ZDate(2017, 1, 1);
								}
								else if (filter is ModuleLocationFilter filterLocation)
								{
									filterLocation.Property1 = "here";
									filterLocation.Property2 = "there";
								}
								else if (filter is ModuleFlagsFilter filterFlags)
								{
									filterFlags.Property1 = true;
								}
								else if (filter is ModuleGuidFilter filterGuid)
								{
									filterGuid.Property = ZGuid.BrettsGuid;
								}
								else if (filter is ValueAnalysisQuantityFilter filterQuantity)
								{
									filterQuantity.Period = SalesAnalysisPeriodList.Codes.TotalTradingLifetime;
									filterQuantity.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
									filterQuantity.Property1 = 7;
								}
								else
								{
									Assert(key, false);
								}
								var query = filters.GetFilterQuery(new[] { filter });
								AssertEquals(key, item.Value, query.LiteralTextADO);
								++count;
							}
						}
					}
				}
			}
			AssertEquals("Total number of filter strips", 201, count);
		}

		#region Quantity Filters

		[TestDate(2018, 1, 1)]
		public void TestQuantityFilterWithLoginCompany()
		{
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};

			var schema = OrgHeaderSchema.PK;

			SetupViewValueAnalysisProperties();

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			Values2.PAV_GC = newCompany.PK;
			Factory.Save();

			foreach (var module in modules)
			{
				var collection = new ViewValueAnalysisCollection(Factory);
				var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
				var filters = new ModuleFilterCollection();
				helper.AddFilterStrips(filters);

				var periodFilter = (ValueAnalysisQuantityFilter)filters[ValueAnalysisFilterStripsHelper.Description.TradedJobCost];
				periodFilter.IsActive = true;
				periodFilter.Period = SalesAnalysisPeriodList.Codes.TotalTradingLifetime;
				periodFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
				periodFilter.Property2 = Values.PAV_Cost + Values2.PAV_Cost;

				AssertEquals("Values' PAV_GC should be set to the current company for this test", Env.CurrentCompanyPK, Values.PAV_GC);
				AssertEquals("Values3's PAV_GC should be set to the current company for this test", Env.CurrentCompanyPK, Values3.PAV_GC);
				AssertNotEquals(newCompany.PK.ToGuid(), Env.CurrentCompanyPK);

				collection.Load(periodFilter.Query);
				AssertEquals(2, collection.Count);

				Values3.PAV_GC = newCompany.PK;
				Factory.Save();

				collection.Load(periodFilter.Query);
				AssertEquals(1, collection.Count);

				Values3.PAV_GC = Env.CurrentCompanyPK;
				Factory.Save();
			}
		}

		public void TestTradedQuantityFilterShouldReturnPAS_OH_ClientSpecificRecords()
		{
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};

			var schema = OrgHeaderSchema.PK;

			SetupViewValueAnalysisProperties();

			var newPeriod = Details.TradedPeriods.AddNew();
			newPeriod.PAS_Period = new ZDate(2017, 8, 1);
			newPeriod.PAS_RepeatsMnth = 100;
			newPeriod.PAS_Weight = 200;
			newPeriod.PAS_Volume = 300;
			newPeriod.PAS_TEUQuantity = 399;
			newPeriod.PAS_OH_Client = Supplier.PK;
			newPeriod.PAS_IsJobValue = false;

			Factory.Save();

			foreach (var module in modules)
			{
				var collection = new ViewValueAnalysisCollection(Factory);
				var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
				var filters = new ModuleFilterCollection();
				helper.AddFilterStrips(filters);

				var filter = (ValueAnalysisQuantityFilter)filters[ValueAnalysisFilterStripsHelper.Description.TradedTEUCount];
				filter.IsActive = true;
				filter.Period = SalesAnalysisPeriodList.Codes.TotalTradingLifetime;
				filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
				filter.Property1 = Period1.PAS_TEUQuantity + newPeriod.PAS_TEUQuantity;

				collection.Load(filter.Query);
				AssertEquals(0, collection.Count);

				filter.Property1 = Period1.PAS_TEUQuantity;
				collection.Load(filter.Query);
				AssertEquals(1, collection.Count);

				filter.Property1 = newPeriod.PAS_TEUQuantity;
				collection.Load(filter.Query);
				AssertEquals(2, collection.Count);
			}
		}

		#endregion

		#region Analysis Period Filters

		[TestDate(2018, 1, 1)]
		public void TestAnalysisPeriodFilter()
		{
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};

			var schema = OrgHeaderSchema.PK;

			SetupViewValueAnalysisProperties();

			foreach (var module in modules)
			{
				var collection = new ViewValueAnalysisCollection(Factory);
				var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
				var filters = new ModuleFilterCollection();
				helper.AddFilterStrips(filters);

				var periodFilter = (ValueAnalysisFilterStripsHelper.ValueAnalysisPeriodFilter)filters[ValueAnalysisFilterStripsHelper.Description.AnalysisPeriod];
				periodFilter.IsActive = true;
				periodFilter.Property = SalesAnalysisPeriodList.Descriptions.Trailing6Months;
				collection.Load(periodFilter.Query);
				AssertEquals(1, collection.Count);

				periodFilter.Property = SalesAnalysisPeriodList.Descriptions.Trailing12Months;
				collection.Load(periodFilter.Query);
				AssertEquals(2, collection.Count);

				periodFilter.Property = SalesAnalysisPeriodList.Descriptions.TotalTradingLifetime;
				collection.Load(periodFilter.Query);
				AssertEquals(2, collection.Count);
			}
		}

		#endregion

		#region Vertical Market Filters

		public void TestVerticalMarketFilter()
		{
			var module = ModuleIDs.ValueAnalysisForwardingOpp;
			var schema = OrgHeaderSchema.PK;

			SetupViewValueAnalysisPipelineProperties();

			var collection = new ViewValueAnalysisCollection(Factory);
			var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			Prospect.PAP_IndustryVertical = "";
			Buyer.MiscServ.OM_CMIndustryVertical = "";
			SetupVerticalMarketFilter(filters, collection);
			AssertEquals(0, collection.Count);

			var bizOToReload = Factory.Load<ViewValueAnalysis>(new ZQuery()).FirstOrDefault();

			AssertCountForCriteria(1, string.Empty, "INDUS", filters, collection, bizOToReload);
			AssertCountForCriteria(1, "INDUS", string.Empty, filters, collection, bizOToReload);
			AssertCountForCriteria(0, string.Empty, string.Empty, filters, collection, bizOToReload);
			AssertCountForCriteria(1, "INDUS", "INDUS", filters, collection, bizOToReload);
			AssertCountForCriteria(0, "DEF", "DEF", filters, collection, bizOToReload);
			AssertCountForCriteria(1, "DEF", "INDUS", filters, collection, bizOToReload);
			AssertCountForCriteria(0, "INDUS", "DEF", filters, collection, bizOToReload);
			AssertCountForCriteria(0, "DEF", string.Empty, filters, collection, bizOToReload);
			AssertCountForCriteria(0, string.Empty, "DEF", filters, collection, bizOToReload);
		}

		#endregion

		#region Location Filter

		public void TestLocationFilter()
		{
			var module = ModuleIDs.ValueAnalysisCustomsBrokerageOpp;
			var schema = OrgHeaderSchema.PK;

			SetupViewValueAnalysisLocationProperties();

			var collection = new ViewValueAnalysisCollection(Factory);
			var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			SetupLocationFilter(filters, collection, "USLAX", "AUMEL");
			AssertEquals(1, collection.Count);

			SetupLocationFilter(filters, collection, "AUJF", "AUMEL");
			AssertEquals(3, collection.Count);

			SetupLocationFilter(filters, collection, "AUSYD", "AUMEL");
			AssertEquals(1, collection.Count);

			SetupLocationFilter(filters, collection, "AUMEL", "AUMEL");
			AssertEquals(1, collection.Count);
		}

		#endregion

		#region Pipeline Filters

		public void TestPipelineFilter()
		{
			var module = ModuleIDs.ValueAnalysisCustomsBrokerageOpp;
			var schema = OrgOpportunitySchema.PK;

			SetupViewValueAnalysisPipelineProperties();

			var collection = new ViewValueAnalysisCollection(Factory);
			var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ValueAnalysisPipelineFilter)filters[ValueAnalysisFilterStripsHelper.Description.EstimateTEUCountPA];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			var expectedValue = Period1.PAS_TEUQuantity * Period1.PAS_RepeatsMnth * 12;
			filter.Property1 = expectedValue - 1;
			filter.Property2 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property2 = expectedValue - 1;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property1 = expectedValue - 1;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property2 = expectedValue - 1;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			Period1.PAS_RepeatsMnth = 0;
			Factory.Save();

			expectedValue = Period1.PAS_TEUQuantity * 1 * 12;

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);
		}

		public void TestPipelineJobCountFilter()
		{
			var module = ModuleIDs.ValueAnalysisCustomsBrokerageOpp;
			var schema = OrgOpportunitySchema.PK;

			SetupViewValueAnalysisPipelineProperties();

			var collection = new ViewValueAnalysisCollection(Factory);
			var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ValueAnalysisPipelineFilter)filters[ValueAnalysisFilterStripsHelper.Description.EstimateJobCountPA];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			var expectedValue = Period1.PAS_RepeatsMnth * 12;
			filter.Property1 = expectedValue - 1;
			filter.Property2 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property2 = expectedValue - 1;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property1 = expectedValue - 1;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property2 = expectedValue - 1;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);
		}

		public void TestPipelineOneOffAndYearlyRecurrenceFilter()
		{
			var module = ModuleIDs.ValueAnalysisCustomsBrokerageOpp;
			var schema = OrgOpportunitySchema.PK;

			SetupViewValueAnalysisPipelineProperties();
			Prospect.PAP_RecurrenceType = "ONE";
			Factory.Save();

			var collection = new ViewValueAnalysisCollection(Factory);
			var helper = new ValueAnalysisFilterStripsHelper(ValueAnalysisModuleHelper.GetProductCode(module), schema.Name, Factory);
			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ValueAnalysisPipelineFilter)filters[ValueAnalysisFilterStripsHelper.Description.EstimateTEUCountPA];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			var expectedValue = Period1.PAS_TEUQuantity * Period1.PAS_RepeatsMnth;
			filter.Property1 = expectedValue * 12;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			filter.Property1 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			Prospect.PAP_RecurrenceType = "YR";
			Factory.Save();

			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);

			filter.Property1 = expectedValue * 12;
			collection.Load(filter.Query);
			AssertEquals(0, collection.Count);

			Period1.PAS_RepeatsMnth = 0;
			Factory.Save();

			expectedValue = Period1.PAS_TEUQuantity * 1;

			filter.Property1 = expectedValue;
			collection.Load(filter.Query);
			AssertEquals(1, collection.Count);
		}

		#endregion

		#region Implementation

		protected OrgHeader Buyer;
		protected OrgHeader Supplier;
		protected OrgHeader Supplier1;
		protected OrgHeader Supplier2;
		protected ViewLocation Origin;
		protected ViewLocation Origin1;
		protected ViewLocation Origin2;
		protected ViewLocation Destination;
		protected OrgSales Sales;
		protected OrgSales Sales2;
		protected OrgSales Sales3;
		protected OrgTradeDetail Details;
		protected OrgTradeDetail Details2;
		protected OrgTradeDetail Details3;
		protected OrgTradePeriod Period1;
		protected OrgTradePeriod Period1a;
		protected OrgTradeValue Values;
		protected OrgTradePeriod Period2;
		protected OrgTradePeriod Period2a;
		protected OrgTradePeriod Period3;
		protected OrgTradeValue Values2;
		protected OrgTradeValue Values3;
		protected OrgTradeProspect Prospect;

		protected void SetupViewValueAnalysisProperties()
		{
			Buyer = Factory.NewWithValidTestData<OrgHeader>();
			Supplier = Factory.NewWithValidTestData<OrgHeader>();
			Origin = ViewLocationHelper.GetLocationFromString(Factory, "DEHAM", "RL");
			Destination = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL");

			Sales = Factory.NewWithValidTestData<OrgSales>();
			Sales.OW_OH_Buyer = Buyer.PK;
			Sales.OW_OH_Supplier = Supplier.PK;
			Sales.OW_OriginID = Origin.PK;
			Sales.OW_DestinationID = Destination.PK;

			Details = Sales.TradeDetails.AddNew();

			Period1 = Details.TradedPeriods.AddNew();
			Period1.PAS_Period = new ZDate(2017, 8, 1);
			Period1.PAS_RepeatsMnth = 100;
			Period1.PAS_Weight = 200;
			Period1.PAS_Volume = 300;
			Period1.PAS_TEUQuantity = 400;
			Period1.PAS_OH_Client = Buyer.PK;
			Period1.PAS_IsJobValue = true;

			Period1a = Details.TradedPeriods.AddNew();
			Period1a.PAS_Period = new ZDate(2017, 8, 1);
			Period1a.PAS_RepeatsMnth = 100;
			Period1a.PAS_Weight = 200;
			Period1a.PAS_Volume = 300;
			Period1a.PAS_TEUQuantity = 400;
			Period1a.PAS_OH_Client = Buyer.PK;
			Period1a.PAS_IsJobValue = false;

			Values = Period1.TradeValues.AddNew();
			Values.PAV_Cost = 1000;
			Values.PAV_Revenue = 2000;
			Values.PAV_GC = Env.CurrentCompanyPK;
			Values2 = Period1.TradeValues.AddNew();
			Values2.PAV_Cost = 10000;
			Values2.PAV_Revenue = 20000;
			Values2.PAV_GC = Env.CurrentCompanyPK;

			Sales2 = Factory.NewWithValidTestData<OrgSales>();
			Sales2.OW_OH_Buyer = Buyer.PK;
			Sales2.OW_OH_Supplier = Supplier.PK;
			Sales2.OW_OriginID = Origin.PK;
			Sales2.OW_DestinationID = Destination.PK;

			Details2 = Sales2.TradeDetails.AddNew();

			Period2 = Details2.TradedPeriods.AddNew();
			Period2.PAS_Period = new ZDate(2017, 1, 1);
			Period2.PAS_RepeatsMnth = 1;
			Period2.PAS_Weight = 2;
			Period2.PAS_Volume = 3;
			Period2.PAS_TEUQuantity = 4;
			Period2.PAS_OH_Client = Supplier.PK;
			Period2.PAS_IsJobValue = true;

			Period2a = Details2.TradedPeriods.AddNew();
			Period2a.PAS_Period = new ZDate(2017, 1, 1);
			Period2a.PAS_RepeatsMnth = 1;
			Period2a.PAS_Weight = 2;
			Period2a.PAS_Volume = 3;
			Period2a.PAS_TEUQuantity = 4;
			Period2a.PAS_OH_Client = Supplier.PK;
			Period2a.PAS_IsJobValue = false;

			Values3 = Period2.TradeValues.AddNew();
			Values3.PAV_Cost = 1;
			Values3.PAV_Revenue = 2;
			Values3.PAV_GC = Env.CurrentCompanyPK;

			Sales.OW_IsTraded = true;
			Sales2.OW_IsTraded = true;

			Factory.Save();
		}

		protected void SetupViewValueAnalysisPipelineProperties()
		{
			Buyer = Factory.NewWithValidTestData<OrgHeader>();
			Supplier = Factory.NewWithValidTestData<OrgHeader>();
			Origin = ViewLocationHelper.GetLocationFromString(Factory, "DEHAM", "RL");
			Destination = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL");

			Sales = Factory.NewWithValidTestData<OrgSales>();
			Sales.OW_OH_Buyer = Buyer.PK;
			Sales.OW_OH_Supplier = Supplier.PK;
			Sales.OW_OH_Primary = Buyer.PK;
			Sales.OW_OriginID = Origin.PK;
			Sales.OW_DestinationID = Destination.PK;

			Details = Sales.TradeDetails.AddNew();
			Details.PA_Status = "ACT";

			Period1 = Details.TradedPeriods.AddNew();
			Period1.PAS_RepeatsMnth = 2;
			Period1.PAS_Weight = 200;
			Period1.PAS_Volume = 300;
			Period1.PAS_TEUQuantity = 400;
			Period1.PAS_OH_Client = Buyer.PK;
			Period1.PAS_IsTraded = false;

			Values = Period1.TradeValues.AddNew();
			Values.PAV_Cost = 1000;
			Values.PAV_Revenue = 2000;
			Values.PAV_GC = Env.CurrentCompanyPK;

			Prospect = Details.ProspectDetail;
			Prospect.PAP_RecurrenceType = "MTH";

			Sales.OW_IsTraded = false;

			Factory.Save();
		}

		void SetupLocationFilter(ModuleFilterCollection filters, ViewValueAnalysisCollection collection, ZString origin, ZString destination)
		{
			var filter = (ValueAnalysisLocationFilter)filters[ValueAnalysisFilterStripsHelper.Description.TradedOriginDestination];

			filter.IsActive = true;
			filter.Property1 = origin;
			filter.Property2 = destination;
			var query = filters.GetFilterQuery(new[] { filter });
			collection.Load(query);
		}

		void SetupVerticalMarketFilter(ModuleFilterCollection filters, ViewValueAnalysisCollection collection)
		{
			var filter = (ModuleTextFilter)filters[ValueAnalysisFilterStripsHelper.Description.VerticalMarket];

			filter.IsActive = true;
			filter.Property = "INDUS";
			var query = filters.GetFilterQuery(new[] { filter });
			collection.Load(query);
		}

		protected void SetupViewValueAnalysisLocationProperties()
		{
			Supplier = Factory.NewWithValidTestData<OrgHeader>();
			Supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier.OH_RL_NKClosestPort = "USLAX";
			Supplier1.OH_RL_NKClosestPort = "AUSYD";
			Supplier2.OH_RL_NKClosestPort = "AUMEL";

			Buyer = Factory.NewWithValidTestData<OrgHeader>();
			Origin = ViewLocationHelper.GetLocationFromString(Factory, "USLAX", "RL");
			Origin1 = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", "RL");
			Origin2 = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL");
			Destination = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", "RL");

			RefZoneHeader zone = Factory.NewWithValidTestData<RefZoneHeader>();
			var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var australia = sydney.Country;

			zone.UNLOCOs.Add(Supplier.UNLOCO);
			zone.UNLOCOs.Add(Supplier1.UNLOCO);
			zone.Countries.Add(australia);
			zone.Code = "AUJF";

			Sales = Factory.NewWithValidTestData<OrgSales>();
			Sales.OW_OH_Buyer = Buyer.PK;
			Sales.OW_OH_Supplier = Supplier.PK;
			Sales.OW_OH_Primary = Buyer.PK;
			Sales.OW_OriginID = Origin.PK;
			Sales.OW_DestinationID = Destination.PK;

			Details = Sales.TradeDetails.AddNew();

			Period1 = Details.TradedPeriods.AddNew();
			Period1.PAS_RepeatsMnth = 2;
			Period1.PAS_Weight = 200;
			Period1.PAS_Volume = 300;
			Period1.PAS_TEUQuantity = 400;
			Period1.PAS_OH_Client = Buyer.PK;
			Period1.PAS_IsTraded = false;

			Sales2 = Factory.NewWithValidTestData<OrgSales>();
			Sales2.OW_OH_Buyer = Buyer.PK;
			Sales2.OW_OH_Supplier = Supplier.PK;
			Sales2.OW_OriginID = Origin1.PK;
			Sales2.OW_DestinationID = Destination.PK;

			Details2 = Sales2.TradeDetails.AddNew();

			Period2 = Details2.TradedPeriods.AddNew();
			Period2.PAS_RepeatsMnth = 2;
			Period2.PAS_Weight = 200;
			Period2.PAS_Volume = 300;
			Period2.PAS_TEUQuantity = 400;
			Period2.PAS_OH_Client = Buyer.PK;
			Period2.PAS_IsTraded = false;

			Sales3 = Factory.NewWithValidTestData<OrgSales>();
			Sales3.OW_OH_Buyer = Buyer.PK;
			Sales3.OW_OH_Supplier = Supplier.PK;
			Sales3.OW_OH_Primary = Buyer.PK;
			Sales3.OW_OriginID = Origin2.PK;
			Sales3.OW_DestinationID = Destination.PK;

			Details3 = Sales3.TradeDetails.AddNew();

			Period3 = Details3.TradedPeriods.AddNew();
			Period3.PAS_RepeatsMnth = 2;
			Period3.PAS_Weight = 200;
			Period3.PAS_Volume = 300;
			Period3.PAS_TEUQuantity = 400;
			Period3.PAS_OH_Client = Buyer.PK;
			Period3.PAS_IsTraded = false;

			Sales.OW_IsTraded = true;
			Sales2.OW_IsTraded = true;
			Sales3.OW_IsTraded = true;

			Factory.Save();
		}

		void AssertCountForCriteria(int expectedCount, string orgVertical, string prospectVertical, ModuleFilterCollection filters, ViewValueAnalysisCollection collection, ViewValueAnalysis bizOToReload)
		{
			Buyer.MiscServ.OM_CMIndustryVertical = orgVertical;
			Prospect.IsIndustryVerticalOverridden = !string.IsNullOrEmpty(prospectVertical);
			Prospect.PAP_IndustryVertical = prospectVertical;
			Factory.Save();

			bizOToReload.Reload();

			SetupVerticalMarketFilter(filters, collection);
			AssertEquals(expectedCount, collection.Count);
		}

		#endregion

	}
}
