using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class RefTransitTimeDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTransitDays()
		{
			var transitTimeDetail = Factory.New<RefTransitTimeDetail>();
			AssertNoErrors(transitTimeDetail.TransitDaysInfo);

			transitTimeDetail.TransitDays = -5;
			AssertHasError(transitTimeDetail.TransitDaysInfo, "value cannot be negative.");

			transitTimeDetail.TransitDays = 5;
			AssertNoErrors(transitTimeDetail.TransitDaysInfo);
		}

		public void TestCheckTransitHours()
		{
			var transitTimeDetail = Factory.New<RefTransitTimeDetail>();
			AssertNoErrors(transitTimeDetail.TransitHoursInfo);

			transitTimeDetail.TransitHours = -5;
			AssertHasError(transitTimeDetail.TransitHoursInfo, "value cannot be negative.");

			transitTimeDetail.TransitHours = 5;
			AssertNoErrors(transitTimeDetail.TransitHoursInfo);

			transitTimeDetail.TransitHours = 25;
			AssertHasError(transitTimeDetail.TransitHoursInfo, "Hours must be less than 24. Please use the 'Days' component for longer times.");

			transitTimeDetail.TransitHours = 23;
			AssertNoErrors(transitTimeDetail.TransitHoursInfo);

			transitTimeDetail.RTD_ArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
			transitTimeDetail.TransitHours = 11;
			AssertHasError(transitTimeDetail.TransitHoursInfo, "When arrival time has a value, transit hours must be zero, and conversely.");

			transitTimeDetail.TransitDays = 1;
			transitTimeDetail.TransitHours = 0;
			AssertNoErrors(transitTimeDetail.TransitHoursInfo);

			transitTimeDetail.TransitHours = 0;
			transitTimeDetail.TransitDays = 0;
			AssertHasError(transitTimeDetail.TransitHoursInfo, "Please specify a Transit time greater than zero.");
		}

		public void TestCheckRTD_ArrivalTime()
		{
			var refTransitTimeDetail = Factory.New<RefTransitTimeDetail>();
			AssertNoErrors(refTransitTimeDetail.RTD_ArrivalTimeInfo);

			refTransitTimeDetail.TransitHours = 11;
			refTransitTimeDetail.RTD_ArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
			AssertHasError(refTransitTimeDetail.RTD_ArrivalTimeInfo, "When arrival time has a value, transit hours must be zero, and conversely.");

			refTransitTimeDetail.TransitHours = 0;
			refTransitTimeDetail.RTD_ArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
			AssertNoErrors(refTransitTimeDetail.RTD_ArrivalTimeInfo);
		}

		public void TestCheckRTD_EffectiveDateIsValidZDateTimeOffsetRange()
		{
			var refTransitTimeDetail = Factory.New<RefTransitTimeDetail>();
			AssertNoErrors(refTransitTimeDetail.RTD_EffectiveDateInfo);

			refTransitTimeDetail.RTD_EffectiveDate = new ZDateTimeOffset(1900, 1, 1, 1, 1, 1);
			AssertNoErrors(refTransitTimeDetail.RTD_EffectiveDateInfo);

			refTransitTimeDetail.RTD_EffectiveDate = new ZDateTimeOffset(4200, 1, 1, 1, 1, 1);
			AssertNoErrors(refTransitTimeDetail.RTD_EffectiveDateInfo);
		}

		public void TestCheckRTD_EndDateIsValidZDateTimeOffsetRange()
		{
			var refTransitTimeDetail = Factory.New<RefTransitTimeDetail>();
			AssertNoErrors(refTransitTimeDetail.RTD_EndDateInfo);

			refTransitTimeDetail.RTD_EndDate = new ZDateTimeOffset(1900, 1, 1, 1, 1, 1);
			AssertNoErrors(refTransitTimeDetail.RTD_EndDateInfo);

			refTransitTimeDetail.RTD_EndDate = new ZDateTimeOffset(4200, 1, 1, 1, 1, 1);
			AssertNoErrors(refTransitTimeDetail.RTD_EndDateInfo);
		}

		public void TestCheckOverlapOnNewEntryBasedOnEffectiveDateEndDateDayOfWeek()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			var transitTimeDetail1 = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail1.RTD_RTT_Parent = transitTime.PK;
			transitTimeDetail1.RTD_DayOfWeek = 1;
			transitTimeDetail1.RTD_EffectiveDate = new ZDateTimeOffset(2001, 1, 1, 1, 1, 1);
			transitTimeDetail1.RTD_EndDate = new ZDateTimeOffset(2022, 1, 1, 1, 1, 1);
			var transitTimeDetail2 = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail2.RTD_RTT_Parent = transitTime.PK;
			transitTimeDetail2.RTD_DayOfWeek = 1;
			transitTimeDetail2.RTD_EffectiveDate = new ZDateTimeOffset(2011, 1, 1, 1, 1, 1);
			transitTimeDetail2.RTD_EndDate = new ZDateTimeOffset(2023, 1, 1, 1, 1, 1);

			AssertHasError(transitTimeDetail2.RTD_EffectiveDateInfo, "The Effective/End Dates overlap on another record for the same Day of the Week , please select different dates.");

			transitTimeDetail2.RTD_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 1, 1, 1);
			transitTimeDetail2.Validation.ValidateDayOfWeek();

			AssertNoError(transitTimeDetail2.RTD_EffectiveDateInfo, "The Effective/End Dates overlap on another record for the same Day of the Week , please select different dates.");
		}

		public void TestArrivalTimeValidZDateTimeRange()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			var transitTimeDetail = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail.RTD_RTT_Parent = transitTime.PK;
			transitTimeDetail.RTD_ArrivalTime = new ZDateTime(1900, 1, 1, 10, 10, 0);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestCheckDayOfWeek()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			var transitTimeDetail1 = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail1.RTD_RTT_Parent = transitTime.PK;

			transitTimeDetail1.RTD_DayOfWeek = 0;
			transitTimeDetail1.Validation.ValidateDayOfWeek();

			AssertHasError(transitTimeDetail1.DayOfWeekInfo, "The Correct Day Of week should be selected");

			transitTimeDetail1.RTD_DayOfWeek = 8;
			transitTimeDetail1.Validation.ValidateDayOfWeek();
			AssertHasError(transitTimeDetail1.DayOfWeekInfo, "The Correct Day Of week should be selected");

			transitTimeDetail1.RTD_DayOfWeek = 1;
			var transitTimeDetail2 = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail2.RTD_RTT_Parent = transitTime.PK;

			transitTimeDetail2.RTD_DayOfWeek = 1;
			transitTimeDetail2.Validation.ValidateDayOfWeek();

			AssertHasError(transitTimeDetail2.DayOfWeekInfo, "The Effective/End Dates overlap on another record for the same Day of the Week , please select different dates.");

			transitTimeDetail1.RTD_EffectiveDate = new ZDateTimeOffset(2001, 1, 1, 1, 1, 1);
			transitTimeDetail1.RTD_EndDate = new ZDateTimeOffset(2022, 1, 1, 1, 1, 1);

			transitTimeDetail2.RTD_EffectiveDate = new ZDateTimeOffset(2011, 1, 1, 1, 1, 1);
			transitTimeDetail2.RTD_EndDate = new ZDateTimeOffset(2023, 1, 1, 1, 1, 1);

			transitTimeDetail2.Validation.ValidateDayOfWeek();
			AssertHasError(transitTimeDetail2.DayOfWeekInfo, "The Effective/End Dates overlap on another record for the same Day of the Week , please select different dates.");

			transitTimeDetail2.RTD_EffectiveDate = new ZDateTimeOffset(2022, 1, 1, 1, 1, 1);
			transitTimeDetail2.Validation.ValidateDayOfWeek();

			AssertNoError(transitTimeDetail2.DayOfWeekInfo, "The Effective/End Dates overlap on another record for the same Day of the Week , please select different dates.");
		}

		public void TestCheckDayOfWeekWhenIsEmptyAndTransitHoursHasValue()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			var transitTimeDetail = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail.RTD_RTT_Parent = transitTime.PK;
			transitTimeDetail.TransitHours = 11;
			transitTimeDetail.Validation.ValidateAll();
			AssertHasError(transitTimeDetail.DayOfWeekInfo, "The Correct Day Of week should be selected");
		}

		public void TestValidateEffectiveAndEndDate()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			var transitTimeDetail1 = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail1.RTD_RTT_Parent = transitTime.PK;

			transitTimeDetail1.RTD_EffectiveDate = new ZDateTimeOffset(2020, 10, 12);
			transitTimeDetail1.RTD_EndDate = new ZDateTimeOffset(2020, 10, 11);

			AssertHasError(transitTimeDetail1.RTD_EndDateInfo, "End Date Should be greater than Effective Date");
		}
	}
}



