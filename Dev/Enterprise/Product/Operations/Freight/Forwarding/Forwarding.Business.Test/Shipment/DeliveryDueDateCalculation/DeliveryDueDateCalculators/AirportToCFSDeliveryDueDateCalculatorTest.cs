using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AirportToCFSDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 3PM Wednesday 3rd Aug
Transit From CFS to CFS: 3 days -> 3PM Saturday 6th Aug (SYD time) -> 5PM Saturday 6th Aug (AKL time)
Delivery CFS Preparation Time: 6 hours -> 3PM Monday 8th Aug
Expected Delivery Due Date: 3PM Monday 8th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 8, 15, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			var consol = Shipment.Consols.AddNew();
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2022, 8, 3, 15, 0, 0);
			transport.JW_RL_NKDiscPort = "NZAKL";

			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 3, ZDateTime.Empty);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(PickupCFSAddress, 6 * 60);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(DeliveryCFSAddress, 6 * 60);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_E_DEP = new ZDateTime(2022, 8, 3, 15, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_E_DEP = ZDateTime.Empty;
			Shipment.Consols[0].Transports.RemoveAndDeleteAll();
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
				transitDayOfWeek: 3,
				expectedTimeTransitTime: new ZDateTime(2022, 8, 5, 13, 0, 0),
				expectedTimeServiceLevel: new ZDateTime(2022, 8, 5, 12, 30, 0));
		}

		public void TestCalculate_CutOffTime()
		{
			AdditionalBasicSetUp();
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(PickupCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Pickup);
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(DeliveryCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Deliver);
			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(14);
			Factory.Save();

			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();

			Assert("The calculation will not consider the cut-off time", !result.CalculationLog.Contains("Applying Cutoff time for PIC"));
			Assert("The calculation will not consider the cut-off time", !result.CalculationLog.Contains("Applying Cutoff time for DLV"));

			AssertEquals(@"
Default Hold For Pickup Time: 14:00, 3PM is after 2PM, so add an additonal day
Expected Delivery Due Date: 14:00 Tuesday 9th Aug", new ZDateTime(2022, 8, 9, 14, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}
	}

	sealed class AirportToCFSDeliveryDueDateCalculator_HasTransitTime_NoDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Pickup Address and No Pickup Zone Item.
No Delivery Address and No Delivery Zone Item.
Transit time between CFS Zones is 10 hrs, using this to calculate DDD.
Expected Delivery Due Date: 9:00AM Thursday 26th Sep

Note: All addresses operate Mon-Fri 9AM-5PM.";
		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 26, 9, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			var consol = Shipment.Consols.AddNew();
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2024, 9, 25, 9, 0, 0);
			transport.JW_RL_NKDiscPort = "NZAKL";
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_E_DEP = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_E_DEP = ZDateTime.Empty;
			Shipment.Consols[0].Transports.RemoveAndDeleteAll();
		}

		protected override bool ShouldSetupPickupAddress => false;
		protected override bool ShouldSetupDeliveryAddress => false;
	}

	sealed class AirportToCFSDeliveryDueDateCalculator_NoTransitTime_NoPickupAndDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Delivery Address and No Delivery Zone Item.
Using Service Level Default Transit Hours: 48
Using Service Level Default Arrival Time: 10:30AM
Expected Delivery Due Date: 10:30AM Friday 27th Sep

Note: All addresses operate Mon-Fri 9AM-5PM.";
		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 27, 10, 30, 0);

		protected override void AdditionalBasicSetUp()
		{
			var consol = Shipment.Consols.AddNew();
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2024, 9, 25, 9, 0, 0);
			transport.JW_RL_NKDiscPort = "NZAKL";
			ServiceLevel.RS_DefaultTransitHours = 48;
			ServiceLevel.RS_DefaultArrivalTime = ZDateTime.DefaultDurationEpoch.Add(new TimeSpan(10, 30, 0));
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_E_DEP = new ZDateTime(2024, 9, 25, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_E_DEP = ZDateTime.Empty;
			Shipment.Consols[0].Transports.RemoveAndDeleteAll();
		}

		protected override bool ShouldSetupPickupAddress => false;
		protected override bool ShouldSetupDeliveryAddress => false;
		protected override bool ShouldSetupTransitTime => false;
	}
}
