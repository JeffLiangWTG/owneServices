using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class PreparationTimeAtCFSDDDCalculationStepTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 8 * 60, OrgTimetableType.Codes.Pickup);
			Factory.Save();

			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 10, 2, 0, 0, 0));
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);
			var preparationTimeAtCFSStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var sunday1Am = new ZDateTime(2022, 10, 2, 1, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(sunday1Am, ZString.Empty);
			var result = preparationTimeAtCFSStep.Calculate(initialInput);
			AssertEquals("Initial datetime does not fall in Opening Hours, start at 9am on Monday and finish at 5pm the same day", new ZDateTime(2022, 10, 3, 17, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"[Header #1] non working days: Sunday 02-Oct-22; 
Adding processing time step for PIC: 02-Oct-22 01:00:00 wasn't matched with opening hours of [Header #1]. Closest opening hour was: 03-Oct-22 09:00:00
Processing time at PIC CFS for Monday: 8 Hours
Adding processing time step for PIC: 03-Oct-22 09:00:00 adjusted to 03-Oct-22 17:00:00 based on time table of PIC CFS
", result.CalculationLog);

			DeliveryDueDateCalculationTestHelper.SetCutOffTime(orgAddress, new ZDateTime(1900, 1, 1, 16, 0, 0), OrgTimetableType.Codes.Pickup);
			initialInput = DeliveryDueDateCalculationResult.Success(sunday1Am, ZString.Empty);
			result = preparationTimeAtCFSStep.Calculate(initialInput);
			AssertEquals(@"CFS address cut off time is 4pm, so to will be shipped the next working day at 9am Wednesday.", new ZDateTime(2022, 10, 4, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"[Header #1] non working days: Sunday 02-Oct-22; 
Adding processing time step for PIC: 02-Oct-22 01:00:00 wasn't matched with opening hours of [Header #1]. Closest opening hour was: 03-Oct-22 09:00:00
Processing time at PIC CFS for Monday: 8 Hours
Adding processing time step for PIC: 03-Oct-22 09:00:00 adjusted to 03-Oct-22 17:00:00 based on time table of PIC CFS
Cutoff Time on Monday for PIC CFS: 16:00
Calculated time is after Cutoff time. Finding next business day.
Applying Cutoff time for PIC: 03-Oct-22 17:00:00 adjusted to 04-Oct-22 09:00:00 based on time table of PIC CFS
", result.CalculationLog);
		}

		public void TestTryResetAdjustedFinalResultIfNecessaryWillNotThrowExceptionWhenTimeAfterProcessingIsNull()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 8 * 60, OrgTimetableType.Codes.Pickup);
			Factory.Save();

			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 10, 2, 0, 0, 0));
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);
			var preparationTimeAtCFSStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var tryResetAdjustedFinalResultIfNecessary = typeof(PreparationTimeAtCFSDDDCalculationStep).GetMethod("TryResetAdjustedFinalResultIfNecessary", BindingFlags.NonPublic | BindingFlags.Instance);

			var calculationLogBuilder = new ZStringBuilder();
			var timeAfterProcessing = new ZDateTime(2023, 11, 7, 9, 0, 0);
			var finalResult = new ZDateTime(2023, 11, 10, 10, 0, 0);

			AssertNoExceptionThrown(() => tryResetAdjustedFinalResultIfNecessary.Invoke(preparationTimeAtCFSStep, new object[] { calculationLogBuilder, timeAfterProcessing, null }));
		}

		public void TestCalculate_ClosureTime()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 5 * 60, OrgTimetableType.Codes.Pickup);
			Factory.Save();

			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 10, 8, 0, 0, 0));
			holidays.Add(new DateTime(2022, 10, 9, 0, 0, 0));
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);
			var preparationTimeAtCFSCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true,orgAddress), OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var friday5Pm = new ZDateTime(2022, 10, 7, 17, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(friday5Pm, ZString.Empty);
			var result = preparationTimeAtCFSCalculationStep.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 10, 10, 14, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Adding processing time step for PIC: 07-Oct-22 17:00:00 is after address closure time. Checking for next available time...
[Header #1] non working days: Saturday 08-Oct-22; Sunday 09-Oct-22; 
07-Oct-22 17:00:00 adjusted to 10-Oct-22 09:00:00 due to opening hours of [Header #1]
Processing time at PIC CFS for Monday: 5 Hours
Adding processing time step for PIC: 10-Oct-22 09:00:00 adjusted to 10-Oct-22 14:00:00 based on time table of PIC CFS
", result.CalculationLog);
		}

		public void TestCalculate_UseHoldForPickupTime()
		{
			var defaultHoldForPickupTime = ZDateTime.DefaultDurationEpoch.AddHours(12).TimeOfDay;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 10, 1, 0, 0, 0));
			holidays.Add(new DateTime(2022, 10, 2, 0, 0, 0));
			holidays.Add(new DateTime(2022, 10, 8, 0, 0, 0));
			holidays.Add(new DateTime(2022, 10, 9, 0, 0, 0));
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);

			var preparationTimeAtCFSCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			FieldInfo holdForPickupTime = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("holdForPickupTime", BindingFlags.NonPublic | BindingFlags.Instance);
			holdForPickupTime.SetValue(preparationTimeAtCFSCalculationStep, defaultHoldForPickupTime);

			var friday9Am = new ZDateTime(2022, 9, 30, 9, 0, 0);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 2 * 60, OrgTimetableType.Codes.Deliver);
			var initialInput = DeliveryDueDateCalculationResult.Success(friday9Am, ZString.Empty);
			var result = preparationTimeAtCFSCalculationStep.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 9, 30, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Processing time at DLV CFS for Friday: 2 Hours
Adding processing time step for DLV: 30-Sep-22 09:00:00 adjusted to 30-Sep-22 11:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:00:00
Set Hold For Pickup Time: 30-Sep-22 11:00:00 adjusted to 30-Sep-22 12:00:00 based on DLV CFS's Hold for pickup time
", result.CalculationLog);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 4 * 60, OrgTimetableType.Codes.Deliver);
			initialInput = DeliveryDueDateCalculationResult.Success(friday9Am, ZString.Empty);
			result = preparationTimeAtCFSCalculationStep.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 10, 3, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Processing time at DLV CFS for Friday: 4 Hours
Adding processing time step for DLV: 30-Sep-22 09:00:00 adjusted to 30-Sep-22 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:00:00
Adding one extra day: 30-Sep-22 13:00:00 adjusted to 01-Oct-22 12:00:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 01-Oct-22; Sunday 02-Oct-22; 
01-Oct-22 12:00:00 adjusted to 03-Oct-22 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 03-Oct-22 09:00:00 adjusted to 03-Oct-22 12:00:00 based on DLV CFS's Hold for pickup time
", result.CalculationLog);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, (2 * 8 + 2) * 60, OrgTimetableType.Codes.Deliver);
			initialInput = DeliveryDueDateCalculationResult.Success(friday9Am, ZString.Empty);
			result = preparationTimeAtCFSCalculationStep.Calculate(initialInput);
			AssertEquals(new ZDateTime(2022, 10, 4, 12, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Processing time at DLV CFS for Friday: 18 Hours
[Header #1] non working days: Saturday 01-Oct-22; Sunday 02-Oct-22; 
Adding processing time step for DLV: 30-Sep-22 09:00:00 adjusted to 04-Oct-22 11:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:00:00
Set Hold For Pickup Time: 04-Oct-22 11:00:00 adjusted to 04-Oct-22 12:00:00 based on DLV CFS's Hold for pickup time
", result.CalculationLog);
		}

		public void TestCalculate_ArrivalTimeUsedAtDeliveryCFS()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 4 * 60, OrgTimetableType.Codes.Deliver);
			Factory.Save();

			var holidays = new List<DateTime>();
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);
			var monday1am = new ZDateTime(2022, 10, 3, 1, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday1am, ZString.Empty, arrivalTimeUsedForDeliveryCFS: false);
			var preparationTimeAtCFSCalculationStepWithoutSkip = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			AssertEquals("Initial datetime does not fall in Opening Hours, start at 9am on Monday and finish at 1pm the same day", new ZDateTime(2022, 10, 3, 13, 0, 0), preparationTimeAtCFSCalculationStepWithoutSkip.Calculate(initialInput).DeliveryDueDate);

			var preparationTimeAtCFSCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			initialInput = DeliveryDueDateCalculationResult.Success(monday1am, ZString.Empty, arrivalTimeUsedForDeliveryCFS: true);
			AssertEquals("Opening hours check should be skipped, start at 1am on Monday and finish at 5am the same day", new ZDateTime(2022, 10, 3, 5, 0, 0), preparationTimeAtCFSCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_SplitAcrossMultipleDays_DefaultTimetable()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var preparationTimeCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var friday9Am = new ZDateTime(2022, 9, 30, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(friday9Am, ZString.Empty);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 8 * 60, OrgTimetableType.Codes.Deliver);
			AssertEquals("Same day preparation time: 8 hrs, start at 9am on Friday, finish at 5pm the same day", new ZDateTime(2022, 9, 30, 17, 0, 0), preparationTimeCalculationStep.Calculate(initialInput).DeliveryDueDate);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, (2 * 8 + 2) * 60, OrgTimetableType.Codes.Deliver);
			AssertEquals("Multi days preparation time: 18 hrs (2 days + 2 hrs), start at 9am on Friday, finish at 11am next Tuesday", new ZDateTime(2022, 10, 4, 11, 0, 0), preparationTimeCalculationStep.Calculate(initialInput).DeliveryDueDate);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, (2 * 5 * 8 + 2 * 8 + 2) * 60, OrgTimetableType.Codes.Deliver);
			AssertEquals("Multi weeks preparation time: 18 hrs (2 weeks + 2 days + 2 hrs), start at 9am on Friday 30/09/2022, finish at 11am on Tuesday 18/10/2022", new ZDateTime(2022, 10, 18, 11, 0, 0), preparationTimeCalculationStep.Calculate(initialInput).DeliveryDueDate);

			var saturday9Am = new ZDateTime(2022, 10, 1, 10, 9, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(saturday9Am, ZString.Empty);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 30, OrgTimetableType.Codes.Deliver);
			AssertEquals("Initial datetime does not fall in Opening Hours, start at 9am on next Monday", new ZDateTime(2022, 10, 3, 9, 30, 0), preparationTimeCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_SplitAcrossMultipleDays_CustomTimetable()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			orgAddress.Timetables.DeleteAll();
			orgAddress.Timetables[0].OTT_TimeFrom = new DateTime(2022, 1, 1, 12, 0, 0);
			orgAddress.Timetables[0].OTT_TimeTo = new DateTime(2022, 1, 1, 13, 0, 0);
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var preparationTimeCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var friday9Am = new ZDateTime(2022, 9, 30, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(friday9Am, ZString.Empty);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 8 * 60, OrgTimetableType.Codes.Deliver);
			AssertEquals("CFS address opens 1 hour a week on Monday from Noon to 1pm. 8 hrs preparation should be done in 8 weeks i.e. by the end of Monday 21/11/2022", new ZDateTime(2022, 11, 21, 13, 0, 0), preparationTimeCalculationStep.Calculate(initialInput).DeliveryDueDate);

			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 13, 0, 14, 0, true, "MON");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 15, 0, 17, 0, true, "MON");
			AssertEquals("CFS address opens 4 hours on Monday from Noon to 2pm and 3pm to 5pm. 8 hrs preparation should be done in 2 weeks i.e. by the end of Monday 10/10/2022", new ZDateTime(2022, 10, 10, 17, 0, 0), preparationTimeCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_SplitAcrossMultipleDays_CutOffTime()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			orgAddress.Timetables.DeleteAll();
			orgAddress.Timetables[0].OTT_Monday = false;
			orgAddress.Timetables[0].OTT_Friday = true;
			orgAddress.Timetables[0].OTT_TimeFrom = new DateTime(1900, 1, 1, 9, 0, 0);
			orgAddress.Timetables[0].OTT_TimeTo = new DateTime(1900, 1, 1, 17, 0, 0);
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 17, 0, true, "MON", cutOffTime: new DateTime(1900, 1, 1, 14, 0, 0));
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 17, 0, true, "TUE", cutOffTime: new DateTime(1900, 1, 1, 14, 0, 0));
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var preparationTimeCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);
			var friday2pm = new ZDateTime(2022, 9, 30, 14, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(friday2pm, ZString.Empty);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 9 * 60, OrgTimetableType.Codes.Deliver);
			var result = preparationTimeCalculationStep.Calculate(initialInput);
			AssertEquals("Preparation time: 9 hrs, start at 2pm on Friday, finish at 3pm next Monday", new ZDateTime(2022, 10, 4, 9, 0, 0), result.DeliveryDueDate);
			AssertContains("Should use Monday's cutoff time", "Cutoff Time on Monday for DLV CFS: 14:00", result.CalculationLog);
		}

		public void TestProcessingTimeAndCutoffTimeShouldBeIgnoredWhenCFSAddressIsNull()
		{
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var preparationTimeCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);

			var result = preparationTimeCalculationStep.Calculate(initialInput);
			AssertEquals("Process Time and Cutoff Time should be ignored when address is null", initialDateTime, result.DeliveryDueDate);
			AssertEquals("Finding Processing and Cutoff Time: Skipped\r\n", result.CalculationLog);
		}

		public void TestPreparationTimeAtCFSShouldNotThrowExceptionWhenAddressIsNull()
		{
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var preparationTimeCalculationStep = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);

			AssertNoExceptionThrown(() => preparationTimeCalculationStep.Calculate(initialInput));
		}

		public void TestCalculate_DeliverOnWeekends()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(orgAddress, 4 * 60, OrgTimetableType.Codes.Deliver);
			Factory.Save();

			var holidays = new List<DateTime>();
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);
			var monday1am = new ZDateTime(2022, 10, 3, 1, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday1am, ZString.Empty, arrivalTimeUsedForDeliveryCFS: false);
			var preparationTimeAtCFSCalculationStepWeekendDelivery = new PreparationTimeAtCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			FieldInfo deliverOnWeekendField = typeof(PreparationTimeAtCFSDDDCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendField.SetValue(preparationTimeAtCFSCalculationStepWeekendDelivery, true);

			var result = preparationTimeAtCFSCalculationStepWeekendDelivery.Calculate(initialInput);
			AssertEquals("Processing time Skipped because of Delivery on Weekend", initialInput.DeliveryDueDate, result.DeliveryDueDate);
			AssertContains("Should skip processing for delivery on weekends", "Processing time Skipped because of Delivery on Weekend", result.CalculationLog);
		}

		#region Helper Methods

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext(bool isPickup = false, OrgAddress orgAddress = null)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				isPickup ? orgAddress?.Header.OH_Code ?? ZString.Empty : ZString.Empty,
				isPickup ? orgAddress?.AddressCode ?? ZString.Empty : ZString.Empty,
				isPickup ? ZString.Empty : orgAddress?.Header.OH_Code ?? ZString.Empty,
				isPickup ? ZString.Empty : orgAddress?.AddressCode ?? ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty
			);
		}

		#endregion
	}
}
