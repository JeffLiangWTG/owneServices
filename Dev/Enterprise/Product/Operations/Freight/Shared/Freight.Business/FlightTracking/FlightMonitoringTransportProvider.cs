using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	internal class FlightMonitoringTransportProvider
	{
		public FlightMonitoringTransportProvider(Transport transport)
		{
			this.transport = transport;
		}
		readonly Transport transport;

		public void UpdateFlightSubscriptionEvent()
		{
			if (IsApplicableForFlightSubscription)
			{
				var transportParent = (EnterpriseBusinessObject)transport.Parent;
				if (transportParent.IsInDatabase)
				{
					FlightMonitoringSystemManager.RemoveSubscripionLogIfExists(transportParent.Logs);
				}

				if (IsAllDataValid)
				{
					var sailingLog = GetExistingSailingLog();

					if (sailingLog == null || sailingLog.IsInDatabase)
					{
						CreateSubscriptionLogRecord(transportParent);
					}
					else
					{
						FlightMonitoringSystemManager.RemoveSubscripionLogIfExists(transportParent.Logs);
					}
				}
			}
		}

		public void TransportRemoved()
		{
			var transportParent = (EnterpriseBusinessObject)transport.Parent;
			if (transportParent.IsInDatabase && transport.IsInDatabase && transport.IsAir)
			{
				CreateSubscriptionLogRecord(transportParent);
			}
		}

		void CreateSubscriptionLogRecord(EnterpriseBusinessObject transportParent)
		{
			transportParent.Logs.CreateRecreateOrUpdateEventLog(
				AutoEvents.SubscriptionRequested,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetFlightSubscriptionEventParameters().ToArray());
		}

		bool IsAllDataValid => !transport.JW_VoyageFlight.IsEmpty
			&& ((transport.LoadPort != null && !transport.LoadPort.RL_IATA.IsEmpty && IsDepartureDateValid)
			|| (transport.DiscPort != null && !transport.DiscPort.RL_IATA.IsEmpty && IsArrivalDateValid))
			&& MAWBNumberIsValid;

		bool IsDepartureDateValid =>
			(!transport.JW_ETD.IsEmpty && transport.JW_ETD.IsValid)
			|| (!transport.JW_ATD.IsEmpty && transport.JW_ATD.IsValid);

		bool IsArrivalDateValid =>
			(!transport.JW_ETA.IsEmpty && transport.JW_ETA.IsValid)
			|| (!transport.JW_ATA.IsEmpty && transport.JW_ATA.IsValid);

		bool IsApplicableForFlightSubscription
		{
			get
			{
				bool result = false;

				if (transport.IsAir && transport.AreArrivalAndDepartureDatesRecent())
				{
					if (transport.JW_IsLinked)
					{
						result = !transport.IsInDatabase
								|| transport.JW_JXInfo.HasChanges
								|| MAWBNumberHasChanges;
					}
					else
					{
						result = !transport.IsInDatabase
							   || transport.JW_JXInfo.HasChanges
							   || transport.JW_VoyageFlightInfo.HasChanges
							   || transport.JW_RL_NKLoadPortInfo.HasChanges
							   || transport.JW_RL_NKDiscPortInfo.HasChanges
							   || transport.JW_ETDInfo.HasChanges
							   || transport.JW_ETAInfo.HasChanges
							   || transport.JW_ATDInfo.HasChanges
							   || transport.JW_ATAInfo.HasChanges
							   || MAWBNumberHasChanges;
					}
				}

				return result;
			}
		}

		StmALog GetExistingSailingLog()
		{
			StmALog result = null;
			if (transport.Sailing != null)
			{
				var parameters = new Dictionary<string, string>()
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Core.Constants.EventReferenceParameterTypes.AWBAutomation,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber] = transport.JW_VoyageFlight,
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.FlightDate] = GetFlightDate()
				};

				var eventReference = StmALog.GenerateEventReference("", parameters);
				result = transport.Sailing.Logs.MostRecentLogByEventTime(AutoEvents.SubscriptionRequested, eventReference);
			}

			return result;
		}

		Dictionary<string, string> GetFlightSubscriptionEventParameters()
		{
			return new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Core.Constants.EventReferenceParameterTypes.AWBAutomation,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber] = MAWBNumber
			};
		}

		string GetFlightDate()
		{
			return transport.JW_ETD.IsEmpty || !transport.JW_ETD.IsValid ? transport.JW_ETA.ToISO8601ShortDateString() : transport.JW_ETD.ToISO8601ShortDateString();
		}

		bool MAWBNumberIsValid => MasterBillValidator.GetMAWBFormatValidMessage(MAWBNumber, transport.Factory).IsEmpty;

		bool MAWBNumberHasChanges => (transport.Parent as ITransportParent)?.TransportSupporter?.BillOfLadingHasChanges ?? false;

		ZString MAWBNumber => (transport.Parent as ITransportParent)?.TransportSupporter?.BillOfLading ?? ZString.Empty;
	}
}
