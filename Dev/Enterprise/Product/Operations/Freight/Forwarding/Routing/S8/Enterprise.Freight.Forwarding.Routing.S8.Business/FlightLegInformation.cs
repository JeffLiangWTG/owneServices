namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;

	public class FlightLegInformation
	{
		public static MethodCallResult<FlightLegInformation> Parse(ZDate requestDate, string str)
		{
			Argument.NotNull(str, nameof(str));
			if (!requestDate.IsValid)
			{
				throw new ArgumentException("Invalid argument.", nameof(requestDate));
			}

			var leg = new FlightLegInformation
			{
				RequestDate = requestDate
			};

			// columns example: QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388
			var columns = Regex.Split(str, @"\s+");

			if (columns.Length < 14)
			{
				return ParseErrorResult(requestDate, str, (NoResString)"The number of columns is less than 14."); // Developer Only
			}

			// columns[0] airline example: QF
			if (columns[0].Length != 2)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The Carrier '{0}' is not correct.", columns[0])); // Developer Only
			}

			leg.Airline = columns[0];

			// columns[1] flight number example: 00001

			if (columns[1].Length != 4 || !Int32.TryParse(columns[1].TrimStart('0'), out int flightNumber) || flightNumber < 1 || flightNumber > 9999)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The flight number '{0}' is not correct.", columns[1])); // Developer Only
			}

			leg.FlightNumber = flightNumber;

			// columns[2] effective date example: 2018/07/26

			if (!DateTime.TryParseExact(columns[2], "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime effectiveDate))
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The effective date '{0}' is not correct.", columns[2])); // Developer Only
			}

			leg.EffectiveDate = new ZDateTime(effectiveDate).Date;

			// columns[3] discontinued date example: 2018/10/06

			if (!DateTime.TryParseExact(columns[3], "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime discontinuedDate))
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The discontinued date '{0}' is not correct.", columns[3])); // Developer Only
			}

			leg.DiscontinuedDate = new ZDateTime(discontinuedDate).Date;

			if (leg.EffectiveDate > leg.DiscontinuedDate)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The effective date '{0}' is after discontinued date '{1}'.", columns[2], columns[3])); // Developer Only
			}

			// columns[4] operation day example: 1234.67

			if (!IsValidOperationDay(columns[4]))
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The operation day '{0}' is not correct.", columns[4])); // Developer Only
			}

			leg.OperationDay = columns[4];

			// columns[5] leg number example: 2

			if (!Int32.TryParse(columns[5], out int legNumber) || legNumber < 1 || legNumber > 9)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The leg number '{0}' is not correct.", columns[5])); // Developer Only
			}

			leg.LegNumber = legNumber;

			// columns[6] Service Type example: J

			if (columns[6].Length != 1)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The service type '{0}' is not correct.", columns[6])); // Developer Only
			}

			leg.ServiceType = columns[6][0];

			// columns[7] Origin example: SIN

			if (columns[7].Length != 3)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The origin '{0}' is not correct.", columns[7])); // Developer Only
			}

			leg.Origin = columns[7];

			// columns[8] + columns[9] departure time and day offset example: 23:55 0

			if (!TryParseDatetime(requestDate, columns[8], columns[9], out ZDateTime departureTime))
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The departure time '{0}' or day offset '{1}' is not correct.", columns[8], columns[9])); // Developer Only
			}

			leg.DepartureDateTime = departureTime;

			// columns[10] Destination: LHR

			if (columns[10].Length != 3)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The destination '{0}' is not correct.", columns[10])); // Developer Only
			}

			leg.Destination = columns[10];

			// columns[11] + columns[12] arrival time and day offset example: 06:55 1

			if (!TryParseDatetime(requestDate, columns[11], columns[12], out ZDateTime arrivalTime))
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The arrival time '{0}' or day offset '{1}' is not correct.", columns[11], columns[12])); // Developer Only
			}

			leg.ArrivalDateTime = arrivalTime;

			// columns[13] equipment example: 388

			if (columns[13].Length != 3)
			{
				return ParseErrorResult(requestDate, str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The equipment '{0}' is not correct.", columns[13])); // Developer Only
			}

			leg.Equipment = columns[13];

			return new MethodCallResult<FlightLegInformation>(leg);
		}

		static bool TryParseDatetime(ZDate baseDate, string timeStr, string dayOffsetStr, out ZDateTime datetime)
		{
			datetime = ZDateTime.Empty;

			if (dayOffsetStr.Length > 2 || !Int32.TryParse(dayOffsetStr, out int dayOffset) || dayOffset > 9)
			{
				return false;
			}

			var timeParts = timeStr.Split(new[] { ':' });

			if (timeParts.Length != 2)
			{
				return false;
			}

			var hoursStr = timeParts[0];
			var minutesStr = timeParts[1];

			if (hoursStr.Length == 0 || !Int32.TryParse(hoursStr, out int hours) || hours < 0 || hours > 23)
			{
				return false;
			}

			if (minutesStr.Length == 0 || !Int32.TryParse(minutesStr, out int minutes) || minutes < 0 || minutes > 59)
			{
				return false;
			}

			datetime = baseDate.AddDays(dayOffset).AddHours(hours).AddMinutes(minutes);

			return true;
		}

		static bool IsValidOperationDay(string str)
		{
			char[] digits = { '1', '2', '3', '4', '5', '6', '7' };
			bool hasDigits = false;

			if (str.Length != 7)
			{
				return false;
			}

			for (var i = 0; i < 7; i++)
			{
				var ch = str[i];

				if (ch != '.' && ch != digits[i])
				{
					return false;
				}

				hasDigits |= (ch == digits[i]);
			}

			return hasDigits;
		}

		static MethodCallResult<FlightLegInformation> ParseErrorResult(ZDate requestDate, string str, string errorMessage)
		{
			var message = string.Format(
				CultureInfo.InvariantCulture,
(NoResString)"Error when parsing string to FlightLegInformation:\r\nRequestDate is '{0}'\r\nString is '{1}'\r\n{2}", // Developer Only
				requestDate.ToShortDateString(),
				str,
				errorMessage);
			return new MethodCallResult<FlightLegInformation>(message);
		}

		public ZDate RequestDate { get; private set; }

		public ZString Airline { get; private set; }
		public int FlightNumber { get; private set; }
		public ZDate EffectiveDate { get; private set; }
		public ZDate DiscontinuedDate { get; private set; }
		public string OperationDay { get; private set; }
		public int LegNumber { get; private set; }
		public char ServiceType { get; private set; }
		public ZString Origin { get; private set; }
		public ZString Destination { get; private set; }
		public ZDateTime DepartureDateTime { get; private set; }
		public ZDateTime ArrivalDateTime { get; private set; }
		public string Equipment { get; private set; }

		public ZDate DepartureDate => DepartureDateTime.Date;
		public ZDate ArrivalDate => ArrivalDateTime.Date;
		public int DepartureDayOffset => (DepartureDate - RequestDate).Days;
		public int ArrivalDayOffset => (ArrivalDate - RequestDate).Days;
	}
}
