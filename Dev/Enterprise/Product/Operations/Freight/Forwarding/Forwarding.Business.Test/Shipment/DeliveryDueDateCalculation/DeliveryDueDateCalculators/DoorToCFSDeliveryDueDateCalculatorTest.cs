using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DoorToCFSDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Friday 5th Aug
Transit from Pickup Address to Pickup CFS Address: 8 hours -> 5PM Friday 5th Aug
Pickup CFS Preparation Time: 6 hours -> 3PM Monday 8th Aug
Transit From CFS to CFS: 5 days -> 3PM Saturday 13th Aug (SYD time) -> 5PM Saturday 13th Aug (AKL time)
Delivery CFS Preparation Time: 8 hours -> 5PM Monday 15th Aug
Expected Delivery Due Date: 5PM Monday 15th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 15, 17, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 5 * 24, 1, ZDateTime.Empty);

			OriginZone.Items[0].TQ_BeyondHours = 8;
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(PickupCFSAddress, 6 * 60);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(DeliveryCFSAddress, 8 * 60);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2022, 8, 5, 9, 0, 0);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2022, 8, 5, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
		}

		public void TestCalculate_DefaultHoldForPickupTime()
		{
			AdditionalBasicSetUp();

			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(16);
			Factory.Save();

			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(@"
Default Hold For Pickup Time: 16:00, 5PM is after 4PM, so add an additonal day
Expected Delivery Due Date: 16:00 Tuesday 16th Aug", new ZDateTime(2022, 8, 16, 16, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);

			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = ZDateTime.DefaultDurationEpoch.AddHours(17);
			Factory.Save();

			result = calculator.CalculateDeliveryDueDate();
			AssertEquals(@"
Default Hold For Pickup Time: 17:00, 5PM is equal to 5PM, so no additonal day
Expected Delivery Due Date: 17:00 Monday 15th Aug", new ZDateTime(2022, 8, 15, 17, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}

		public void TestCalculate_CutOffTime()
		{
			AdditionalBasicSetUp();
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(PickupCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Pickup);
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(DeliveryCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Deliver);

			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = ZDateTime.DefaultDurationEpoch.AddHours(16);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 5 * 24, 2, ZDateTime.Empty);
			Factory.Save();

			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();

			Assert("The calculation will consider the cut-off time for PIC but not for DLV", result.CalculationLog.Contains("Applying Cutoff time for PIC"));
			Assert("The calculation will consider the cut-off time for PIC but not for DLV", !result.CalculationLog.Contains("Applying Cutoff time for DLV"));

			AssertEquals(@"
Default Hold For Pickup Time: 16:00, 5PM is after 4PM, so add an additonal day
Expected Delivery Due Date: 16:00 Tuesday 16th Aug", new ZDateTime(2022, 8, 16, 16, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);

			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(17);
			Factory.Save();

			result = calculator.CalculateDeliveryDueDate();
			AssertEquals(@"
Default Hold For Pickup Time: 17:00, 5PM is equal to 5PM, so no additonal day
Expected Delivery Due Date: 17:00 Monday 15th Aug", new ZDateTime(2022, 8, 15, 17, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}

		public void TestCalculate_ArrivalTimeBeforeOpeningHoursAtDeliveryCFS()
		{
			AdditionalBasicSetUp();
			ArrivalTimeDeliveryDueDateCalculatorTest.TestCalculate_ArrivalTimeBeforeOpeningHoursAtDeliveryCFS(
				Factory,
				AssertDeliveryDueDate,
				Shipment,
				ServiceLevel,
				TransitTime,
				transitHours: 3 * 24,
				transitDayOfWeek: 2,
				expectedTimeTransitTime: new ZDateTime(2022, 8, 12, 15, 0, 0),
				expectedTimeServiceLevel: new ZDateTime(2022, 8, 11, 14, 30, 0));
		}
		protected override bool ShouldSetupDummyOriginZone => true;
	}

	sealed class DoorToCFSDeliveryDueDateCalculator_HasTransitTime_NoDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 4PM Wednesday 25th Sep
No Delivery Address and No Delivery Zone Item.
Transit Time is 10 hrs, using Transit Time to Calculate CFS to CFS.
Expected Delivery Due Date: 9:00AM Thursday 26th Sep

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 26, 9, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2024, 9, 25, 16, 0, 0);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2024, 9, 25, 16, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
		}

		protected override bool ShouldSetupDeliveryAddress => false;
		protected override bool ShouldSetupDummyOriginZone => true;
	}

	sealed class DoorToCFSDeliveryDueDateCalculator_NoTransitTime_NoDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Delivery Address and No Delivery Zone Item.
No Transit time between CFS Zones, using Service Level to calculate DDD.
Using Service Level Default Transit Hours: 6 days
Using Service Level Default Arrival Time: 10:30AM
Expected Delivery Due Date: 10:30AM Tuesday 1st Oct

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 10, 1, 10, 30, 0);

		protected override void AdditionalBasicSetUp()
		{
			ServiceLevel.RS_DefaultTransitHours = 144;
			ServiceLevel.RS_DefaultArrivalTime = ZDateTime.DefaultDurationEpoch.Add(new TimeSpan(10, 30, 0));
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
		}

		protected override bool ShouldSetupDeliveryAddress => false;
		protected override bool ShouldSetupTransitTime => false;
	}
}
