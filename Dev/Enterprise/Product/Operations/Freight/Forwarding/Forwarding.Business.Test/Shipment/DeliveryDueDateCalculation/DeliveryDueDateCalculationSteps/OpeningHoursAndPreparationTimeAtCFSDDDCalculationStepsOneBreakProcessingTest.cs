using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsOneBreakProcessingTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		public void TestPickup_Processing_OneBreak_AB()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AB_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AB_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AC()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at PIC CFS for Monday: 3 Hours and 30 Minutes
Adding processing time step for PIC: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 13:15:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_Processing_OneBreak_AC_BeforeCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(210, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AC_AfterCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(210, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AD()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AD_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 20, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AD_BeforeCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AD_AfterCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AD_AfterCutoffD_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AZ()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_AZ_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BB()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BC()
		{
			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BD()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BD_BeforeCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BD_AfterCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BD_AfterCutoffD_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BZ()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_BZ_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CD()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 00, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CD_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 20, 17, 00, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CD_BeforeCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 30, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CD_AfterCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CD_AfterCutoffD_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			var expectedLog = @"20-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 20-Sep-24 13:00:00
Processing time at PIC CFS for Friday: 4 Hours
Adding processing time step for PIC: 20-Sep-24 13:00:00 adjusted to 20-Sep-24 17:00:00 based on time table of PIC CFS
Cutoff Time on Friday for PIC CFS: 15:00
Calculated time is after Cutoff time. Finding next business day.
[Header #1 YY] non working days: Saturday 21-Sep-24; Sunday 22-Sep-24; 
Applying Cutoff time for PIC: 20-Sep-24 17:00:00 adjusted to 23-Sep-24 09:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestPickup_Processing_OneBreak_CZ()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_CZ_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DD()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DD_BeforeCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DD_AfterCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DD_AfterCutoffD_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DZ()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DZ_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DZ_AfterCutoffD()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_DZ_AfterCutoffD_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 15, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_ZB()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_ZB_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 0, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_ZD()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestPickup_Processing_OneBreak_ZD_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 45, 0);
			ExecutePickupTest(orgAddress, arrival, expected);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AB_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AB_Deliver_OnSaturday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 14, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AB_Deliver_OnSunday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 15, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AC_Collect()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AC_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			var expectedLog = @"16-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 09:00:00
Processing time at DLV CFS for Monday: 3 Hours and 30 Minutes
Adding processing time step for DLV: 16-Sep-24 09:00:00 adjusted to 16-Sep-24 13:15:00 based on time table of DLV CFS
Hold For Pickup Time: 14:10:00
Set Hold For Pickup Time: 16-Sep-24 13:15:00 adjusted to 16-Sep-24 14:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_AC_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AC_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AC_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AD_Collect()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AD_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 20, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 14, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 13, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 19, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			var expectedLog = @"19-Sep-24 08:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 19-Sep-24 09:00:00
Processing time at DLV CFS for Thursday: 7 Hours and 30 Minutes
Adding processing time step for DLV: 19-Sep-24 09:00:00 adjusted to 20-Sep-24 09:15:00 based on time table of DLV CFS
Hold For Pickup Time: 09:10:00
Adding one extra day: 20-Sep-24 09:15:00 adjusted to 21-Sep-24 09:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 09:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 09:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 24, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 16, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_AZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(450);
			var arrival = new ZDateTime(2024, 9, 20, 8, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BB_Collect()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BB_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 12, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BC_Collect()
		{
			var orgAddress = CreateAddressOneBreak(160);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 25, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BC_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(160);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BC_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(160);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Monday: 2 Hours and 40 Minutes
Adding processing time step for DLV: 16-Sep-24 10:00:00 adjusted to 16-Sep-24 13:25:00 based on time table of DLV CFS
Hold For Pickup Time: 11:10:00
Adding one extra day: 16-Sep-24 13:25:00 adjusted to 17-Sep-24 11:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_BC_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(160);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BC_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(160);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 13, 25, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BD_Collect()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BD_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BD_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BD_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BD_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 14, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BD_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 20, 14, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(300, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 19, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 20, 10, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 12, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 12, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 19, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 24, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 16, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_BZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(480);
			var arrival = new ZDateTime(2024, 9, 20, 10, 0, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CD_Collect()
		{
			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CD_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CD_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			var expectedLog = @"16-Sep-24 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 16-Sep-24 13:00:00
Processing time at DLV CFS for Monday: 2 Hours and 30 Minutes
Adding processing time step for DLV: 16-Sep-24 13:00:00 adjusted to 16-Sep-24 15:30:00 based on time table of DLV CFS
Hold For Pickup Time: 15:10:00
Adding one extra day: 16-Sep-24 15:30:00 adjusted to 17-Sep-24 15:10:00 because calculated time is after hold for pickup
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_CD_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(150);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CD_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 17, 00, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 16, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 30, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(150, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 19, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 24, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 16, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_CZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(300);
			var arrival = new ZDateTime(2024, 9, 20, 12, 30, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DD_Collect()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DD_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DD_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 20, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DD_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DD_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DD_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DD_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DD_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 16, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 16, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DD_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DD_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 15, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 15, 10, 0).TimeOfDay;
			var expectedLog = @"Processing time at DLV CFS for Friday: 2 Hours
Adding processing time step for DLV: 20-Sep-24 13:15:00 adjusted to 20-Sep-24 15:15:00 based on time table of DLV CFS
Hold For Pickup Time: 15:10:00
Adding one extra day: 20-Sep-24 15:15:00 adjusted to 21-Sep-24 15:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 15:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 15:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_DD_BeforeCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 30, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 16, 15, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DD_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DD_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(120, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 13, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 19, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 24, 10, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 10, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DZ_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect_BeforeHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect_BeforeHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect_AfterHoldForPickup()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect_AfterHoldForPickup_OnThursday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 19, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_WithCutoffD_Collect_AfterHoldForPickup_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 24, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_DZ_AfterCutoffD_Deliver()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 16, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_DZ_AfterCutoffD_Deliver_OnFriday()
		{
			var cutoffTime = new DateTime(2024, 1, 1, 15, 0, 0);
			var orgAddress = CreateAddressOneBreak(240, cutoffTime);
			var arrival = new ZDateTime(2024, 9, 20, 14, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 10, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 11, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 19, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 24, 9, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 9, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZB_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(120);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 11, 0, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 19, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 24, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZC_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(210);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 15, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect_BeforeHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect_BeforeHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 14, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 14, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect_AfterHoldForPickup()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 18, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect_AfterHoldForPickup_OnThursday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 19, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			var expectedLog = @"19-Sep-24 18:15:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 20-Sep-24 09:00:00
Processing time at DLV CFS for Friday: 4 Hours
Adding processing time step for DLV: 20-Sep-24 09:00:00 adjusted to 20-Sep-24 13:45:00 based on time table of DLV CFS
Hold For Pickup Time: 13:10:00
Adding one extra day: 20-Sep-24 13:45:00 adjusted to 21-Sep-24 13:10:00 because calculated time is after hold for pickup
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Saturday 21-Sep-24; Sunday 22-Sep-24; 
21-Sep-24 13:10:00 adjusted to 23-Sep-24 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 23-Sep-24 09:00:00 adjusted to 23-Sep-24 13:10:00 based on DLV CFS's Hold for pickup time
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime, expectedLog: expectedLog);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Collect_AfterHoldForPickup_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 24, 13, 10, 0);
			var holdForPickupTime = new ZDateTime(1900, 1, 1, 13, 10, 0).TimeOfDay;
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: true, holdForPickupTime: holdForPickupTime);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Deliver()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 16, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 17, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}

		public void TestDelivery_Processing_OneBreak_ZD_Deliver_OnFriday()
		{
			var orgAddress = CreateAddressOneBreak(240);
			var arrival = new ZDateTime(2024, 9, 20, 18, 15, 0);
			var expected = new ZDateTime(2024, 9, 23, 13, 45, 0);
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false);
		}
	}
}
