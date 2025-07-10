using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsHolidayTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		protected virtual void SetHolidays(string countryCode = "XX", string stateCode = "YY", params DateTime[] days)
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = countryCode;
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = stateCode;
			state.RW_RN_NKCountryCode = countryCode;

			foreach (var day in days)
			{
				var holiday = Factory.NewWithValidTestData<GlbHoliday>();
				holiday.GH_Date = day;
				holiday.GH_IsWorkingDay = false;
				holiday.GH_Recurring = true;
				holiday.GH_ParentID = country.PK;
				holiday.GH_ParentTableCode = "RN";
				holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
				holiday.GH_IsActive = true;
			}

			Factory.Save();
		}

		public void TestPickup_NoProcessing_NoBreaks_A_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_A_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_B_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_B_OnHolidayFriday()
		{
			SetHolidays(days: new DateTime(2024, 9, 20));
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AZ_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(600);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AZ_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(600);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BZ_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at PIC CFS for Tuesday: 4 Hours
Adding processing time step for PIC: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_Processing_NoBreaks_BZ_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_Z_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect_OnPublicHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Sunday 15-Sep-24; Monday 16-Sep-24; 
15-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Deliver_OnPublicHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDeliver_NoProcessing_TwentyFourSeven_Normal_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 07:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 00:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 00:00:00 adjusted to 17-Sep-24 00:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDeliver_NoProcessing_TwentyFourSeven_Normal_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDeliver_Processing_TwentyFourSeven_Normal_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 8, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 8, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 07:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 00:00:00
Processing time at DLV CFS for Tuesday: 8 Hours
Adding processing time step for DLV: 17-Sep-24 00:00:00 adjusted to 17-Sep-24 08:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_NoBreaks_Z_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 10:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 10:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Sunday 15-Sep-24; Monday 16-Sep-24; 
15-Sep-24 12:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 10:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Deliver_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Deliver_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_A_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_A_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_A_Collect_BeforeHoldForPickup_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_A_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_A_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_B_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_B_Collect_BeforeHoldForPickup_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 10:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Set Hold For Pickup Time: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 12:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_B_Collect_AfterHoldForPickup_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_B_Collect_AfterHoldForPickup_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_B_Collect_AfterHoldForPickup_OnThursdayHolidayOnFriday()
		{
			SetHolidays(days: new DateTime(2024, 9, 20));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 19, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_B_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_Z_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_NoBreaks_Z_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect_AfterHoldForPickup_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect_AfterHoldForPickup_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay; // TODO: Check if this is correct
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 12:00:00 based on time table of DLV CFS
Hold For Pickup Time: 09:10:00
Adding one extra day: 16-Sep-24 12:00:00 adjusted to 17-Sep-24 09:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Tuesday 17-Sep-24; 
17-Sep-24 09:10:00 adjusted to 18-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 18-Sep-24 09:00:00 adjusted to 18-Sep-24 09:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect_AfterHoldForPickup_OnThursdayHolidayOnFriday()
		{
			SetHolidays(days: new DateTime(2024, 9, 20));

			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 19, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestPickup_NoProcessing_OneBreak_A_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_A_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_B_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_B_OnHolidayFriday()
		{
			SetHolidays(days: new DateTime(2024, 9, 20));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_C_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_D_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_Z_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AB_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AB_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AD_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AZ_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AZ_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BB_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CD_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at PIC CFS for Tuesday: 4 Hours
Adding processing time step for PIC: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:45:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_Processing_OneBreak_DD_onHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DZ_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_ZB_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_ZB_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestPickup_Processing_NoBreaks_BB_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AC_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AB_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AB_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BB_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BB_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CD_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 30, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: 2 Hours and 30 Minutes
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 11:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_CD_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 14, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_OnHoliday12()
		{
			SetHolidays(days: new DateTime[] { new DateTime(2024, 9, 16), new DateTime(2024, 9, 17) });

			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 14, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Deliver_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 14, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Deliver_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DD_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DD_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 13:15:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: 2 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 11:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Deliver_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Deliver_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Deliver_OnHoliday1()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Deliver_OnHoliday2()
		{
			SetHolidays(days: new DateTime(2024, 9, 17));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_OneBreak_A_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_OneBreak_A_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_OneBreak_A_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17,9, 0, 0); // TODO: Check this
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_OneBreak_A_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 16, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Collect_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver_OnSundayWithHoliday_AfterCutoff()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Sunday 15-Sep-24; Monday 16-Sep-24; 
15-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 3 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 12:00:00 based on time table of DLV CFS
Cutoff Time on Tuesday for DLV CFS: 11:00
Calculated time is after Cutoff time. Finding next business day.
Applying Cutoff time for DLV: 17-Sep-24 12:00:00 adjusted to 18-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Deliver_OnSundayWithHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 15, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(210, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestPickup_NoProcessing_TwentyFourSeven_Normal_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));

			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 07:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 00:00:00
Processing time at PIC CFS for Tuesday: -
Adding processing time step for PIC: 17-Sep-24 00:00:00 adjusted to 17-Sep-24 00:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_TwentyFourSeven_Normal_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_Collect_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 8, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_Deliver_OnHoliday()
		{
			SetHolidays(days: new DateTime(2024, 9, 16));
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 8, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Monday 16-Sep-24; 
16-Sep-24 07:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 00:00:00
Processing time at PIC CFS for Tuesday: 8 Hours
Adding processing time step for PIC: 17-Sep-24 00:00:00 adjusted to 17-Sep-24 08:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}
	}
}
