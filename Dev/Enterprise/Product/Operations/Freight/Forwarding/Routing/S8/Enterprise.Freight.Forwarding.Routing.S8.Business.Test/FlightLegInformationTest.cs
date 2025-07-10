using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	public class FlightLegInformationTest : TestCase
	{
		public void TestParseErrorColumns()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertEquals(@"Error when parsing string to FlightLegInformation:
RequestDate is '30-Jul-18'
String is 'QF 0001 2018/07/26 2018/10/06 1234567 2 SIN 23:55 0 LHR 06:55 1 388'
The number of columns is less than 14.", parseResult.ErrorMessage);
		}

		public void TestParseErrorCarrier()
		{
			var str = "QF1 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The Carrier 'QF1' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorFlightNumber()
		{
			var str = "QF -001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The flight number '-001' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorEffectiveDate()
		{
			var str = "QF 0001 2018/7/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The effective date '2018/7/26' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorDiscontinuedDate()
		{
			var str = "QF 0001 2018/07/26 2018/09/31 1234567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The discontinued date '2018/09/31' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorEffectiveDateIsAfterDiscontinuedDate()
		{
			var str = "QF 0001 2018/10/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The effective date '2018/10/26' is after discontinued date '2018/10/06'.", parseResult.ErrorMessage);
		}

		public void TestParseErrorOperationDay()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 123_567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The operation day '123_567' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorServiceType()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 JC SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The service type 'JC' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorOrigin()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SGSIN 23:55 0 GBLHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The origin 'SGSIN' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorDepartureTime()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 24:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The departure time '24:55' or day offset '0' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorDepartureDayOffset()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 X LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The departure time '23:55' or day offset 'X' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorDestination()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 GBLHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The destination 'GBLHR' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorArrivalTime_NegativeHour()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR -6:00 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The arrival time '-6:00' or day offset '1' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorArrivalTime_ExceededHour()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 24:00 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The arrival time '24:00' or day offset '1' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorArrivalTime_NegativeMinute()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:-1 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The arrival time '06:-1' or day offset '1' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorArrivalTime_ExceededMinute()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:60 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The arrival time '06:60' or day offset '1' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorArrivalDayOffset()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 X 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The arrival time '06:55' or day offset 'X' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseErrorEquipment()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 ABCD";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The equipment 'ABCD' is not correct.", parseResult.ErrorMessage);
		}

		public void TestParseFlightSegments()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 7 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);
			var leg = parseResult.Result;

			AssertEquals(true, parseResult.Succeeded);
			AssertEquals(7, leg.LegNumber);

			var str2 = "QF 0001 2018/07/26 2018/10/06 1234567 8 J SIN 23:55 0 LHR 06:55 1 388 ZL 5661 00:00";
			var requestDate2 = new ZDate(2018, 8, 30);
			var parseResult2 = FlightLegInformation.Parse(requestDate2, str2);
			var leg2 = parseResult2.Result;

			AssertEquals(true, parseResult2.Succeeded);
			AssertEquals(8, leg2.LegNumber);

			var str3 = "QF 0001 2018/07/26 2018/10/06 1234567 9 J SIN 23:55 0 LHR 06:55 1 388 ZL 5661 00:00";
			var requestDate3 = new ZDate(2018, 9, 30);
			var parseResult3 = FlightLegInformation.Parse(requestDate3, str3);
			var leg3 = parseResult3.Result;

			AssertEquals(true, parseResult3.Succeeded);
			AssertEquals(9, leg3.LegNumber);

			var str4 = "QF 0001 2018/07/26 2018/10/06 1234567 10 J SIN 23:55 0 LHR 06:55 1 388 ZL 5661 00:00";
			var requestDate4 = new ZDate(2018, 10, 30);
			var parseResult4 = FlightLegInformation.Parse(requestDate4, str4);

			AssertEquals(false, parseResult4.Succeeded);
			AssertContains("The leg number '10' is not correct.", parseResult4.ErrorMessage);
		}

		public void TestParseSuccessfully()
		{
			var str = "QF 0001 2018/07/26 2018/10/06 1234567 2 J SIN 23:55 0 LHR 06:55 1 388";
			var requestDate = new ZDate(2018, 7, 30);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(true, parseResult.Succeeded);

			var leg = parseResult.Result;

			AssertEquals(new ZDate(2018, 7, 30), leg.RequestDate);

			AssertEquals("QF", leg.Airline);
			AssertEquals(1, leg.FlightNumber);
			AssertEquals(new ZDate(2018, 7, 26), leg.EffectiveDate);
			AssertEquals(new ZDate(2018, 10, 6), leg.DiscontinuedDate);
			AssertEquals("1234567", leg.OperationDay);
			AssertEquals(2, leg.LegNumber);
			AssertEquals('J', leg.ServiceType);
			AssertEquals("SIN", leg.Origin);
			AssertEquals("LHR", leg.Destination);
			AssertEquals(new ZDateTime(2018, 7, 30, 23, 55, 0), leg.DepartureDateTime);
			AssertEquals(new ZDateTime(2018, 7, 31, 6, 55, 0), leg.ArrivalDateTime);
			AssertEquals("388", leg.Equipment);
			AssertEquals(new ZDate(2018, 7, 30), leg.DepartureDate);
			AssertEquals(new ZDate(2018, 7, 31), leg.ArrivalDate);
			AssertEquals(0, leg.DepartureDayOffset);
			AssertEquals(1, leg.ArrivalDayOffset);
		}

		public void TestParseUA200()
		{
			var str = "UA 0200 2018/07/26 2018/09/04 1234567  1 J GUM 06:40 0 HNL 09:00 -1 777";
			var requestDate = new ZDate(2018, 8, 2);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(true, parseResult.Succeeded);

			var leg = parseResult.Result;

			AssertEquals(new ZDate(2018, 8, 2), leg.RequestDate);

			AssertEquals("UA", leg.Airline);
			AssertEquals(200, leg.FlightNumber);
			AssertEquals(new ZDate(2018, 7, 26), leg.EffectiveDate);
			AssertEquals(new ZDate(2018, 9, 4), leg.DiscontinuedDate);
			AssertEquals("1234567", leg.OperationDay);
			AssertEquals(1, leg.LegNumber);
			AssertEquals('J', leg.ServiceType);
			AssertEquals("GUM", leg.Origin);
			AssertEquals("HNL", leg.Destination);
			AssertEquals(new ZDateTime(2018, 8, 2, 6, 40, 0), leg.DepartureDateTime);
			AssertEquals(new ZDateTime(2018, 8, 1, 9, 00, 0), leg.ArrivalDateTime);
			AssertEquals("777", leg.Equipment);
			AssertEquals(new ZDate(2018, 8, 2), leg.DepartureDate);
			AssertEquals(new ZDate(2018, 8, 1), leg.ArrivalDate);
			AssertEquals(0, leg.DepartureDayOffset);
			AssertEquals(-1, leg.ArrivalDayOffset);
		}

		public void TesParseErrorHA898()
		{
			var str = "HA 0898 2018/08/01 2018/09/02 ..3.5.7  1 J PEK 01:00 0 HNL -6:-45 0 332";
			var requestDate = new ZDate(2018, 8, 3);
			var parseResult = FlightLegInformation.Parse(requestDate, str);

			AssertEquals(false, parseResult.Succeeded);
			AssertContains("The arrival time '-6:-45' or day offset '0' is not correct.", parseResult.ErrorMessage);
		}
	}
}
