using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsArrivalTimeProcessingTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 8, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 14, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Collect_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 04:00:00 adjusted to 16-Sep-24 08:00:00 based on time table of DLV CFS
Hold For Pickup Time: 15:10:00
Set Hold For Pickup Time: 16-Sep-24 08:00:00 adjusted to 16-Sep-24 15:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 8, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 14, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AA_Deliver_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Friday: 4 Hours
Adding processing time step for DLV: 20-Sep-24 08:00:00 adjusted to 20-Sep-24 12:00:00 based on time table of DLV CFS
Hold For Pickup Time: 11:10:00
Adding one extra day: 20-Sep-24 12:00:00 adjusted to 21-Sep-24 11:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 11:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 11:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_AB_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 0, 0);
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 12:00:00 adjusted to 16-Sep-24 16:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_BeforeCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressNoBreaks(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BB_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_BZ_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Friday: 4 Hours
Adding processing time step for DLV: 20-Sep-24 14:30:00 adjusted to 20-Sep-24 18:30:00 based on time table of DLV CFS
Cutoff Time on Friday for DLV CFS: 15:00
Calculated time is after Cutoff time. Finding next business day.
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Applying Cutoff time for DLV: 20-Sep-24 18:30:00 adjusted to 23-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Collect_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
20-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 23-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 21, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 10, 0);
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 10:10:00
Adding one extra day: 17-Sep-24 13:00:00 adjusted to 18-Sep-24 10:10:00 because calculated time is after hold for pickup
";
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(150);
			var arrival = new ZDateTime(2024, 9, 20, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 30, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
20-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 23-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 2 Hours and 30 Minutes
Adding processing time step for DLV: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 11:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(150);
			var arrival = new ZDateTime(2024, 9, 21, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Deliver_WithHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 10:10:00
Adding one extra day: 17-Sep-24 13:00:00 adjusted to 18-Sep-24 10:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_NoBreaks_ZZ_Deliver_OnFriday_WithHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Collect()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 8, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 14, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Collect_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 15, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 04:00:00 adjusted to 16-Sep-24 08:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Set Hold For Pickup Time: 16-Sep-24 08:00:00 adjusted to 16-Sep-24 12:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 8, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 14, 4, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AA_Deliver_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Collect()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 3 Hours
Adding processing time step for DLV: 16-Sep-24 08:00:00 adjusted to 16-Sep-24 11:00:00 based on time table of DLV CFS
Cutoff Time on Monday for DLV CFS: 13:00
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver_AfterCutoff()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 10, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver_OnSaturday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_AB_Deliver_OnSunday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Deliver_OnSaturday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_Deliver_OnSunday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 15, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_BeforeCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 10, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 12:00:00 based on time table of DLV CFS
Cutoff Time on Monday for DLV CFS: 12:10
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BB_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 2 Hours and 30 Minutes
Adding processing time step for DLV: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 12:30:00 based on time table of DLV CFS
Hold For Pickup Time: 13:10:00
Set Hold For Pickup Time: 16-Sep-24 12:30:00 adjusted to 16-Sep-24 13:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_BeforeCutoffC_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 40, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_AfterCutoffC_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 20, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BC_AfterCutoffC_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 20, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Friday: 2 Hours and 30 Minutes
Adding processing time step for DLV: 20-Sep-24 10:00:00 adjusted to 20-Sep-24 12:30:00 based on time table of DLV CFS
Cutoff Time on Friday for DLV CFS: 12:20
Calculated time is after Cutoff time. Finding next business day.
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Applying Cutoff time for DLV: 20-Sep-24 12:30:00 adjusted to 23-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 30, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(270, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(450, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(450, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(450, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(450, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_BZ_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(450, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 16-Sep-24 12:30:00 adjusted to 16-Sep-24 14:30:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Adding one extra day: 16-Sep-24 14:30:00 adjusted to 17-Sep-24 12:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 20, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_CZ_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 30, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 30, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 30, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 30, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 30, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(90, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(180);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_DZ_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(180, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Friday: 3 Hours
Adding processing time step for DLV: 20-Sep-24 14:30:00 adjusted to 20-Sep-24 17:30:00 based on time table of DLV CFS
Cutoff Time on Friday for DLV CFS: 16:00
Calculated time is after Cutoff time. Finding next business day.
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Applying Cutoff time for DLV: 20-Sep-24 17:30:00 adjusted to 23-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_ZZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_ZZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Adding one extra day: 17-Sep-24 13:00:00 adjusted to 18-Sep-24 12:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_ZZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 24, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
20-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 23-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Adding one extra day: 23-Sep-24 13:00:00 adjusted to 24-Sep-24 12:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_ZZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"16-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_OneBreak_ZZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 20, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 30, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
20-Sep-24 17:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 23-Sep-24 09:00:00

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 2 Hours and 30 Minutes
Adding processing time step for DLV: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 11:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}
	}
}
