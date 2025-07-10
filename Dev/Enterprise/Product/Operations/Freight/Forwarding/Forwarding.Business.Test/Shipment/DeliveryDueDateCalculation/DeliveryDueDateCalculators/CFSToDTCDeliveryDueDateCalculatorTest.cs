using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CFSToDTCDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 3PM Wednesday 3rd Aug
Pickup CFS Preparation Time: 6 hours -> 9PM Wednesday 3rd Aug
Transit From CFS to CFS: 3 days -> 9PM Saturday 6th Aug (SYD time) -> 11PM Saturday 6th Aug (AKL time)
Expected Delivery Due Date: 11PM Saturday 6th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 7, 15, 0, 0);

		protected override bool ShouldSetupDTC => true;

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 4, ZDateTime.Empty);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(PickupCFSAddress, 6 * 60);
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

		public void TestCalculate_WithDeliveryDueTime()
		{
			AdditionalBasicSetUp();
			ServiceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 12, 0, 0);
			Factory.Save();
			AssertDeliveryDueDate("DTC Shipment should not use DeliveryDueTime from ServiceLevel", ExpectedDeliveryDueDate);
		}
	}
}
