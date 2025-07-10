using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCarrierAppointedAgentPortsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActiveAddressChangesWithOrganisation()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.MainAddress.OA_Address1 = "111 first street";

			OrgCarrierAppointedAgentPorts agentPorts = Factory.New<OrgCarrierAppointedAgentPorts>();
			agentPorts.OrganisationPK = orgHeader1.PK;

			AssertEquals(agentPorts.Lookups.ActiveAddresses[0], orgHeader1.ActiveOrAllAddresses[0]);

			OrgHeader orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.MainAddress.OA_Address1 = "222 second street";
			agentPorts.OrganisationPK = orgHeader2.PK;

			AssertEquals(agentPorts.Lookups.ActiveAddresses[0], orgHeader2.ActiveOrAllAddresses[0]);
		}

		public void TestOrganisationListCoversAllSupportedTypes()
		{
			var port = Factory.New<OrgCarrierAppointedAgentPorts>();
			port.O5_SeaAirCarrierOrForwarderType = "";

			AssertEquals("default", typeof(OrganisationsFindBoxCollection), port.Lookups.OrganisationList.GetType());

			foreach (CodeDescriptionPair pair in new CarrierOrForwarderType())
			{
				port.O5_SeaAirCarrierOrForwarderType = pair.Code;
				var collection = port.Lookups.OrganisationList;
				switch (port.O5_SeaAirCarrierOrForwarderType)
				{
					case CarrierOrForwarderType.Codes.Agency:
						AssertEquals("Agency", typeof(OrganisationsFindBoxCollection), port.Lookups.OrganisationList.GetType());
						break;
					case CarrierOrForwarderType.Codes.AirCTO:
						AssertEquals("AirCTO", typeof(AirCTOCollection), port.Lookups.OrganisationList.GetType());
						break;
					case CarrierOrForwarderType.Codes.Stevedore:
						AssertEquals("Stevedore", typeof(SeaCTOCollection), port.Lookups.OrganisationList.GetType());
						break;
					case CarrierOrForwarderType.Codes.RoadDepotShed:
						AssertEquals("RoadDepotShed", typeof(RoadDepotTransitShedCollection), port.Lookups.OrganisationList.GetType());
						break;
					case CarrierOrForwarderType.Codes.RailHeadDepot:
						AssertEquals("RailHeadDepot", typeof(RailHeadDepotCollection), port.Lookups.OrganisationList.GetType());
						break;
					case CarrierOrForwarderType.Codes.ContainerYard:
						AssertEquals("ContainerYard", typeof(ContainerYardCollection), port.Lookups.OrganisationList.GetType());
						break;
					default:
						AssertEquals("Other", typeof(OrganisationsFindBoxCollection), port.Lookups.OrganisationList.GetType());
						break;
				}
			}
		}

		public void TestOrganisationList()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();

			OrgCarrierAppointedAgentPorts seaPorts = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			AssertEquals("Sea", typeof(SeaCTOCollection), seaPorts.Lookups.OrganisationList.GetType());

			OrgCarrierAppointedAgentPorts airPorts = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			AssertEquals("Air", typeof(AirCTOCollection), airPorts.Lookups.OrganisationList.GetType());

			OrgCarrierAppointedAgentPorts railPorts = carrier.CarrierAppointedAgentPorts_RailHeadDepot.AddNew();
			AssertEquals("Rail", typeof(RailHeadDepotCollection), railPorts.Lookups.OrganisationList.GetType());

			OrgCarrierAppointedAgentPorts roadPorts = carrier.CarrierAppointedAgentPorts_RoadDepotShed.AddNew();
			AssertEquals("Road", typeof(RoadDepotTransitShedCollection), roadPorts.Lookups.OrganisationList.GetType());
		}

		public void TestCarrierAgentDirections()
		{
			string expected = ""
				+ "BTH - Both\r\n"
				+ "ARV - Arrival\r\n"
				+ "DEP - Departure"
				+ "";

			var carrierAgentPorts = Factory.New<OrgCarrierAppointedAgentPorts>();
			AssertEquals(expected, carrierAgentPorts.Lookups.AgentDirections.ElementsAsString);
		}
	}
}
