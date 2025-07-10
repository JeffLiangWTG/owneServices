using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingResponseLine))]
	public class RoutingResponseLineTest : NonPersistentBusinessObjectTestCase
	{
		#region Parse Message

		public void TestParseMessage()
		{
			string testMessageLine = "SYD 1 SIN 3   20:45 1+09:45 ZZ  Q123  744/702   0 9012   >";
			RoutingResponseLine line = new RoutingResponseLine(testMessageLine, Factory);

			AssertEquals("SYD", line.Origin);
			AssertEquals("1", line.OriginTerminal);
			AssertEquals("Sydney", line.OriginDescription);
			AssertEquals("SIN", line.Destination);
			AssertEquals("3", line.DestinationTerminal);
			AssertEquals("Singapore", line.DestinationDescription);

			AssertEquals("20:45", line.DepartureTime);
			AssertEquals("1+09:45", line.ArrivalTime);
			AssertEquals("ZZ", line.TicketingCarrier);
			AssertEquals("Q123", line.FlightNumber);
			AssertEquals("744/702", line.Aircraft);
			AssertEquals(0, line.Stops);
			AssertEquals(9012, line.MilesDistance);
		}

		public void TestParseMessage_OperationDay_EffYMD_DscYMD_FlightType()
		{
			var testMessageLine = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J> ";
			var line = new RoutingResponseLine(testMessageLine, Factory);

			AssertEquals("SYD", line.Origin);
			AssertEquals("1", line.OriginTerminal);
			AssertEquals("Sydney", line.OriginDescription);
			AssertEquals("HKG", line.Destination);
			AssertEquals("1", line.DestinationTerminal);
			AssertEquals("Hong Kong", line.DestinationDescription);

			AssertEquals("08:45", line.DepartureTime);
			AssertEquals("15:05", line.ArrivalTime);
			AssertEquals("CX", line.TicketingCarrier);
			AssertEquals("110", line.FlightNumber);
			AssertEquals("333", line.Aircraft);
			AssertEquals(0, line.Stops);
			AssertEquals(4580, line.MilesDistance);

			AssertEquals("1_34567", line.OperationDay);
			AssertEquals(new ZDateTime(2016, 10, 31), line.EffectiveDate);
			AssertEquals(new ZDateTime(2017, 1, 25), line.DiscontinuedDate);
			AssertEquals(FlightTypeConstants.PassengerOnly, line.FlightType);
		}

		public void TestParseFlightNumber()
		{
			var testMessageLine = "SFO   ORD     20:30 1+10:00 UA  4029T   RFS     0  1846                                        ....5.. 20/11/06 20/11/06 T> ";
			var line = new RoutingResponseLine(testMessageLine, Factory);

			AssertEquals("4029T", line.FlightNumber);

			testMessageLine = "SFO   ORD     07:00   13:03 UA   556    753     0  1846                                        ....5.. 20/11/06 20/11/06 O> ";
			line = new RoutingResponseLine(testMessageLine, Factory);

			AssertEquals("556", line.FlightNumber);
		}

		public void TestCloneLine()
		{
			var testMessageLine = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J> ";
			var line = new RoutingResponseLine(testMessageLine, Factory);

			var newFactory = new BusinessObjectFactory();
			var newLine = line.Clone(newFactory);

			AssertEquals("SYD", newLine.Origin);
			AssertEquals("1", newLine.OriginTerminal);
			AssertEquals("Sydney", newLine.OriginDescription);
			AssertEquals("HKG", newLine.Destination);
			AssertEquals("1", newLine.DestinationTerminal);
			AssertEquals("Hong Kong", newLine.DestinationDescription);

			AssertEquals("08:45", newLine.DepartureTime);
			AssertEquals("15:05", newLine.ArrivalTime);
			AssertEquals("CX", newLine.TicketingCarrier);
			AssertEquals("110", newLine.FlightNumber);
			AssertEquals("333", newLine.Aircraft);
			AssertEquals(0, newLine.Stops);
			AssertEquals(4580, newLine.MilesDistance);

			AssertEquals("1_34567", newLine.OperationDay);
			AssertEquals(new ZDateTime(2016, 10, 31), newLine.EffectiveDate);
			AssertEquals(new ZDateTime(2017, 1, 25), newLine.DiscontinuedDate);
			AssertEquals(FlightTypeConstants.PassengerOnly, newLine.FlightType);
		}

		public void TestParseMessage_FlightType_PassengerOrCargo()
		{
			var testMessageLine = new StringBuilder("SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J> ");
			const int serviceTypeIndex = 121;
			Assert("Precondition: Service Type is 'J'", testMessageLine.Length > serviceTypeIndex && testMessageLine[serviceTypeIndex] == 'J');

			var passengerServiceTypes = new char[] { 'J', 'S', 'U', 'Q', 'G', 'B', 'C', 'O', 'L', 'R' };
			foreach (var serviceType in passengerServiceTypes)
			{
				testMessageLine[serviceTypeIndex] = serviceType;
				var line = new RoutingResponseLine(testMessageLine.ToString(), Factory);
				AssertEquals(FlightTypeConstants.PassengerOnly, line.FlightType);
			}

			var cargoServiceTypes = new char[] { 'F', 'V', 'M', 'A', 'H' };
			foreach (var serviceType in cargoServiceTypes)
			{
				testMessageLine[serviceTypeIndex] = serviceType;
				var line = new RoutingResponseLine(testMessageLine.ToString(), Factory);
				AssertEquals(FlightTypeConstants.CargoOnly, line.FlightType);
			}
		}

		public void TestParseMessage_MultipleAircraftTypes()
		{
			var testMessageLine = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J> ";
			var line = new RoutingResponseLine(testMessageLine, Factory);
			AssertEquals("333", line.Aircraft);
			AssertEquals("333", line.AircraftForImporting);
			Assert(!line.HasMultipleAircraftTypes);

			testMessageLine = "SYD 3 MEL 1   07:00   08:35 BA  9437  332/334   0   439                                        12345.. 16/10/03 17/03/31 J> ";
			line = new RoutingResponseLine(testMessageLine, Factory);
			AssertEquals("332/334", line.Aircraft);
			AssertEquals("332", line.AircraftForImporting);
			Assert(line.HasMultipleAircraftTypes);
		}

		public void TestParseMessage_ForArrivalDepartureDescription()
		{
			var testMessageLine = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J> ";

			var request = new RoutingRequest(Factory);
			request.IncludeWeeklyTimetable = true;
			request.DepartureDate = new ZDateTime(2018, 6, 25);

			var lineWeekly = new RoutingResponseLine(testMessageLine, Factory, request);

			AssertEquals("15:05", lineWeekly.ArrivalDescription);
			AssertEquals("08:45", lineWeekly.DepartureDescription);

			request.IncludeWeeklyTimetable = false;
			var lineSingle = new RoutingResponseLine(testMessageLine, Factory, request);

			AssertEquals("25-Jun-18 15:05", lineWeekly.ArrivalDescription);
			AssertEquals("25-Jun-18 08:45", lineWeekly.DepartureDescription);

			var testMessageLine2 = "SYD 1 HKG 1 1+08:45 1+15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J> ";

			var lineSingleTomorrow = new RoutingResponseLine(testMessageLine2, Factory, request);

			AssertEquals("26-Jun-18 15:05", lineSingleTomorrow.ArrivalDescription);
			AssertEquals("26-Jun-18 08:45", lineSingleTomorrow.DepartureDescription);

			var testMessageLine3 = "SYD 1 HKG 1 2+08:45 2+15:05 CX   110    333     0  4580                                        1.34567 16 / 10 / 31 17 / 01 / 25 J > ";

			var lineSingleTwoDaysAfter = new RoutingResponseLine(testMessageLine3, Factory, request);

			AssertEquals("27-Jun-18 15:05", lineSingleTwoDaysAfter.ArrivalDescription);
			AssertEquals("27-Jun-18 08:45", lineSingleTwoDaysAfter.DepartureDescription);
		}

		public void TestParseMessage_ForCO2Emission()
		{
			var testMessageLine = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J 123.4560000001> ";

			var request = new RoutingRequest(Factory);
			request.IncludeCO2EmissionValue = true;

			var line = new RoutingResponseLine(testMessageLine, Factory, request);
			AssertEquals(123.4560000001m, line.CO2Emission);

			var testMessageLine2 = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J 12345649824.5672778509> ";

			line = new RoutingResponseLine(testMessageLine2, Factory, request);
			AssertEquals(12345649824.5672778509m, line.CO2Emission);

			var zeroCO2Line = "SYD 1 HKG 1   08:45   15:05 CX   110    333     0  4580                                        1.34567 16/10/31 17/01/25 J 0.0000000000> ";

			line = new RoutingResponseLine(zeroCO2Line, Factory, request);
			Assert(line.CO2Emission.IsEmpty);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RoutingResponseLine("", Factory);
		}

		#endregion
	}
}
