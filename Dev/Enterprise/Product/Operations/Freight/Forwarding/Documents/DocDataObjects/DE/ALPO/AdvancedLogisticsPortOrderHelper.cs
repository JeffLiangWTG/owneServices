using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	static class AdvancedLogisticsPortOrderHelper
	{
		public static Freight.Business.Transport GetMainTransport(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			return consol.Transports.Cast<Freight.Business.Transport>()
				.Where(transport => transport.IsSea && (IsALPOPort(transport.LoadPort) || IsALPOPort(transport.DiscPort)))
				.OrderBy(transport => transport, new ALPOTransportComparer())
				.FirstOrDefault();
		}

		public static RefUNLOCO GetALPOPort(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			RefUNLOCO result = null;
			var transport = GetMainTransport(consol);

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
				if (IsALPOPort(consol.LoadPort))
				{
					result = consol.LoadPort;
				}
				else if (IsALPOPort(consol.DischargePort))
				{
					result = consol.DischargePort;
				}
			}

			return result;
		}

		public static RefUNLOCO GetOrigin(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			RefUNLOCO result = null;
			Freight.Business.Transport transport = GetMainTransport(consol);

			if (transport != null && transport.LoadPort != null)
			{
				result = transport.LoadPort;
			}
			else
			{
				result = consol.LoadPort;
			}

			return result;
		}

		public static RefUNLOCO GetDestination(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			RefUNLOCO result = null;
			Freight.Business.Transport transport = GetMainTransport(consol);

			if (transport != null && transport.DiscPort != null)
			{
				result = transport.DiscPort;
			}
			else
			{
				result = consol.DischargePort;
			}

			return result;
		}
		public static bool IsALPOExport(ForwardingConsol consol)
		{
			Freight.Business.Transport transport = GetMainTransport(consol);
			return (transport != null && IsALPOPort(transport.LoadPort)) || (consol != null && IsALPOPort(consol.LoadPort));
		}

		internal static bool IsALPOPort(RefUNLOCO port)
		{
			return (port != null && (port.RL_Code == "DEBRE" || port.RL_Code == "DEBRV" || port.RL_Code == "DECUX" || port.RL_Code == "DEWVN"));
		}
	}

	sealed class ALPOTransportComparer : IComparer<Freight.Business.Transport>
	{
		public int Compare(Freight.Business.Transport x, Freight.Business.Transport y)
		{
			if (object.ReferenceEquals(x, y))
			{
				return 0;
			}

			int result = 0;
			if (x != null && y != null)
			{
				if (x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel
					&& y.JW_TransportType != Core.Constants.TransportPlanningType.MainVessel)
				{
					result = -1;
				}
				else if (x.JW_TransportType != Core.Constants.TransportPlanningType.MainVessel
					&& y.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
				{
					result = 1;
				}
				else if (x.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel
					&& y.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
				{
					if (AdvancedLogisticsPortOrderHelper.IsALPOPort(x.LoadPort) && AdvancedLogisticsPortOrderHelper.IsALPOPort(y.LoadPort))
					{
						return 0;
					}
					else if (AdvancedLogisticsPortOrderHelper.IsALPOPort(x.LoadPort))
					{
						result = -1;
					}
					else if (AdvancedLogisticsPortOrderHelper.IsALPOPort(y.LoadPort))
					{
						result = 1;
					}
				}
			}

			return result;
		}
	}
}
