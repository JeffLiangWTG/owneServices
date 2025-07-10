using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal static class CMMTransportLocator
	{
		public static JobVoyage FindBestGuessVoyage(AgencyShipment shipment, string port, string movementType)
		{
			Argument.NotNull(shipment, "shipment");
			Argument.NotNull(port, "port");
			Argument.NotNull(movementType, "movementType");

			Transport transport;

			switch (movementType)
			{
				case ContainerMovementTypes.Codes.Discharge:
				case ContainerMovementTypes.Codes.WharfGateOut:
					transport = shipment.Transports.FindTransportByDischargePort(port);
					break;

				case ContainerMovementTypes.Codes.Load:
				case ContainerMovementTypes.Codes.WharfGateIn:
					transport = shipment.Transports.FindTransportByLoadPort(port);
					break;

				case ContainerMovementTypes.Codes.YardGateIn:
					transport = GetTransportByEndPort(shipment, port, false, true);
					break;

				case ContainerMovementTypes.Codes.YardGateOut:
					transport = GetTransportByEndPort(shipment, port, true, false);
					break;

				case ContainerMovementTypes.Codes.DepotGateIn:
				case ContainerMovementTypes.Codes.DepotGateOut:
					transport = GetTransportByEndPort(shipment, port, false, false);
					break;

				default:
					transport = null;
					break;
			}

			if (transport != null &&
				transport.JW_TransportMode == Constants.TransportModes.Sea &&
				transport.JW_IsLinked)
			{
				return transport.Voyage;
			}
			else
			{
				return null;
			}
		}

		static Transport GetTransportByEndPort(AgencyShipment shipment, string port, bool favorOrigin, bool favorDestination)
		{
			if (port.Length < 2)
			{
				return null;
			}

			int weight = 0;
			string origin = shipment.JS_RL_NKOrigin;
			string destination = shipment.JS_RL_NKDestination;

			if (origin != null && origin.Length >= 2)
			{
				if (origin == port)
				{
					weight -= 4;
				}
				else if (origin.Substring(0, 2) == port.Substring(0, 2))
				{
					weight -= 2;
				}

				if (favorOrigin)
				{
					weight -= 1;
				}
			}

			if (destination != null && destination.Length >= 2)
			{
				if (destination == port)
				{
					weight += 4;
				}
				else if (destination.Substring(0, 2) == port.Substring(0, 2))
				{
					weight += 2;
				}

				if (favorDestination)
				{
					weight += 1;
				}
			}

			if (weight < 0)
			{
				Transport[] routing = shipment.TransportsIncludingRelated.ToArray<Transport>();
				MovementLegComparer.SortMovementLegsByPorts(routing);

				for (int i = 0; i < routing.Length; i++)
				{
					Transport transport = routing[i];
					if (transport.JW_TransportMode == Constants.TransportModes.Sea)
					{
						return transport;
					}
				}
			}
			else if (weight > 0)
			{
				Transport[] routing = shipment.TransportsIncludingRelated.ToArray<Transport>();
				MovementLegComparer.SortMovementLegsByPorts(routing);

				for (int i = routing.Length - 1; i >= 0; i--)
				{
					Transport transport = routing[i];
					if (transport.JW_TransportMode == Constants.TransportModes.Sea)
					{
						return transport;
					}
				}
			}

			return null;
		}
	}
}


