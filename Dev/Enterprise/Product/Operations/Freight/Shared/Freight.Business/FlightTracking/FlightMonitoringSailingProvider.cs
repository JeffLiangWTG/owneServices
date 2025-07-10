using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	internal class FlightMonitoringSailingProvider
	{
		public FlightMonitoringSailingProvider(JobSailing sailing)
		{
			this.sailing = sailing;
		}
		readonly JobSailing sailing;

		public void UpdateFlightSubscriptionEvent()
		{
			if (IsApplicableForFlightSubscription)
			{
				if (sailing.IsInDatabase)
				{
					FlightMonitoringSystemManager.RemoveSubscripionLogIfExists(sailing.Logs);
				}

				if (IsAllDataValid)
				{
					sailing.Logs.CreateRecreateOrUpdateEventLog(
						AutoEvents.SubscriptionRequested,
						EstimateActual.Actual,
						ZDateTimeOffset.Now,
						ZString.Empty,
						GetFlightSubscriptionEventParameters().ToArray());
				}
			}
		}

		bool IsAllDataValid
		{
			get
			{
				var result = false;
				var voyage = sailing.Voyage;
				if (voyage != null && voyage.IsAir)
				{
					result = !voyage.JV_VoyageFlight.IsEmpty
							&& (sailing.Origin != null
								&& sailing.Origin.PortOfLoading != null
								&& !sailing.Origin.PortOfLoading.RL_IATA.IsEmpty
								&& (sailing.Origin.JA_E_DEP.IsValid || sailing.Origin.JA_A_DEP.IsValid))
							&& (sailing.Destination != null
								&& sailing.Destination.PortOfDischarge != null
								&& !sailing.Destination.PortOfDischarge.RL_IATA.IsEmpty);
				}
				return result;
			}
		}

		bool IsApplicableForFlightSubscription
		{
			get
			{
				var result = false;
				var voyage = sailing.Voyage;

				if (voyage != null && voyage.IsAir && sailing.AreArrivalAndDepartureDatesRecent())
				{
					result = (!voyage.IsInDatabase
							|| voyage.JV_VoyageFlightInfo.HasChanges)
							|| (sailing.Origin != null && (!sailing.Origin.IsInDatabase
								|| sailing.Origin.JA_RL_NKPortOfLoadingInfo.HasChanges
								|| sailing.Origin.JA_E_DEPInfo.HasChanges
								|| sailing.Origin.JA_A_DEPInfo.HasChanges))
							|| (sailing.Destination != null
								&& (!sailing.Destination.IsInDatabase
									|| sailing.Destination.JB_RL_NKPortOfDischargeInfo.HasChanges
									|| sailing.Destination.JB_E_ARVInfo.HasChanges
									|| sailing.Destination.JB_A_ARVInfo.HasChanges));
				}
				return result;
			}
		}

		Dictionary<string, string> GetFlightSubscriptionEventParameters()
		{
			var parameters = new Dictionary<string, string>();
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Core.Constants.EventReferenceParameterTypes.AWBAutomation;
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber] = sailing.Voyage.JV_VoyageFlight;
			parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.FlightDate] = sailing.Origin.JA_A_DEP.IsEmpty
				? sailing.Origin.JA_E_DEP.ToISO8601ShortDateString()
				: sailing.Origin.JA_A_DEP.ToISO8601ShortDateString();

			return parameters;
		}
	}
}
