using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using static Enterprise.Freight.Forwarding.Business.DeliveryDueDateCalculator;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TransitFromCFSToCFSDDDCalculationStepTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var transitFromCFSToCFSCalculationStep = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("ALL"), AddressType.CFSDeliveryAddress);

			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);

			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, ZDateTime.Empty);
			var expectedFinalDateTime = initialDateTime.AddHours(10);

			AssertEquals("Final = Initial + 10 hrs transit time", expectedFinalDateTime, transitFromCFSToCFSCalculationStep.Calculate(initialInput).DeliveryDueDate);

			transitFromCFSToCFSCalculationStep = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("ALL"), AddressType.CFSDeliveryAddress);

			FieldInfo originZoneField = typeof(TransitFromCFSToCFSDDDCalculationStep).GetField("originZone", BindingFlags.NonPublic | BindingFlags.Instance);
			originZoneField.SetValue(transitFromCFSToCFSCalculationStep, null);
			FieldInfo destinationZoneField = typeof(TransitFromCFSToCFSDDDCalculationStep).GetField("destinationZone", BindingFlags.NonPublic | BindingFlags.Instance);
			destinationZoneField.SetValue(transitFromCFSToCFSCalculationStep, null);

			var result = transitFromCFSToCFSCalculationStep.Calculate(initialInput);
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Finding Transit Time: Failed to calculate the Delivery Due Date because the Shipment's dates are outside of the validity period defined on the Transit Time record, please check the Effective/End Dates on the Transit Time module.", result.ErrorMessage);
			AssertEquals("Finding Transit Time: Failed to calculate the Delivery Due Date because the Shipment's dates are outside of the validity period defined on the Transit Time record, please check the Effective/End Dates on the Transit Time module.", result.CalculationLog);
		}

		public void TestCalculate_DifferentTimeZones()
		{
			deliveryCFSAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			deliveryCFSAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			destinationZone.Items[0].TQ_RN_NKCountry = "NZ";

			Factory.Save();

			var transitFromCFSToCFSCalculationStep = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("ALL"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);

			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, ZDateTime.Empty);
			var expectedFinalDateTime = initialDateTime.AddHours(10 + 3);
			var result = transitFromCFSToCFSCalculationStep.Calculate(initialInput);
			AssertEquals("Final = Initial + 3 hrs timezone difference + 10 hrs transit time", expectedFinalDateTime, result.DeliveryDueDate);
			AssertEquals(@"Time Conversion between  (NSW) and  (VIC): 26-Sep-22 09:00:00 adjusted to 26-Sep-22 12:00:00, because of the time zone difference between Pickup CFS and Delivery CFS.
Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 10 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 12:00:00 adjusted to 26-Sep-22 22:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);
		}

		public void TestCalculate_TheSameZoneWillNotChangeTheTime()
		{
			var context = CreateDeliveryDueDateCalculationContext("ALL");
			FieldInfo originZoneField = typeof(DeliveryDueDateCalculationContext).GetField("originZone", BindingFlags.NonPublic | BindingFlags.Instance);
			originZoneField.SetValue(context, originZone);
			FieldInfo destinationZoneField = typeof(DeliveryDueDateCalculationContext).GetField("destinationZone", BindingFlags.NonPublic | BindingFlags.Instance);
			destinationZoneField.SetValue(context, originZone);
			var transitFromCFSToCFSCalculationStep = new TransitFromCFSToCFSDDDCalculationStep(context, AddressType.CFSDeliveryAddress);

			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 68, 1, ZDateTime.Empty);
			AssertEquals("Result Should Not Change", initialDateTime, transitFromCFSToCFSCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestCalculate_RefTransitTimeModeFallbacks()
		{
			var lseCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("LSE"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, ZDateTime.Empty);

			var result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, TransitHours = 10hrs", new ZDateTime(2022, 9, 26, 19, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 10 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 19:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			var transittimeAir = DeliveryDueDateCalculationTestHelper.SetupTransitTime(Factory, originZone.PK, destinationZone.PK, "STD", "AIR", 5);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transittimeAir, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 5, 1, ZDateTime.Empty);
			Factory.Save();
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = AIR, TransitHours = 5hrs", new ZDateTime(2022, 9, 26, 14, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 5 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 14:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			var transittimeLSE = DeliveryDueDateCalculationTestHelper.SetupTransitTime(Factory, originZone.PK, destinationZone.PK, "STD", "LSE", 2);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transittimeLSE, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 2, 1, ZDateTime.Empty);
			Factory.Save();
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = LSE, TransitHours = 2hrs", new ZDateTime(2022, 9, 26, 11, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 2 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 11:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTime.RTT_Mode = "ROA";
			Factory.Save();
			var seaCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("SEA"), AddressType.CFSDeliveryAddress);
			result = seaCFSToCFS.Calculate(initialInput);
			AssertEquals("No transit time configured for SEA, should return empty date time", ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Finding Transit Time: Failed to calculate the Delivery Due Date because there is no Transit Time record (or Service Level > Default Transit Time for the current service level) exists between the Origin and Destination Transport Zones for the selected Transport Mode and/or Service Level. Please review the Transit Time setup within Maintain > Locations > Transit Time or select a different Transport Mode and/or Service Level.", result.ErrorMessage);
			AssertEquals("Finding Transit Time: Failed to calculate the Delivery Due Date because there is no Transit Time record (or Service Level > Default Transit Time for the current service level) exists between the Origin and Destination Transport Zones for the selected Transport Mode and/or Service Level. Please review the Transit Time setup within Maintain > Locations > Transit Time or select a different Transport Mode and/or Service Level.", result.CalculationLog);
		}

		public void TestCalculate_UseTransportProviderFallback()
		{
			var originZoneWithEmptyZoneItems = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupCFSAddress.OA_RN_NKCountryCode);
			var destinationZoneWithEmptyZoneItems = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryCFSAddress.OA_RN_NKCountryCode);
			transitTime = DeliveryDueDateCalculationTestHelper.SetupTransitTime(Factory, originZoneWithEmptyZoneItems.PK, destinationZoneWithEmptyZoneItems.PK, "STD", "ALL", 10);

			var transitTimeDetail = DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, new ZDateTime(1900, 1, 1, 8, 10, 0));
			Factory.Save();

			var context = CreateDeliveryDueDateCalculationContext("ALL");
			FieldInfo originZoneField = typeof(DeliveryDueDateCalculationContext).GetField("originZone", BindingFlags.NonPublic | BindingFlags.Instance);
			originZoneField.SetValue(context, originZoneWithEmptyZoneItems);

			FieldInfo destinationZoneField = typeof(DeliveryDueDateCalculationContext).GetField("destinationZone", BindingFlags.NonPublic | BindingFlags.Instance);
			destinationZoneField.SetValue(context, destinationZoneWithEmptyZoneItems);

			var transitFromCFSToCFSCalculationStep = new TransitFromCFSToCFSDDDCalculationStep(context, AddressType.CFSDeliveryAddress);

			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			var expectedFinalDateTime = initialDateTime.AddHours(transitTimeDetail.RTD_TransitHours);
			AssertEquals("Matches TransitTime with Mode = ALL, TransitHours = 10hrs", new ZDateTime(2022, 9, 26, 8, 10, 0), transitFromCFSToCFSCalculationStep.Calculate(initialInput).DeliveryDueDate);
		}

		public void TestFindTransitTimeHoursAndArrivalTimeForDayOfWeek()
		{
			serviceLevel.RS_DefaultTransitHours = 1;//fix
			var lseCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("LSE"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);

			transitTime.RefTransitTimeDetails.RemoveAndDeleteAll();
			var transitTimeDetail = DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, new ZDateTime(1900, 1, 1, 8, 10, 0));
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			var result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, TransitHours = 10hrs", new ZDateTime(2022, 9, 26, 8, 10, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 0 Days
Arrival Time: 08:10:00
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 08:10:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			initialDateTime = new ZDateTime(2022, 9, 27, 9, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, TransitDetail has not been set, fallback to Service Level", new ZDateTime(2022, 9, 27, 10, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"No transit time detail between Origin and Destination found for the specified day of week: Tuesday
Service Level > Default Transit Days/Arrival Time used for the calculation
Transit Time: 1 Hours
Arrival Time: -
Adding Transit Time step: 27-Sep-22 09:00:00 adjusted to 27-Sep-22 10:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			transitTimeDetail.RTD_EffectiveDate = new ZDateTimeOffset(2023, 9, 26, 0, 0, 0);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, EffectiveDate is after initialDateTime, fallback to Service Level", new ZDateTime(2022, 9, 26, 10, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"No transit time detail between Origin and Destination found for the specified day of week: Monday
Service Level > Default Transit Days/Arrival Time used for the calculation
Transit Time: 1 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 10:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.RTD_EffectiveDate = new ZDateTimeOffset(2022, 8, 1, 0, 0, 0);
			transitTimeDetail.RTD_EndDate = new ZDateTimeOffset(2022, 8, 28, 0, 0, 0);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, End Date is before initialDateTime, fallback to Service Level", new ZDateTime(2022, 9, 26, 10, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"No transit time detail between Origin and Destination found for the specified day of week: Monday
Service Level > Default Transit Days/Arrival Time used for the calculation
Transit Time: 1 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 10:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.RTD_EffectiveDate = new ZDateTimeOffset(2022, 8, 1, 0, 0, 0);
			transitTimeDetail.RTD_EndDate = ZDateTimeOffset.Empty;
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, TransitHours = 10hrs", new ZDateTime(2022, 9, 26, 8, 10, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 0 Days
Arrival Time: 08:10:00
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 08:10:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.RTD_EffectiveDate = ZDateTimeOffset.Empty;
			transitTimeDetail.RTD_EndDate = new ZDateTimeOffset(2023, 8, 28, 0, 0, 0);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, TransitHours = 10hrs", new ZDateTime(2022, 9, 26, 8, 10, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 0 Days
Arrival Time: 08:10:00
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 08:10:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.RTD_TransitHours = 0;
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, RTD_TransitHours is zero", new ZDateTime(2022, 9, 26, 10, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"No transit time detail between Origin and Destination found for the specified day of week: Monday
Service Level > Default Transit Days/Arrival Time used for the calculation
Transit Time: 1 Hours
Arrival Time: -
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 10:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			initialDateTime = new ZDateTime(2022, 9, 27, 9, 0, 0);
			initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			transitTimeDetail.RTD_TransitHours = 70;
			transitTimeDetail.RTD_DayOfWeek = 2;
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("The result should consider the number of days in TransitHours and ArrivalTime", new ZDateTime(2022, 9, 29, 8, 10, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Tuesday
Transit Time: 2 Days
Arrival Time: 08:10:00
Adding Transit Time step: 27-Sep-22 09:00:00 adjusted to 29-Sep-22 08:10:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.RTD_ArrivalTime = new ZDateTime(1900, 1, 1, 0, 0, 0);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("The result should consider TransitHours because ArrivalTime is empty", new ZDateTime(2022, 9, 30, 7, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Tuesday
Transit Time: 70 Hours
Arrival Time: -
Adding Transit Time step: 27-Sep-22 09:00:00 adjusted to 30-Sep-22 07:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.TransitDays = 2;
			transitTimeDetail.RTD_EndDate = new ZDateTimeOffset(2022, 9, 28, 0, 0, 0);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, End Date is before afterTransitTime, fallback to Service Level", new ZDateTime(2022, 9, 27, 10, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"No transit time detail between Origin and Destination found for the specified day of week: Tuesday
Service Level > Default Transit Days/Arrival Time used for the calculation
Transit Time: 1 Hours
Arrival Time: -
Adding Transit Time step: 27-Sep-22 09:00:00 adjusted to 27-Sep-22 10:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);

			transitTimeDetail.RTD_EndDate = new ZDateTimeOffset(2022, 9, 30, 0, 0, 0);
			result = lseCFSToCFS.Calculate(initialInput);
			AssertEquals("Matches TransitTime with Mode = ALL, End Date is after afterTransitTime", new ZDateTime(2022, 9, 29, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Tuesday
Transit Time: 48 Hours
Arrival Time: -
Adding Transit Time step: 27-Sep-22 09:00:00 adjusted to 29-Sep-22 09:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);
		}

		public void TestTransitHoursAndArrivalTimeFallbackToServiceLevel()
		{
			var step = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("ALL"), AddressType.CFSDeliveryAddress);
			transitTime.RefTransitTimeDetails.RemoveAndDeleteAll();

			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			var result = step.Calculate(initialInput);
			AssertEquals("No transit time found, service level does not have default transit time", ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Finding Transit Time: Failed to calculate the Delivery Due Date because the Shipment's dates are outside of the validity period defined on the Transit Time record, please check the Effective/End Dates on the Transit Time module.", result.ErrorMessage);

			serviceLevel.RS_DefaultTransitHours = 30;
			AssertEquals("Transit time hours from Service Level", new ZDateTime(2022, 9, 27, 15, 0, 0), step.Calculate(initialInput).DeliveryDueDate);

			serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 0, 0);
			AssertEquals("Transit time days and arrival time from service level", new ZDateTime(2022, 9, 27, 10, 0, 0), step.Calculate(initialInput).DeliveryDueDate);

			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, new ZDateTime(1900, 1, 1, 8, 10, 0));
			AssertEquals("Transit time days and arrival time from transit time details", new ZDateTime(2022, 9, 26, 8, 10, 0), step.Calculate(initialInput).DeliveryDueDate);
			result = step.Calculate(initialInput);
			AssertEquals("Transit time days and arrival time from transit time details\"", new ZDateTime(2022, 9, 26, 8, 10, 0), result.DeliveryDueDate);
			AssertEquals(@"Transit time detail between Origin and Destination found for the specified day of week: Monday
Transit Time: 0 Days
Arrival Time: 08:10:00
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 26-Sep-22 08:10:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);
		}

		public void TestCalculationFlagArrivalTimeFoundInTransitTimeDetail()
		{
			var lseCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("LSE"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			DeliveryDueDateCalculationTestHelper.SetupTransitTimeDetails(Factory, transitTime, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 10, 1, new ZDateTime(1900, 1, 1, 8, 10, 0));
			var result = lseCFSToCFS.Calculate(initialInput);
			Assert(result.ArrivalTimeUsedForDeliveryCFS);
		}

		public void TestCalculationFlagArrivalTimeFoundInServiceLevel()
		{
			var lseCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("LSE"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			transitTime.Delete();
			serviceLevel.RS_DefaultTransitHours = 48;
			serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 8, 0, 0);
			var result = lseCFSToCFS.Calculate(initialInput);
			Assert(result.ArrivalTimeUsedForDeliveryCFS);
		}

		public void TestCalculationFlagNoArrivalTimeFound()
		{
			var lseCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContext("LSE"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			var result = lseCFSToCFS.Calculate(initialInput);
			Assert(!result.ArrivalTimeUsedForDeliveryCFS);
		}

		public void TestTransitFromCFSToCFSShouldNotThrowExceptionWhenAddressesAreNull()
		{
			pickupCFSAddress = deliveryCFSAddress = null;
			originZone = destinationZone = null;
			serviceLevel.RS_DefaultTransitHours = 30;
			serviceLevel.RS_DefaultArrivalTime = new ZDateTime(1900, 1, 1, 10, 0, 0);

			var lseCFSToCFS = new TransitFromCFSToCFSDDDCalculationStep(CreateDeliveryDueDateCalculationContextWithNull("LSE"), AddressType.CFSDeliveryAddress);
			var initialDateTime = new ZDateTime(2022, 9, 26, 9, 0, 0);
			var initialInput = DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty);
			AssertNoExceptionThrown(() =>
			{
				var result = lseCFSToCFS.Calculate(initialInput);
				AssertEquals("Default Transit Time from Service Level", new ZDateTime(2022, 9, 27, 10, 0, 0), result.DeliveryDueDate);
				AssertEquals(@"No transit time detail between Origin and Destination found for the specified day of week: Monday
Service Level > Default Transit Days/Arrival Time used for the calculation
Transit Time: 1 Days
Arrival Time: 10:00:00
Adding Transit Time step: 26-Sep-22 09:00:00 adjusted to 27-Sep-22 10:00:00 based on Transit Time record (Or Service level default)
", result.CalculationLog);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			serviceLevel = DeliveryDueDateCalculationTestHelper.GetOrCreateRefServiceLevelIfNotExist(Factory, "STD");

			pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Melbourne", "VIC", "AUMEL");

			originZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupCFSAddress.Postcode, ZString.Empty, pickupCFSAddress.OA_RN_NKCountryCode);
			destinationZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, "6150", "6160", deliveryCFSAddress.OA_RN_NKCountryCode);
			transitTime = DeliveryDueDateCalculationTestHelper.SetupTransitTime(Factory, originZone.PK, destinationZone.PK, "STD", "ALL", 10);

			Factory.Save();
		}

		OrgHeader pickupCFSOrg, deliveryCFSOrg;
		OrgAddress pickupCFSAddress, deliveryCFSAddress;
		RateTransportZone originZone, destinationZone;
		RefTransitTime transitTime;
		RefServiceLevel serviceLevel;

		#region Helper Methods

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContextWithNull(string mode)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				mode,
				ZString.Empty);
		}

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext(string mode)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				pickupCFSOrg.OH_Code,
				pickupCFSAddress.AddressCode,
				pickupCFSOrg.OH_Code,
				pickupCFSAddress.AddressCode,
				deliveryCFSOrg.OH_Code,
				deliveryCFSAddress.AddressCode,
				deliveryCFSOrg.OH_Code,
				deliveryCFSAddress.AddressCode,
				mode,
				ZString.Empty
			);
		}

		#endregion
	}
}
