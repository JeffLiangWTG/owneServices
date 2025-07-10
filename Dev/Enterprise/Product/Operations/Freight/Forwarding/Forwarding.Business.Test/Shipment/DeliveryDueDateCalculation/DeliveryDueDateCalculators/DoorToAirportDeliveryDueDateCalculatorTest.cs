using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DoorToAiportDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Friday 5th Aug
Transit from Pickup Address to Pickup CFS Address: 8 hours -> 5PM Friday 5th Aug
Pickup from CFS on Monday: 9AM Friday 8th Aug
Transit From CFS to Airport: 5 days -> 9AM Saturday 13th Aug (SYD time) -> 11AM Saturday 13th Aug (AKL time)
Expected Delivery Due Date: 11AM Saturday 13th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 13, 11, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 5 * 24, 1, ZDateTime.Empty);
			OriginZone.Items[0].TQ_BeyondHours = 8;
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
		protected override bool ShouldSetupDummyOriginZone => true;
	}

	sealed class DoorToAiportDeliveryDueDateCalculator_HasTransitTime_NoDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;

		protected override ZString AssertMessage => @"Cargo Pickup By: 4PM Wednesday 25th Sep
No Delivery Address and No Delivery Zone Item.
Transit Time is 10 hrs, using Transit Time to Calculate CFS to CFS.
Expected Delivery Due Date: 4:00AM Thursday 26th Sep";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 26, 4, 0, 0);

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

	sealed class DoorToAiportDeliveryDueDateCalculator_NoTransitTime_NoDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT;

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
			ServiceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 30, 0);
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
