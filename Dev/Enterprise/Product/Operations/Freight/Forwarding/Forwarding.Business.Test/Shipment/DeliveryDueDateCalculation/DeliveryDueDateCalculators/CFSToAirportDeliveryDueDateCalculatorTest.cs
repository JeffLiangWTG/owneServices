using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CFSToAirportDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT;

		protected override ZString AssertMessage => @"Cargo Pickup By: 3PM Wednesday 3rd Aug
Transit From CFS to Airport: 3 days -> 3PM Saturday 6th Aug (SYD time) -> 5PM Saturday 6th Aug (AKL time)
";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 6, 17, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 3, ZDateTime.Empty);
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
	}

	sealed class CFSToAirportDeliveryDueDateCalculator_HasTransitTime_NoPickupAndDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Pickup Address and No Pickup Zone Item.
No Delivery Address and No Delivery Zone Item.
Transit time between CFS Zones is 10 hrs, using this to calculate DDD.
Expected Delivery Due Date: 9:00PM Wednesday 25th Sep

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 25, 21, 0, 0);

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

	sealed class CFSToAirportDeliveryDueDateCalculator_NoTransitTime_NoPickupAndDeliveryAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT;

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
			ServiceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 30, 0);
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
