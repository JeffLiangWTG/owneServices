using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class FlightInformationTest : TestCase
	{
		public void TestParseErrorFileNotFound()
		{
			var str = "|Flight not found";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertEquals(@"Error when parsing string to FlightInformation:
String is: '|Flight not found'
Error is: Flight not found.", parseResult.ErrorMessage);
		}

		public void TestParseErrorFewerLines()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The string should have at least 3 lines.", parseResult.ErrorMessage);
		}

		public void TestParseErrorFirstLineFewerColumns()
		{
			var str = "GetFlight ( \"2018/7/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The number of columns of the first line is less than 7.", parseResult.ErrorMessage);
		}

		public void TestParseErrorDateFormat()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/07/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(true, parseResult.Succeeded);

			str = "GetFlight ( \"SSIM\", \"2018-7-30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The request date '\"2018-7-30\",' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorAirline()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QFX\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The Carrier '\"QFX\",' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorFlightNumber()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 22X )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The Flight Number '22X' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorLeg()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/7/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("Error when parsing the line 3", parseResult.ErrorMessage);
		}

		public void TestParseErrorLegCarrier()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"MU 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("Error with Carrier or Flight Number when parsing the line 4", parseResult.ErrorMessage);
		}

		public void TestParseErrorLegFlightNumber()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0002 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("Error with Carrier or Flight Number when parsing the line 4", parseResult.ErrorMessage);
		}

		public void TestParseSuccessfully()
		{
			var str = "GetFlight ( \"SSIM\", \"2018/7/30\", \"QF\", 1 )\n" +
				"AL FltN EffectDate DisconDate Op-Days L# S Org DepTm O Dst ArrTm O Eqp\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 1 J SYD 15:55 0 SIN 22:15 0 388\n" +
				"QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388\n";

			var parseResult = FlightInformation.Parse(str);

			AssertEquals(true, parseResult.Succeeded);

			var flight = parseResult.Result;

			AssertEquals(new ZDate(2018, 7, 30), flight.RequestDate);
			AssertEquals("QF", flight.Airline);
			AssertEquals(1, flight.FlightNumber);

			AssertEquals(2, flight.Legs.Count);
			var legs = flight.Legs;

			AssertEquals(flight.Airline, legs[0].Airline);
			AssertEquals(flight.Airline, legs[1].Airline);

			AssertEquals(flight.FlightNumber, legs[0].FlightNumber);
			AssertEquals(flight.FlightNumber, legs[1].FlightNumber);

			AssertEquals(1, legs[0].LegNumber);
			AssertEquals(2, legs[1].LegNumber);

			AssertEquals(legs[0].Destination, legs[1].Origin);
		}
	}
}
