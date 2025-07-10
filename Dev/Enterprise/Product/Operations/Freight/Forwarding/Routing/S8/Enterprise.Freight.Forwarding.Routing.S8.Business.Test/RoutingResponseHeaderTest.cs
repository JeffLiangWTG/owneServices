using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingResponseHeader))]
	public class RoutingResponseHeaderTest : NonPersistentBusinessObjectTestCase
	{
		#region Parse Message

		public void TestParseMessage()
		{
			string testMessageLine = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   > Some Crap At the End";
			RoutingResponseHeader header = new RoutingResponseHeader(testMessageLine, Factory);

			AssertEquals("SYD", header.Origin);
			AssertEquals("Sydney", header.OriginDescription);
			AssertEquals("BOM", header.Destination);
			AssertEquals("Mumbai (ex Bombay)", header.DestinationDescription);

			AssertEquals(1, header.Connections);
			AssertEquals(3, header.Stops);
			AssertEquals(2, header.Layers);

			AssertEquals("13:30", header.Duration);
			AssertEquals("20:45", header.DepartureTime);
			AssertEquals("2+08:30", header.ArrivalTime);

			AssertEquals("ZZ", header.Carrier1);
			AssertEquals("AA", header.Carrier2);
			AssertEquals("BB", header.Carrier3);
			AssertEquals("", header.Carrier4);

			AssertEquals(2, header.Lines.Count);

			AssertEquals("SYD", header.Lines[0].Origin);
			AssertEquals("1", header.Lines[0].OriginTerminal);
			AssertEquals("Sydney", header.Lines[0].OriginDescription);
			AssertEquals("SIN", header.Lines[0].Destination);
			AssertEquals("3", header.Lines[0].DestinationTerminal);
			AssertEquals("Singapore", header.Lines[0].DestinationDescription);

			AssertEquals("20:45", header.Lines[0].DepartureTime);
			AssertEquals("1+09:45", header.Lines[0].ArrivalTime);
			AssertEquals("ZZ", header.Lines[0].TicketingCarrier);
			AssertEquals("123", header.Lines[0].FlightNumber);
			AssertEquals(0, header.Lines[0].Stops);
			AssertEquals(9012, header.Lines[0].MilesDistance);

			AssertEquals("SIN", header.Lines[1].Origin);
			AssertEquals("4", header.Lines[1].OriginTerminal);
			AssertEquals("Singapore", header.Lines[1].OriginDescription);
			AssertEquals("BOM", header.Lines[1].Destination);
			AssertEquals("A", header.Lines[1].DestinationTerminal);
			AssertEquals("Mumbai (ex Bombay)", header.Lines[1].DestinationDescription);

			AssertEquals("1+10:45", header.Lines[1].DepartureTime);
			AssertEquals("2+08:30", header.Lines[1].ArrivalTime);
			AssertEquals("XX", header.Lines[1].TicketingCarrier);
			AssertEquals("090", header.Lines[1].FlightNumber);
			AssertEquals(3, header.Lines[1].Stops);
			AssertEquals(1003, header.Lines[1].MilesDistance);
		}

		public void TestParseMessage_OperationDay_Flight_FlightType_Equipment()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertRoutingResponseHeaderContent(header);
		}

		public void TestParseMessage_EffectiveDate_DiscontinuedDate_OperationDay()
		{
			var testMessageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/21 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/21 18/10/06 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals(new ZDateTime(2018, 06, 21), header.EffectiveDate);
			AssertEquals(new ZDateTime(2018, 10, 06), header.DiscontinuedDate);
			AssertEquals("1__4_6_", header.OperationDay);
		}

		public void TestOperationDayHasNumber()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals(true, header.OperationDayHasNumber(1));
			AssertEquals(true, header.OperationDayHasNumber(2));
			AssertEquals(true, header.OperationDayHasNumber(3));
			AssertEquals(true, header.OperationDayHasNumber(4));
			AssertEquals(true, header.OperationDayHasNumber(5));
			AssertEquals(false, header.OperationDayHasNumber(6));
			AssertEquals(false, header.OperationDayHasNumber(7));
			AssertEquals(false, header.OperationDayHasNumber(0));
			AssertEquals(false, header.OperationDayHasNumber(8));

			testMessageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/21 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/21 18/10/06 J> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals(true, header.OperationDayHasNumber(1));
			AssertEquals(false, header.OperationDayHasNumber(2));
			AssertEquals(false, header.OperationDayHasNumber(3));
			AssertEquals(true, header.OperationDayHasNumber(4));
			AssertEquals(false, header.OperationDayHasNumber(5));
			AssertEquals(true, header.OperationDayHasNumber(6));
			AssertEquals(false, header.OperationDayHasNumber(7));
			AssertEquals(false, header.OperationDayHasNumber(0));
			AssertEquals(false, header.OperationDayHasNumber(8));

			testMessageLine = "100 ICN LIS 104:55   08:05 4+08:00 CV CV        2025/03/02 2025/03/23 ......7 <ICN   LUX     08:05   16:00 CV  7226    74F     0  5417                                        ......7 25/03/02 25/03/23 F> <LUX   LIS   1+11:00 4+08:00 CV  8360Z   RFS     0  1064                                        1...... 25/03/03 25/03/24 V> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals(false, header.OperationDayHasNumber(1));
			AssertEquals(false, header.OperationDayHasNumber(2));
			AssertEquals(false, header.OperationDayHasNumber(3));
			AssertEquals(false, header.OperationDayHasNumber(4));
			AssertEquals(false, header.OperationDayHasNumber(5));
			AssertEquals(false, header.OperationDayHasNumber(6));
			AssertEquals(true, header.OperationDayHasNumber(7));
			AssertEquals(false, header.OperationDayHasNumber(0));
			AssertEquals(false, header.OperationDayHasNumber(8));
		}

		public void TestHasOperationDay()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals(true, header.HasOperation(new ZDateTime(2016, 10, 3)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2016, 10, 4)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2016, 10, 5)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2016, 10, 6)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2016, 10, 7)));
			AssertEquals(false, header.HasOperation(new ZDateTime(2016, 10, 8)));
			AssertEquals(false, header.HasOperation(new ZDateTime(2016, 10, 9)));

			testMessageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/21 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/21 18/10/06 J> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals(false, header.HasOperation(new ZDateTime(2018, 6, 18)));
			AssertEquals(false, header.HasOperation(new ZDateTime(2018, 6, 19)));
			AssertEquals(false, header.HasOperation(new ZDateTime(2018, 6, 20)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2018, 6, 21)));
			AssertEquals(false, header.HasOperation(new ZDateTime(2018, 6, 22)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2018, 6, 23)));
			AssertEquals(false, header.HasOperation(new ZDateTime(2018, 6, 24)));
			AssertEquals(true, header.HasOperation(new ZDateTime(2018, 6, 25)));
		}

		public void TestParseMessage_ForArrivalDepartureDescription()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";

			var request = new RoutingRequest(Factory);
			request.IncludeWeeklyTimetable = true;
			request.DepartureDate = new ZDateTime(2018, 6, 25);

			var headerWeekly = new RoutingResponseHeader(testMessageLine, Factory, request);

			AssertEquals("15:15", headerWeekly.ArrivalDescription);
			AssertEquals("07:00", headerWeekly.DepartureDescription);

			request.IncludeWeeklyTimetable = false;
			var headerSingle = new RoutingResponseHeader(testMessageLine, Factory, request);

			AssertEquals("25-Jun-18 15:15", headerSingle.ArrivalDescription);
			AssertEquals("25-Jun-18 07:00", headerSingle.DepartureDescription);

			var testMessageLine2 = "202 SYD LCY 30:25   10:45 1+08:10 CZ CZ KL     <SYD 1 CAN     10:45   18:30 KL  4406    330     0  4664                                        1234567 17/06/01 17/09/30 J> <CAN   AMS   1+00:05 1+06:45 KL  4300    330     0  5689                                        1234567 17/06/01 17/10/28 J> ";

			var headerSingleTomorrow = new RoutingResponseHeader(testMessageLine2, Factory, request);

			AssertEquals("26-Jun-18 08:10", headerSingleTomorrow.ArrivalDescription);
			AssertEquals("25-Jun-18 10:45", headerSingleTomorrow.DepartureDescription);

			var testMessageLine3 = "132 SYD BOM 13:30   20:45 2+08:30 ZZ AA BB    <SYD 1 SIN 3   20:45 1+09:45 ZZ   123            0 9012   > <SIN 4 BOM A 1+10:45 2+08:30 XX   090            3 1003   > Some Crap At the End";

			var headerSingleTwoDaysAfter = new RoutingResponseHeader(testMessageLine3, Factory, request);

			AssertEquals("27-Jun-18 08:30", headerSingleTwoDaysAfter.ArrivalDescription);
			AssertEquals("25-Jun-18 20:45", headerSingleTwoDaysAfter.DepartureDescription);
		}

		public void TestGetDayNumber()
		{
			AssertEquals(1, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 6, 25)));
			AssertEquals(2, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 6, 26)));
			AssertEquals(3, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 6, 27)));
			AssertEquals(4, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 6, 28)));
			AssertEquals(5, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 6, 29)));
			AssertEquals(6, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 6, 30)));
			AssertEquals(7, RoutingResponseHeader.GetDayNumber(new ZDateTime(2018, 7, 1)));
		}

		public void TestRouteCreationLogs()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			var departureDates = new List<ZDateTime>() { new ZDateTime(2016, 10, 4) };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);
			var expectedMessage = @"
Sydney (SYD) - Hong Kong (HKG) 04-Oct-16 07:00 - 04-Oct-16 15:15
    Sydney (SYD) - Melbourne (MEL) BA7437 04-Oct-16 07:00 - 04-Oct-16 08:35
    Melbourne (MEL) - Hong Kong (HKG) BA4138 04-Oct-16 08:50 - 04-Oct-16 15:15
";
			var actualMessage = string.Concat(header.RouteCreationLogs);
			AssertEquals(expectedMessage, actualMessage);

			testMessageLine = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/21 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/21 18/10/06 J> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			departureDates = new List<ZDateTime>() { new ZDateTime(2018, 6, 23) };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);
			expectedMessage = @"
Sydney (SYD) - Wuhan Tianhe International Apt (WUH) 23-Jun-18 11:20 - 23-Jun-18 20:15
    Sydney (SYD) - Wuhan Tianhe International Apt (WUH) MU750 23-Jun-18 11:20 - 23-Jun-18 20:15
";
			actualMessage = string.Concat(header.RouteCreationLogs);
			AssertEquals(expectedMessage, actualMessage);
		}

		public void TestParseMessage_DepartureTime()
		{
			var testMessageLine = "000 SYD HKG  9:20   21:55 1+05:15 CX           <SYD 1 HKG 1   21:55 1+05:15 CX   138    77W     0  4580                                        1...56. 17/06/02 17/09/25 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("21:55", header.DepartureTime);

			testMessageLine = "000 SYD HKG  9:40 1+07:30 1+15:10 CX           <SYD 1 HKG 1 1+07:30 1+15:10 CX   110    333     0  4580                                        1234567 17/06/03 17/09/30 J> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("1+07:30", header.DepartureTime);
		}

		public void TestParseMessage_Via()
		{
			var testMessageLine = "202 SYD LCY 30:25   10:45 1+08:10 CZ CZ KL     <SYD 1 CAN     10:45   18:30 KL  4406    330     0  4664                                        1234567 17/06/01 17/09/30 J> <CAN   AMS   1+00:05 1+06:45 KL  4300    330     0  5689                                        1234567 17/06/01 17/10/28 J> <AMS   LCY   1+08:00 1+08:10 KL   987    ARJ     0   209                                        12345.. 17/06/05 17/08/25 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("CAN-AMS", header.Via);

			testMessageLine = "202 SYD LCY 30:25   10:45 1+08:10 CZ CZ KL     <SYD 1 CAN     10:45   18:30 KL  4406    330     0  4664                                        1234567 17/06/01 17/09/30 J> <CAN   AMS   1+00:05 1+06:45 KL  4300    330     0  5689                                        1234567 17/06/01 17/10/28 J> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("CAN", header.Via);

			testMessageLine = "202 SYD LCY 30:25   10:45 1+08:10 CZ CZ KL     <SYD 1 CAN     10:45   18:30 KL  4406    330     0  4664                                        1234567 17/06/01 17/09/30 J> ";
			header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("Direct", header.Via);
		}

		public void TestCloneHeader()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);

			var newFactory = new BusinessObjectFactory();
			var newHeader = header.Clone(newFactory);
			AssertRoutingResponseHeaderContent(newHeader);
		}

		public void TestParseMessage_Invalid()
		{
			RoutingResponseHeader header = new RoutingResponseHeader("", Factory);
			AssertEquals("", header.Origin);
			AssertEquals("", header.Destination);

			header = new RoutingResponseHeader("HELLO", Factory);
			AssertEquals("", header.Origin);
			AssertEquals("", header.Destination);
		}

		public void TestParseMessage_WithCO2()
		{
			var testMessageLine =
				"100 SYD HKG 11:15   07:00   15:15 QF CX        "
				+ "<SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J 123.4560000001> "
				+ "<MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J 654.3219999999>";

			var header = new RoutingResponseHeader(testMessageLine, Factory);

			AssertEquals("Expected 2 lines", 2, header.Lines.Count);
			AssertEquals("CO2 value from substring", 123.4560000001m, header.Lines[0].CO2Emission);
			AssertEquals("CO2 value from substring", 654.3219999999m, header.Lines[1].CO2Emission);
			AssertEquals("CO2 sum", 777.778m, header.CO2EmissionTotal);
		}

		public void TestParseMessage_WithCO2_ZeroValue()
		{
			var testMessageLine =
				"100 SYD HKG 11:15   07:00   15:15 QF CX        "
				+ "<SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J 123.4560000001> "
				+ "<MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J 0.0000000000>";

			var header = new RoutingResponseHeader(testMessageLine, Factory);

			AssertEquals("Expected 2 lines", 2, header.Lines.Count);
			AssertEquals("CO2 value from substring", 123.4560000001m, header.Lines[0].CO2Emission);
			Assert(header.Lines[1].CO2Emission.IsEmpty);
			Assert(header.CO2EmissionTotal.IsEmpty);
		}

		#endregion

		#region TryCreateEnterpriseVoyageSailing

		public void TestCreateEnterpriseVoyage()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA4138");
			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			var departureDates = new List<ZDateTime>() { new ZDateTime(2016, 10, 4) };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			AssertCreateEnterpriseVoyage(header, departureDates.Single());
		}

		public void TestCreateEnterpriseVoyage_WithoutErrorReport()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA4138");
			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			ErrorReporter.Clear();

			var departureDates = new List<ZDateTime>() { new ZDateTime(2016, 10, 4) };
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			foreach (var sailings in sailingList)
			{
				foreach (var sailing in sailings)
				{
					var voyage = (BusinessObject)sailing["Voyage"];
					voyage["EnableOrphanedVoyageReportingForTests"] = true;
				}
			}

			Factory.Save();

			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestCreateEnterpriseVoyage_WithMultipleAircraftTypes()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA4138");
			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437  332/334   0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138  333/335   0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(header.AnyLineHasMultipleAircraftTypes);

			var departureDates = new List<ZDateTime>() { new ZDateTime(2016, 10, 4) };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			AssertCreateEnterpriseVoyage(header, departureDates.Single());
		}

		void AssertCreateEnterpriseVoyage(RoutingResponseHeader header, ZDateTime departureDate)
		{
			AssertEquals(2, header.Lines.Count);
			var sailing1 = FindMatchingJobSailing(header.Lines[0], departureDate);
			var sailing2 = FindMatchingJobSailing(header.Lines[1], departureDate);

			AssertCreateEnterpriseVoyageAndSailing("BA9437",
				new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 7, 0, 0), "332",
				"AUSYD", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 7, 0, 0),
				"AUMEL", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 35, 0),
				sailing1.PK);

			AssertCreateEnterpriseVoyageAndSailing("BA4138",
				new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 50, 0), "333",
				"AUMEL", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 50, 0),
				"HKHKG", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 15, 15, 0),
				sailing2.PK);
		}

		public void TestUpdateEnterpriseVoyageWhenVoyageExisted()
		{
			ZDateTime testDate = new ZDateTime(2016, 10, 4);

			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var query2 = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA4138");
			voyages = Factory.Load<IJobVoyage>(query2);
			AssertEquals("Pre condition: the voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var oldVoyage = (BusinessObject)Factory.New<IJobVoyage>();
			var oldOrigin = (BusinessObject)Factory.New<IVoyageOrigin>();
			var oldDestination = (BusinessObject)Factory.New<IVoyageDestination>();
			var oldSailing = (BusinessObject)Factory.New<IJobSailing>();
			CreateVoyage(oldVoyage, oldOrigin, oldDestination, oldSailing, testDate.AddHours(1), testDate.AddHours(1), testDate.AddHours(2));

			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("The voyage with flight number BA9437 should exist now.", 1, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			var departureDates = new List<ZDateTime>() { testDate };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			AssertUpdateEnterpriseVoyageWhenVoyageExisted(header, departureDates.Single(), oldVoyage.PK, oldOrigin.PK, oldDestination.PK, oldSailing.PK);
		}

		public void TestUpdateEnterpriseVoyageWhenVoyageExisted_WithMultipleAircraftTypes()
		{
			ZDateTime testDate = new ZDateTime(2016, 10, 4);

			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var query2 = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA4138");
			voyages = Factory.Load<IJobVoyage>(query2);
			AssertEquals("Pre condition: the voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var oldVoyage = (BusinessObject)Factory.New<IJobVoyage>();
			var oldOrigin = (BusinessObject)Factory.New<IVoyageOrigin>();
			var oldDestination = (BusinessObject)Factory.New<IVoyageDestination>();
			var oldSailing = (BusinessObject)Factory.New<IJobSailing>();
			CreateVoyage(oldVoyage, oldOrigin, oldDestination, oldSailing, testDate.AddHours(1), testDate.AddHours(1), testDate.AddHours(2));

			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("The voyage with flight number BA9437 should exist now.", 1, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437  332/334   0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(header.AnyLineHasMultipleAircraftTypes);

			var departureDates = new List<ZDateTime>() { testDate };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			AssertUpdateEnterpriseVoyageWhenVoyageExisted(header, departureDates.Single(), oldVoyage.PK, oldOrigin.PK, oldDestination.PK, oldSailing.PK);
		}

		void AssertUpdateEnterpriseVoyageWhenVoyageExisted(RoutingResponseHeader header, ZDateTime departureDate,
			ZGuid oldVoyagePK, ZGuid oldOriginPK, ZGuid oldDestinationPK, ZGuid oldSailingPK)
		{
			AssertEquals(2, header.Lines.Count);

			var sailing1 = FindMatchingJobSailing(header.Lines[0], departureDate);
			var sailing2 = FindMatchingJobSailing(header.Lines[1], departureDate);
			AssertEquals(oldSailingPK, sailing1.PK);

			AssertCreateEnterpriseVoyageAndSailing("BA9437",
				new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 7, 0, 0), "332",
				"AUSYD", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 7, 0, 0),
				"AUMEL", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 35, 0),
				sailing1.PK, 1,
				true, oldVoyagePK, oldOriginPK, oldDestinationPK);

			AssertCreateEnterpriseVoyageAndSailing("BA4138",
				new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 50, 0), "333",
				"AUMEL", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 8, 50, 0),
				"HKHKG", new ZDateTime(departureDate.Year, departureDate.Month, departureDate.Day, 15, 15, 0),
				sailing2.PK);
		}

		public void TestNotCreateEnterpriseVoyage_WhenNoLineExisted()
		{
			var query = new ZQuery();
			var voyages = Factory.Load<IJobVoyage>(query);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("Pre condition: no lines existed", 0, header.Lines.Count);

			var departureDates = new List<ZDateTime>() { new ZDateTime(2016, 10, 4) };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery();
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);
			AssertEquals("No new voyage is created", voyages.Length, newVoyages.Length);
		}

		public void TestNotCreateEnterpriseVoyage_WhenDepartureDateIsEmpty()
		{
			var query = new ZQuery();
			var voyages = Factory.Load<IJobVoyage>(query);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("Pre condition: 2 lines existed", 2, header.Lines.Count);

			var departureDates = new List<ZDateTime>() { ZDateTime.Empty };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery();
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);
			AssertEquals("No new voyage is created", voyages.Length, newVoyages.Length);
		}

		public void TestNotCreateEnterpriseVoyage_DeleteTemporaryVoyageWhenNoOriginDestinationSailingCreated()
		{
			var query = new ZQuery();
			var voyages = Factory.Load<IJobVoyage>(query);

			var testMessageLine = "100 SYD QQQ 11:15   07:00   08:35 QF CX        <SYD 3 QQQ 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			AssertEquals("Pre condition: 1 lines existed", 1, header.Lines.Count);

			var departureDates = new List<ZDateTime>() { new ZDateTime(2016, 10, 4) };
			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery();
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);
			AssertEquals("No origin/destination/sailing is created and the temporary voyage is deleted.", voyages.Length, newVoyages.Length);
		}

		public void TestNewEnterpriseVoyageShouldUseSpecifiedFlightDateWhenVeryOldVoyageExisted()
		{
			var departureDate = new ZDateTime(2016, 10, 4);
			var oldDate = departureDate.AddDays(-30);
			var departureDates = new List<ZDateTime>() { departureDate };

			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Prerequisite: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var query2 = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA4138");
			voyages = Factory.Load<IJobVoyage>(query2);
			AssertEquals("Prerequisite: the voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var oldVoyage = (BusinessObject)Factory.New<IJobVoyage>();
			var oldOrigin = (BusinessObject)Factory.New<IVoyageOrigin>();
			var oldDestination = (BusinessObject)Factory.New<IVoyageDestination>();
			var oldSailing = (BusinessObject)Factory.New<IJobSailing>();
			CreateVoyage(oldVoyage, oldOrigin, oldDestination, oldSailing, oldDate.AddHours(1), oldDate.AddHours(1), oldDate.AddHours(2));

			voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("The voyage with flight number BA9437 should exist now.", 1, voyages.Length);

			voyages = Factory.Load<IJobVoyage>(query2);
			AssertEquals("The voyage with flight number BA4138 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);
			Assert(!header.AnyLineHasMultipleAircraftTypes);

			RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, Factory);

			Factory.Save();

			AssertEquals(2, header.Lines.Count);
			var sailing1 = FindMatchingJobSailing(header.Lines[0], departureDate);
			var sailing2 = FindMatchingJobSailing(header.Lines[1], departureDate);

			AssertCreateEnterpriseVoyageAndSailing("BA9437", departureDate.AddHours(7), "332",
				"AUSYD", departureDate.AddHours(7),
				"AUMEL", departureDate.AddHours(8).AddMinutes(35),
				sailing1.PK, 2);

			AssertCreateEnterpriseVoyageAndSailing("BA4138", departureDate.AddHours(8).AddMinutes(50), "333",
				"AUMEL", departureDate.AddHours(8).AddMinutes(50),
				"HKHKG", departureDate.AddHours(15).AddMinutes(15),
				sailing2.PK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RoutingResponseHeader("", Factory);
		}

		void CreateVoyage(BusinessObject voyage, BusinessObject origin, BusinessObject destination, BusinessObject sailing,
			ZDateTime departureDate, ZDateTime originEstimatedDepartureTime, ZDateTime destEstimatedArrivalTime)
		{
			voyage[JobVoyageSchema.JV_VoyageFlight] = "BA9437";
			voyage[JobVoyageSchema.JV_FlightDate] = departureDate;
			voyage[JobVoyageSchema.JV_IsCargoOnly] = false;
			voyage[JobVoyageSchema.JV_AirSeaRoad] = Core.Constants.TransportModes.Air;
			voyage[JobVoyageSchema.JV_IsActive] = true;

			origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = "AUSYD";
			origin[JobVoyOriginSchema.JA_AutoCreated] = true;
			origin[JobVoyOriginSchema.JA_JV] = voyage.PK;
			origin[JobVoyOriginSchema.JA_E_DEP] = originEstimatedDepartureTime;

			destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = "AUMEL";
			destination[JobVoyDestinationSchema.JB_AutoCreated] = true;
			destination[JobVoyDestinationSchema.JB_JV] = voyage.PK;
			destination[JobVoyDestinationSchema.JB_E_ARV] = destEstimatedArrivalTime;

			sailing[JobSailingSchema.JX_JA] = origin.PK;
			sailing[JobSailingSchema.JX_JB] = destination.PK;
			sailing[JobSailingSchema.JX_IsPublished] = true;

			Factory.Save();
		}

		void AssertRoutingResponseHeaderContent(RoutingResponseHeader header)
		{
			AssertEquals("SYD", header.Origin);
			AssertEquals("Sydney", header.OriginDescription);
			AssertEquals("HKG", header.Destination);
			AssertEquals("Hong Kong", header.DestinationDescription);

			AssertEquals(1, header.Connections);
			AssertEquals(0, header.Stops);
			AssertEquals(0, header.Layers);

			AssertEquals("11:15", header.Duration);
			AssertEquals("07:00", header.DepartureTime);
			AssertEquals("15:15", header.ArrivalTime);

			AssertEquals("QF", header.Carrier1);
			AssertEquals("CX", header.Carrier2);
			AssertEquals("", header.Carrier3);
			AssertEquals("", header.Carrier4);

			AssertEquals(2, header.Lines.Count);

			AssertEquals("SYD", header.Lines[0].Origin);
			AssertEquals("3", header.Lines[0].OriginTerminal);
			AssertEquals("Sydney", header.Lines[0].OriginDescription);
			AssertEquals("MEL", header.Lines[0].Destination);
			AssertEquals("1", header.Lines[0].DestinationTerminal);
			AssertEquals("Melbourne", header.Lines[0].DestinationDescription);

			AssertEquals("07:00", header.Lines[0].DepartureTime);
			AssertEquals("08:35", header.Lines[0].ArrivalTime);
			AssertEquals("BA", header.Lines[0].TicketingCarrier);
			AssertEquals("7437", header.Lines[0].FlightNumber);
			AssertEquals(0, header.Lines[0].Stops);
			AssertEquals(439, header.Lines[0].MilesDistance);

			AssertEquals("MEL", header.Lines[1].Origin);
			AssertEquals("2", header.Lines[1].OriginTerminal);
			AssertEquals("Melbourne", header.Lines[1].OriginDescription);
			AssertEquals("HKG", header.Lines[1].Destination);
			AssertEquals("1", header.Lines[1].DestinationTerminal);
			AssertEquals("Hong Kong", header.Lines[1].DestinationDescription);

			AssertEquals("08:50", header.Lines[1].DepartureTime);
			AssertEquals("15:15", header.Lines[1].ArrivalTime);
			AssertEquals("BA", header.Lines[1].TicketingCarrier);
			AssertEquals("4138", header.Lines[1].FlightNumber);
			AssertEquals(0, header.Lines[1].Stops);
			AssertEquals(4590, header.Lines[1].MilesDistance);

			AssertEquals("12345__", header.OperationDay);
			AssertEquals("BA7437-BA4138", header.Flight);
			AssertEquals("PAS-PAS", header.FlightType);
			AssertEquals("332-333", header.Aircraft);
			AssertEquals("MEL", header.Via);
		}

		void AssertCreateEnterpriseVoyageAndSailing(ZString voyageFlight,
			ZDateTime expectedVoyageFlightDate, ZString expectedAircraftType,
			ZString expectedLoadPort, ZDateTime originEstimatedDepartureTime,
			ZString expectedDischargePort, ZDateTime destEstimatedArrivalTime,
			ZGuid expectedSailingPK, int expectedVoyageCount = 1,
			bool checkOtherExistedBusinessObject = false,
			ZGuid expectedVoyagePK = default,
			ZGuid expectedOriginPK = default,
			ZGuid expectedDestPK = default)
		{
			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageFlight)
			{
				OrderBy = JobVoyageSchema.JV_FlightDate.Name + OrderByClause.Descending
			};
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);
			AssertEquals(expectedVoyageCount, newVoyages.Length);

			var voyage = (BusinessObject)newVoyages[0];
			AssertEquals(expectedVoyageFlightDate, voyage[JobVoyageSchema.JV_FlightDate]);
			AssertEquals(false, voyage[JobVoyageSchema.JV_IsCargoOnly]);
			AssertEquals(Core.Constants.TransportModes.Air, voyage[JobVoyageSchema.JV_AirSeaRoad]);
			AssertEquals(true, voyage[JobVoyageSchema.JV_IsActive]);
			AssertEquals(expectedAircraftType, voyage[JobVoyageSchema.JV_AircraftType]);

			var originQuery = new ZQuery(JobVoyOriginSchema.JA_JV, voyage.PK);
			var origins = newFactory.Load<IVoyageOrigin>(originQuery);
			AssertEquals(1, origins.Length);

			var origin1 = (BusinessObject)origins.FirstOrDefault(origin => (ZString)((BusinessObject)origin)[JobVoyOriginSchema.JA_RL_NKPortOfLoading] == expectedLoadPort);
			AssertNotNull(origin1);
			AssertEquals(originEstimatedDepartureTime, origin1[JobVoyOriginSchema.JA_E_DEP]);
			AssertEquals(true, origin1[JobVoyOriginSchema.JA_AutoCreated]);

			var destQuery = new ZQuery(JobVoyDestinationSchema.JB_JV, voyage.PK);
			var dests = newFactory.Load<IVoyageDestination>(destQuery);
			AssertEquals(1, dests.Length);

			var dest1 = (BusinessObject)dests.FirstOrDefault(dest => (ZString)((BusinessObject)dest)[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] == expectedDischargePort);
			AssertNotNull(dest1);
			AssertEquals(destEstimatedArrivalTime, dest1[JobVoyDestinationSchema.JB_E_ARV]);
			AssertEquals(true, dest1[JobVoyDestinationSchema.JB_AutoCreated]);

			var sailingQuery = new ZQuery(JobSailingSchema.JX_JA, origin1.PK);
			var sailings = newFactory.Load<IJobSailing>(sailingQuery);
			AssertEquals(1, sailings.Length);

			var sailing1 = (BusinessObject)sailings[0];
			AssertEquals(expectedSailingPK, sailing1.PK);
			AssertEquals(dest1.PK, sailing1[JobSailingSchema.JX_JB]);
			AssertEquals(true, sailing1[JobSailingSchema.JX_IsPublished]);

			if (checkOtherExistedBusinessObject)
			{
				AssertEquals(expectedVoyagePK, voyage.PK);
				AssertEquals(expectedOriginPK, origin1.PK);
				AssertEquals(expectedDestPK, dest1.PK);
			}
		}

		BusinessObject FindMatchingJobSailing(RoutingResponseLine line, ZDateTime departureDate)
		{
			if (line != null)
			{
				var voyage = FindMatchingVoyage(line, departureDate);
				if (voyage != null)
				{
					var loadingPort = RoutingUpdaterHelper.GetUNLOCOFromIATACode(line.Origin, Factory);
					var dischargePort = RoutingUpdaterHelper.GetUNLOCOFromIATACode(line.Destination, Factory);
					if (!loadingPort.IsEmpty && !dischargePort.IsEmpty && loadingPort != dischargePort)
					{
						var origin = RoutingResponseHeader.FindMatchingOrigin(loadingPort, voyage);
						var destination = RoutingResponseHeader.FindMatchinDestination(dischargePort, voyage);
						if (origin != null && destination != null)
						{
							return RoutingResponseHeader.FindMatchingSailing(origin, destination);
						}
					}
				}
			}

			return null;
		}

		BusinessObject FindMatchingVoyage(RoutingResponseLine line, ZDateTime departureDate)
		{
			var voyageFlight = line.TicketingCarrier + line.FlightNumber;
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, voyageFlight);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Air);
			query.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.EqualToDatePartOnly, departureDate);
			query.OrderBy = JobVoyageSchema.JV_FlightDate.Name + OrderByClause.Descending;
			return (BusinessObject)Factory.LoadTop1<IJobVoyage>(query);
		}

		#endregion
	}
}
