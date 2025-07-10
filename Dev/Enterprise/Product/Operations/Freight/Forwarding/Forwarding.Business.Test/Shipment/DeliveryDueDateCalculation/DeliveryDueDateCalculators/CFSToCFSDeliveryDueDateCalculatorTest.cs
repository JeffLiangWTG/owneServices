using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CFSToCFSDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 3PM Wednesday 3rd Aug
Pickup CFS Preparation Time: 6 hours -> 9PM Wednesday 3rd Aug
Transit From CFS to CFS: 3 days -> 9PM Saturday 6th Aug (SYD time) -> 11PM Saturday 6th Aug (AKL time)
Delivery CFS Preparation Time: 6 hours -> 3PM Monday 8th Aug
Expected Delivery Due Date: 3PM Monday 8th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 8, 15, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 3, ZDateTime.Empty);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 4, ZDateTime.Empty);

			DeliveryDueDateCalculationTestHelper.SetProcessingTime(PickupCFSAddress, 6 * 60);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(DeliveryCFSAddress, 6 * 60);
			Shipment.JS_A_RCV = new ZDateTime(2022, 8, 3, 15, 0, 0);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_A_RCV = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2022, 8, 3, 15, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_A_RCV = ZDateTime.Empty;
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
Default Hold For Pickup Time: 16:00, 4PM is after 3PM, so no additonal day
Expected Delivery Due Date: 16:00 Monday 8th Aug", new ZDateTime(2022, 8, 8, 16, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);

			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(11);
			Factory.Save();

			result = calculator.CalculateDeliveryDueDate();
			AssertEquals(@"
Default Hold For Pickup Time: 11:00, 3PM is after 11AM, so add an additonal day
Expected Delivery Due Date: 11:00 Monday 9th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.", new ZDateTime(2022, 8, 9, 11, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}

		public void TestCalculate_CutOffTime()
		{
			AdditionalBasicSetUp();
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(PickupCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Pickup);
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(DeliveryCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Deliver);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 4, ZDateTime.Empty);
			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(16);
			Factory.Save();

			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			Assert("The calculation will consider the cut-off time for PIC but not for DLV", result.CalculationLog.Contains("Applying Cutoff time for PIC"));
			Assert("The calculation will consider the cut-off time for PIC but not for DLV", !result.CalculationLog.Contains("Applying Cutoff time for DLV"));

			AssertEquals(@"
Default Hold For Pickup Time: 16:00, 4PM is after 3PM, so no additonal day
Expected Delivery Due Date: 16:00 Monday 8th Aug", new ZDateTime(2022, 8, 8, 16, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);

			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(11);
			Factory.Save();

			result = calculator.CalculateDeliveryDueDate();
			AssertEquals(@"
Default Hold For Pickup Time: 11:00, 3PM is after 11AM, so add an additonal day
Expected Delivery Due Date: 11:00 Monday 9th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.", new ZDateTime(2022, 8, 9, 11, 0, 0), result.DeliveryDueDate);
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
				transitHours: 2 * 24,
				transitDayOfWeek: 1,
				expectedTimeTransitTime: new ZDateTime(2022, 8, 10, 13, 0, 0),
				expectedTimeServiceLevel: new ZDateTime(2022, 8, 10, 12, 30, 0));
		}
	}

	sealed class CFSToCFSDeliveryDueDateCalculator_HasTransitTime_NoPickupAndDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Pickup Address and No Pickup Zone Item.
No Delivery Address and No Delivery Zone Item.
Transit time between CFS Zones is 10 hrs, using this to calculate DDD.
Expected Delivery Due Date: 9:00AM Thursday 26th Sep

Note: All addresses operate Mon-Fri 9AM-5PM.";
		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 26, 9, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			Shipment.JS_A_RCV = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_A_RCV = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_A_RCV = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
		}

		protected override bool ShouldSetupPickupAddress => false;
		protected override bool ShouldSetupDeliveryAddress => false;
	}

	sealed class CFSToCFSDeliveryDueDateCalculator_NoTransitTime_NoPickupAndDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Pickup Address and No Pickup Zone Item.
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
			Shipment.JS_A_RCV = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_A_RCV = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_A_RCV = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
		}

		protected override bool ShouldSetupPickupAddress => false;
		protected override bool ShouldSetupDeliveryAddress => false;
		protected override bool ShouldSetupTransitTime => false;
	}
}
