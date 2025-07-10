using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.DeliveryDueDateCalculator;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class OpeningHoursDDDCalculationStepTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "WISGLOSYD";
			orgHeader.OH_FullName = "Wisetech Global";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "357/4 Bindon Place";
			orgAddress.OA_PostCode = "2217";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_State = "NSW";

			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var dateTimeInOpeningHours = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(dateTimeInOpeningHours, ZString.Empty);

			AssertEquals("Initial date time is in opening hours, final date time should be initial date time",
				dateTimeInOpeningHours, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
			AssertEquals("Initial date time is in opening hours, final date time should be initial date time. CalculationLog is empty",
				ZString.Empty, openingHoursCalculationStep.Calculate(initialInput).CalculationLog);

			var dateTimeIsAfterOpeningHoursOnSameDay = new ZDateTime(2022, 9, 26, 23, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeIsAfterOpeningHoursOnSameDay, ZString.Empty);

			AssertEquals("Initial date time is after opening hours on same day, final date time should be first hour on the next opening day",
				new ZDateTime(2022, 9, 27, 9, 0, 0), openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
			AssertEquals("Initial date time is after opening hours on same day, final date time should be first hour on the next opening day. CalculationLog should reflect this",
				@"26-Sep-22 23:00:00 wasn't matched with opening hours of [Wisetech Global 357/4 Bindon Place Sydney NSW 2217]. Closest opening hour was: 27-Sep-22 09:00:00
",
				openingHoursCalculationStep.Calculate(initialInput).CalculationLog);

			var dateTimeIsBeforeOpeningHoursOnSameDay = new ZDateTime(2022, 9, 26, 5, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTimeIsBeforeOpeningHoursOnSameDay, ZString.Empty);

			AssertEquals("Initial date time is before opening hours on same day, final date time should be first opening hour on the same day",
				new ZDateTime(2022, 9, 26, 9, 0, 0), openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
			AssertEquals("Initial date time is before opening hours on same day, final date time should be first opening hour on the same day. Calculation log should reflect this",
				@"26-Sep-22 05:00:00 wasn't matched with opening hours of [Wisetech Global 357/4 Bindon Place Sydney NSW 2217]. Closest opening hour was: 26-Sep-22 09:00:00
",
				openingHoursCalculationStep.Calculate(initialInput).CalculationLog);

			var overridenAddress = Factory.NewWithValidTestData<JobDocAddress>();

			openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, null), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);
			FieldInfo addressField = typeof(OpeningHoursDDDCalculationStep).GetField("address", BindingFlags.NonPublic | BindingFlags.Instance);
			addressField.SetValue(openingHoursCalculationStep, overridenAddress);

			var dateTime = new ZDateTime(2022, 9, 26, 23, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(dateTime, ZString.Empty);

			AssertEquals("Overriden address does not have a matched time table", dateTime, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
			AssertEquals("Overriden address does not have a matched time table", ZString.Empty, openingHoursCalculationStep.Calculate(initialInput).CalculationLog);
		}
		
		public void TestCalculate_Skip()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime> { new DateTime(2022, 1, 1) });
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var monday9Am = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday9Am, ZString.Empty);
			AssertEquals("Initial date time is in opening hours, final date time should be initial date time", monday9Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);

			var monday9Pm = new ZDateTime(2022, 9, 26, 21, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(monday9Pm, ZString.Empty);
			AssertEquals("Initial date time is not in opening hours and no skip", new ZDateTime(2022, 9, 27, 9, 0, 0), openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);

			var saturday9Am = new ZDateTime(2022, 9, 17, 9, 0, 0);
			openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false , orgAddress), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			FieldInfo deliverOnWeekendField = typeof(OpeningHoursDDDCalculationStep).GetField("deliverOnWeekend", BindingFlags.NonPublic | BindingFlags.Instance);
			deliverOnWeekendField.SetValue(openingHoursCalculationStep, true);

			initialInput = DeliveryDueDateCalculationResult.Success(saturday9Am, ZString.Empty);
			AssertEquals("Initial date time is on weekend but skipping", saturday9Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);

			openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);
			deliverOnWeekendField.SetValue(openingHoursCalculationStep, false);

			initialInput = DeliveryDueDateCalculationResult.Success(saturday9Am, ZString.Empty, arrivalTimeUsedForDeliveryCFS: true);
			AssertEquals("Initial date time is on weekend and not skipping", new ZDateTime(2022, 9, 19, 9, 0, 0), openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);

			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "XX";
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "YY";
			state.RW_RN_NKCountryCode = "XX";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var pubHolidayOrgAddress = org.Addresses[0];
			pubHolidayOrgAddress.OA_RN_NKCountryCode = "XX";
			pubHolidayOrgAddress.State = "YY";

			Factory.Save();

			var saturdayPublicHoliday = new ZDateTime(2022, 1, 1, 9, 0, 0);
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = saturdayPublicHoliday;
			glbHoliday.GH_IsWorkingDay = false;
			glbHoliday.GH_Recurring = true;
			glbHoliday.GH_ParentID = state.PK;
			glbHoliday.GH_ParentTableCode = "RW";
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = true;

			Factory.Save();

			openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, pubHolidayOrgAddress), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);
			deliverOnWeekendField.SetValue(openingHoursCalculationStep, true);

			initialInput = DeliveryDueDateCalculationResult.Success(saturdayPublicHoliday, ZString.Empty, arrivalTimeUsedForDeliveryCFS: true);
			AssertEquals("Initial date time is on weekend, and public holiday, ignore skip for using arrival time but skip because deliverOnWeekend is true", saturdayPublicHoliday, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_PickupMondayWithinHours()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var monday10Am = new ZDateTime(2022, 9, 26, 10, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday10Am, ZString.Empty);

			AssertEquals("Initial date time is in opening hours, final date time should be initial date time", monday10Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_PickupMondayOutOfHours()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, orgAddress), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var monday9Pm = new ZDateTime(2022, 9, 26, 21, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday9Pm, ZString.Empty);
			AssertEquals("Initial date time is not in opening hours and no skip", new ZDateTime(2022, 9, 27, 9, 0, 0), openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_DeliverSaturday_DeliverOnWeekend()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress, deliverOnWeekend: true), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var saturday10Am = new ZDateTime(2022, 9, 17, 10, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(saturday10Am, ZString.Empty);
			AssertEquals("Initial date time is on weekend but skipping", saturday10Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_DeliverSaturday_NoDeliverOnWeekend()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress, deliverOnWeekend: false), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var saturday10Am = new ZDateTime(2022, 9, 17, 10, 0, 0);
			var monday9Am = new ZDateTime(2022, 9, 19, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(saturday10Am, ZString.Empty);
			AssertEquals("Initial date time is on weekend but using arrival time", monday9Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_DeliverSaturday_DeliverOnWeekend_UseArrivalTime()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress, deliverOnWeekend: true), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var saturday10Am = new ZDateTime(2022, 9, 17, 10, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(saturday10Am, ZString.Empty, arrivalTimeUsedForDeliveryCFS: true);
			AssertEquals("Initial date time is on weekend but using arrival time", saturday10Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_DeliverSaturday_NoDeliverOnWeekend_UseArrivalTime()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, orgAddress, deliverOnWeekend: false), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var saturday10Am = new ZDateTime(2022, 9, 17, 10, 0, 0);
			var monday9Am = new ZDateTime(2022, 9, 19, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(saturday10Am, ZString.Empty, arrivalTimeUsedForDeliveryCFS: true);
			AssertEquals("Initial date time is on weekend but using arrival time", monday9Am, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_DeliverOnPublicHolidaySaturday_DeliverOnWeekend_UseArrivalTime()
		{
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>() { new DateTime(2022, 1, 1, 9, 0, 0) });

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var pubHolidayOrgAddress = org.Addresses[0];

			Factory.Save();

			var saturdayPublicHoliday = new ZDateTime(2022, 1, 1, 9, 0, 0);

			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, pubHolidayOrgAddress, deliverOnWeekend: true), AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, calendarDayTypeProvider);

			var initialInput = DeliveryDueDateCalculationResult.Success(saturdayPublicHoliday, ZString.Empty, arrivalTimeUsedForDeliveryCFS: true);
			AssertEquals("Initial date time is on weekend, and public holiday, ignore skip for using arrival time but skip because deliverOnWeekend is true", saturdayPublicHoliday, openingHoursCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestOpeningHoursShouldBeIgnoredWhenAddressIsNull()
		{
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, null), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);
			var monday9Pm = new ZDateTime(2022, 9, 26, 21, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday9Pm, ZString.Empty);

			var result = openingHoursCalculationStep.Calculate(initialInput);
			AssertEquals("Opening hours should be ignored when address is null", monday9Pm, result.DeliveryDueDate);
			AssertEquals("Finding Opening Hours: Skipped\r\n", result.CalculationLog);
		}

		public void TestOpeningHoursShouldNotThrowExceptionWhenAddressIsNull()
		{
			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var openingHoursCalculationStep = new OpeningHoursDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, null), AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider);

			var monday9Pm = new ZDateTime(2022, 9, 26, 21, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(monday9Pm, ZString.Empty);
			AssertNoExceptionThrown(() => openingHoursCalculationStep.Calculate(initialInput));
		}

		#region Helper Methods

		RefServiceLevel Normal;
		RefServiceLevel DeliverOnWeekends;

		protected override void SetUp()
		{
			base.SetUp();

			Normal = DeliveryDueDateCalculationTestHelper.GetOrCreateRefServiceLevelIfNotExist(Factory, "NRM");
			Normal.RS_IsActive = true;
			Normal.RS_DeliverOnSaturday = false;
			Normal.RS_DeliverOnSunday = false;

			DeliverOnWeekends = DeliveryDueDateCalculationTestHelper.GetOrCreateRefServiceLevelIfNotExist(Factory, "WKE");
			DeliverOnWeekends.RS_IsActive = true;
			DeliverOnWeekends.RS_DeliverOnSaturday = true;
			DeliverOnWeekends.RS_DeliverOnSunday = true;

			Factory.Save();
		}

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext(bool isPickup = false, OrgAddress orgAddress = null, bool deliverOnWeekend = false)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				deliverOnWeekend ? "WKE" : "NRM",
				ZString.Empty,
				isPickup ? orgAddress?.Header.OH_Code ?? ZString.Empty : ZString.Empty,
				isPickup ? orgAddress?.AddressCode ?? ZString.Empty : ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				isPickup ? ZString.Empty : orgAddress?.Header.OH_Code ?? ZString.Empty,
				isPickup ? ZString.Empty : orgAddress?.AddressCode ?? ZString.Empty,
				ZString.Empty,
				ZString.Empty
			);
		}

		#endregion
	}
}
