using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAppointedAgentPortsLookups : OrgAppointedAgentPortsLookups
	{
		public OrgCarrierAppointedAgentPortsLookups(OrgCarrierAppointedAgentPorts parent)
			: base(parent)
		{
		}

		public new OrgCarrierAppointedAgentPorts Parent
		{
			get { return (OrgCarrierAppointedAgentPorts)base.Parent; }
		}

		#region Organisations

		public virtual OrganisationsFindBoxCollection OrganisationList
		{
			get
			{
				switch (Parent.O5_SeaAirCarrierOrForwarderType)
				{
					case CarrierOrForwarderType.Codes.AirCTO:
						return new AirCTOCollection(Factory);
					case CarrierOrForwarderType.Codes.Stevedore:
						return new SeaCTOCollection(Factory);
					case CarrierOrForwarderType.Codes.RoadDepotShed:
						return new RoadDepotTransitShedCollection(Factory);
					case CarrierOrForwarderType.Codes.RailHeadDepot:
						return new RailHeadDepotCollection(Factory);
					case CarrierOrForwarderType.Codes.ContainerYard:
						return new ContainerYardCollection(Factory);
					default:
						return new OrganisationsFindBoxCollection(Factory);
				}
			}
		}

		public override OrgAddressDependentCollection Addresses
		{
			get { return Parent.Organisation != null ? Parent.Organisation.Addresses : new OrgAddressDependentCollection(Factory); }
		}

		#endregion

		#region Stevedore Types

		public CodeDescriptionPairList StevedoreTypeList
		{
			get { return Factory.GetCachedValue("StevedoreTerminalTypeList", delegate { return new StevedoreTerminalType(); }); }
		}

		#endregion

		#region Carrier Agent Direction

		public override CodeDescriptionPairList AgentDirections
		{
			get
			{
				return Factory.GetCachedValue("CarrierAgentDirectionList",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(OrgConstants.CarrierAgentDirections.Code.Both, OrgConstants.CarrierAgentDirections.Description.Both);
						result.AddPair(OrgConstants.CarrierAgentDirections.Code.Arrival, OrgConstants.CarrierAgentDirections.Description.Arrival);
						result.AddPair(OrgConstants.CarrierAgentDirections.Code.Departure, OrgConstants.CarrierAgentDirections.Description.Departure);
						return result;
					});
			}
		}

		#endregion
	}
}
