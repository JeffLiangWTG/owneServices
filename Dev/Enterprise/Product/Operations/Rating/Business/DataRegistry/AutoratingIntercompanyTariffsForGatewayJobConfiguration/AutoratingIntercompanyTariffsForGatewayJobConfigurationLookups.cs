using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups : ZLookups
	{
		public AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups(AutoratingIntercompanyTariffsForGatewayJobConfiguration parent)
			: base(parent)
		{
		}

		public new AutoratingIntercompanyTariffsForGatewayJobConfiguration Parent => (AutoratingIntercompanyTariffsForGatewayJobConfiguration)base.Parent;

		public CodeDescriptionPairList LoginRoleList
		{
			get
			{
				if (fLoginRoleList == null)
				{
					fLoginRoleList = new CodeDescriptionPairList();
					fLoginRoleList.AddPair(GatewayPickupAgentStatus.Code.PickupAgent, GatewayPickupAgentStatus.Description.PickupAgent);
					fLoginRoleList.AddPair(GatewayPickupAgentStatus.Code.NotPickupAgent, GatewayPickupAgentStatus.Description.NotPickupAgent);
				}
				return fLoginRoleList;
			}
		}
		CodeDescriptionPairList fLoginRoleList;

		public CodeDescriptionPairList LoginAgentRoleList
		{
			get
			{
				if (fLoginAgentRoleList == null)
				{
					fLoginAgentRoleList = new CodeDescriptionPairList();
					fLoginAgentRoleList.AddPair(AgentStatusList.Codes.GatewayAgent, AgentStatusList.Descriptions.GatewayAgent);
					fLoginAgentRoleList.AddPair(AgentStatusList.Codes.GatewayAgentWithTariff, AgentStatusList.Descriptions.GatewayAgentWithTariff);
					fLoginAgentRoleList.AddPair(GatewayLoginAgentRole.Code.NotGateway, GatewayLoginAgentRole.Description.NotGateway);
					fLoginAgentRoleList.AddPair(GatewayLoginAgentRole.Code.Gateway, GatewayLoginAgentRole.Description.Gateway);
				}
				return fLoginAgentRoleList;
			}
		}
		CodeDescriptionPairList fLoginAgentRoleList;

		public CodeDescriptionPairList ShipmentDirectionList
		{
			get
			{
				if (fShipmentDirectionList == null)
				{
					fShipmentDirectionList = new CodeDescriptionPairList();
					fShipmentDirectionList.AddPair(FreightShipmentDirection.Code.All, FreightShipmentDirection.Description.All);
					fShipmentDirectionList.AddPair(FreightShipmentDirection.Code.Export, FreightShipmentDirection.Description.Export);
					fShipmentDirectionList.AddPair(FreightShipmentDirection.Code.Import, FreightShipmentDirection.Description.Import);
					fShipmentDirectionList.AddPair(FreightShipmentDirection.Code.Domestic, FreightShipmentDirection.Description.Domestic);
					fShipmentDirectionList.AddPair(FreightShipmentDirection.Code.Other, FreightShipmentDirection.Description.Other);
				}
				return fShipmentDirectionList;
			}
		}
		CodeDescriptionPairList fShipmentDirectionList;

		public CodeDescriptionPairList AutoratingJobList
		{
			get
			{
				if (fAutoratingJobList == null)
				{
					fAutoratingJobList = new CodeDescriptionPairList();
					{
						fAutoratingJobList.Add(JobInvoicingConsumerTypes.Shipment);
						fAutoratingJobList.Add(JobInvoicingConsumerTypes.ForwardingConsol);
					}
				}
				return fAutoratingJobList;
			}
		}
		CodeDescriptionPairList fAutoratingJobList;

		public CodeDescriptionPairList AutoratingRuleList
		{
			get
			{
				if (fCAutoratingRuleList == null)
				{
					fCAutoratingRuleList = new CodeDescriptionPairList();
					fCAutoratingRuleList.AddPair(GatewayAutoratingRule.Code.AutoratingCost, GatewayAutoratingRule.Description.AutoratingCost);
					fCAutoratingRuleList.AddPair(GatewayAutoratingRule.Code.AutoratingRevenue, GatewayAutoratingRule.Description.AutoratingRevenue);
					fCAutoratingRuleList.AddPair(GatewayAutoratingRule.Code.StopAutoratingCost, GatewayAutoratingRule.Description.StopAutoratingCost);
					fCAutoratingRuleList.AddPair(GatewayAutoratingRule.Code.StopAutoratingCostFromICT, GatewayAutoratingRule.Description.StopAutoratingCostFromICT);
				}
				return fCAutoratingRuleList;
			}
		}
		CodeDescriptionPairList fCAutoratingRuleList;

		public CodeDescriptionPairList ICTServiceProviderList
		{
			get
			{
				if (fICTServiceProviderList == null)
				{
					fICTServiceProviderList = new CodeDescriptionPairList();
					fICTServiceProviderList.AddPair(GatewayICTServiceProvider.Code.First, GatewayICTServiceProvider.Description.First);
					fICTServiceProviderList.AddPair(GatewayICTServiceProvider.Code.CurrentGatewayOrganization, GatewayICTServiceProvider.Description.CurrentGatewayOrganization);
					fICTServiceProviderList.AddPair(GatewayICTServiceProvider.Code.NextGatewayOrganization, GatewayICTServiceProvider.Description.NextGatewayOrganization);
					fICTServiceProviderList.AddPair(GatewayICTServiceProvider.Code.NotAutorate, GatewayICTServiceProvider.Description.NotAutorate);
				}

				return fICTServiceProviderList;
			}
		}
		CodeDescriptionPairList fICTServiceProviderList;
	}
}

