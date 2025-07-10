using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItem : StronglyTypedRegistryItem<AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection>
	{
		public AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		public class AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public AutoratingIntercompanyTariffsForGatewayJobConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new AutoratingIntercompanyTariffsForGatewayJobConfigurationDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultCollection = new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();

				defaultCollection.SuspendValidation();
				GetDefaultValues(defaultCollection);
				defaultCollection.ResumeValidation();
				return defaultCollection;
			}

			public static void GetDefaultValues(AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection collection)
			{
				var defaultConfigurationItems = new (string loginRole, string agentRole, string direction, string job, string rule, string provider)[]
				{
					(
						/*     */ string.Empty,
						/* NGW */ GatewayLoginAgentRole.Code.NotGateway,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* NXT */ GatewayICTServiceProvider.Code.NextGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTA */ AgentStatusList.Codes.GatewayAgent,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* NXT */ GatewayICTServiceProvider.Code.NextGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/* SPA */ GatewayPickupAgentStatus.Code.PickupAgent,
						/* NGW */ GatewayLoginAgentRole.Code.NotGateway,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* NXT */ GatewayICTServiceProvider.Code.NextGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTA */ AgentStatusList.Codes.GatewayAgent,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* NXT */ GatewayICTServiceProvider.Code.NextGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.StopAutoratingCost,
						/* NOT */ GatewayICTServiceProvider.Code.NotAutorate
					),
					(
						/*     */ string.Empty,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* FCN */ JobInvoicingConsumerTypes.ForwardingConsolCode,
						/* ICR */ GatewayAutoratingRule.Code.AutoratingRevenue,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* FCN */ JobInvoicingConsumerTypes.ForwardingConsolCode,
						/* ICR */ GatewayAutoratingRule.Code.AutoratingRevenue,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* IMP */ FreightShipmentDirection.Code.Import,
						/* FCN */ JobInvoicingConsumerTypes.ForwardingConsolCode,
						/* ICR */ GatewayAutoratingRule.Code.AutoratingRevenue,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTA */ AgentStatusList.Codes.GatewayAgent,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* FCN */ JobInvoicingConsumerTypes.ForwardingConsolCode,
						/* ICR */ GatewayAutoratingRule.Code.AutoratingRevenue,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTA */ AgentStatusList.Codes.GatewayAgent,
						/* IMP */ FreightShipmentDirection.Code.Import,
						/* FCN */ JobInvoicingConsumerTypes.ForwardingConsolCode,
						/* ICR */ GatewayAutoratingRule.Code.AutoratingRevenue,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* DOM */ FreightShipmentDirection.Code.Domestic,
						/* FCN */ JobInvoicingConsumerTypes.ForwardingConsolCode,
						/* ICR */ GatewayAutoratingRule.Code.AutoratingRevenue,
						/* CUR */ GatewayICTServiceProvider.Code.CurrentGatewayOrganization
					),
					(
						/*     */ string.Empty,
						/* GTW */ GatewayLoginAgentRole.Code.Gateway,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* NXT */ GatewayICTServiceProvider.Code.NextGatewayOrganization
					),
					(
						/* SPA */ GatewayPickupAgentStatus.Code.PickupAgent,
						/* GTW */ GatewayLoginAgentRole.Code.Gateway,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* NXT */ GatewayICTServiceProvider.Code.NextGatewayOrganization
					),
					(
						/* NSP */ GatewayPickupAgentStatus.Code.NotPickupAgent,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* SCI */ GatewayAutoratingRule.Code.StopAutoratingCostFromICT,
						/* NOT */ GatewayICTServiceProvider.Code.NotAutorate
					),
					(
						/* NSP */ GatewayPickupAgentStatus.Code.NotPickupAgent,
						/* NGW */ GatewayLoginAgentRole.Code.NotGateway,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* SCI */ GatewayAutoratingRule.Code.StopAutoratingCostFromICT,
						/* NOT */ GatewayICTServiceProvider.Code.NotAutorate
					),
					(
						/* NSP */ GatewayPickupAgentStatus.Code.NotPickupAgent,
						/* GTW */ GatewayLoginAgentRole.Code.Gateway,
						/* EXP */ FreightShipmentDirection.Code.Export,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* SCI */ GatewayAutoratingRule.Code.StopAutoratingCostFromICT,
						/* NOT */ GatewayICTServiceProvider.Code.NotAutorate
					),
					(
						/* SPA */ GatewayPickupAgentStatus.Code.PickupAgent,
						/* GTT */ AgentStatusList.Codes.GatewayAgentWithTariff,
						/* OTH */ FreightShipmentDirection.Code.Other,
						/* SHP */ JobInvoicingConsumerTypes.ShipmentCode,
						/* ICC */ GatewayAutoratingRule.Code.AutoratingCost,
						/* FST */ GatewayICTServiceProvider.Code.First
					)
				};

				foreach (var configItem in defaultConfigurationItems)
				{
					var newDefaultConfigItem = collection.AddNew();
					newDefaultConfigItem.LoginRole = configItem.loginRole;
					newDefaultConfigItem.LoginAgentRole = configItem.agentRole;
					newDefaultConfigItem.ShipmentDirection = configItem.direction;
					newDefaultConfigItem.AutoratingJob = configItem.job;
					newDefaultConfigItem.AutoratingRule = configItem.rule;
					newDefaultConfigItem.ICTServiceProvider = configItem.provider;
				}
			}
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.Registry.AutoratingIntercompanyTariffsForGatewayJobConfigurationItemEditor, Enterprise.Rating.GUI")]
	class AutoratingIntercompanyTariffsForGatewayJobConfigurationDataType : NonPersistentBusinessObjectRegistryDataType<AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection>
	{
	}
}
