using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public static class OnlineFlightMatchingValidationHelper
	{
		public static List<string> GetOnlineFlightMatchStatusWarnings(IFlightInformationProvider parent)
		{
			if (!parent.MatchedSchedule.IsEmpty
				&& (parent.OnlineScheduleStatus == Constants.FlightScheduleStatus.PartiallyMatched
					|| parent.OnlineScheduleStatus == Constants.FlightScheduleStatus.Unmatched))
			{
				return new List<string>() { GetSpecificWarningsForMatchedSchedule(parent) }
					.AddFlightServerConnectionSuppressionWarning(parent);
			}
			else
			{
				return GetGenericWarningForMatchStatus(parent)
					.AddFlightServerConnectionSuppressionWarning(parent);
			}
		}

		static string GetSpecificWarningsForMatchedSchedule(IFlightInformationProvider parent)
		{
			var warningBuilder = new ZStringBuilder();

			var loadPort = parent.LoadPort?.RL_IATA ?? string.Empty;
			var dischargePort = parent.DiscPort?.RL_IATA ?? string.Empty;

			warningBuilder.AppendLine(Res.GetString("cf56147d-0498-40d8-909f-4de3a1c6cb58", "The following flight details do not match"));

			if (loadPort != parent.MatchedSchedule.Origin)
			{
				warningBuilder.AppendLine(Res.GetString("9d6e0066-c087-48b9-8ad0-17b1a9ab09bb", "Load Port: {0}", parent.MatchedSchedule.Origin));
			}
			if (parent.ETD.Date != parent.MatchedSchedule.DepartureDate)
			{
				warningBuilder.AppendLine(Res.GetString("52cd3a26-3753-4583-8cfa-b12cf8edfda6", "Departure Date: {0}", parent.MatchedSchedule.DepartureDate.ToShortDateString()));
			}
			if (dischargePort != parent.MatchedSchedule.Destination)
			{
				warningBuilder.AppendLine(Res.GetString("0bfd491c-a143-47c9-ab43-52b710ec45ae", "Discharge Port: {0}", parent.MatchedSchedule.Destination));
			}
			if (parent.ETA.Date != parent.MatchedSchedule.ArrivalDate)
			{
				warningBuilder.AppendLine(Res.GetString("79a3d385-48ca-49a2-b8a5-840022c7559f", "Arrival Date: {0}", parent.MatchedSchedule.ArrivalDate.ToShortDateString()));
			}

			return warningBuilder.ToString();
		}

		public static List<string> GetGenericWarningForMatchStatus(IFlightInformationProvider parent)
		{
			var warnings = new List<string>();

			switch (parent.OnlineScheduleStatus)
			{
				case Constants.FlightScheduleStatus.PartiallyMatched:
					warnings.Add(Res.GetString("991505d8-a3f5-4bab-a112-11a0e4cdc3d0", "This Air Routing Leg only partially matches to a Global Flight Schedule flight. This flight may not be fully tracked correctly. You may want to check Global Flight Schedules for correct flight details by using \"Import Global Flights\" to search and import most appropriate flight."));
					break;

				case Constants.FlightScheduleStatus.Unmatched:
					if (!string.IsNullOrEmpty(parent.MatchErrorMessage))
					{
						warnings.Add(parent.MatchErrorMessage);
					}
					warnings.Add(Res.GetString("ab3faef2-fd67-4ea2-899a-b0b11e43fc75", "This Air Routing Leg does not match any of the Global Flight Schedule flights. This flight may not be tracked correctly. You may want to check Global Flight Schedules for correct flight details by using \"Import Global Flights\" to search and import most appropriate flight."));
					break;
			}

			return warnings;
		}

		static List<string> AddFlightServerConnectionSuppressionWarning(this List<string> warnings, IFlightInformationProvider parent)
		{
			if (parent.Matcher?.ServiceRequestManager?.IsSuppressed() == true)
			{
				warnings.Add(Res.GetString("98b9c9df-0d16-4ed1-b4ac-fb5f9f4acc7a", "Cannot connect to flight web service. Please wait for {0} minutes and try again.", FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.Value));
			}

			return warnings;
		}
	}
}
