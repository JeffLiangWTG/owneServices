using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DoorToDTCDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Friday 5th Aug
Transit from Pickup Address to Pickup CFS Address: 8 hours -> 5PM Friday 5th Aug
Pickup CFS Preparation Time: 6 hours -> 3PM Monday 8th Aug
Transit From CFS to CFS: 5 days -> 3PM Saturday 13th Aug (SYD time) -> 5PM Saturday 13th Aug (AKL time)
Expected Delivery Due Date: 5PM Saturday 13th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 13, 17, 0, 0);

		protected override bool ShouldSetupDTC => true;

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 5 * 24, 1, ZDateTime.Empty);
			OriginZone.Items[0].TQ_BeyondHours = 8;
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(PickupCFSAddress, 6 * 60);
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

		public void TestCalculate_WithDeliveryDueTime()
		{
			AdditionalBasicSetUp();
			ServiceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(1900, 1, 1, 12, 0, 0);
			Factory.Save();
			AssertDeliveryDueDate("DTC Shipment should not use DeliveryDueTime from ServiceLevel", ExpectedDeliveryDueDate);
		}
	}
}
