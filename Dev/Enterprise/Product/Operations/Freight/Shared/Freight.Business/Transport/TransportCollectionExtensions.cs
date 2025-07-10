using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class TransportCollectionExtensions
	{
		public static void TryToSetTransportType(this ITransportCollection transportCollection)
		{
			UpdateTransportTypes(transportCollection, false);
		}

		public static void UpdateTransportTypes(this ITransportCollection transportCollection, bool recalculateTransportTypesForAllLegs)
		{
			if (transportCollection == null || transportCollection.Count == 0)
			{
				return;
			}

			var transports = transportCollection.Cast<Transport>().ToList();

			if (recalculateTransportTypesForAllLegs)
			{
				RecalculateTransportTypesForAllLegs(transports);
			}
			else if (!SkipTransportLegCalculation(transports))
			{
				TryToSetTransportTypeCore(transports);
			}
		}

		static void RecalculateTransportTypesForAllLegs(IList<Transport> transports)
		{
			if (transports.Count == 1)
			{
				transports[0].JW_TransportType = Constants.TransportPlanningType.MainVessel;
				return;
			}

			var sortedTransports = transports.Where(t => !t.TransportMode.IsEmpty).ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
			var hasInternationalTransport = sortedTransports.Any(IsInternationalTransport);
			var nonEUInternationalPortExist = sortedTransports.Any(x => IsInternationalTransport(x) && (!IsInEU(x.LoadPort) || !IsInEU(x.DiscPort)));
			var foundMain = false;

			foreach (var transport in sortedTransports)
			{
				if (!foundMain)
				{
					if (IsNonFeederSeaTransport(transport) && (!hasInternationalTransport || IsInternationalTransport(transport, nonEUInternationalPortExist)))
					{
						foundMain = true;
						transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
					}
					else
					{
						transport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
					}
				}
				else
				{
					transport.JW_TransportType = Constants.TransportPlanningType.OnForwarding;
				}
			}
		}

		static void TryToSetTransportTypeCore(IList<Transport> transports)
		{
			if (transports.Count == 1 && transports.Any(t => t.JW_TransportType.IsEmpty))
			{
				transports.FirstOrDefault().JW_TransportType = Constants.TransportPlanningType.MainVessel;
			}
			else
			{
				var mainLegs = transports.Where(t => t.JW_TransportType == Constants.TransportPlanningType.MainVessel).ToList();
				var applyEUException = !mainLegs.Any();
				var isAllDomesticSeaLeg = transports.Where(t => t.IsSea).All(t => !IsInternationalTransport(t, applyEUException));
				var oneMainleg = mainLegs.Count == 1 ? mainLegs.First() : null;
				var sortedTransports = transports.Where(t => !t.TransportMode.IsEmpty).ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
				var foundMain = false;

				foreach (var transport in sortedTransports)
				{
					if (!foundMain && transport.IsSea
						&& !(mainLegs.Count > 1 && transport.JW_TransportType != Constants.TransportPlanningType.MainVessel)
						&& ((IsInternationalTransport(transport, applyEUException) && oneMainleg == null) || oneMainleg == transport || isAllDomesticSeaLeg))
					{
						foundMain = true;
						transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
					}
					else
					{
						transport.JW_TransportType = foundMain
									? Constants.TransportPlanningType.OnForwarding
									: Constants.TransportPlanningType.PreCarriage;
					}
				}
			}
		}

		static bool SkipTransportLegCalculation(List<Transport> transports)
		{
			var mainSealLegCount = transports.Count(transport => transport.IsSea && transport.JW_TransportType == Constants.TransportPlanningType.MainVessel);

			return !transports.Any(t => t.IsSea) || mainSealLegCount == 1 && !transports.Any(t => t.JW_TransportType.IsEmpty);
		}

		static bool IsInternationalTransport(Transport transport, bool nonEUInternationalTransportExist)
		{
			_ = transport ?? throw new ArgumentNullException(nameof(transport));

			if (IsInEU(transport.LoadPort) && IsInEU(transport.DiscPort) && nonEUInternationalTransportExist)
			{
				return false;
			}

			return IsInternationalTransport(transport);
		}

		static bool IsInternationalTransport(Transport transport)
		{
			return transport.JW_RL_NKLoadPort.SubstringSafe(0, 2) != transport.JW_RL_NKDiscPort.SubstringSafe(0, 2);
		}

		static bool IsInEU(RefUNLOCO portUnloco)
		{
			return portUnloco?.IsInEU ?? false;
		}

		static bool IsNonFeederSeaTransport(Transport transport)
		{
			return transport.IsSea && !transport.IsFeeder;
		}
	}
}
