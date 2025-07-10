using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Business
{
	public static class CommonConsolComparisonHelper
	{
		public enum ConsolsPortComparisonResult
		{
			HasSameDischargePort,
			HasSameLoadPort,
			HasSameDischargeAndLoadPort,
			InOnePortWithTimeOverlap,
			AcceptableSameLoadOrDischargePort,
			DifferentDischargeAndLoadPort
		}

		public static ConsolsPortComparisonResult CompareConsolsLoadAndDischargePorts(CommonConsol consolA, CommonConsol consolB, bool ignoreDomesticRailOrRoad = true)
		{
			if (!consolA.HasSameDischargePortWith(consolB) && !consolA.HasSameLoadPortWith(consolB) &&
				!consolA.HasSameDischargePortWithTransportsOf(consolB) && !consolA.HasSameLoadPortWithTransportsOf(consolB))
			{
				return ConsolsPortComparisonResult.DifferentDischargeAndLoadPort;
			}

			if (!consolA.IsDomesticRailOrRoad() && !consolB.IsDomesticRailOrRoad())
			{
				return GetSimilarityType(consolA, consolB);
			}

			if (ignoreDomesticRailOrRoad)
			{
				if (!IsInOnePortConsol(consolA) || !IsInOnePortConsol(consolB))
				{
					return ConsolsPortComparisonResult.AcceptableSameLoadOrDischargePort;
				}

				if (consolA.HasTimeOverlapWith(consolB))
				{
					return ConsolsPortComparisonResult.InOnePortWithTimeOverlap;
				}
				else
				{
					return ConsolsPortComparisonResult.AcceptableSameLoadOrDischargePort;
				}
			}
			else
			{
				return GetSimilarityType(consolA, consolB);
			}
		}

		static ConsolsPortComparisonResult GetSimilarityType(CommonConsol consolA, CommonConsol consolB)
		{
			var hasSameTransportDischarge = consolA.HasSameDischargePortWithTransportsOf(consolB);
			var hasSameTransportLoad = consolA.HasSameLoadPortWithTransportsOf(consolB);

			if ((consolA.HasSameDischargePortWith(consolB) || hasSameTransportDischarge) &&
				(consolA.HasSameLoadPortWith(consolB) || hasSameTransportLoad))
			{
				return ConsolsPortComparisonResult.HasSameDischargeAndLoadPort;
			}
			else if (consolA.HasSameDischargePortWith(consolB) || hasSameTransportDischarge)
			{
				return ConsolsPortComparisonResult.HasSameDischargePort;
			}
			else if (consolA.HasSameLoadPortWith(consolB) || hasSameTransportLoad)
			{
				return ConsolsPortComparisonResult.HasSameLoadPort;
			}

			return ConsolsPortComparisonResult.DifferentDischargeAndLoadPort;
		}

		static bool HasTimeOverlapWith(this CommonConsol consol, CommonConsol anotherConsol)
		{
			return consol.HasEstimationTimeOverlapWith(anotherConsol) || consol.HasActualTimeOverlapWith(anotherConsol);
		}

		static bool HasEstimationTimeOverlapWith(this CommonConsol consol, CommonConsol anotherConsol)
		{
			if (consol.Transports.MostInterestingTransport.JW_ETD.IsEmpty ||
				anotherConsol.Transports.MostInterestingTransport.JW_ETD.IsEmpty)
			{
				return false;
			}
			return ZDateTime.Overlaps(
				consol.Transports.MostInterestingTransport.JW_ETD,
				consol.Transports.MostInterestingTransport.JW_ETA,
				anotherConsol.Transports.MostInterestingTransport.JW_ETD,
				anotherConsol.Transports.MostInterestingTransport.JW_ETA);
		}

		static bool HasActualTimeOverlapWith(this CommonConsol consol, CommonConsol anotherConsol)
		{
			if (consol.Transports.MostInterestingTransport.JW_ATD.IsEmpty ||
				anotherConsol.Transports.MostInterestingTransport.JW_ATD.IsEmpty)
			{
				return false;
			}
			return ZDateTime.Overlaps(
				consol.Transports.MostInterestingTransport.JW_ATD,
				consol.Transports.MostInterestingTransport.JW_ATA,
				anotherConsol.Transports.MostInterestingTransport.JW_ATD,
				anotherConsol.Transports.MostInterestingTransport.JW_ATA);
		}

		static bool IsInOnePortConsol(CommonConsol consol)
		{
			return !consol.JK_RL_NKLoadPort.IsEmpty && consol.JK_RL_NKLoadPort == consol.JK_RL_NKDischargePort;
		}

		static bool HasSameLoadPortWith(this CommonConsol consol, CommonConsol anotherConsol)
		{
			return !consol.JK_RL_NKLoadPort.IsEmpty && consol.JK_RL_NKLoadPort == anotherConsol.JK_RL_NKLoadPort;
		}

		static bool HasSameDischargePortWith(this CommonConsol consolA, CommonConsol anotherConsol)
		{
			return !consolA.JK_RL_NKDischargePort.IsEmpty && consolA.JK_RL_NKDischargePort == anotherConsol.JK_RL_NKDischargePort;
		}

		static bool HasSameLoadPortWithTransportsOf(this CommonConsol consol, CommonConsol anotherConsol)
		{
			var consolTransports = consol.Transports;
			var anotherConsolTransports = anotherConsol.Transports;

			foreach (Transport transport in consolTransports)
			{
				if (!anotherConsol.JK_RL_NKLoadPort.IsEmpty && transport.JW_RL_NKLoadPort == anotherConsol.JK_RL_NKLoadPort)
				{
					return true;
				}
			}

			foreach (Transport transport in anotherConsolTransports)
			{
				if (!consol.JK_RL_NKLoadPort.IsEmpty && transport.JW_RL_NKLoadPort == consol.JK_RL_NKLoadPort)
				{
					return true;
				}
			}

			return false;
		}

		static bool HasSameDischargePortWithTransportsOf(this CommonConsol consol, CommonConsol anotherConsol)
		{
			var consolTransports = consol.Transports;
			var anotherConsolTransports = anotherConsol.Transports;

			foreach (Transport transport in consolTransports)
			{
				if (!anotherConsol.JK_RL_NKDischargePort.IsEmpty && transport.JW_RL_NKDiscPort == anotherConsol.JK_RL_NKDischargePort)
				{
					return true;
				}
			}

			foreach (Transport transport in anotherConsolTransports)
			{
				if (!consol.JK_RL_NKDischargePort.IsEmpty && transport.JW_RL_NKDiscPort == consol.JK_RL_NKDischargePort)
				{
					return true;
				}
			}

			return false;
		}
	}
}
