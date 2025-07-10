using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAppointedAgentPortsLookups : AutoOrgAppointedAgentPortsLookups
	{
		public OrgAppointedAgentPortsLookups(AutoOrgAppointedAgentPorts parent)
			: base(parent)
		{
		}

		public new OrgAppointedAgentPorts Parent
		{
			get { return (OrgAppointedAgentPorts)base.Parent; }
		}

		#region Agent status list

		public AgentStatusList AgentStatuses
		{
			get
			{
				if (Parent.O5_SeaAirCarrierOrForwarderType == OrgAppointedAgentPorts.GatewayAgent)
				{
					return Factory.GetCachedValue("GatewayLoginAgentRole", () =>
					{
						var agentStatusList = new AgentStatusList();
						agentStatusList.Clear();

						agentStatusList.AddPair(AgentStatusList.Codes.GatewayAgent, AgentStatusList.Descriptions.GatewayAgent);
						agentStatusList.AddPair(AgentStatusList.Codes.GatewayAgentWithTariff, AgentStatusList.Descriptions.GatewayAgentWithTariff);

						return agentStatusList;
					});
				}
				else
				{
					return Factory.GetCachedValue("ForwarderAgentStatusList", () =>
					{
						var agentStatusList = new AgentStatusList();
						agentStatusList.Clear();

						agentStatusList.AddPair(AgentStatusList.Codes.Appointed, AgentStatusList.Descriptions.Appointed);
						agentStatusList.AddPair(AgentStatusList.Codes.Handles, AgentStatusList.Descriptions.Handles);
						agentStatusList.AddPair(AgentStatusList.Codes.Published, AgentStatusList.Descriptions.Published);

						return agentStatusList;
					});
				}
			}
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Addresses

		public virtual OrgAddressDependentCollection Addresses
		{
			get { return Parent.Header.Addresses; }
		}

		public ActiveOrAllAddressesCollection ActiveAddresses
		{
			get { return new ActiveOrAllAddressesCollection(Addresses); }
		}

		#endregion

		#region AgentDirections

		public virtual CodeDescriptionPairList AgentDirections
		{
			get { return Factory.GetCachedValue("AgentDirectionList", () => new AgentDirectionList()); }
		}

		#endregion
	}
}
