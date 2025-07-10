using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class CTOFromCarrierDefaulter
	{
		public static void SetCTOFromCarrier(JobVoyage voyage, ZString port, ZString direction, ZPropertyInfo addressInfo, ZPropertyInfo orgInfo)
		{
			if (!port.IsEmpty && voyage != null && voyage.Line != null)
			{
				var carrierParties = GetCarrierParties(voyage.JV_AirSeaRoad, voyage.Line);
				if (carrierParties != null)
				{
					var terminalType = ZString.Empty;
					if (voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea)
					{
						terminalType = StevedoreTerminalTypeHelper.GetFromVesselType(voyage.Vessel);
					}

					var address = carrierParties.FindAddress(port, terminalType, direction);
					if (address != null)
					{
						addressInfo.Value = address.PK;
					}
					else
					{
						orgInfo.Value = ZGuid.Empty;
					}
				}
			}
		}

		public static void SetCTOFromCarrier(CommonConsol consol, ZString port, ZString direction, ZPropertyInfo addressInfo)
		{
			if (!port.IsEmpty && consol != null && consol.ShippingLine != null)
			{
				var carrierParties = GetCarrierParties(consol.JK_TransportMode, consol.ShippingLine);
				if (carrierParties != null)
				{
					var terminalType = ZString.Empty;
					if (consol.JK_TransportMode == Core.Constants.TransportModes.Sea &&
						consol.Transports.MostInterestingTransport != null &&
						consol.Transports.MostInterestingTransport.Vessel != null)
					{
						terminalType = StevedoreTerminalTypeHelper.GetFromVesselType(
								consol.Transports.MostInterestingTransport.Vessel);
					}

					var address = carrierParties.FindAddress(port, terminalType, direction);
					addressInfo.Value = address != null ? address.PK : ZGuid.Empty;
				}
			}
		}

		public static OrgCarrierAppointedAgentPortsDependentCollection GetCarrierParties(ZString mode, OrgHeader shippingLine)
		{
			OrgCarrierAppointedAgentPortsDependentCollection carrierParties = null;

			switch (mode)
			{
				case Core.Constants.TransportModes.Air:
				case Core.Constants.TransportModes.Mail:
				case Core.Constants.TransportModes.AirSea:
					carrierParties = shippingLine.CarrierAppointedAgentPorts_AirCTO;
					break;

				case Core.Constants.TransportModes.Sea:
				case Core.Constants.TransportModes.SeaAir:
					carrierParties = shippingLine.CarrierAppointedAgentPorts_Stevedore;
					break;

				case Core.Constants.TransportModes.Rail:
					carrierParties = shippingLine.CarrierAppointedAgentPorts_RailHeadDepot;
					break;

				case Core.Constants.TransportModes.Road:
					carrierParties = shippingLine.CarrierAppointedAgentPorts_RoadDepotShed;
					break;
			}

			return carrierParties;
		}
	}
}
