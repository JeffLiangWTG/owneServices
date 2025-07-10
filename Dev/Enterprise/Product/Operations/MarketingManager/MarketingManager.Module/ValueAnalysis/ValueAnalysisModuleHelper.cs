using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public sealed class ValueAnalysisModuleHelper : IValueAnalysisModuleHelper
	{
		public void AddValueAnalysisModuleFilterStrips(SchemaGuidColumn primaryKeyColumn, IModuleFilterCollection filterCollection, BusinessObjectFactory factory, Type parentBusinessObjectType)
		{
			var filters = filterCollection as ModuleFilterCollection;
			if (filters == null)
			{
				return;
			}

			var category = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("MarketingManager|ValueAnalysisModuleFilter|Category", "Value Analysis"));

			var forwardingDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisModuleFilter|ForwardingValueAnalysis", "Forwarding Value Analysis");
			var linerAgencyDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisModuleFilter|LinerAgencyValueAnalysis", "Liner Agency Value Analysis");
			var customsBrokerageDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisModuleFilter|CustomsBrokerageValueAnalysis", "Customs Brokerage Value Analysis");
			var transportDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisModuleFilter|TransportValueAnalysis", "Transport Value Analysis");
			var warehouseDescription = ResString.GetMultilingualString("MarketingManager|ValueAnalysisModuleFilter|WarehouseValueAnalysis", "Warehouse Value Analysis");

			if (primaryKeyColumn == OrgHeaderSchema.PK || primaryKeyColumn == ViewCampaignContactSchema.VCC_OH)
			{
				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisForwardingOrg, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = forwardingDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisLinerAgencyOrg, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = linerAgencyDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisCustomsBrokerageOrg, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = customsBrokerageDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisTransportOrg, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = transportDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisWarehouseOrg, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = warehouseDescription,
					Category = category
				});
			}
			else if (primaryKeyColumn == OrgOpportunitySchema.PK)
			{
				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisForwardingOpp, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = forwardingDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisLinerAgencyOpp, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = linerAgencyDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisCustomsBrokerageOpp, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = customsBrokerageDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisTransportOpp, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = transportDescription,
					Category = category
				});

				filters.AddFilter(new ValueAnalysisModuleFilter(ModuleIDs.ValueAnalysisWarehouseOpp, primaryKeyColumn, factory, parentBusinessObjectType)
				{
					MultilingualDescription = warehouseDescription,
					Category = category
				});
			}
		}

		public static string GetProductCode(ModuleIdentifier moduleID)
		{
			if (moduleID != null)
			{
				switch (moduleID.ID)
				{
					case ModuleId.ValueAnalysisCustomsBrokerageOrg:
					case ModuleId.ValueAnalysisCustomsBrokerageOpp:
						return SystemDefinedSalesProductList.Codes.CustomsBrokerage;
					case ModuleId.ValueAnalysisForwardingOrg:
					case ModuleId.ValueAnalysisForwardingOpp:
						return SystemDefinedSalesProductList.Codes.ForwardingShipment;
					case ModuleId.ValueAnalysisLinerAgencyOrg:
					case ModuleId.ValueAnalysisLinerAgencyOpp:
						return SystemDefinedSalesProductList.Codes.LinerAgency;
					case ModuleId.ValueAnalysisTransportOrg:
					case ModuleId.ValueAnalysisTransportOpp:
						return SystemDefinedSalesProductList.Codes.Transport;
					case ModuleId.ValueAnalysisWarehouseOrg:
					case ModuleId.ValueAnalysisWarehouseOpp:
						return SystemDefinedSalesProductList.Codes.Warehouse;
				}
			}
			return string.Empty;
		}
	}
}
