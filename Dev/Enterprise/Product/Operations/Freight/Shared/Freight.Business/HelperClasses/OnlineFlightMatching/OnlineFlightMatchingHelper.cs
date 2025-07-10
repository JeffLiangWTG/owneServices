using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IFlightInformationProvider
	{
		ScheduleInfo MatchedSchedule { get; }
		ZString OnlineScheduleStatus { get; }
		string MatchErrorMessage { get; }
		ZString VoyageFlight { get; }
		RefUNLOCO DiscPort { get; }
		RefUNLOCO LoadPort { get; }
		ZDateTime ETD { get; }
		ZDateTime ETA { get; }
		IS8Matcher Matcher { get; }
	}

	public class OnlineFlightMatchingHelper
	{
		readonly IS8Matcher matcher;

		public OnlineFlightMatchingHelper(BusinessObjectFactory factory = null)
		{
			factory = factory ?? new BusinessObjectFactory();
			matcher = ObjectFactory.Get<IS8Matcher>("IS8Matcher", factory);
		}

		public IS8Matcher Matcher => matcher;

		public (string errorMessage, ScheduleInfo matchedSchedule, string scheduleStatus) MatchAgainstOnlineFlights(IFlightInformationProvider parent)
		{
			var result = (string.Empty, ScheduleInfo.Empty, Constants.FlightScheduleStatus.Unmatched);

			var scheduleToMatch = CreateScheduleInfoToTryMatch(parent);
			if (scheduleToMatch.IsValidToTryMatch())
			{
				var matchResult = matcher.Match(scheduleToMatch);
				result = (matchResult.MatchErrorMessage, matchResult.MatchedSchedule, matchResult.ScheduleStatus);
			}

			return result;
		}

		ScheduleInfo CreateScheduleInfoToTryMatch(IFlightInformationProvider parent)
		{
			if (int.TryParse(Regex.Match(parent.VoyageFlight.SubstringSafe(2), "^([0-9]+)[a-zA-Z]?\\0?$").Groups[1].Value, out int flightNumber))
			{
				var airline = parent.VoyageFlight.SubstringSafe(0, 2);

				var arrivalDate = parent.ETA.Date;
				var departureDate = parent.ETD.Date;

				var loadPort = parent.LoadPort?.RL_IATA ?? string.Empty;
				var dischargePort = parent.DiscPort?.RL_IATA ?? string.Empty;

				return new ScheduleInfo(airline, flightNumber, loadPort, departureDate, dischargePort, arrivalDate);
			}

			return ScheduleInfo.Empty;
		}
	}
}
