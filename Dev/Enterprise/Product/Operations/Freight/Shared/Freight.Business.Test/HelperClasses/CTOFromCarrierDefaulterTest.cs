using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CTOFromCarrierDefaulterTest : TestCaseWithFactory
	{
		public void TestSetCTOFromCarrier()
		{
			OrgAddress address1 = Factory.New<OrgAddress>();
			OrgAddress address2 = Factory.New<OrgAddress>();

			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgCarrierAppointedAgentPorts agentPort = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = address1.PK;

			OrgCarrierAppointedAgentPorts agentPort2 = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			agentPort2.O5_PortOrCountry = "USLAX";
			agentPort2.O5_OA_AgentOfficeAddress = address2.PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination dest = voyage.Destinations.AddNew();

			AssertEquals(ZGuid.Empty, origin.JA_OA_DepartureCTOAddress);
			AssertEquals(ZGuid.Empty, origin.JA_Calc_DepartureCTOAddressOrg);
			CTOFromCarrierDefaulter.SetCTOFromCarrier(voyage, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Departure, origin.JA_OA_DepartureCTOAddressInfo, origin.JA_Calc_DepartureCTOAddressOrgInfo);
			AssertEquals(address1.PK, origin.JA_OA_DepartureCTOAddress);

			CTOFromCarrierDefaulter.SetCTOFromCarrier(voyage, "AUBNE", OrgConstants.CarrierAgentDirections.Code.Departure, origin.JA_OA_DepartureCTOAddressInfo, origin.JA_Calc_DepartureCTOAddressOrgInfo);
			AssertEquals(ZGuid.Empty, origin.JA_Calc_DepartureCTOAddressOrg);

			AssertEquals(ZGuid.Empty, dest.JB_OA_ArrivalCTOAddress);
			AssertEquals(ZGuid.Empty, dest.JB_Calc_ArrivalCTOAddressOrg);
			CTOFromCarrierDefaulter.SetCTOFromCarrier(voyage, "USLAX", OrgConstants.CarrierAgentDirections.Code.Arrival, dest.JB_OA_ArrivalCTOAddressInfo, dest.JB_Calc_ArrivalCTOAddressOrgInfo);
			AssertEquals(address2.PK, dest.JB_OA_ArrivalCTOAddress);

			CTOFromCarrierDefaulter.SetCTOFromCarrier(voyage, "USMEM", OrgConstants.CarrierAgentDirections.Code.Arrival, dest.JB_OA_ArrivalCTOAddressInfo, dest.JB_Calc_ArrivalCTOAddressOrgInfo);
			AssertEquals(ZGuid.Empty, dest.JB_Calc_ArrivalCTOAddressOrg);
		}

		public void TestSetCTOFromCarrierConsol()
		{
			OrgAddress address1 = Factory.New<OrgAddress>();
			OrgAddress address2 = Factory.New<OrgAddress>();

			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgCarrierAppointedAgentPorts agentPort = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = address1.PK;

			OrgCarrierAppointedAgentPorts agentPort2 = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
			agentPort2.O5_PortOrCountry = "USLAX";
			agentPort2.O5_OA_AgentOfficeAddress = address2.PK;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals(ZGuid.Empty, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK);
			CTOFromCarrierDefaulter.SetCTOFromCarrier(consol, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Departure, consol.JK_OA_DepartureCTOAddressInfo);
			AssertEquals(address1.PK, consol.JK_OA_DepartureCTOAddress);

			CTOFromCarrierDefaulter.SetCTOFromCarrier(consol, "AUBNE", OrgConstants.CarrierAgentDirections.Code.Departure, consol.JK_OA_DepartureCTOAddressInfo);
			AssertEquals(ZGuid.Empty, consol.JK_OA_DepartureCTOAddress);

			AssertEquals(ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress_ZAddress.OrgPK);
			CTOFromCarrierDefaulter.SetCTOFromCarrier(consol, "USLAX", OrgConstants.CarrierAgentDirections.Code.Arrival, consol.JK_OA_ArrivalCTOAddressInfo);
			AssertEquals(address2.PK, consol.JK_OA_ArrivalCTOAddress);

			CTOFromCarrierDefaulter.SetCTOFromCarrier(consol, "USMEM", OrgConstants.CarrierAgentDirections.Code.Arrival, consol.JK_OA_ArrivalCTOAddressInfo);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress);
		}

		public void TestSetCTOFromCarrierConsolVessel()
		{
			OrgAddress address1 = Factory.New<OrgAddress>();
			OrgAddress address2 = Factory.New<OrgAddress>();

			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgCarrierAppointedAgentPorts agentPort = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = address1.PK;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			CTOFromCarrierDefaulter.SetCTOFromCarrier(consol, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Departure, consol.JK_OA_DepartureCTOAddressInfo);
			AssertEquals(address1.PK, consol.JK_OA_DepartureCTOAddress);

			OrgCarrierAppointedAgentPorts agentPort2 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			agentPort2.O5_PortOrCountry = "AUSYD";
			agentPort2.O5_OA_AgentOfficeAddress = address2.PK;
			agentPort2.O5_TerminalType = StevedoreTerminalType.Codes.ROROTerminal;

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_VesselType = Core.Constants.VesselType.CarCarringVessel;
			vessel.RV_Name = "NII1502";
			consol.Transports.MostInterestingTransport.JW_Vessel = vessel.RV_FK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			CTOFromCarrierDefaulter.SetCTOFromCarrier(consol, "AUSYD", OrgConstants.CarrierAgentDirections.Code.Departure, consol.JK_OA_DepartureCTOAddressInfo);
			AssertEquals(address2.PK, consol.JK_OA_DepartureCTOAddress);
		}
	}
}
