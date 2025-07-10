using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingMultiDaysSelection))]
	public class RoutingMultiDaysSelectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMultiDaysSelectionCreation()
		{
			AssertEquals(new ZDateTime(2018, 7, 6), multiDaysSelection.FromDateLimit);
			AssertEquals(new ZDateTime(2018, 10, 8), multiDaysSelection.ToDateLimit);
			AssertEquals(true, multiDaysSelection.UseDailyPattern);
			AssertEquals(false, multiDaysSelection.UseWeeklyPattern);
			AssertEquals(false, multiDaysSelection.UseMonthlyPattern);

			var weeklyDaysCheckedList = multiDaysSelection.WeeklyDaysCheckedList;
			AssertEquals(4, weeklyDaysCheckedList.Count);
			AssertEquals(true, weeklyDaysCheckedList["Monday"].Value);
			AssertEquals(true, weeklyDaysCheckedList["Wednesday"].Value);
			AssertEquals(true, weeklyDaysCheckedList["Thursday"].Value);
			AssertEquals(true, weeklyDaysCheckedList["Saturday"].Value);

			AssertEquals(54, multiDaysSelection.DepartureDates.Count);
		}

		public void TestFlightNumber_ForMultipleFlightSchedules()
		{
			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.DailyFromDate = new ZDateTime(2018, 7, 9);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 15);
			AssertEquals(4, multiDaysSelection.DepartureDates.Count);
			AssertEquals(6, multiDaysSelection.GetFlightNumber());

			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.RangeFromDate = new ZDateTime(2018, 7, 9);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 15);
			AssertEquals(4, multiDaysSelection.DepartureDates.Count);
			AssertEquals(6, multiDaysSelection.GetFlightNumber());

			multiDaysSelection.UseMonthlyPattern = true;
			multiDaysSelection.MonthlyFromDate = new ZDateTime(2018, 7, 9);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 8, 9);
			AssertEquals(2, multiDaysSelection.DepartureDates.Count);
			AssertEquals(3, multiDaysSelection.GetFlightNumber());
		}

		public void TestUseDailyPattern()
		{
			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.DailyRecurEvery = 2;
			multiDaysSelection.DailyFromDate = new ZDateTime(2018, 7, 15);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 22);

			var departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(2, departureDates.Count);
			AssertEquals(new ZDateTime(2018, 7, 19), departureDates[0]);
			AssertEquals(new ZDateTime(2018, 7, 21), departureDates[1]);
		}

		public void TestUseDailyPattern_ConsolTemplate()
		{
			var multiDaysSelection1 = GetNewBusinessObjectForDailyPattern();
			AssertEquals("Precondition", new ZDateTime(2024, 3, 22), multiDaysSelection1.FromDateLimit);
			AssertEquals(new ZDateTime(2024, 3, 22), multiDaysSelection1.ToDateLimit);
			AssertEquals(1, multiDaysSelection1.GetFlightNumber());

			var consolTemplate = new ConsolTemplate(multiDaysSelection1);
			AssertEquals((ZShort)1, consolTemplate.ConsolsPerFlight);
			AssertEquals(1, consolTemplate.TotalConsols);

			consolTemplate.ConsolsPerFlight = 2;
			AssertEquals(2, consolTemplate.TotalConsols);
		}

		public void TestOnflightNumberChangedEvent()
		{
			int flightNumber = 0;
			multiDaysSelection.OnFlightNumberChanged += (sender, args) => { flightNumber = args.FlightNumber; };

			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.DailyRecurEvery = 2;
			multiDaysSelection.DailyFromDate = new ZDateTime(2018, 7, 15);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 22);

			var departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(2, departureDates.Count);
			AssertEquals(3, flightNumber);
		}

		public void TestUseWeeklyPattern()
		{
			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.WeeklyRecurEvery = 2;
			var weeklyDaysCheckedList = multiDaysSelection.WeeklyDaysCheckedList;
			weeklyDaysCheckedList["Monday"].Value = false;
			weeklyDaysCheckedList["Thursday"].Value = false;
			multiDaysSelection.RangeFromDate = new ZDateTime(2018, 7, 15);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 29);

			var departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(2, departureDates.Count);
			AssertEquals(new ZDateTime(2018, 7, 18), departureDates[0]);
			AssertEquals(new ZDateTime(2018, 7, 21), departureDates[1]);
		}

		public void TestUseMonthlyPattern()
		{
			multiDaysSelection.UseMonthlyPattern = true;
			multiDaysSelection.MonthlyRecurEvery = 2;

			var departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(1, departureDates.Count);
			AssertEquals(new ZDateTime(2018, 9, 6), departureDates[0]);
		}

		public void TestEmptyDepartureDates()
		{
			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.DailyFromDate = new ZDateTime(2018, 7, 15);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 15);
			var departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(0, departureDates.Count);

			multiDaysSelection.UseWeeklyPattern = true;
			var weeklyDaysCheckedList = multiDaysSelection.WeeklyDaysCheckedList;
			weeklyDaysCheckedList["Monday"].Value = false;
			weeklyDaysCheckedList["Wednesday"].Value = false;
			weeklyDaysCheckedList["Thursday"].Value = false;
			weeklyDaysCheckedList["Saturday"].Value = false;
			multiDaysSelection.RangeToDate = multiDaysSelection.ToDateLimit;
			departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(0, departureDates.Count);

			weeklyDaysCheckedList["Wednesday"].Value = true;
			weeklyDaysCheckedList["Saturday"].Value = true;
			multiDaysSelection.RangeFromDate = new ZDateTime(2018, 7, 15);
			multiDaysSelection.RangeToDate = new ZDateTime(2018, 7, 17);
			departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(0, departureDates.Count);

			multiDaysSelection.UseMonthlyPattern = true;
			multiDaysSelection.MonthlyRecurEvery = 3;
			multiDaysSelection.MonthlyFromDate = new ZDateTime(2018, 7, 10);
			multiDaysSelection.RangeFromDate = multiDaysSelection.FromDateLimit;
			multiDaysSelection.RangeToDate = multiDaysSelection.ToDateLimit;
			departureDates = multiDaysSelection.DepartureDates;
			AssertEquals(0, departureDates.Count);
		}

		public void TestGeneratedSailingsHaveMatchedScheduleStatus()
		{
			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			var multiDaysSelection = RoutingMultiDaysSelection.Create(requestedDate, CreateHeaderCollection(), false, false, Factory);
			AssertEquals(true, multiDaysSelection.RecurrenceEnabled);
			multiDaysSelection.CreateVoyagesSailings(departureDates);

			foreach (JobSailing smallChild in multiDaysSelection.SailingCollection)
			{
				AssertEquals(Core.Constants.FlightScheduleStatus.Matched, smallChild.JX_OnlineScheduleStatus);
			}
		}

		public void TestFirstCarrier()
		{
			var requestedDate = new ZDateTime(2018, 11, 20);
			var multiDaysSelection = RoutingMultiDaysSelection.Create(requestedDate, CreateHeaderCollection(), false, false, Factory);
			AssertEquals("QF", multiDaysSelection.FirstCarrier);
		}

		#region Implementation

		RoutingResponseHeaderCollection CreateHeaderCollection()
		{
			var messageLine = "100 SYD BNE 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 BNE 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(messageLine, Factory);
			Factory.Save();
			return new RoutingResponseHeaderCollection(Factory) { header };
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var requestDate = new ZDateTime(2018, 7, 6);
			var testMessageLine1 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1,
				header2
			};

			return RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
		}

		protected RoutingMultiDaysSelection GetNewBusinessObjectForDailyPattern()
		{
			var requestDate = new ZDateTime(2024, 3, 22);
			var testMessageLine1 =
				"000 SYD LAX 13:55   10:15   06:10 DL           <SYD 1 LAX B   10:15   06:10 DL    40    359     0  7487                                        .2..5.. 24/03/22 24/03/26 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1
			};

			return RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			multiDaysSelection = (RoutingMultiDaysSelection)GetNewBusinessObject();
		}

		RoutingMultiDaysSelection multiDaysSelection;

		#endregion
	}
}
