using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlightScheduleStatus = Enterprise.Core.Constants.FlightScheduleStatus;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class S8Matcher : IS8Matcher
	{
		IServiceRequestManager serviceRequestManager;

		public IServiceRequestManager ServiceRequestManager
		{
			get
			{
				serviceRequestManager ??= new S8ServiceRequestManager();
				return serviceRequestManager;
			}

			set
			{
				serviceRequestManager = value;
			}
		}

		public virtual IS8MatchResult Match(ScheduleInfo scheduleToMatch)
		{
			var internalMatcher = new S8MatcherInternal(scheduleToMatch, ShouldReportError(), ServiceRequestManager);
			return internalMatcher.Match();
		}

		protected virtual bool ShouldReportError()
		{
			if (!Globals.IsTest)
			{
				return true;
			}
			return false;
		}
	}

	class S8MatcherInternal
	{
		readonly string airline;
		readonly int flightNumber;
		readonly ZDate departureDate;
		readonly ZDate arrivalDate;
		readonly string origin;
		readonly string destination;
		readonly IServiceRequestManager requestManager;

		S8MatchResult Result { get; set; }
		readonly bool shouldReportError;

		public S8MatcherInternal(ScheduleInfo scheduleToMatch, bool shouldReportError, IServiceRequestManager requestManager)
		{
			airline = scheduleToMatch.Carrier.RemoveDiacritics().ConvertToWesternEuropeanCharacters().ToUpperInvariant();
			flightNumber = scheduleToMatch.FlightNumber;
			departureDate = scheduleToMatch.DepartureDate;
			arrivalDate = scheduleToMatch.ArrivalDate;
			origin = scheduleToMatch.Origin.ToUpperInvariant();
			destination = scheduleToMatch.Destination.ToUpperInvariant();
			this.shouldReportError = shouldReportError;
			this.requestManager = requestManager;
		}

		protected virtual IS8Client GetNewClient()
		{
			return new S8Client(null, requestManager);
		}

		public IS8MatchResult Match()
		{
			Result = new S8MatchResult
			{
				ScheduleStatus = FlightScheduleStatus.Unknown,
				MatchErrorMessage = ""
			};

			if (departureDate.IsValid || arrivalDate.IsValid)
			{
				var successful = MatchWithRequestDate(departureDate);
				if (!successful && departureDate != arrivalDate && arrivalDate.IsValid)
				{
					Result.MatchErrorMessage = "";
					MatchWithRequestDate(arrivalDate);
				}
			}

			return Result;
		}

		// return true: if a matching schedule is found
		bool MatchWithRequestDate(ZDate requestDate)
		{
			var result = MatchWithRequestDateCore(requestDate);

			var matchedSchedule = Result.MatchedSchedule;

			if (!matchedSchedule.IsEmpty)
			{
				var originMatched = origin == matchedSchedule.Origin
					&& departureDate == matchedSchedule.DepartureDate;

				var destinationMatched = destination == matchedSchedule.Destination
					&& arrivalDate == matchedSchedule.ArrivalDate;

				if (originMatched && destinationMatched)
				{
					Result.ScheduleStatus = FlightScheduleStatus.Matched;
				}
				else if (originMatched || destinationMatched)
				{
					Result.ScheduleStatus = FlightScheduleStatus.PartiallyMatched;
				}
				else
				{
					Result.ScheduleStatus = FlightScheduleStatus.Unmatched;
				}
			}

			return result;
		}

		// return true: if a matching schedule is found
		bool MatchWithRequestDateCore(ZDate requestDate)
		{
			var scheduleInfos = GetScheduleInfos(requestDate);

			if (scheduleInfos == null)
			{
				return false;
			}

			if (scheduleInfos.Count == 0)
			{
				if (Result.ScheduleStatus == FlightScheduleStatus.Unknown)
				{
					Result.ScheduleStatus = FlightScheduleStatus.Unmatched;
				}

				return false;
			}

			if (scheduleInfos.Count == 1)
			{
				TryToMatch(scheduleInfos);
				return true;
			}

			if (MatchScheduleInfos(scheduleInfos, out bool foundBestMatched))
			{
				return true;
			}

			if (scheduleInfos.Any(scheduleInfo => scheduleInfo.Origin == origin || scheduleInfo.Destination == destination))
			{
				if (MatchWithVariantRequestDates(requestDate))
				{
					return true;
				}
			}

			return false;
		}

		// return true: if a matching schedule is found
		bool MatchWithVariantRequestDates(ZDate requestDate)
		{
			var result = false;

			foreach (var dayOffset in new[] { -1, -2 })
			{
				var variantRequestDate = requestDate.AddDays(dayOffset);
				var scheduleInfos = GetScheduleInfosCore(variantRequestDate);

				if (scheduleInfos != null)
				{
					result |= MatchScheduleInfos(scheduleInfos, out bool foundBestMatched);

					if (foundBestMatched)
					{
						return true;
					}
				}
			}

			return result;
		}

		// return true: if a matching schedule is found
		// out bool foundBestMatched: indicates whether it is the best matching schedule
		bool MatchScheduleInfos(IEnumerable<ScheduleInfo> scheduleInfos, out bool foundBestMatched)
		{
			if (scheduleInfos.Any(scheduleInfo => origin == scheduleInfo.Origin && departureDate == scheduleInfo.DepartureDate || destination == scheduleInfo.Destination && arrivalDate == scheduleInfo.ArrivalDate))
			{
				foundBestMatched = (TryToMatchDepartureDateAndOrigin(scheduleInfos) || TryToMatchArrivalDateAndDestination(scheduleInfos));
				return true;
			}

			foundBestMatched = TryToMatch(scheduleInfos);
			return foundBestMatched;
		}

		// return true: if the best matching schedule is found
		bool TryToMatchDepartureDateAndOrigin(IEnumerable<ScheduleInfo> scheduleInfos)
		{
			return TryToMatch(scheduleInfos.Where(scheduleInfo => origin == scheduleInfo.Origin && departureDate == scheduleInfo.DepartureDate));
		}

		// return true: if the best matching schedule is found
		bool TryToMatchArrivalDateAndDestination(IEnumerable<ScheduleInfo> scheduleInfos)
		{
			return TryToMatch(scheduleInfos.Where(scheduleInfo => destination == scheduleInfo.Destination && arrivalDate == scheduleInfo.ArrivalDate));
		}

		// return true: if the best matching schedule is found
		bool TryToMatch(IEnumerable<ScheduleInfo> scheduleInfos)
		{
			foreach (var scheduleInfo in scheduleInfos)
			{
				if (TryToMatch(scheduleInfo))
				{
					return true;
				}
			}

			return false;
		}

		// return true: if the newScheduleInfo is the best matching schedule
		bool TryToMatch(ScheduleInfo newScheduleInfo)
		{
			var result = false;

			if (GetMatchingScore(newScheduleInfo) > GetMatchingScore(Result.MatchedSchedule))
			{
				Result.MatchedSchedule = newScheduleInfo;

				result |= ((origin == ZString.Empty || origin == Result.MatchedSchedule.Origin)
					&& (departureDate == ZDate.Empty || departureDate == Result.MatchedSchedule.DepartureDate)
					&& (destination == ZString.Empty || destination == Result.MatchedSchedule.Destination)
					&& (arrivalDate == ZDate.Empty || arrivalDate == Result.MatchedSchedule.ArrivalDate));
			}

			return result;
		}

		int GetMatchingScore(ScheduleInfo scheduleInfo)
		{
			if (scheduleInfo.IsEmpty)
			{
				return 0;
			}

			const int Origin = 0b1;
			const int Destination = 0b10;
			const int ArrivalDate = 0b100;
			const int DepartureDate = 0b1000;

			var matchingScores = new Dictionary<int, int>()
			{
				{ DepartureDate + Origin + Destination, 9 },
				{ ArrivalDate   + Origin + Destination, 9 },
				{ DepartureDate + Origin,      5 },
				{ ArrivalDate   + Destination, 4 },
				{ DepartureDate + ArrivalDate, 3 },
				{ DepartureDate, 1 },
			};

			var matchedItems = 0;
			var count = 0;

			if (origin == scheduleInfo.Origin)
			{
				matchedItems += Origin;
				count++;
			}

			if (departureDate == scheduleInfo.DepartureDate)
			{
				matchedItems += DepartureDate;
				count++;
			}

			if (destination == scheduleInfo.Destination)
			{
				matchedItems += Destination;
				count++;
			}

			if (arrivalDate == scheduleInfo.ArrivalDate)
			{
				matchedItems += ArrivalDate;
				count++;
			}

			return count * 10 + (matchingScores.TryGetValue(matchedItems, out int result) ? result : 0);
		}

		List<ScheduleInfo> GetScheduleInfos(ZDate date)
		{
			if (date.IsEmpty)
			{
				return null;
			}

			var scheduleInfos = GetScheduleInfosCore(date);
			var isArrivalDate = (date != departureDate);

			if (isArrivalDate && scheduleInfos != null && scheduleInfos.Count >= 1 && scheduleInfos.All(scheduleInfo => scheduleInfo.ArrivalDate != date))
			{
				var possibleRequestDate = GetPossibleRequestDate(date, scheduleInfos);

				if (date != possibleRequestDate)
				{
					var possibleScheduleInfos = GetScheduleInfosCore(possibleRequestDate);

					if (possibleScheduleInfos != null && possibleScheduleInfos.Any(scheduleInfo => scheduleInfo.ArrivalDate == date))
					{
						scheduleInfos = possibleScheduleInfos;
					}
				}
			}

			return scheduleInfos;
		}

		ZDate GetPossibleRequestDate(ZDate date, List<ScheduleInfo> scheduleInfos)
		{
			var matchedCount = 0;
			var matchedArrivalDate = ZDate.Empty;

			foreach (var scheduleInfo in scheduleInfos)
			{
				var count = 0;

				if (origin == scheduleInfo.Origin)
				{
					count++;
				}

				if (destination == scheduleInfo.Destination)
				{
					count++;
				}

				if (count > matchedCount)
				{
					matchedCount = count;
					matchedArrivalDate = scheduleInfo.ArrivalDate;
				}
			}

			if (matchedArrivalDate == ZDate.Empty)
			{
				matchedArrivalDate = scheduleInfos[0].ArrivalDate;
			}

			var dayOffset = (date - matchedArrivalDate).Days;
			return date.AddDays(dayOffset);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only")]
		List<ScheduleInfo> GetScheduleInfosCore(ZDate date)
		{
			var scheduleInfos = new List<ScheduleInfo>();
			using (var s8client = GetNewClient())
			{
				var getFlightResult = s8client.GetFlight(new FlightRequest(date, airline, flightNumber));

				if (!getFlightResult.Succeeded)
				{
					if (getFlightResult.ErrorMessage.Contains((NoResString)"Login is suppressed"))
					{
						return scheduleInfos;
					}

					if (getFlightResult.ErrorMessage.Contains((NoResString)"Flight not found."))
					{
						return scheduleInfos;
					}

					if (getFlightResult.ErrorMessage.Contains((NoResString)"Error in Date"))
					{
						Result.MatchErrorMessage = Res.GetString("62faa891-4c34-45a1-b1ca-7ab9dbb053af",
							"Flight Schedules are only available two years in advance, and therefore the results returned will not exceed two years from now.");
						return scheduleInfos;
					}

					if (getFlightResult.ErrorMessage.Contains((NoResString)"Execution Timeout Expired"))
					{
						Result.MatchErrorMessage = Res.GetString("7d55f371-c243-440b-abf6-dcd97628efa7",
							"The connection to the server has a problem. Please contact your administrator");
						return scheduleInfos;
					}

					ReportError(getFlightResult, date);

					return null;
				}

				var flightInfo = getFlightResult.Result;

				if (airline != flightInfo.Airline || flightNumber != flightInfo.FlightNumber || date != flightInfo.RequestDate)
				{
					ErrorReporter.ReportOnce("S8Matcher_GetScheduleInfosCore_FlightInfoMismatch", MatcherDetailsMessage + string.Format(CultureInfo.InvariantCulture, "The request date is: {0}\r\nBut in the response, the airline '{1}', flight number '{2}' or request date '{3}' does not match.", date, flightInfo.Airline, flightInfo.FlightNumber, flightInfo.RequestDate));
					return null;
				}

				var legs = flightInfo.Legs;

				if (legs.Count == 0)
				{
					ErrorReporter.ReportOnce("S8Matcher_GetScheduleInfosCore_LegCountIsZero", MatcherDetailsMessage + string.Format(CultureInfo.InvariantCulture, "The request date is: {0}\r\nThe number of legs is 0.", date));
					return null;
				}

				for (var i = 0; i < legs.Count; i++)
				{
					for (var j = i; j < legs.Count; j++)
					{
						var schedule = new ScheduleInfo(flightInfo.Airline, flightInfo.FlightNumber, legs[i].Origin, legs[i].DepartureDate, legs[j].Destination, legs[j].ArrivalDate, aircraftType: legs[i].Equipment);
						scheduleInfos.Add(schedule);
					}
				}
			}

			return scheduleInfos;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		void ReportError(MethodCallResult<FlightInformation> getFlightResult, ZDate date)
		{
			if (getFlightResult.ShouldBeReported && shouldReportError)
			{
				var errorKey = "S8Matcher_GetScheduleInfosCore_ErrorMessage";
				ErrorReporter.ReportOnce(errorKey, MatcherDetailsMessage + string.Format(CultureInfo.InvariantCulture, "The request date is: {0}\r\nThe GetFlight error message is:\r\n{1}\r\n{2}", date, getFlightResult.ErrorMessage, getFlightResult.StackTrace));
			}
		}

		string MatcherDetailsMessage => string.Format(CultureInfo.InvariantCulture,
(NoResString)"Current flight schedule is:\r\nAirline: {0}\r\nFlight Number: {1}\r\nOrigin : {2}\r\nDeparture Date: {3}\r\nDestination : {4}\r\nArrival Date: {5}\r\n", // Developer Only
			airline,
			flightNumber,
			origin,
			departureDate,
			destination,
			arrivalDate);
	}
}
