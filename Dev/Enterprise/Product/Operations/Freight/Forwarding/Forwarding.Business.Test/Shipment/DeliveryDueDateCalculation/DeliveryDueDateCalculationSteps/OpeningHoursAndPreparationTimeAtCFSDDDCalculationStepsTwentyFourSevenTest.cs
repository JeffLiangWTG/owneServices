using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTwentyFourSevenTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		public void TestPickup_NoProcessing_TwentyFourSeven_Normal_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 7, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestPickup_NoProcessing_TwentyFourSeven_Normal_WithCutoff_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestPickup_NoProcessing_TwentyFourSeven_Normal_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 7, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestPickup_NoProcessing_TwentyFourSeven_Normal_WithCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			var expectedLog = @"Processing time at PIC CFS for Monday: -
Adding processing time step for PIC: 16-Sep-24 17:30:00 adjusted to 16-Sep-24 17:30:00 based on time table of PIC CFS
Cutoff Time on Monday for PIC CFS: 14:00
Calculated time is after Cutoff time. Finding next business day.
Applying Cutoff time for PIC: 16-Sep-24 17:30:00 adjusted to 17-Sep-24 00:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_WithCutoff_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 9, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(600, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestPickup_Processing_TwentyFourSeven_Normal_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestPickup_Processing_TwentyFourSeven_Overlap_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true, allowedError: 1);
		}

		public void TestPickup_Processing_TwentyFourSeven_Overlap_WithCutoff_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 20, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			var expectedLog = @"Processing time at PIC CFS for Monday: 8 Hours
Adding processing time step for PIC: 16-Sep-24 17:30:00 adjusted to 17-Sep-24 01:31:00 based on time table of PIC CFS
Cutoff Time on Tuesday for PIC CFS: 20:00
";
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: true, allowedError: 1, expectedLog: expectedLog);
		}

		public void TestPickup_Processing_TwentyFourSeven_Overlap_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false, allowedError: 1);
		}

		public void TestPickup_Processing_TwentyFourSeven_Overlap_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 20, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false, allowedError: 1);
		}

		public void TestPickup_Processing_TwentyFourSeven_Overlap_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 1, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 0, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_TwentyFourSeven_Normal_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expectedLog = @"Processing time at DLV CFS for Monday: -
Adding processing time step for DLV: 16-Sep-24 07:30:00 adjusted to 16-Sep-24 07:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_NoProcessing_TwentyFourSeven_Normal_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 15, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 18, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_TwentyFourSeven_Normal_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 14, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_NoProcessing_TwentyFourSeven_Normal_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 7, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_TwentyFourSeven_Normal_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_NoProcessing_TwentyFourSeven_Normal_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_TwentyFourSeven_Normal_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 7, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_TwentyFourSeven_Normal_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 18, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_TwentyFourSeven_Normal_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_TwentyFourSeven_Normal_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(0);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 7, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_TwentyFourSeven_Normal_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 15, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_NoProcessing_TwentyFourSeven_Normal_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(0, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 15, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 18, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Normal_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 11, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Overlap_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, allowedError: 1);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Overlap_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Overlap_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 1, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 1, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Overlap_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, allowedError: 1);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Overlap_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 20, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, allowedError: 1);
		}

		public void TestDelivery_Processing_TwentyFourSeven_Overlap_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 1, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 0, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Normal_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Normal_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 18, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 18, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Normal_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Normal_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Normal_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Normal_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 7, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 0, 0, 0); // TODO: Check this
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 8 Hours
Adding processing time step for DLV: 16-Sep-24 07:30:00 adjusted to 16-Sep-24 15:30:00 based on time table of DLV CFS
Cutoff Time on Monday for DLV CFS: 14:00
Calculated time is after Cutoff time. Finding next business day.
Applying Cutoff time for DLV: 16-Sep-24 15:30:00 adjusted to 17-Sep-24 00:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Overlap_Collect()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Overlap_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay; // TODO: Check this
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 8 Hours
Adding processing time step for DLV: 16-Sep-24 17:30:00 adjusted to 17-Sep-24 01:30:00 based on time table of DLV CFS
Hold For Pickup Time: 12:10:00
Set Hold For Pickup Time: 17-Sep-24 01:30:00 adjusted to 17-Sep-24 12:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Overlap_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 1, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 1, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, arrivalTimeUsedForDeliveryCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Overlap_Deliver()
		{
			var orgAddress = CreateAddressTwentyFourSeven(480);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Overlap_BeforeCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 20, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 1, 30, 0); // TODO: Check this
			var expectedLog = @"Finding Opening Hours: Skipped

Processing Time - Find Opening Hours: Skipped
Processing time at DLV CFS for Monday: 8 Hours
Adding processing time step for DLV: 16-Sep-24 17:30:00 adjusted to 17-Sep-24 01:30:00 based on time table of DLV CFS
Cutoff Time on Tuesday for DLV CFS: 20:00
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true, expectedLog: expectedLog);
		}

		public void TestDelivery_WithArrivalTime_Processing_TwentyFourSeven_Overlap_AfterCutoff_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 1, 0, 0);
			var orgAddress = CreateAddressTwentyFourSeven(480, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 17, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 0, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, arrivalTimeUsedForDeliveryCFS: true);
		}
	}
}
