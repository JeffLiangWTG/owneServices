using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsOneBreakNoProcessingTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		public void TestPickup_NoProcessing_OneBreak_A()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_A_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 14, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_A_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Sunday 15-Sep-24; 
15-Sep-24 12:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at PIC CFS for Monday: -
Adding processing time step for PIC: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_OneBreak_B()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 10, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_B_BeforeCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_B_AfterCutoffB()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_B_AfterCutoffB_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 13, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"Processing time at PIC CFS for Friday: -
Adding processing time step for PIC: 20-Sep-24 13:00:00 adjusted to 20-Sep-24 13:00:00 based on time table of PIC CFS
Cutoff Time on Friday for PIC CFS: 11:00
Calculated time is after Cutoff time. Finding next business day.
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Applying Cutoff time for PIC: 20-Sep-24 13:00:00 adjusted to 23-Sep-24 09:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_OneBreak_C()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			var expectedLog = @"16-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 13:00:00
Processing time at PIC CFS for Monday: -
Adding processing time step for PIC: 16-Sep-24 13:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_NoProcessing_OneBreak_D()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_Z()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_NoProcessing_OneBreak_Z_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Collect()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Collect_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_A_Deliver_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Collect()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 10:00:00 based on time table of DLV CFS
Hold For Pickup Time: 13:10:00
Set Hold For Pickup Time: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 13:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_WithCutoffB_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_WithCutoffB_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_WithCutoffB_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 10, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_WithCutoffB_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_BeforeCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 5, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_AfterCutoffB_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_B_AfterCutoffB_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Collect()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			var expectedLog = @"16-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 13:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 13:00:00 adjusted to 16-Sep-24 13:00:00 based on time table of DLV CFS
Hold For Pickup Time: 13:10:00
Set Hold For Pickup Time: 16-Sep-24 13:00:00 adjusted to 16-Sep-24 13:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_BeforeCutoffC_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 40, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_AfterCutoffC_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 20, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_C_AfterCutoffC_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 12, 20, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Collect()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 10, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_D_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 10, 0);
			var orgAddress = CreateAddressOneBreak(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Collect()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			var expectedLog = @"16-Sep-24 18:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 17-Sep-24 09:00:00
Processing time at DLV CFS for Tuesday: -
Adding processing time step for DLV: 17-Sep-24 09:00:00 adjusted to 17-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 16, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_OneBreak_Z_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(0);
			var arrival = new ZDateTime(2024, 9, 20, 18, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
20-Sep-24 18:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 23-Sep-24 09:00:00
Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 09:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}
	}
}
