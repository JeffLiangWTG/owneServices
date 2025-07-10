using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class ALPOHelper
	{
		public static Transport GetShipmentTransport(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			return shipment.TransportsIncludingRelated.Cast<Transport>()
				.Where(transport => (transport.IsSea || transport.JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport) && (IsALPOPort(transport.LoadPort) || IsALPOPort(transport.DiscPort)))
				.OrderBy(transport => transport, new ALPOTransportComparer())
				.FirstOrDefault();
		}

		public static CommonConsol GetConsol(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			return shipment.Consols.Cast<CommonConsol>()
				.FirstOrDefault(consol => consol.IsSea && (IsALPOPort(consol.LoadPort) || IsALPOPort(consol.DischargePort)));
		}

		public static bool IsLegacyALPOInterfaceAvailable()
		{
			return ZDateTimeOffset.GetNowFromUNLOCO("DEHAM").Date < new ZDate(2026, 01, 01);
		}

		internal static bool IsALPOPort(RefUNLOCO port)
		{
			return (port != null && (port.RL_Code == "DEHAM" || port.RL_Code == "DEBRE" || port.RL_Code == "DEBRV" || port.RL_Code == "DECUX"));
		}

		internal static RefUNLOCO GetALPOPort(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			RefUNLOCO result = null;
			Transport transport = GetShipmentTransport(shipment);

			if (transport != null)
			{
				if (IsALPOPort(transport.LoadPort))
				{
					result = transport.LoadPort;
				}
				else if (IsALPOPort(transport.DiscPort))
				{
					result = transport.DiscPort;
				}
			}
			else
			{
				if (IsALPOPort(shipment.Origin))
				{
					result = shipment.Origin;
				}
				else if (IsALPOPort(shipment.Destination))
				{
					result = shipment.Destination;
				}
			}

			return result;
		}

		internal static bool IsALPOExport(CommonShipment shipment)
		{
			Transport transport = GetShipmentTransport(shipment);

			return (transport != null && IsALPOPort(transport.LoadPort)) || (shipment != null && IsALPOPort(shipment.Origin));
		}

		internal static bool IsALPOImport(CommonShipment shipment)
		{
			Transport transport = GetShipmentTransport(shipment);

			return (transport != null && IsALPOPort(transport.DiscPort)) || (shipment != null && IsALPOPort(shipment.Destination));
		}

		internal static RefUNLOCO GetOrigin(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			RefUNLOCO result = null;
			Transport transport = GetShipmentTransport(shipment);

			if (transport != null && transport.LoadPort != null)
			{
				result = transport.LoadPort;
			}
			else
			{
				result = shipment.Origin;
			}

			return result;
		}

		internal static RefUNLOCO GetDestination(CommonShipment shipment)
		{
			if (shipment == null)
			{
				return null;
			}

			RefUNLOCO result = null;
			Transport transport = GetShipmentTransport(shipment);

			if (transport != null && transport.DiscPort != null)
			{
				result = transport.DiscPort;
			}
			else
			{
				result = shipment.Destination;
			}

			return result;
		}
	}

	#region Implementation

	class ALPOTransportComparer : IComparer<Transport>
	{
		public int Compare(Transport x, Transport y)
		{
			if (object.ReferenceEquals(x, y))
			{
				return 0;
			}

			int result = 0;
			if (x != null && y != null)
			{
				if (x.JW_TransportType == Constants.TransportPlanningType.MainVessel
					&& y.JW_TransportType != Constants.TransportPlanningType.MainVessel)
				{
					result = -1;
				}
				else if (x.JW_TransportType != Constants.TransportPlanningType.MainVessel
					&& y.JW_TransportType == Constants.TransportPlanningType.MainVessel)
				{
					result = 1;
				}
				else if (x.JW_TransportType == Constants.TransportPlanningType.MainVessel
					&& y.JW_TransportType == Constants.TransportPlanningType.MainVessel)
				{
					if (ALPOHelper.IsALPOPort(x.LoadPort) && ALPOHelper.IsALPOPort(y.LoadPort))
					{
						return 0;
					}
					else if (ALPOHelper.IsALPOPort(x.LoadPort))
					{
						result = -1;
					}
					else if (ALPOHelper.IsALPOPort(y.LoadPort))
					{
						result = 1;
					}
				}
			}

			return result;
		}
	}

	#endregion
}
