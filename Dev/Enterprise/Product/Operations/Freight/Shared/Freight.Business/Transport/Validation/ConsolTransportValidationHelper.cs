using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class ConsolTransportValidationHelper
	{
		#region Cut Off Dates

		public static void CheckDepotCutOff(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport.GetParentSafe() as CommonConsol;
			if (consol != null && (consol.JK_ConsolMode == Core.Constants.ContainerModes.LCL || consol.JK_TransportMode == Core.Constants.TransportModes.Air))
			{
				ValidateCutOffDate(propertyInfo, transport, consol);
			}
		}

		public static void CheckTerminalCutOff(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport.GetParentSafe() as CommonConsol;
			if (consol != null && consol.JK_ConsolMode == Core.Constants.ContainerModes.FCL)
			{
				ValidateCutOffDate(propertyInfo, transport, consol);
			}
		}

		static void ValidateCutOffDate(ZPropertyInfo info, Transport transport, CommonConsol consol)
		{
			if (transport.JW_LegOrder == 1 &&
				transport.JW_IsLinked &&
				consol.IsExport() &&
				FreightConfigurationRegistry.Instance.ConsolCutOffDate.Value &&
				info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("51f2b958-77b2-4d9d-a977-fe593b74468e", "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Consolidations -> Consol Cut Off Mandatory."));
			}
		}

		#endregion

		#region CheckLoadPort

		public static void CheckLoadPort(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport?.GetParentSafe() as CommonConsol;
			if (!propertyInfo.HasErrors() &&
				consol != null &&
				RoutingLegLoadsAtPort(transport, (ZString)propertyInfo.Value, consol.JK_RL_NKDischargePort))
			{
				if ((!transport.IsRailOrRoadOrInlandWaterway) || (transport.JW_RL_NKLoadPort != transport.JW_RL_NKDiscPort))
				{
					propertyInfo.AddError(Res.GetString("35d3c308-3728-4f48-bf90-6d128dc29d52", "A routing leg cannot load at the consol's final discharge."));
				}
			}
		}

		#endregion

		#region CheckDiscPort

		public static void CheckDiscPort(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport?.GetParentSafe() as CommonConsol;
			if (!propertyInfo.HasErrors() &&
				consol != null &&
				RoutingLegDischargesAtPort(transport, (ZString)propertyInfo.Value, consol.JK_RL_NKLoadPort))
			{
				if (!transport.IsRailOrRoadOrInlandWaterway || (transport.JW_RL_NKLoadPort != transport.JW_RL_NKDiscPort))
				{
					propertyInfo.AddError(Res.GetString("05735f23-7921-42c2-9682-f61be1f5f361", "A routing leg cannot discharge at the consol's first load."));
				}
			}
		}

		#endregion

		#region Ports Validation Helper

		public static bool RoutingLegLoadsAtPort(Transport transport, ZString loadPort, ZString port)
		{
			if (transport.JW_TransportMode != Constants.TransportModes.Storage
				&& !loadPort.IsEmpty
				&& loadPort == port
				&& (transport.JW_OA_DepartureLocation.IsEmpty || !transport.IsWithinPort()))
			{
				return true;
			}

			return false;
		}

		public static bool RoutingLegDischargesAtPort(Transport transport, ZString discPort, ZString port)
		{
			if (transport.JW_TransportMode != Constants.TransportModes.Storage
				&& !discPort.IsEmpty
				&& discPort == port
				&& (transport.JW_OA_ArrivalLocation.IsEmpty || !transport.IsWithinPort()))
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Check Estimated And Actual Dates

		public static void CheckETD(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport?.GetParentSafe() as CommonConsol;
			if (consol != null)
			{
				if (propertyInfo.Value.IsValid)
				{
					CheckThisLegDepartsAfterPrecedingLegsArrive((ZDateTime)propertyInfo.Value, t => (t.JW_IsLinked && t.Sailing?.Destination != null ? t.Sailing.Destination.JB_E_ARV : t.JW_ETA), propertyInfo, transport, consol);
				}

				ValidateConsolTransportEstimatedDates(consol);
			}
		}

		public static void CheckETA(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport?.GetParentSafe() as CommonConsol;
			if (consol != null)
			{
				if (propertyInfo.Value.IsValid)
				{
					CheckThisDepartsLegBeforeFollowingLegsDepart((ZDateTime)propertyInfo.Value, t => (t.JW_IsLinked && t.Sailing?.Origin != null ? t.Sailing.Origin.JA_E_DEP : t.JW_ETD), propertyInfo, transport, consol);
				}

				ValidateConsolTransportEstimatedDates(consol);
			}
		}

		public static void CheckATD(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport?.GetParentSafe() as CommonConsol;
			if (consol != null)
			{
				if (propertyInfo.Value.IsValid)
				{
					CheckThisLegDepartsAfterPrecedingLegsArrive((ZDateTime)propertyInfo.Value, t => (t.JW_IsLinked && t.Sailing?.Destination != null ? t.Sailing.Destination.JB_A_ARV : t.JW_ATA), propertyInfo, transport, consol);
				}

				ValidateConsolTransportActualDates(consol);
			}
		}

		public static void CheckATA(ZPropertyInfo propertyInfo, Transport transport)
		{
			var consol = transport?.GetParentSafe() as CommonConsol;
			if (consol != null)
			{
				if (propertyInfo.Value.IsValid)
				{
					CheckThisDepartsLegBeforeFollowingLegsDepart((ZDateTime)propertyInfo.Value, t => (t.JW_IsLinked && t.Sailing?.Origin != null ? t.Sailing.Origin.JA_A_DEP : t.JW_ATD), propertyInfo, transport, consol);
				}

				ValidateConsolTransportActualDates(consol);
			}
		}

		static void CheckThisLegDepartsAfterPrecedingLegsArrive(ZDateTime currentDate, Func<Transport, ZDateTime> targetDateGetter, ZPropertyInfo info, Transport transport, CommonConsol consol)
		{
			var precedingLegs = PrecedingTransports(transport, consol);

			foreach (var transportToCompare in precedingLegs)
			{
				var intermediateTransports = IntermediateTransports(transportToCompare, transport, consol);

				var shouldBufferDate = (transportToCompare.IsAir || intermediateTransports.Any(t => t.IsAir))
										&& !intermediateTransports.Any(t => t.IsSea)
										&& transportToCompare.JW_RL_NKDiscPort.CompareTo(transport.JW_RL_NKLoadPort) != 0;

				var dateToCompare = GetDateForLegOrderComparison(targetDateGetter(transportToCompare), shouldBufferDate, -1);

				if (dateToCompare.IsValid && currentDate < dateToCompare)
				{
					info.AddError(Res.GetString("bc02d069-72a1-436f-a026-fd2534317232", "The date order does not reflect the leg order."));
				}
			}
		}

		static void CheckThisDepartsLegBeforeFollowingLegsDepart(ZDateTime currentDate, Func<Transport, ZDateTime> targetDateGetter, ZPropertyInfo info, Transport transport, CommonConsol consol)
		{
			var followingLegs = FollowingTransports(transport, consol);

			foreach (var transportToCompare in followingLegs)
			{
				var intermediateTransports = IntermediateTransports(transport, transportToCompare, consol);

				var shouldBufferDate = (transportToCompare.IsAir || intermediateTransports.Any(t => t.IsAir))
										&& !intermediateTransports.Any(t => t.IsSea)
										&& transportToCompare.JW_RL_NKLoadPort.CompareTo(transport.JW_RL_NKDiscPort) != 0;

				var dateToCompare = GetDateForLegOrderComparison(targetDateGetter(transportToCompare), shouldBufferDate, +1);

				if (dateToCompare.IsValid && currentDate > dateToCompare)
				{
					info.AddError(Res.GetString("a63a3bcd-4198-4436-8387-b2013428ed8a", "The date order does not reflect the leg order."));
				}
			}
		}

		static IEnumerable<Transport> IntermediateTransports(Transport firstTransport, Transport lastTransport, CommonConsol consol)
		{
			var consolTransports = consol?.Transports ?? new ConsolTransportCollection(consol);
			return consolTransports.Cast<Transport>().Where(t => IsFollowingInCollection(firstTransport, t, consol) && IsPrecedingInCollection(lastTransport, t, consol));
		}

		static IEnumerable<Transport> FollowingTransports(Transport firstTransport, CommonConsol consol)
		{
			var consolTransports = consol?.Transports ?? new ConsolTransportCollection(consol);
			return consolTransports.Cast<Transport>().Where(t => IsFollowingInCollection(firstTransport, t, consol));
		}

		static IEnumerable<Transport> PrecedingTransports(Transport lastTransport, CommonConsol consol)
		{
			var consolTransports = consol?.Transports ?? new ConsolTransportCollection(consol);
			return consolTransports.Cast<Transport>().Where(t => IsPrecedingInCollection(lastTransport, t, consol));
		}

		static bool IsFollowingInCollection(Transport transport1, Transport transport2, CommonConsol consol)
		{
			var consolTransports = consol?.Transports;
			if (consolTransports != null)
			{
				if (consolTransports.Contains(transport1) && consolTransports.Contains(transport2))
				{
					return (transport1.JW_LegOrder - transport2.JW_LegOrder) < 0;
				}
			}

			return false;
		}

		static bool IsPrecedingInCollection(Transport transport1, Transport transport2, CommonConsol consol)
		{
			var consolTransports = consol?.Transports;
			if (consolTransports != null)
			{
				if (consolTransports.Contains(transport1) && consolTransports.Contains(transport2))
				{
					return (transport1.JW_LegOrder - transport2.JW_LegOrder) > 0;
				}
			}

			return false;
		}

		static ZDateTime GetDateForLegOrderComparison(ZDateTime date, bool shouldBufferDate, ZInt timeDifference)
		{
			var result = ZDateTime.Empty;

			if (date.IsValid)
			{
				result = shouldBufferDate ? date.AddDays(timeDifference) : date;
			}

			return result;
		}

		static void ValidateConsolTransportEstimatedDates(CommonConsol consol)
		{
			var consolTransports = consol.Transports;
			if (consolTransports != null)
			{
				foreach (Transport transport in consolTransports)
				{
					if (transport.JW_IsLinked && transport.Sailing?.Destination != null)
					{
						transport.Sailing.Destination.Validation.ValidateJB_E_ARV();
					}
					else
					{
						transport.Validation.ValidateJW_ETA();
					}

					if (transport.JW_IsLinked && transport.Sailing?.Origin != null)
					{
						transport.Sailing.Origin.Validation.ValidateJA_E_DEP();
					}
					else
					{
						transport.Validation.ValidateJW_ETD();
					}
				}
			}
		}

		static void ValidateConsolTransportActualDates(CommonConsol consol)
		{
			var consolTransports = consol?.Transports;
			if (consolTransports != null)
			{
				foreach (Transport transport in consolTransports)
				{
					if (transport.JW_IsLinked && transport.Sailing?.Destination != null)
					{
						transport.Sailing.Destination.Validation.ValidateJB_A_ARV();
					}
					else
					{
						transport.Validation.ValidateJW_ATA();
					}

					if (transport.JW_IsLinked && transport.Sailing?.Origin != null)
					{
						transport.Sailing.Origin.Validation.ValidateJA_A_DEP();
					}
					else
					{
						transport.Validation.ValidateJW_ATD();
					}
				}
			}
		}

		#endregion
	}
}
