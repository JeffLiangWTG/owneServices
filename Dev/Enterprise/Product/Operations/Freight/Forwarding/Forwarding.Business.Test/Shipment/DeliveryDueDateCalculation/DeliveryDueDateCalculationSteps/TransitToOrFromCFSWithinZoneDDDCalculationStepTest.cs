using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TransitToOrFromCFSWithinZoneDDDCalculationStepTest : TestCaseWithFactory
	{
		public void TestTransitFromPickupAddressToPickupCFS_ShouldReturnEmptyDateTime_WhenThereIsNotAnyActiveTransportZoneSetForPickupCFS()
		{
			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var zone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupAddress.Postcode, ZString.Empty, pickupAddress.OA_RN_NKCountryCode, 0);
			zone.TZ_IsActive = false;

			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 9, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, pickupOrg.OH_Code, pickupAddress.AddressCode, pickupCFSOrg.OH_Code, pickupCFSAddress.AddressCode), OrgTimetableType.Codes.Pickup);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Return initial time when timezone is null", readyDate, actualResult.DeliveryDueDate);
			AssertEquals(true, actualResult.IsSuccess);
			AssertEquals(@"Beyond Days/Hours could not be found as the Zone Item for Pickup/Delivery CFS is not present.
", actualResult.CalculationLog);
		}

		public void TestTransitFromPickupAddressToPickupCFS_ShouldChangeTimeToCFSLocalTime()
		{
			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupAddress.Postcode, ZString.Empty, pickupAddress.OA_RN_NKCountryCode, 0);
			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 9, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var expectedResult = new ZDateTime(2022, 09, 26, 7, 12, 22);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, pickupOrg.OH_Code, pickupAddress.AddressCode, pickupCFSOrg.OH_Code, pickupCFSAddress.AddressCode), OrgTimetableType.Codes.Pickup);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Time zone difference is based on OA_City when they have value", expectedResult, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Pickup CFS and Pickup address: 26-Sep-22 09:12:22 adjusted to 26-Sep-22 07:12:22, because of the time zone difference between Pickup CFS and Pickup address.
Beyond days for Pickup: -
", actualResult.CalculationLog);

			pickupCFSAddress.OA_City = ZString.Empty;
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Time zone difference is based on OA_RL_NKRelatedPortCode when OA_City is empty", expectedResult, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Pickup CFS and Pickup address: 26-Sep-22 09:12:22 adjusted to 26-Sep-22 07:12:22, because of the time zone difference between Pickup CFS and Pickup address.
Beyond days for Pickup: -
", actualResult.CalculationLog);

			pickupCFSAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Time zone difference is based on OH_RL_NKClosestPort when OA_RL_NKRelatedPortCode is empty", expectedResult, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Pickup CFS and Pickup address: 26-Sep-22 09:12:22 adjusted to 26-Sep-22 07:12:22, because of the time zone difference between Pickup CFS and Pickup address.
Beyond days for Pickup: -
", actualResult.CalculationLog);

			pickupCFSOrg.OH_RL_NKClosestPort = ZString.Empty;
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Time zone difference is 0 when OA_City, OA_RL_NKRelatedPortCode, OH_RL_NKClosestPort are all empty", readyDate, actualResult.DeliveryDueDate);
			AssertEquals(@"Beyond days for Pickup: -
", actualResult.CalculationLog);
		}

		public void TestTransitFromPickupAddressToPickupCFS_ShouldAddBeyondHours()
		{
			var beyondHours1 = 20;
			var beyondHours2 = 50;

			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone1 = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupAddress.Postcode, ZString.Empty, pickupAddress.OA_RN_NKCountryCode, beyondHours1);
			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 9, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var expectedResult1 = (new ZDateTime(2022, 09, 26, 7, 12, 22)).AddHours(beyondHours1);
			var expectedResult2 = (new ZDateTime(2022, 09, 26, 7, 12, 22)).AddHours(beyondHours2);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, pickupOrg.OH_Code, pickupAddress.AddressCode, pickupCFSOrg.OH_Code, pickupCFSAddress.AddressCode), OrgTimetableType.Codes.Pickup);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(expectedResult1, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Pickup CFS and Pickup address: 26-Sep-22 09:12:22 adjusted to 26-Sep-22 07:12:22, because of the time zone difference between Pickup CFS and Pickup address.
Beyond days for Pickup: 0 Days + 20 Hours
Adding Beyond days/hours step for Pickup: 26-Sep-22 07:12:22 adjusted to 27-Sep-22 03:12:22 based on zone definition
", actualResult.CalculationLog);

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, "2000", "2500", pickupAddress.OA_RN_NKCountryCode, beyondHours2);
			transportZone1.TZ_IsActive = false;
			Factory.Save();

			beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, pickupOrg.OH_Code, pickupAddress.AddressCode, pickupCFSOrg.OH_Code, pickupCFSAddress.AddressCode), OrgTimetableType.Codes.Pickup);
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(expectedResult2, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Pickup CFS and Pickup address: 26-Sep-22 09:12:22 adjusted to 26-Sep-22 07:12:22, because of the time zone difference between Pickup CFS and Pickup address.
Beyond days for Pickup: 2 Days + 2 Hours
Adding Beyond days/hours step for Pickup: 26-Sep-22 07:12:22 adjusted to 28-Sep-22 09:12:22 based on zone definition
", actualResult.CalculationLog);
		}

		public void TestTransitFromDeliveryCFSToDeliveryAddress_ShouldReturnReadyDateAtDeliveryAddressLocalTime_WhenThereIsNotAnyActiveTransportZoneSetForDeliveryCFS()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var zone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 0);
			zone.TZ_IsActive = false;

			Factory.Save();

			var readyDateAtCannington = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDateAtCannington, ZString.Empty);

			var readyDateAtSydney = readyDateAtCannington.AddHours(2);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, deliveryCFSOrg.OH_Code, deliveryCFSAddress.AddressCode), OrgTimetableType.Codes.Deliver);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtSydney, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Delivery CFS and Delivery address: 26-Sep-22 07:12:22 adjusted to 26-Sep-22 09:12:22, because of the time zone difference between Delivery CFS and Delivery address.
Beyond days for Delivery: -
", actualResult.CalculationLog);
		}

		public void TestTransitFromDeliveryCFSToDeliveryAddress_ShouldChangeTimeToDeliveryAddressLocalTime()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 0);
			Factory.Save();

			var readyDateAtCannington = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDateAtCannington, ZString.Empty);

			var readyDateAtSydney = readyDateAtCannington.AddHours(2);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, deliveryCFSOrg.OH_Code, deliveryCFSAddress.AddressCode), OrgTimetableType.Codes.Deliver);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtSydney, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Delivery CFS and Delivery address: 26-Sep-22 07:12:22 adjusted to 26-Sep-22 09:12:22, because of the time zone difference between Delivery CFS and Delivery address.
Beyond days for Delivery: -
", actualResult.CalculationLog);

			deliveryCFSAddress.OA_City = ZString.Empty;
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtSydney, actualResult.DeliveryDueDate);
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtSydney, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Delivery CFS and Delivery address: 26-Sep-22 07:12:22 adjusted to 26-Sep-22 09:12:22, because of the time zone difference between Delivery CFS and Delivery address.
Beyond days for Delivery: -
", actualResult.CalculationLog);

			deliveryCFSAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtSydney, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Delivery CFS and Delivery address: 26-Sep-22 07:12:22 adjusted to 26-Sep-22 09:12:22, because of the time zone difference between Delivery CFS and Delivery address.
Beyond days for Delivery: -
", actualResult.CalculationLog);

			deliveryCFSOrg.OH_RL_NKClosestPort = ZString.Empty;
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtCannington, actualResult.DeliveryDueDate);
			AssertEquals(@"Beyond days for Delivery: -
", actualResult.CalculationLog);
		}

		public void TestTransitFromDeliveryCFSToDeliveryAddress_ShouldAddBeyondHours()
		{
			var beyondHours1 = 20;
			var beyondHours2 = 50;

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone1 = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, beyondHours1);
			Factory.Save();

			var readyDateAtCannington = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDateAtCannington, ZString.Empty);

			var readyDateAtSydney = readyDateAtCannington.AddHours(2);
			var expectedResult1 = readyDateAtSydney.AddHours(beyondHours1);
			var expectedResult2 = readyDateAtSydney.AddHours(beyondHours2);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, deliveryCFSOrg.OH_Code, deliveryCFSAddress.AddressCode), OrgTimetableType.Codes.Deliver);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(expectedResult1, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Delivery CFS and Delivery address: 26-Sep-22 07:12:22 adjusted to 26-Sep-22 09:12:22, because of the time zone difference between Delivery CFS and Delivery address.
Beyond days for Delivery: 0 Days + 20 Hours
Adding Beyond days/hours step for Delivery: 26-Sep-22 09:12:22 adjusted to 27-Sep-22 05:12:22 based on zone definition
", actualResult.CalculationLog);

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, "2000", "2500", deliveryAddress.OA_RN_NKCountryCode, beyondHours2);
			transportZone1.TZ_IsActive = false;
			Factory.Save();

			beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, deliveryCFSOrg.OH_Code, deliveryCFSAddress.AddressCode), OrgTimetableType.Codes.Deliver);
			actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(expectedResult2, actualResult.DeliveryDueDate);
			AssertEquals(@"Transit between Delivery CFS and Delivery address: 26-Sep-22 07:12:22 adjusted to 26-Sep-22 09:12:22, because of the time zone difference between Delivery CFS and Delivery address.
Beyond days for Delivery: 2 Days + 2 Hours
Adding Beyond days/hours step for Delivery: 26-Sep-22 09:12:22 adjusted to 28-Sep-22 11:12:22 based on zone definition
", actualResult.CalculationLog);
		}

		public void TestTransitFromDeliveryCFSToDeliveryAddress_ShouldIgnoreBeyondHours_WhenTransitTimeUsedServiceLevel()
		{
			var deliveryDueTime = new ZDateTime(1900, 1, 1, 12, 0, 0);
			var beyondHours = 20;

			var refServiceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery(RefServiceLevelSchema.RS_Code, "STD"));
			refServiceLevel.RS_DefaultDeliveryDueTime = deliveryDueTime;

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, beyondHours);
			Factory.Save();

			var readyDateAtCannington = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDateAtCannington, ZString.Empty);

			var readyDateAtSydney = readyDateAtCannington.AddHours(2);

			var context = CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, deliveryCFSOrg.OH_Code, deliveryCFSAddress.AddressCode);
			FieldInfo serviceLevelGenericTransitTimeHasBeenUsed = typeof(DeliveryDueDateCalculationContext).GetField("<ServiceLevelGenericTransitTimeHasBeenUsed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
			serviceLevelGenericTransitTimeHasBeenUsed.SetValue(context, true);
			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(context, OrgTimetableType.Codes.Deliver);

			var actualResult = beyondHoursStep.Calculate(initialInput);
			AssertEquals(readyDateAtSydney, actualResult.DeliveryDueDate);
		}

		public void TestTransitFromPickupAddressToPickupCFS_ShouldIgnoreBeyondHours_WhenCFSAddressIsNull()
		{
			var beyondHours = 20;

			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupAddress.Postcode, ZString.Empty, pickupAddress.OA_RN_NKCountryCode, beyondHours);
			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, pickupOrg.OH_Code, pickupAddress.AddressCode, null, null), OrgTimetableType.Codes.Pickup);
			var result = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Beyond Hours should be ignored because pickup cfs address is null", readyDate, result.DeliveryDueDate);
			AssertEquals("Finding Beyond Hours: Skipped\r\n", result.CalculationLog);
		}

		public void TestTransitFromDeliveryCFSToDeliveryAddress_ShouldIgnoreBeyondHours_WhenCFSAddressIsNull()
		{
			var beyondHours = 20;

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, beyondHours);
			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, null, null), OrgTimetableType.Codes.Deliver);
			var result = beyondHoursStep.Calculate(initialInput);
			AssertEquals("Beyond Hours should be ignored because delivery cfs address is null", readyDate, result.DeliveryDueDate);
			AssertEquals("Finding Beyond Hours: Skipped\r\n", result.CalculationLog);
		}

		public void TestTransitFromPickupAddressToPickupCFS_ShouldNotThrowException_WhenAddressIsNull()
		{
			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, pickupAddress.Postcode, ZString.Empty, pickupAddress.OA_RN_NKCountryCode, 0);
			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(true, pickupOrg.OH_Code, pickupAddress.AddressCode, null, null), OrgTimetableType.Codes.Pickup);
			AssertNoExceptionThrown(() => beyondHoursStep.Calculate(initialInput));
		}

		public void TestTransitFromDeliveryCFSToDeliveryAddress_ShouldNotThrowException_WhenAddressIsNull()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 0);
			Factory.Save();

			var readyDate = new ZDateTime(2022, 09, 26, 7, 12, 22);
			var initialInput = DeliveryDueDateCalculationResult.Success(readyDate, ZString.Empty);

			var beyondHoursStep = new TransitToOrFromCFSWithinZoneDDDCalculationStep(CreateDeliveryDueDateCalculationContext(false, deliveryOrg.OH_Code, deliveryAddress.AddressCode, null, null), OrgTimetableType.Codes.Deliver);
			AssertNoExceptionThrown(() => beyondHoursStep.Calculate(initialInput));
		}

		#region Helper Methods

		DeliveryDueDateCalculationContext CreateDeliveryDueDateCalculationContext(bool isPickup,string addressOrg,string addressCode,string cfsOrgCode,string cfsAddressCode)
		{
			return new DeliveryDueDateCalculationContext(
				Factory,
				ZDateTime.Now,
				"STD",
				ZString.Empty,
				isPickup ? addressOrg : ZString.Empty,
				isPickup ? addressCode : ZString.Empty,
				isPickup ? cfsOrgCode : ZString.Empty,
				isPickup ? cfsAddressCode : ZString.Empty,
				isPickup ? ZString.Empty : cfsOrgCode,
				isPickup ? ZString.Empty : cfsAddressCode,
				isPickup ? ZString.Empty : addressOrg,
				isPickup ? ZString.Empty : addressCode,
				ZString.Empty,
				ZString.Empty
			);
		}

		#endregion
	}
}
