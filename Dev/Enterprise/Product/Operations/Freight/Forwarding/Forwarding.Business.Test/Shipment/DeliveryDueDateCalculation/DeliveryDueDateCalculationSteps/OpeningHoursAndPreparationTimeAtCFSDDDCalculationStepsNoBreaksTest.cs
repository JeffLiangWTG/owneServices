using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsNoBreaksTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		public void TestPickup_NoProcessing_NoBreaks_A()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at PIC CFS for Monday: -
Adding processing time step for PIC: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_NoBreaks_A_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_A_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_B()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 10, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_B_BeforeCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_B_AfterCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"Processing time at PIC CFS for Monday: -
Adding processing time step for PIC: 16-Sep-24 13:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of PIC CFS
Cutoff Time on Monday for PIC CFS: 11:00
Calculated time is after Cutoff time. Finding next business day.
Applying Cutoff time for PIC: 16-Sep-24 13:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_NoBreaks_B_AfterCutoffB_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_Z()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_NoBreaks_Z_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_BeforeCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_AfterCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AB_AfterCutoffB_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AZ()
		{
			var orgAddress = CreateAddressNoBreaks(600);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_AZ_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(600);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BB()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BB_BeforeCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressNoBreaks(210, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BB_AfterCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BB_AfterCutoffB_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BZ()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BZ_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BZ_AfterCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_BZ_AfterCutoffB_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_Z()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_NoBreaks_Z_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 14-Sep-24; Sunday 15-Sep-24; 
14-Sep-24 10:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Set Hold For Pickup Time: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 12:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_A_Deliver_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 10:00:00 based on time table of DLV CFS
Hold For Pickup Time: 09:10:00
Adding one extra day: 16-Sep-24 10:00:00 adjusted to 17-Sep-24 09:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Friday: -
Adding processing time step for DLV: 20-Sep-24 10:00:00 adjusted to 20-Sep-24 10:00:00 based on time table of DLV CFS
Hold For Pickup Time: 09:10:00
Adding one extra day: 20-Sep-24 10:00:00 adjusted to 21-Sep-24 09:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 09:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 09:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_BeforeCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expectedLog = @"Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 13:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of DLV CFS
Cutoff Time on Monday for DLV CFS: 14:00
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_B_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressNoBreaks(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"Processing time at DLV CFS for Friday: -
Adding processing time step for DLV: 20-Sep-24 13:00:00 adjusted to 20-Sep-24 13:00:00 based on time table of DLV CFS
Cutoff Time on Friday for DLV CFS: 11:00
Calculated time is after Cutoff time. Finding next business day.
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Applying Cutoff time for DLV: 20-Sep-24 13:00:00 adjusted to 23-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Collect_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_NoBreaks_Z_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 14-Sep-24; Sunday 15-Sep-24; 
14-Sep-24 10:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			var expectedLog = @"20-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 20-Sep-24 09:00:00
Processing time at DLV CFS for Friday: 4 Hours
Adding processing time step for DLV: 20-Sep-24 09:00:00 adjusted to 20-Sep-24 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Adding one extra day: 20-Sep-24 13:00:00 adjusted to 21-Sep-24 12:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 12:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 12:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 14, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 14-Sep-24; Sunday 15-Sep-24; 
14-Sep-24 18:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_AB_Deliver_OnSunday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 15, 14, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Sunday 15-Sep-24; 
15-Sep-24 14:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 30, 0);
			var expectedLog = @"Processing time at DLV CFS for Friday: 4 Hours
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Adding processing time step for DLV: 20-Sep-24 14:30:00 adjusted to 23-Sep-24 10:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 19, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Thursday: 4 Hours
Adding processing time step for DLV: 19-Sep-24 14:30:00 adjusted to 20-Sep-24 10:30:00 based on time table of DLV CFS
Hold For Pickup Time: 09:10:00
Adding one extra day: 20-Sep-24 10:30:00 adjusted to 21-Sep-24 09:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 09:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 09:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 24, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect_BeforeHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 16-Sep-24 14:30:00 adjusted to 17-Sep-24 10:30:00 based on time table of DLV CFS
Hold For Pickup Time: 09:10:00
Adding one extra day: 17-Sep-24 10:30:00 adjusted to 18-Sep-24 09:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect_AfterHoldForPickup_OnThursday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 19, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 24, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BZ_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BB_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_BB_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_WithCutoffB_Collect_BeforeHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 20, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_BB_BeforeCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BB_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_BB_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressNoBreaks(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_NoBreaks_Z_Collect()
		{
			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_Z_Collect_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_NoBreaks_Z_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_NoBreaks_Z_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
20-Sep-24 18:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 23-Sep-24 09:00:00
Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 11:00:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Set Hold For Pickup Time: 23-Sep-24 11:00:00 adjusted to 23-Sep-24 12:10:00 based on DLV CFS's Hold for pickup time
";
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_Z_Deliver()
		{
			var orgAddress = CreateAddressNoBreaks(240);
			var arrival = new ZDateTime(2024, 9, 16, 19, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 0, 0);
			var expectedLog = @"16-Sep-24 19:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: 4 Hours
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_NoBreaks_Z_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressNoBreaks(180);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}
	}
}
