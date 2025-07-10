using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DoorToDoorDeliveryDueDateCalculatorTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 10AM Tuesday 9th Aug
Transit from Pickup Address to Pickup CFS Address: 8 hours -> 6PM Tuesday 9th Aug
Pickup CFS Preparation Time: 2 hours -> 11AM Wednesday 10th Aug
Transit From CFS to CFS: 3 days -> 11AM Saturday 13th Aug (SYD time) -> 1PM Saturday 13th Aug (AKL time)
Delivery CFS Preparation Time: 4 hours -> 1PM Monday 15th Aug
Transit from Delivery CFS Address to Delivery Address: 4 hours -> 1PM Monday 15th Aug
Expected Delivery Due Date: 5PM Monday 15th Aug

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2022, 8, 15, 17, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 3, ZDateTime.Empty);

			OriginZone.Items[0].TQ_BeyondHours = 8;
			DestinationZone.Items[0].TQ_BeyondHours = 4;
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(PickupCFSAddress, 2 * 60);
			DeliveryDueDateCalculationTestHelper.SetProcessingTime(DeliveryCFSAddress, 4 * 60);
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2022, 8, 9, 10, 0, 0);
		}

		public void TestDBHits()
		{
			AdditionalBasicSetUp();
			Factory.Save();
			var calculator = GetNewDeliveryDueDateCalculatorForTest();

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgTimetableSchema.Constants.TableName, 2 }, // OpeningHoursDDDCalculationStep for 4 addresses
				{ RateTransportZoneItemSchema.Constants.TableName, 4 }, // OriginZoneItem Provider=OPS, OriginZoneItem Provider=ALL, DestinationZoneItem Provider=OPS, DestinationZoneItem Provider=ALL
				{ RefTransitTimeSchema.Constants.TableName, 1 }, // TransitFromCFSToCFSDDDCalculationStep
				{ RefUNLOCOSchema.Constants.TableName, 2 } // UNLOCOs to be loaded: AUSYD, NZAKL
			};

			using (AssertDbHitsForAllFactories(expectedDbHits, ignoreUnspecified: true, thresholdForUnspecified: 10))
			{
				var result = calculator.CalculateDeliveryDueDate();
				AssertEquals(true, result.IsSuccess);
			}
		}

		protected override void AdditionalFallBackSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2022, 8, 9, 10, 0, 0);
		}

		protected override void EmptyReadyDateSetUp()
		{
			Shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			Shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
		}

		public void TestCalculate_DeliveryDueTime()
		{
			AdditionalBasicSetUp();
			var deliveryDueTime = ZDateTime.DefaultDurationEpoch.AddHours(16);

			ServiceLevel.RS_DefaultDeliveryDueTime = deliveryDueTime;
			DestinationZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.AddMinutes(1);
			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = deliveryDueTime.AddMinutes(2);
			DestinationZone.TransportProvider.TP_DefaultHoldForPickupTime = TimeSpan.FromHours(17);

			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 16:01", new ZDateTime(2022, 8, 16, 16, 1, 0));

			DestinationZone.Items[0].TQ_BeyondHours = 1;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 16:01", new ZDateTime(2022, 8, 15, 16, 1, 0));

			DestinationZone.Items[0].TQ_BeyondHours = 0;
			DestinationZone.Items[0].TQ_IsBeyond = false;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Zone Item: 16:01", new ZDateTime(2022, 8, 15, 16, 1, 0));

			DestinationZone.Items[0].TQ_DeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DeliveryDueTime from Transport Provider: 16:02", new ZDateTime(2022, 8, 15, 16, 2, 0));

			DestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = ZDateTime.Empty;
			Factory.Save();
			AssertDeliveryDueDate("Uses DefaultDeliveryDueTime from Service Level: 16:00", new ZDateTime(2022, 8, 15, 16, 0, 0));
		}

		public void TestCalculate_DeliverOnWeekend()
		{
			AdditionalBasicSetUp();
			DeliveryCFSAddress.SetTimetablesRangeType(MasterFiles.Business.OrgTimeTableRangeType.NotApplicable);
			ServiceLevel.RS_DefaultDeliveryDueTime = ZDateTime.DefaultDurationEpoch.AddHours(16);
			ServiceLevel.RS_DeliverOnSaturday = true;
			Factory.Save();
			AssertDeliveryDueDate("Deliver to Saturday and uses DefaultDeliveryDueTime from Service Level: 16:00", new ZDateTime(2022, 8, 13, 16, 0, 0));
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
				transitHours: 24,
				transitDayOfWeek: 2,
				expectedTimeTransitTime: new ZDateTime(2022, 8, 10, 15, 0, 0),
				expectedTimeServiceLevel: new ZDateTime(2022, 8, 11, 14, 30, 0));
		}

		public void TestCalculate_CutOffTime()
		{
			AdditionalBasicSetUp();

			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, TransitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 3 * 24, 4, ZDateTime.Empty);
			Factory.Save();
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			Assert(!result.CalculationLog.Contains("Applying Cutoff time for PIC"));
			Assert(!result.CalculationLog.Contains("Applying Cutoff time for DLV"));
			AssertEquals("non working days: Saturday 13-Aug-22; Sunday 14-Aug-22", new ZDateTime(2022, 8, 15, 17, 0, 0), result.DeliveryDueDate);

			DeliveryDueDateCalculationTestHelper.SetCutOffTime(PickupCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Pickup);
			DeliveryDueDateCalculationTestHelper.SetCutOffTime(DeliveryCFSAddress, ZDateTime.DefaultDurationEpoch.AddHours(1), OrgTimetableType.Codes.Deliver);
			Factory.Save();
			result = calculator.CalculateDeliveryDueDate();
			Assert("The calculation will consider the cut-off time for PIC and DLV", result.CalculationLog.Contains("Applying Cutoff time for PIC"));
			Assert("The calculation will consider the cut-off time for PIC and DLV", result.CalculationLog.Contains("Applying Cutoff time for DLV"));
			AssertEquals(new ZDateTime(2022, 8, 16, 13, 0, 0), result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}
		protected override bool ShouldSetupDummyOriginZone => true;
		protected override bool ShouldSetupDummyDestinationZone => true;
	}

	sealed class DoorToDoorDeliveryDueDateCalculator_NoTransitTimeSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
No Transit time in zone set between Pickup Zone Item and Delivery Zone Item.
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

		protected override bool ShouldSetupTransitTime => false;
	}

	sealed class DoorToDoorDeliveryDueDateCalculator_NoZoneItemFromNonCFSAddress_NoZoneItemFromCFSAddressSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
Pickup Address and PickupCFS Address exists but no OriginZone is active.
So although tranit time is set, it will not be used as originZone is null.
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

		protected override bool ShouldSetupOriginZoneActive => false;
		protected override bool ShouldSetupDestinationZoneActive => false;
	}

	sealed class DoorToDoorDeliveryDueDateCalculator_NoZoneItemFromNonCFSAddress_HasZoneItemFromCFSAddressAndTransitTimeSetupTest : DeliveryDueDateCalculatorBaseTest
	{
		protected override ZString Mode => Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

		protected override ZString AssertMessage => @"Cargo Pickup By: 9AM Wednesday 25th Sep
Pickup Address exists but no zoneItem from nonCFSAddress is built.
Pickup CFS Address exists and zoneItem from CFSAddress is built,
so zone should fallback to ignoreNonCFSAddress, and transit time is set.
Transit Time is 10 hrs, using Transit Time to Calculate CFS to CFS.
Expected Delivery Due Date: 9:00AM Thursday 26th Sep

Note: All addresses operate Mon-Fri 9AM-5PM.";

		protected override ZDateTime ExpectedDeliveryDueDate => new ZDateTime(2024, 9, 26, 9, 0, 0);

		protected override void AdditionalBasicSetUp()
		{
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

		protected override bool ShouldSetupOriginZoneFromPickupAddress => false;
		protected override bool ShouldSetupDestinationZoneFromDeliveryAddress => false;
	}
}
