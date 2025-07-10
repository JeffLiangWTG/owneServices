using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AirportToDoorDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Monday 1st Aug
Transit From CFS to CFS: 2 days -> 9AM Wednesday 3rd Aug (SYD time) -> 11AM Wednesday 3rd Aug (AKL time)
Delivery CFS Preparation Time: 6 hours -> 5PM Wednesday 3rd Aug
Transit from Delivery CFS Address to Delivery Address: 2 days -> 5PM Friday 5th Aug
Expected Delivery Due Date: 5PM Friday 5th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 5, 17, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			var consol = Shipment.Consols.AddNew();
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2022, 8, 1, 9, 0, 0);
			transport.JW_RL_NKDiscPort = "NZAKL";

			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 2 * 24, 1, ZDateTime.Empty);
			DestinationZone.Items[0].TQ_BeyondHours = 2 * 24;
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(DeliveryCFSAddress, 6 * 60);
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.JS_E_DEP = new ZDateTime(2022, 8, 1, 9, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.JS_E_DEP = ZDateTime.Empty;
			Shipment.Consols[0].Transports.RemoveAndDeleteAll();
		}

		public void TestCalculate_DeliveryDueTime()
		{
			AdditionalBasicSetUp();
			var deliveryDueTime = ZDateTime.DefaultDurationEpoch.AddHours(12);

			ServiceLevel.RS_DefaultDeliveryDueTime = deliveryDueTime;
			DestinationZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.AddMinutes(1);
			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = deliveryDueTime.AddMinutes(2);
			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(17);

			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 12:01", new ZDateTime(2022, 8, 8, 12, 1, 0));

			DestinationZone.Items[0].TQ_BeyondHours = 4 * 24;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 12:01", new ZDateTime(2022, 8, 8, 12, 1, 0));

			DestinationZone.Items[0].TQ_BeyondHours = 0;
			DestinationZone.Items[0].TQ_IsBeyond = false;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 12:01", new ZDateTime(2022, 8, 4, 12, 1, 0));

			DestinationZone.Items[0].TQ_DeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DefaultDeliveryDueTime from Transport Provider: 12:02", new ZDateTime(2022, 8, 4, 12, 2, 0));

			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DefaultDeliveryDueTime from Service Level: 12:00", new ZDateTime(2022, 8, 4, 12, 0, 0));
		}

		public void TestCalculate_DeliverOnWeekend()
		{
			AdditionalBasicSetUp();
			ServiceLevel.RS_DefaultDeliveryDueTime = ZDateTime.DefaultDurationEpoch.AddHours(12);
			ServiceLevel.RS_DeliverOnSaturday = true;
			Factory.Save();
			AssertDeliveryDueDate("Deliver to Saturday and uses DefaultDeliveryDueTime from Service Level: 12:00", new ZDateTime(2022, 8, 6, 12, 0, 0));
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
				expectedTimeTransitTime: new ZDateTime(2022, 8, 5, 13, 0, 0),
				expectedTimeServiceLevel: new ZDateTime(2022, 8, 5, 12, 30, 0));
		}
		protected override bool ShouldSetupDummyDestinationZone => true;
	}

	sealed class AirportToDoorDeliveryDueDateCalculator_NoTransitTime_NoPickupAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Pickup Address and No Pickup Zone Item.
No Transit time between CFS Zones, using Service Level to calculate DDD.
Using Service Level Default Transit Hours: 6 days
Using Service Level Default Arrival Time: 10:30AM
Expected Delivery Due Date: 10:30AM Tuesday 1st Oct

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 10, 1, 10, 30, 0);

		protected override void AdditionalBasicSetUp()
		{
			var consol = Shipment.Consols.AddNew();
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2024, 9, 25, 9, 0, 0);
			transport.JW_RL_NKDiscPort = "NZAKL";

			ServiceLevel.RS_DefaultTransitHours = 144;
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

		public void TestCalculate_DeliveryDueTime()
		{
			AdditionalBasicSetUp();
			var deliveryDueTime = ZDateTime.DefaultDurationEpoch.Add(new TimeSpan(10, 30, 0));

			ServiceLevel.RS_DefaultDeliveryDueTime = deliveryDueTime;
			DestinationZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.AddMinutes(1);
			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = deliveryDueTime.AddMinutes(2);
			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(17);

			Factory.Save();
			AssertDeliveryDueDate("Uses DefaultDeliveryDueTime from Service Level: 10:30, even though transport zone set has a value for DeliveryDueTime", new ZDateTime(2024, 10, 1, 10, 30, 0));

			ServiceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Transport Provider: 10:31, Delivery Due Time fallback to ZoneItem", new ZDateTime(2024, 10, 1, 10, 31, 0));

			DestinationZone.Items.DeleteAll();
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Transport Provider: 10:32, Delivery Due Time fallback to Transport Provider", new ZDateTime(2024, 10, 1, 10, 32, 0));
		}

		protected override bool ShouldSetupPickupAddress => false;
		protected override bool ShouldSetupTransitTime => false;
	}

	sealed class AirportToDoorDeliveryDueDateCalculator_HasTransitTime_NoPickupAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Pickup Address and No Pickup Zone Item.
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

		public void TestCalculate_DeliveryDueTime()
		{
			AdditionalBasicSetUp();
			var deliveryDueTime = new ZDateTime(1900, 1, 1, 10, 0, 0);

			ServiceLevel.RS_DefaultDeliveryDueTime = deliveryDueTime;
			DestinationZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.AddMinutes(1);
			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = deliveryDueTime.AddMinutes(2);
			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(17);

			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 10:01", new ZDateTime(2024, 9, 26, 10, 1, 0));

			DestinationZone.Items[0].TQ_BeyondHours = 4 * 24;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 10:01", new ZDateTime(2024, 9, 30, 10, 1, 0));

			DestinationZone.Items[0].TQ_BeyondHours = 0;
			DestinationZone.Items[0].TQ_IsBeyond = false;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 10:01", new ZDateTime(2024, 9, 26, 10, 1, 0));

			DestinationZone.Items[0].TQ_DeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DefaultDeliveryDueTime from Transport Provider: 10:02", new ZDateTime(2024, 9, 26, 10, 2, 0));

			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DefaultDeliveryDueTime from Service Level: 10:00", new ZDateTime(2024, 9, 26, 10, 0, 0));
		}

		protected override bool ShouldSetupPickupAddress => false;
		protected override bool ShouldSetupDummyDestinationZone => true;
	}
}
