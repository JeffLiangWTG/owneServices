namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;

	public class FlightInformation
	{
		public static MethodCallResult<FlightInformation> Parse(string str)
		{
			Argument.NotNull(str, nameof(str));

			var flight = new FlightInformation()
			{
				Legs = new List<FlightLegInformation>(),
			};

			if (str.Contains((NoResString)"|Flight not found")) // Developer Only
			{
				return ParseErrorResult(str, (NoResString)"Flight not found.", shouldBeReported: false); // Developer Only
			}

			var lines = Regex.Split(str, "\r\n|\r|\n");
			var linesCount = string.IsNullOrEmpty(lines[lines.Length - 1]) ? lines.Length - 1 : lines.Length;

			if (linesCount < 3)
			{
				return ParseErrorResult(str, (NoResString)"The string should have at least 3 lines."); // Developer Only
			}

			var line = lines[0];
			var columns = Regex.Split(line, @"\s+");

			// the first line's columns example: "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\n"
			if (columns.Length < 7)
			{
				return ParseErrorResult(str, (NoResString)"The number of columns of the first line is less than 7."); // Developer Only
			}

			// columns[3] request date example: "2018/07/30",
			var requestDateStr = columns[3].TrimEnd(',').Trim('"');

			if (!DateTime.TryParseExact(requestDateStr, "yyyy/M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime requestDate))
			{
				return ParseErrorResult(str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The request date '{0}' is not correct.", columns[3])); // Developer Only
			}

			flight.RequestDate = new ZDateTime(requestDate).Date;

			// columns[4] airline example: "QF",
			var airline = columns[4].TrimEnd(',').Trim('"');

			if (airline.Length != 2)
			{
				return ParseErrorResult(str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The Carrier '{0}' is not correct.", columns[4])); // Developer Only
			}

			flight.Airline = airline;

			// columns[5] flight number example: 1
			if (!Int32.TryParse(columns[5], out int flightNumber) || flightNumber < 1 || flightNumber > 9999)
			{
				return ParseErrorResult(str, string.Format(CultureInfo.InvariantCulture, (NoResString)"The Flight Number '{0}' is not correct.", columns[5])); // Developer Only
			}

			flight.FlightNumber = flightNumber;

			// each line for a leg
			for (var i = 2; i < linesCount; i++)
			{
				var lineParseResult = FlightLegInformation.Parse(flight.RequestDate, lines[i]);

				if (!lineParseResult.Succeeded)
				{
					return ParseErrorResult(str, string.Format(CultureInfo.InvariantCulture, (NoResString)"Error when parsing the line {0}\n{1}", i + 1, lineParseResult.ErrorMessage)); // Developer Only
				}

				var leg = lineParseResult.Result;

				if (airline != leg.Airline || flightNumber != leg.FlightNumber)
				{
					return ParseErrorResult(str, string.Format(CultureInfo.InvariantCulture, (NoResString)"Error with Carrier or Flight Number when parsing the line {0}\n", i + 1)); // Developer Only
				}

				flight.Legs.Add(leg);
			}

			return new MethodCallResult<FlightInformation>(flight);
		}

		static MethodCallResult<FlightInformation> ParseErrorResult(string str, string errorMessage, bool shouldBeReported = true)
		{
			var message = string.Format(
				CultureInfo.InvariantCulture,
(NoResString)"Error when parsing string to FlightInformation:\r\nString is: '{0}'\r\nError is: {1}", // Developer Only
				str,
				errorMessage);
			return new MethodCallResult<FlightInformation>(message, shouldBeReported);
		}

		public ZDate RequestDate { get; private set; }
		public ZString Airline { get; private set; }
		public int FlightNumber { get; private set; }
		public List<FlightLegInformation> Legs { get; private set; }
	}
}
