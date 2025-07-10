using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class DeliverOnWeekendCalculationStepTest : TestCaseWithFactory
	{
		public void TestCalculate_DeliverOnSaturday()
		{
			serviceLevel.RS_DeliverOnSaturday = true;
			serviceLevel.RS_DeliverOnSunday = false;
			Factory.Save();

			var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

			var dateTimeWeekDay = new ZDateTime(2023, 9, 26, 12, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Next Saturday", new ZDateTime(2023, 9, 30, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD, has delivery on weekend ticked. The next available weekend was: 30-Sep-23 00:00:00 which is Saturday, so we picked that weekend as delivery due date.
", result.CalculationLog);

			var dateTimeSaturday = new ZDateTime(2023, 9, 30, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Same day", new ZDateTime(2023, 9, 30, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD, has delivery on weekend ticked. The next available weekend was: 30-Sep-23 00:00:00 which is Saturday, so we picked that weekend as delivery due date.
", result.CalculationLog);

			var dateTimeSunday = new ZDateTime(2023, 10, 1, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSunday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Next Saturday", new ZDateTime(2023, 10, 7, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD, has delivery on weekend ticked. The next available weekend was: 07-Oct-23 00:00:00 which is Saturday, so we picked that weekend as delivery due date.
", result.CalculationLog);

			AddRecurringDateHolidayToState(state, new ZDateTime(2023, 9, 30, 9, 0, 0));
			Factory.Save();
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Saturday is holiday, Sunday is weekend, result should be next Monday", new ZDateTime(2023, 10, 2, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD has delivery on weekend ticked, but 30-Sep-23 00:00:00 are public holiday(s).
Delivery due date adjusted from 26-Sep-23 12:00:00 to 02-Oct-23 00:00:00.
", result.CalculationLog);
		}

		public void TestCalculate_DeliverOnSunday()
		{
			serviceLevel.RS_DeliverOnSaturday = false;
			serviceLevel.RS_DeliverOnSunday = true;
			Factory.Save();

			var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

			var dateTimeWeekDay = new ZDateTime(2023, 9, 26, 12, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Next Sunday", new ZDateTime(2023, 10, 1, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD, has delivery on weekend ticked. The next available weekend was: 01-Oct-23 00:00:00 which is Sunday, so we picked that weekend as delivery due date.
", result.CalculationLog);

			var dateTimeSaturday = new ZDateTime(2023, 9, 30, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Still Sunday", new ZDateTime(2023, 10, 1, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD, has delivery on weekend ticked. The next available weekend was: 01-Oct-23 00:00:00 which is Sunday, so we picked that weekend as delivery due date.
", result.CalculationLog);

			var dateTimeSunday = new ZDateTime(2023, 10, 1, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSunday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Same day", new ZDateTime(2023, 10, 1, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD, has delivery on weekend ticked. The next available weekend was: 01-Oct-23 00:00:00 which is Sunday, so we picked that weekend as delivery due date.
", result.CalculationLog);

			AddRecurringDateHolidayToState(state, new ZDateTime(2023, 10, 1, 9, 0, 0));
			Factory.Save();
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Sunday is holiday, result should be next Monday", new ZDateTime(2023, 10, 2, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD has delivery on weekend ticked, but 01-Oct-23 00:00:00 are public holiday(s).
Delivery due date adjusted from 26-Sep-23 12:00:00 to 02-Oct-23 00:00:00.
", result.CalculationLog);
		}

		public void TestCalculate_DeliverOnSaturdayAndSunday()
		{
			serviceLevel.RS_DeliverOnSaturday = true;
			serviceLevel.RS_DeliverOnSunday = true;
			Factory.Save();

			var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

			var dateTimeWeekDay = new ZDateTime(2023, 9, 26, 12, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Saturday", new ZDateTime(2023, 9, 30, 12, 0, 0), result.DeliveryDueDate);

			var dateTimeSaturday = new ZDateTime(2023, 9, 30, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Saturday", new ZDateTime(2023, 9, 30, 12, 0, 0), result.DeliveryDueDate);

			var dateTimeSunday = new ZDateTime(2023, 10, 1, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSunday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Sunday", new ZDateTime(2023, 10, 1, 12, 0, 0), result.DeliveryDueDate);

			AddRecurringDateHolidayToState(state, new ZDateTime(2023, 10, 1, 9, 0, 0));
			Factory.Save();
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSunday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Sunday is public holiday, result should be next Monday", new ZDateTime(2023, 10, 2, 12, 0, 0), result.DeliveryDueDate);

			AddRecurringDateHolidayToState(state, new ZDateTime(2023, 9, 30, 9, 0, 0));
			Factory.Save();
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Saturday and Sunday are public holidays, result should be next Monday", new ZDateTime(2023, 10, 2, 12, 0, 0), result.DeliveryDueDate);

			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Saturday and Sunday are public holidays, result should be next Monday", new ZDateTime(2023, 10, 2, 12, 0, 0), result.DeliveryDueDate);
		}

		public void TestCalculate_WeekendNotOnSatAndSun()
		{
			serviceLevel.RS_DeliverOnSaturday = true;
			serviceLevel.RS_DeliverOnSunday = true;
			country.IsFridayNonWorkingDay = true;
			address.Timetables.NotApplicable = true;
			Factory.Save();

			var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

			var dateTimeSaturday = new ZDateTime(2023, 9, 30, 12, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Friday is Weekend Day 1", new ZDateTime(2023, 10, 6, 12, 0, 0), result.DeliveryDueDate);

			AddRecurringDateHolidayToState(state, new ZDateTime(2023, 10, 6, 9, 0, 0));
			Factory.Save();
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Friday is Weekend Day 1, but it's public holiday, deliver on Saturday", new ZDateTime(2023, 10, 7, 12, 0, 0), result.DeliveryDueDate);

			state.IsNonWorkingDaysOverrided = true;
			state.IsThursdayNonWorkingDay = true;
			Factory.Save();
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeSaturday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Thursday is Weekend Day 1, Friday is Weekend Day 2, deliver on Thursday", new ZDateTime(2023, 10, 5, 12, 0, 0), result.DeliveryDueDate);

			var dateTimeFridayPublicHoliday = new ZDateTime(2023, 10, 6, 12, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeFridayPublicHoliday, ZString.Empty);
			result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Friday is public holiday, deliver on Saturday", new ZDateTime(2023, 10, 7, 12, 0, 0), result.DeliveryDueDate);
		}

		public void TestDeliverOnWeekendShouldThrowExceptionWhenAddressIsNull()
		{
			address = null;
			Factory.Save();

			AssertExceptionThrown<Exception>(() =>
			{
				var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

				var dateTimeWeekDay = new ZDateTime(2023, 9, 26, 12, 0, 0);
				var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
				var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
				AssertEquals("Delivery Due Date calculation failed because there's an exception.", result.ErrorMessage);
			});
		}

		public void TestCalculate_DeliverOnSaturdayAndSunday_SaturdayIsPublicHoliday_LogShouldShowPublicHoliday()
		{
			serviceLevel.RS_DeliverOnSaturday = true;
			serviceLevel.RS_DeliverOnSunday = true;

			AddRecurringDateHolidayToState(state, new ZDateTime(2024, 11, 9, 9, 0, 0));

			Factory.Save();

			var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

			var dateTimeWeekDay = new ZDateTime(2024, 11, 9, 12, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Sunday", new ZDateTime(2024, 11, 10, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD has delivery on weekend ticked, but 09-Nov-24 00:00:00 is a public holiday. The next available weekend was: 10-Nov-24 00:00:00 which is Sunday, so we picked that weekend as delivery due date.
", result.CalculationLog);
		}

		public void TestCalculate_DeliveryOnSaturday_SaturdayAndNextMondayArePublicHolidays_LogShouldShowAllPublicHoliday()
		{
			serviceLevel.RS_DeliverOnSaturday = true;

			AddRecurringDateHolidayToState(state, new ZDateTime(2024, 9, 21, 9, 0, 0));
			AddRecurringDateHolidayToState(state, new ZDateTime(2024, 9, 23, 9, 0, 0));

			Factory.Save();

			var deliverOnWeekendCalculationStep = new DeliverOnWeekendCalculationStep(CreateDeliveryDueDateCalculationContext());

			var dateTimeWeekDay = new ZDateTime(2024, 9, 17, 12, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeWeekDay, ZString.Empty);
			var result = deliverOnWeekendCalculationStep.Calculate(initialInput);
			AssertEquals("Tuesday", new ZDateTime(2024, 9, 24, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Service Level STD has delivery on weekend ticked, but 21-Sep-24 00:00:00,23-Sep-24 00:00:00 are public holiday(s).
Delivery due date adjusted from 17-Sep-24 12:00:00 to 24-Sep-24 00:00:00.
", result.CalculationLog);
		}

		protected override void SetUp()
		{
			base.SetUp();

			country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "XX";
			state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "YY";
			state.RW_RN_NKCountryCode = "XX";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			address = org.Addresses[0];
			address.OA_RN_NKCountryCode = "XX";
			address.State = "YY";
			var orgTimeTableCollection = new OrgTimetableCollection(address);

			serviceLevel = DeliveryDueDateCalculationTestHelper.GetOrCreateRefServiceLevelIfNotExist(Factory, "STD");

			Factory.Save();
		}

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext()
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				address.Header.OH_Code,
				address.AddressCode,
				ZString.Empty,
				ZString.Empty );
		}

		void AddRecurringDateHolidayToState(RefCountryStates state, ZDateTime date)
		{
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = date;
			glbHoliday.GH_IsWorkingDay = false;
			glbHoliday.GH_Recurring = true;
			glbHoliday.GH_ParentID = state.PK;
			glbHoliday.GH_ParentTableCode = "RW";
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = true;
		}

		OrgAddress address;
		RefCountry country;
		RefCountryStates state;
		RefServiceLevel serviceLevel;
	}
}
