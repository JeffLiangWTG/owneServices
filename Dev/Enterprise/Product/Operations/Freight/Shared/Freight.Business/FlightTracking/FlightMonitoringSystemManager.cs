using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using TransportParentTypes = Enterprise.Core.Constants.TransportParentTypes;

namespace Enterprise.Freight.Business
{
	public static class FlightMonitoringSystemManager
	{
		public static void UpdateFlightSubscriptionEvent(JobSailing sailing)
		{
			_ = sailing ?? throw new ArgumentNullException(nameof(sailing));

			if (FreightDataRegistry.Instance.AWBTracking.Value)
			{
				new FlightMonitoringSailingProvider(sailing).UpdateFlightSubscriptionEvent();
			}
		}

		public static void UpdateFlightSubscriptionEvent(Transport transport)
		{
			_ = transport ?? throw new ArgumentNullException(nameof(transport));

			if (IsApplicableForSubscription(transport))
			{
				new FlightMonitoringTransportProvider(transport).UpdateFlightSubscriptionEvent();
			}
		}

		public static void UpdateFlightSubscriptionEvent(CommonConsol consol)
		{
			_ = consol ?? throw new ArgumentNullException(nameof(consol));

			if (FreightDataRegistry.Instance.AWBTracking.Value)
			{
				new FlightMonitoringConsolProvider(consol).UpdateFlightSubscriptionEvent();
			}
		}

		public static void UpdateFlightSubscriptionEventAfterTransportRemoved(Transport transport)
		{
			_ = transport ?? throw new ArgumentNullException(nameof(transport));

			if (IsApplicableForSubscription(transport))
			{
				new FlightMonitoringTransportProvider(transport).TransportRemoved();
			}
		}

		internal static void RemoveSubscripionLogIfExists(Logs logs)
		{
			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.SubscriptionRequested.Code);
			logQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, Core.Constants.EventReferenceParameterTypes.AWBAutomation);
			var existingLogs = logs.Find(logQuery);

			foreach (StmALog log in existingLogs)
			{
				if (log.IsInDatabase)
				{
					log.Cancel();
				}
				else
				{
					log.Delete();
				}
			}
		}

		internal static bool AreArrivalAndDepartureDatesRecent(this Transport transport)
		{
			return AreArrivalAndDepartureDatesRecent(transport.JW_ATA, transport.JW_ETA, transport.JW_ATD, transport.JW_ETD);
		}

		internal static bool AreArrivalAndDepartureDatesRecent(this JobSailing jobSailing)
		{
			return AreArrivalAndDepartureDatesRecent(
				jobSailing.Destination?.JB_A_ARV ?? ZDateTime.Empty,
				jobSailing.Destination?.JB_E_ARV ?? ZDateTime.Empty,
				jobSailing.Origin?.JA_A_DEP ?? ZDateTime.Empty,
				jobSailing.Origin?.JA_E_DEP ?? ZDateTime.Empty
			);
		}

		static bool AreArrivalAndDepartureDatesRecent(
			ZDateTime actualArrival,
			ZDateTime estimatedArrival,
			ZDateTime actualDeparture,
			ZDateTime estimatedDeparture)
		{
			bool result = true;
			var arrivalDate = actualArrival.IsEmpty ? estimatedArrival : actualArrival;
			if (!arrivalDate.IsEmpty && arrivalDate.IsValid)
			{
				// https://wisetechacademy.com/search?quickstart=b21c95ee-7e2b-4aed-95d2-3d19b6fdc73c
				// Arrival ETA/ATA is in the future and between today's date and 2 days ago
				result = arrivalDate > 2.DaysAgo();
			}
			else
			{
				var departureDate = actualDeparture.IsEmpty ? estimatedDeparture : actualDeparture;
				if (!departureDate.IsEmpty && departureDate.IsValid)
				{
					// Departure ETD/ATD is in the future or between today's date and 3 days ago
					result = departureDate > 3.DaysAgo();
				}
			}
			return result;
		}

		static bool IsApplicableForSubscription(Transport transport)
		{
			return FreightDataRegistry.Instance.AWBTracking.Value
				&& transport.ParentType != null
				&& transport.Parent != null
				&& SupportedParentTypes.Contains(transport.Parent.TypeCode);
		}

		static List<string> SupportedParentTypes => supportedParentTypes ?? (supportedParentTypes = new List<string>
		{
			TransportParentTypes.Consol,
			TransportParentTypes.Declaration
		});

		[ThreadStatic]
		static List<string> supportedParentTypes;
	}
}
