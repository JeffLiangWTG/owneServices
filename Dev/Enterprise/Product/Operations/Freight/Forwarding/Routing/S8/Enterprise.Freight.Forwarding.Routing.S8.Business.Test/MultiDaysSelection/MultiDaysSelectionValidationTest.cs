namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Freight.Forwarding.Routing.S8.Business;

	public class MultiDaysSelectionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDailyFromDate()
		{
			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);

			multiDaysSelection.DailyFromDate = ZDateTime.Empty;
			AssertHasError(multiDaysSelection.DailyFromDateInfo, "Please enter a From Date.");

			multiDaysSelection.DailyFromDate = ZDateTime.Invalid;
			AssertHasError(multiDaysSelection.DailyFromDateInfo, "Enter a valid From Date.");

			multiDaysSelection.DailyFromDate = rangeFrom.AddDays(-1);
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be before '{rangeFrom.ToShortDateString()}'.");

			multiDaysSelection.DailyFromDate = rangeFrom;
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);

			multiDaysSelection.RangeFromDate = rangeFrom.AddDays(1);
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be before '{rangeFrom.AddDays(1).ToShortDateString()}'.");

			multiDaysSelection.UseWeeklyPattern = true;
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be before '{rangeFrom.AddDays(1).ToShortDateString()}'.");

			multiDaysSelection.DailyFromDate = rangeFrom.AddDays(2);
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);

			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);

			multiDaysSelection.RangeFromDate = rangeFrom;
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);

			multiDaysSelection.DailyFromDate = rangeTo.AddMonths(1);
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be after '{rangeTo.ToShortDateString()}'.");
		}

		public void TestValidateDailyRecurEvery()
		{
			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.DailyRecurEveryInfo);

			multiDaysSelection.DailyRecurEvery = -1;
			AssertHasError(multiDaysSelection.DailyRecurEveryInfo, "The recurrence number must be between 1 and 366.");

			multiDaysSelection.DailyRecurEvery = 3;
			AssertNoNotifications(multiDaysSelection.DailyRecurEveryInfo);

			multiDaysSelection.DailyRecurEvery = 0;
			AssertHasError(multiDaysSelection.DailyRecurEveryInfo, "The recurrence number must be between 1 and 366.");

			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.DailyRecurEvery = -1;
			AssertNoNotifications(multiDaysSelection.DailyRecurEveryInfo);

			multiDaysSelection.UseDailyPattern = true;
			AssertHasError(multiDaysSelection.DailyRecurEveryInfo, "The recurrence number must be between 1 and 366.");

			multiDaysSelection.DailyRecurEvery = 3;
			AssertNoNotifications(multiDaysSelection.DailyRecurEveryInfo);
		}

		public void TestValidateWeeklyRecurEvery()
		{
			multiDaysSelection.UseWeeklyPattern = true;
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);

			multiDaysSelection.WeeklyRecurEvery = -1;
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");

			multiDaysSelection.WeeklyRecurEvery = 3;
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);

			multiDaysSelection.WeeklyRecurEvery = 0;
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");

			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.WeeklyRecurEvery = -1;
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);

			multiDaysSelection.UseWeeklyPattern = true;
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");

			multiDaysSelection.WeeklyRecurEvery = 3;
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);
		}

		public void TestValidateMonthlyFromDate()
		{
			multiDaysSelection.UseMonthlyPattern = true;
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);

			multiDaysSelection.MonthlyFromDate = ZDateTime.Empty;
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, "Please enter a From Date.");

			multiDaysSelection.MonthlyFromDate = ZDateTime.Invalid;
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, "Enter a valid From Date.");

			multiDaysSelection.MonthlyFromDate = rangeFrom.AddDays(-1);
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, $"'From Date' can not be before '{rangeFrom.ToShortDateString()}'.");

			multiDaysSelection.MonthlyFromDate = rangeFrom;
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);

			multiDaysSelection.RangeFromDate = rangeFrom.AddDays(1);
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, $"'From Date' can not be before '{rangeFrom.AddDays(1).ToShortDateString()}'.");

			multiDaysSelection.UseWeeklyPattern = true;
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, $"'From Date' can not be before '{rangeFrom.AddDays(1).ToShortDateString()}'.");

			multiDaysSelection.MonthlyFromDate = rangeFrom.AddDays(2);
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);

			multiDaysSelection.UseMonthlyPattern = true;
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);

			multiDaysSelection.RangeFromDate = rangeFrom;
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);

			multiDaysSelection.MonthlyFromDate = rangeTo.AddMonths(1);
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, $"'From Date' can not be after '{rangeTo.ToShortDateString()}'.");
		}

		public void TestValidateMonthlyRecurEvery()
		{
			multiDaysSelection.UseMonthlyPattern = true;
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			multiDaysSelection.MonthlyRecurEvery = -1;
			AssertHasError(multiDaysSelection.MonthlyRecurEveryInfo, "The recurrence number must be between 1 and 12.");

			multiDaysSelection.MonthlyRecurEvery = 3;
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			multiDaysSelection.MonthlyRecurEvery = 0;
			AssertHasError(multiDaysSelection.MonthlyRecurEveryInfo, "The recurrence number must be between 1 and 12.");

			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.MonthlyRecurEvery = -1;
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			multiDaysSelection.UseMonthlyPattern = true;
			AssertHasError(multiDaysSelection.MonthlyRecurEveryInfo, "The recurrence number must be between 1 and 12.");

			multiDaysSelection.MonthlyRecurEvery = 3;
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);
		}

		public void TestValidateRangeFromDate()
		{
			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.RangeFromDateInfo);

			multiDaysSelection.RangeFromDate = ZDateTime.Empty;
			AssertHasError(multiDaysSelection.RangeFromDateInfo, "Please enter a From Date.");

			multiDaysSelection.RangeFromDate = ZDateTime.Invalid;
			AssertHasError(multiDaysSelection.RangeFromDateInfo, "Enter a valid From Date.");

			multiDaysSelection.RangeFromDate = rangeFrom.AddDays(-1);
			AssertHasError(multiDaysSelection.RangeFromDateInfo, $"'From Date' can not be before '{rangeFrom.ToShortDateString()}'.");

			multiDaysSelection.RangeFromDate = rangeFrom;
			AssertNoNotifications(multiDaysSelection.RangeFromDateInfo);

			multiDaysSelection.UseWeeklyPattern = true;
			AssertNoNotifications(multiDaysSelection.RangeFromDateInfo);

			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.RangeFromDateInfo);

			multiDaysSelection.RangeFromDate = rangeTo.AddMonths(1);
			AssertHasError(multiDaysSelection.RangeFromDateInfo, "'From Date' must be before 'To Date'.");
		}

		public void TestValidateRangeToDate()
		{
			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.RangeToDateInfo);

			multiDaysSelection.RangeToDate = ZDateTime.Empty;
			AssertHasError(multiDaysSelection.RangeToDateInfo, "Please enter a To Date.");

			multiDaysSelection.RangeToDate = ZDateTime.Invalid;
			AssertHasError(multiDaysSelection.RangeToDateInfo, "Enter a valid To Date.");

			multiDaysSelection.RangeToDate = rangeFrom.AddDays(-1);
			AssertHasError(multiDaysSelection.RangeToDateInfo, "'From Date' must be before 'To Date'.");

			multiDaysSelection.RangeToDate = rangeFrom;
			AssertNoNotifications(multiDaysSelection.RangeToDateInfo);

			multiDaysSelection.UseWeeklyPattern = true;
			AssertNoNotifications(multiDaysSelection.RangeToDateInfo);

			multiDaysSelection.UseDailyPattern = true;
			AssertNoNotifications(multiDaysSelection.RangeToDateInfo);

			multiDaysSelection.RangeToDate = rangeFrom.AddMonths(5);
			AssertHasError(multiDaysSelection.RangeToDateInfo, $"'To Date' can not be after '{rangeTo.ToShortDateString()}'.");
		}

		public void TestValidateAll()
		{
			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.DailyFromDate = today.AddDays(8);
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be before '{rangeFrom.ToShortDateString()}'.");
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.WeeklyRecurEvery = 0;
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be before '{rangeFrom.ToShortDateString()}'.");
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			multiDaysSelection.UseMonthlyPattern = true;
			multiDaysSelection.MonthlyRecurEvery = -1;
			AssertHasError(multiDaysSelection.DailyFromDateInfo, $"'From Date' can not be before '{rangeFrom.ToShortDateString()}'.");
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");
			AssertHasError(multiDaysSelection.MonthlyRecurEveryInfo, "The recurrence number must be between 1 and 12.");

			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.Validation.ValidateAll();
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);
		}

		public void TestClearUnrelatedErrorsWhenRunningPreSaveValidation()
		{
			PrepareValidationErrors();
			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.RunPreSaveValidation();

			AssertEquals(true, multiDaysSelection.HasErrors);
			AssertHasError(multiDaysSelection.DailyFromDateInfo, "Enter a valid From Date.");
			AssertHasError(multiDaysSelection.DailyRecurEveryInfo, "The recurrence number must be between 1 and 366.");
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			PrepareValidationErrors();
			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.RunPreSaveValidation();
			AssertEquals(true, multiDaysSelection.HasErrors);
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);
			AssertNoNotifications(multiDaysSelection.DailyRecurEveryInfo);
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");
			AssertNoNotifications(multiDaysSelection.MonthlyFromDateInfo);
			AssertNoNotifications(multiDaysSelection.MonthlyRecurEveryInfo);

			PrepareValidationErrors();
			multiDaysSelection.UseMonthlyPattern = true;
			multiDaysSelection.RunPreSaveValidation();
			AssertEquals(true, multiDaysSelection.HasErrors);
			AssertNoNotifications(multiDaysSelection.DailyFromDateInfo);
			AssertNoNotifications(multiDaysSelection.DailyRecurEveryInfo);
			AssertNoNotifications(multiDaysSelection.WeeklyRecurEveryInfo);
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, "Please enter a From Date.");
			AssertHasError(multiDaysSelection.MonthlyRecurEveryInfo, "The recurrence number must be between 1 and 12.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			multiDaysSelection = GetMultiDaysSelection();
		}

		RoutingMultiDaysSelection GetMultiDaysSelection()
		{
			today = ZDateTime.Today;
			rangeFrom = today.AddDays(9);
			rangeTo = today.AddMonths(3);

			var testMessageLine1 =
				FormattableString.Invariant($"000 SYD WUH 10:55   11:20   20:15 MU           {today.ToString("yyyy/MM/dd")} {rangeTo.ToString("yyyy/MM/dd")} 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. {today.ToString("yy/MM/dd")} {rangeTo.ToString("yy/MM/dd")} J> ");
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 =
				FormattableString.Invariant($"000 SYD WUH 10:55   11:20   20:15 MU           {today.ToString("yyyy/MM/dd")} {rangeTo.ToString("yyyy/MM/dd")} 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. {today.ToString("yy/MM/dd")} {rangeTo.ToString("yy/MM/dd")} J> ");
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1,
				header2
			};

			return RoutingMultiDaysSelection.Create(rangeFrom, routingResponseHeaders, true, false, Factory);
		}

		ZDateTime today;
		ZDateTime rangeFrom;
		ZDateTime rangeTo;

		void PrepareValidationErrors()
		{
			multiDaysSelection.UseDailyPattern = true;
			multiDaysSelection.DailyFromDate = ZDateTime.Invalid;
			multiDaysSelection.DailyRecurEvery = -1;

			multiDaysSelection.UseWeeklyPattern = true;
			multiDaysSelection.WeeklyRecurEvery = -1;

			multiDaysSelection.UseMonthlyPattern = true;
			multiDaysSelection.MonthlyFromDate = ZDateTime.Empty;
			multiDaysSelection.MonthlyRecurEvery = -1;

			AssertHasError(multiDaysSelection.DailyFromDateInfo, "Enter a valid From Date.");
			AssertHasError(multiDaysSelection.DailyRecurEveryInfo, "The recurrence number must be between 1 and 366.");
			AssertHasError(multiDaysSelection.WeeklyRecurEveryInfo, "The recurrence number must be between 1 and 52.");
			AssertHasError(multiDaysSelection.MonthlyFromDateInfo, "Please enter a From Date.");
			AssertHasError(multiDaysSelection.MonthlyRecurEveryInfo, "The recurrence number must be between 1 and 12.");
		}

		RoutingMultiDaysSelection multiDaysSelection;

		#endregion
	}
}
