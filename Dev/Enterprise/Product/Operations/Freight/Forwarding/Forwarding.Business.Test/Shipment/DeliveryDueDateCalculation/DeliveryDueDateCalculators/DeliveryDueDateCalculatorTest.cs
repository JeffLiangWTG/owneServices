using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DeliveryDueDateCalculatorTest : TestCaseWithFactory
	{
		public void TestNewCalculatorFieldsWithDTC()
		{
			var pickupCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var deliveryCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			Factory.Save();

			var calculator = CreateCalculatorWithRealValues(isDTC: true);
			
			AssertEquals(calculator.DeliveryDueDateCalculationContext.DeliveryAgentAddress.AddressFull, deliveryCFSOrg.Addresses[1].AddressFull);
			AssertEquals(calculator.DeliveryDueDateCalculationContext.CFSPickupAddress.AddressFull, pickupCFSOrg.Addresses[1].AddressFull);
			AssertNull(calculator.DeliveryDueDateCalculationContext.CFSDeliveryAddress);
		}

		public void TestNewCalculatorFieldsWithoutDTC()
		{
			var pickupCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var deliveryCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			Factory.Save();

			var calculator = CreateCalculatorWithRealValues(isDTC: false);

			AssertNull(calculator.DeliveryDueDateCalculationContext.DeliveryAgentAddress);
			AssertEquals(calculator.DeliveryDueDateCalculationContext.CFSPickupAddress.AddressFull, pickupCFSOrg.Addresses[1].AddressFull);
			AssertEquals(calculator.DeliveryDueDateCalculationContext.CFSDeliveryAddress.AddressFull, deliveryCFSOrg.Addresses[1].AddressFull);
		}

		public void TestGetDeliveryDueDateCalculatorForDeliveryMode()
		{
			void AssertCalculatorTypeForMode(string deliveryMode, bool isDTC, Type expectedCalculatorType)
			{
				var calculator = CreateCalculatorWithEmptyValues(deliveryMode, isDTC);
				AssertEquals(expectedCalculatorType, calculator.GetType());
			}

			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, false, typeof(DoorToDoorDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, false, typeof(DoorToCFSDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, false, typeof(CFSToDoorDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, false, typeof(CFSToCFSDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, true, typeof(DoorToDTCDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS, true, typeof(DoorToCFSDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR, true, typeof(CFSToDTCDeliveryDueDateCalculator));
			AssertCalculatorTypeForMode(Core.Constants.HBLDeliveryModes.Codes.CFS_CFS, true, typeof(CFSToCFSDeliveryDueDateCalculator));
		}

		public void TestAllSupportedDeliveryModes()
		{
			foreach (var deliveryMode in DeliveryDueDateCalculator.SupportedDeliveryModes)
			{
				AssertNotNull(CreateCalculatorWithEmptyValues(deliveryMode));
			}
		}

		public void TestOriginDestinationZoneItem_ShouldReturnOperationProvider()
		{
			var calculator = CreateCalculatorWithEmptyValues();
			AssertNotNull(calculator);
			AssertNoExceptionThrown(() => {
				var originFromPickupAddress = calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress;
				var originFromCFSPickupAddress = calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromCFSPickupAddress;
				var destinationFromDeliveryAddress = calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress;
				var destinationFromCFSDeliveryAddress = calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromCFSDeliveryAddress;
			});
			AssertNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress);
			AssertNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromCFSPickupAddress);
			AssertNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress);
			AssertNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromCFSDeliveryAddress);

			var pickupOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "WTG", "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var pickupCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var deliveryCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			var deliveryOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "KGB", "20/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			Factory.Save();

			calculator = CreateCalculatorWithRealValues();
			var zoneAllForPickup = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, "2017", ZString.Empty, Core.Constants.CountryCodes.Australia, 18, RatingConstants.RatingZoneTypes.All);
			var zoneOPSForPickup = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSOrg, "2000", "2020", Core.Constants.CountryCodes.Australia, 8, RatingConstants.RatingZoneTypes.Operations);
			Factory.Save();

			AssertNotNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress);
			AssertEquals(zoneOPSForPickup.PK, calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress.Zone.PK);

			zoneOPSForPickup.TZ_IsActive = false;
			Factory.Save();
			AssertEquals("Origin/Destination zone item has been cached and will not be changed.", zoneOPSForPickup.PK, calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress.Zone.PK);

			calculator = CreateCalculatorWithRealValues();
			AssertNotNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress);
			AssertEquals(zoneAllForPickup.PK, calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress.Zone.PK);

			var zoneAllForDelivery = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, "6155", ZString.Empty, Core.Constants.CountryCodes.Australia, 18, RatingConstants.RatingZoneTypes.All);
			var zoneOPSForDelivery = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, "6000", "6200", Core.Constants.CountryCodes.Australia, 8, RatingConstants.RatingZoneTypes.Operations);
			Factory.Save();

			AssertNotNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress);
			AssertEquals(zoneOPSForDelivery.PK, calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress.Zone.PK);

			zoneOPSForDelivery.TZ_IsActive = false;
			Factory.Save();
			AssertEquals("Origin/Destination zone item has been cached and will not be changed.", zoneOPSForDelivery.PK, calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress.Zone.PK);

			calculator = CreateCalculatorWithRealValues();
			AssertNotNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress);
			AssertEquals(zoneAllForDelivery.PK, calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress.Zone.PK);
		}

		public void TestZoneItem_ShouldFallBackIgnoreNonCFSAddress_WhenZoneItemIsNull()
		{
			var pickupCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var deliveryCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			Factory.Save();

			var calculator = CreateCalculatorWithRealValues();
			DeliveryDueDateCalculationTestHelper.SetupTransportZoneWithName(Factory, pickupCFSOrg, "TS", RatingConstants.RatingZoneTypes.All, "TestPickupZoneAll");
			DeliveryDueDateCalculationTestHelper.SetupTransportZoneWithName(Factory, pickupCFSOrg, "TS", RatingConstants.RatingZoneTypes.Operations, "TestPickupZoneOps");

			DeliveryDueDateCalculationTestHelper.SetupTransportZoneWithName(Factory, deliveryCFSOrg, "TS", RatingConstants.RatingZoneTypes.All, "TestDeliveryZoneAll");
			DeliveryDueDateCalculationTestHelper.SetupTransportZoneWithName(Factory, deliveryCFSOrg, "TS", RatingConstants.RatingZoneTypes.Operations, "TestDeliveryZoneOps");

			Factory.Save();

			AssertNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress);
			AssertNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromCFSPickupAddress);
			AssertNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress);
			AssertNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromCFSDeliveryAddress);

			AssertNotNull(calculator.DeliveryDueDateCalculationContext.OriginZone);
			AssertNotNull(calculator.DeliveryDueDateCalculationContext.DestinationZone);

			AssertEquals(calculator.DeliveryDueDateCalculationContext.OriginZone.TZ_ZoneName, "TestPickupZoneOps");
			AssertEquals(calculator.DeliveryDueDateCalculationContext.DestinationZone.TZ_ZoneName, "TestDeliveryZoneOps");
		}

		public void TestDeliveryDueTime()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(Factory.NewWithValidTestData<OrgHeader>(), "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_Postcode = "2017";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(Factory.NewWithValidTestData<OrgHeader>(), "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "6155";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipment.JS_RS_NKServiceLevel = "STD";

			var zoneForDelivery = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSAddress.Header, "6155", ZString.Empty, Core.Constants.CountryCodes.Australia);
			Factory.Save();

			var deliveryDueTime = new ZDateTime(1900, 1, 1, 12, 0, 0);

			var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			serviceLevel.RS_DefaultDeliveryDueTime = deliveryDueTime;

			zoneForDelivery.Items[0].TQ_DeliveryDueTime = deliveryDueTime.AddMinutes(1);
			zoneForDelivery.TransportProvider.TP_DefaultDeliveryDueTime = deliveryDueTime.AddMinutes(2);
			Factory.Save();

			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			var calculator =  DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);

			var expectResult = new DeliveryDueTime(new TimeSpan(12, 1, 0), DeliveryDueTimeSource.ZoneItem);
			AssertEquals(expectResult.Time, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Time);
			AssertEquals(expectResult.Source, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Source);

			calculator.DeliveryDueDateCalculationContext.ServiceLevelGenericTransitTimeHasBeenUsed = true;

			expectResult = new DeliveryDueTime(new TimeSpan(12, 0, 0), DeliveryDueTimeSource.ServiceLevel);
			AssertEquals(expectResult.Time, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Time);
			AssertEquals(expectResult.Source, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Source);

			serviceLevel.RS_DefaultDeliveryDueTime = ZDateTime.Empty;
			Factory.Save();

			expectResult = new DeliveryDueTime(new TimeSpan(12, 1, 0), DeliveryDueTimeSource.ZoneItem);
			AssertEquals(expectResult.Time, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Time);
			AssertEquals(expectResult.Source, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Source);

			zoneForDelivery.Items[0].TQ_DeliveryDueTime = ZDateTime.Empty;
			Factory.Save();

			expectResult = new DeliveryDueTime(new TimeSpan(12, 2, 0), DeliveryDueTimeSource.TransportProvider);
			AssertEquals(expectResult.Time, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Time);
			AssertEquals(expectResult.Source, calculator.DeliveryDueDateCalculationContext.DeliveryDueTime.Source);
		}

		public void TestOriginDestinationZoneItem_WhenPickupDeliveryAddressesAreOverriden()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(Factory.NewWithValidTestData<OrgHeader>(), "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_Postcode = "2017";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(Factory.NewWithValidTestData<OrgHeader>(), "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "6155";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var zoneForPickup = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFSAddress.Header, "2017", ZString.Empty, Core.Constants.CountryCodes.Australia);
			var zoneForDelivery = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSAddress.Header, "6155", ZString.Empty, Core.Constants.CountryCodes.Australia);

			Factory.Save();

			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			var calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			AssertNotNull(calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress);
			AssertEquals(zoneForPickup.PK, calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress.Zone.PK);
			AssertNotNull(calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress);
			AssertEquals(zoneForDelivery.PK, calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress.Zone.PK);
		}

		public void TestCalculatorReturnsEmptyDateTimeAndErrorMessageWhenOneCalculationStepReturnsInvalidDateTime()
		{
			var step1Mock = new Mock<IDeliveryDueDateCalculationStep>();
			step1Mock.Setup(step => step.Calculate(It.IsAny<DeliveryDueDateCalculationResult>())).Returns(DeliveryDueDateCalculationResult.Success(ZDateTime.Now, ZString.Empty));
			var step2Mock = new Mock<IDeliveryDueDateCalculationStep>();
			step2Mock.Setup(step => step.Calculate(It.IsAny<DeliveryDueDateCalculationResult>())).Returns(DeliveryDueDateCalculationResult.Failure(ZString.Empty, ZString.Empty));
			var step3Mock = new Mock<IDeliveryDueDateCalculationStep>();
			step3Mock.Setup(step => step.Calculate(It.IsAny<DeliveryDueDateCalculationResult>())).Returns(DeliveryDueDateCalculationResult.Success(ZDateTime.Now, ZString.Empty));

			var calculator = new DeliveryDueDateCalculatorForTest(CreateContextWithEmptyValues());
			calculator.CalculationSteps.AddRange(new[] { step1Mock.Object, step2Mock.Object, step3Mock.Object });
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals(ZString.Empty, result.ErrorMessage);

			calculator.CalculationSteps.Clear();
			step2Mock.Setup(step => step.Calculate(It.IsAny<DeliveryDueDateCalculationResult>())).Returns(DeliveryDueDateCalculationResult.Failure("Error", ZString.Empty));
			calculator.CalculationSteps.AddRange(new[] { step1Mock.Object, step2Mock.Object, step3Mock.Object });
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals("Error", result.ErrorMessage);
		}

		public void TestOriginDestinationZoneItem_NotFound_ContainsLogMessage()
		{
			var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			serviceLevel.RS_DefaultTransitHours = 10;

			DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "WTG", "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "KGB", "20/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			Factory.Save();

			var calculator = CreateCalculatorWithRealValues();
			AssertNull("Pre-condition: OriginZoneItem", calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress);
			AssertNull("Pre-condition: DestinationZoneItem", calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress);

			var result = calculator.CalculateDeliveryDueDate();
			Assert(result.CalculationLog.Contains("Beyond Days/Hours could not be found as the Zone Item for Pickup/Delivery CFS is not present."));
		}

		public void TestDefaultTimetablesAreSetWhenCalculationInSaveTransaction()
		{
			var temporaryDefaultOrgTimetable = new DefaultOrgTimetableSettingsCollection();
			var defaultSetting = temporaryDefaultOrgTimetable.AddNew();
			var item1 = defaultSetting.Timetables.AddNew();
			item1.Type = OrgTimetableType.Codes.Pickup;
			item1.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item1.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item1.Day = "MON";

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryDefaultOrgTimetable))
			{
				DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "WTG", "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
				DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
				DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
				DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "KGB", "20/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
				Factory.Save();

				Factory.Saving += factory =>
				{
					var calculator = CreateCalculatorWithRealValues();
					calculator.CalculateDeliveryDueDate();
					AssertEquals(1, (calculator.DeliveryDueDateCalculationContext.PickupAddress as OrgAddress).Timetables.Count);
					AssertEquals(1, (calculator.DeliveryDueDateCalculationContext.DeliveryAddress as OrgAddress).Timetables.Count);
					AssertEquals(1, calculator.DeliveryDueDateCalculationContext.CFSPickupAddress.Timetables.Count);
					AssertEquals(1, calculator.DeliveryDueDateCalculationContext.CFSDeliveryAddress.Timetables.Count);
				};

				Factory.Save();
			}
		}

		public void TestCalculateDeliveryDueDateWhenAirportToAirport()
		{
			var deliveryModes = new List<ZString>() {
				Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT
			};

			foreach (var deliveryMode in deliveryModes)
			{
				var shipment = Factory.New<ForwardingShipment>();
				var consol = shipment.Consols.AddNew();
				consol.JK_AgentType = Core.Constants.AgentType.Direct;

				shipment.JS_HBLContainerPackModeOverride = deliveryMode;
				shipment.JS_E_ARV = new ZDateTime(2023, 2, 19, 2, 0, 0);
				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_ETA = new ZDateTime(2023, 2, 12, 6, 0, 0);
				transport.JW_RL_NKDiscPort = "NZAKL";
				Factory.Save();
				AssertCalculateDeliveryDueDate("DDD should not be calculated", ZDateTime.Empty, "ETA of the last transport leg is blank.");

				shipment.JS_RL_NKDestination = "NZAKL";
				Factory.Save();
				AssertCalculateDeliveryDueDate("DDD should not be calculated", ZDateTime.Empty,"ETA of the last transport leg is blank.");

				shipment.Transports.RemoveAndDeleteAll();
				transport = consol.Transports[0];
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_ETA = new ZDateTime(2023, 2, 12, 6, 0, 0);
				transport.JW_RL_NKDiscPort = "NZAKL";

				Factory.Save();
				AssertCalculateDeliveryDueDate("DDD should be calculated", new ZDateTime(2023, 2, 12, 6, 0, 0));

				transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Air;
				transport.JW_ETA = new ZDateTime(2023, 2, 13, 6, 0, 0);
				transport.JW_RL_NKDiscPort = "NZAKL";
				AssertCalculateDeliveryDueDate("DDD should be calculated as the last ETA", new ZDateTime(2023, 2, 13, 6, 0, 0));

				transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport.JW_ETA = new ZDateTime(2023, 2, 14, 6, 0, 0);
				transport.JW_RL_NKDiscPort = "NZAKL";
				AssertCalculateDeliveryDueDate("DDD should be calculated as the last ETA", new ZDateTime(2023, 2, 14, 6, 0, 0));

				void AssertCalculateDeliveryDueDate(string message, ZDateTime expectedDeliveryDueDate, string errorMessage = "")
				{
					var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
					var calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);

					var result = calculator.CalculateDeliveryDueDate();
					AssertEquals(message, errorMessage, result.ErrorMessage);
					AssertEquals(message, expectedDeliveryDueDate, result.DeliveryDueDate);
				}
			}
		}

		public void TestNoteInfo()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_RS_NKServiceLevel = "";
			Factory.Save();

			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			var calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);

			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Ready Date", result.CalculationLog);

			var pickupDate = ZDateTime.Now;
			shipment.DocsAndCartage.JP_PickupRequiredBy = pickupDate;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Service Level", result.CalculationLog);

			shipment.JS_RS_NKServiceLevel = "STD";
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Pickup Address", result.CalculationLog);

			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_Postcode = "2017";
			shipment.ConsignorPickupAddress.E2_Address1 = "104/17 Joynton Avenue, Zetland";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Pickup CFS Address", result.CalculationLog);

			var pickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(
				Factory.NewWithValidTestData<OrgHeader>(), "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			shipment.JS_OA_ExportReceivingDepot = pickupCFSAddress.PK;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Delivery Address", result.CalculationLog);

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "6155";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "20/22 Gadigal Avenue, Zetland";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Delivery CFS Address", result.CalculationLog);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Delivery Agent Address for DTC", result.CalculationLog);

			var deliveryCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(
				Factory, "WTG", "20/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSOrg.MainAddress.PK;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			Assert(result.CalculationLog.Contains("Parameters used for this calculation:"));
			Assert(result.CalculationLog.Contains("DTC: False"));
			Assert(result.CalculationLog.Contains("Service Level: STD"));
			Assert(result.CalculationLog.Contains("HBL Dlv. Mode: DOOR/DOOR"));
			Assert(result.CalculationLog.Contains($"Pickup CFS/Transit Warehouse: {(pickupCFSAddress as IDocAddress).Organisation.Code}"));
			Assert(result.CalculationLog.Contains($"Pickup Address: {shipment.ConsignorPickupAddress.AddressFull}"));
			Assert(result.CalculationLog.Contains($"Pickup CFS Address: {pickupCFSAddress.AddressFull}"));
			Assert(result.CalculationLog.Contains($"Delivery CFS/Transit Warehouse: {deliveryCFSOrg.OH_Code}"));
			Assert(result.CalculationLog.Contains($"Delivery Address: {shipment.ConsigneeDeliveryAddress.AddressFull}"));
			Assert(result.CalculationLog.Contains($"Delivery Agent Address: {DeliveryDueDateCalculationHelper.EmptyValueSignForLog}"));
			Assert(result.CalculationLog.Contains($"Calculation Start Date: {pickupDate}"));
			Assert(result.CalculationLog.Contains("Pickup Required By has been selected as calculation start date"));
		}

		public void TestNoteInfoForDTC()
		{
			var pickupOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "WTG", "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var pickupCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "MEL", "323/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");
			var deliveryCFSOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "NSA", "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			var deliveryOrg = DeliveryDueDateCalculationTestHelper.SetupOrgWithAddress(Factory, "KGB", "20/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");
			Factory.Save();

			var calculator = CreateCalculatorWithRealValues(isDTC: true);
			var result = calculator.CalculateDeliveryDueDate();
			Assert(result.CalculationLog.Contains("Parameters used for this calculation:"));
			Assert(result.CalculationLog.Contains("DTC: True"));
			Assert(result.CalculationLog.Contains("Service Level: STD"));
			Assert(result.CalculationLog.Contains("HBL Dlv. Mode: DOOR/DOOR"));
			Assert(result.CalculationLog.Contains($"Pickup CFS/Transit Warehouse: {pickupCFSOrg.OH_Code}"));
			Assert(result.CalculationLog.Contains($"Pickup Address: {pickupOrg.Addresses[1].AddressFull}"));
			Assert(result.CalculationLog.Contains($"Pickup CFS Address: {pickupCFSOrg.Addresses[1].AddressFull}"));
			Assert(result.CalculationLog.Contains($"Delivery CFS/Transit Warehouse: {DeliveryDueDateCalculationHelper.EmptyValueSignForLog}"));
			Assert(result.CalculationLog.Contains($"Delivery Address: {deliveryOrg.Addresses[1].AddressFull}"));
			Assert(result.CalculationLog.Contains($"Delivery Agent Address: {deliveryCFSOrg.Addresses[1].AddressFull}"));
		}

		public void TestNoteInfoWhenCFSAddressesAreNullAndServiceLevelHasDefaultTransitTime()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_RS_NKServiceLevel = "";
			Factory.Save();

			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			var calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Ready Date", result.CalculationLog);

			var pickupDate = ZDateTime.Now;
			shipment.DocsAndCartage.JP_PickupRequiredBy = pickupDate;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Service Level", result.CalculationLog);

			var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			serviceLevel.RS_DefaultTransitHours = 10;
			shipment.JS_RS_NKServiceLevel = "STD";
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Pickup Address", result.CalculationLog);

			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_Postcode = "2017";
			shipment.ConsignorPickupAddress.E2_Address1 = "104/17 Joynton Avenue, Zetland";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			AssertEquals("Checking Properties: Failed to calculate the Delivery Due Date due to the Invalid Property: Delivery Address", result.CalculationLog);

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "6155";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "20/22 Gadigal Avenue, Zetland";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CFS;
			deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(shipment);
			calculator = DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
			result = calculator.CalculateDeliveryDueDate();
			Assert(result.CalculationLog.Contains("Parameters used for this calculation:"));
			Assert(result.CalculationLog.Contains("Service Level: STD"));
			Assert(result.CalculationLog.Contains("HBL Dlv. Mode: CFS/CFS"));
			Assert(result.CalculationLog.Contains($"Pickup CFS/Transit Warehouse: -"));
			Assert(result.CalculationLog.Contains($"Pickup Address: {shipment.ConsignorPickupAddress.AddressFull}"));
			Assert(result.CalculationLog.Contains($"Delivery CFS/Transit Warehouse: -"));
			Assert(result.CalculationLog.Contains($"Delivery Address: {shipment.ConsigneeDeliveryAddress.AddressFull}"));
			Assert(result.CalculationLog.Contains($"Calculation Start Date: {pickupDate}"));
			Assert(result.CalculationLog.Contains("Pickup Required By has been selected as calculation start date"));
		}

		#region Helper Methods and classes

		DeliveryDueDateCalculationContext CreateContextWithEmptyValues(string deliveryMode = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, bool isDTC = false)
		{
			return new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				ZString.Empty,
				deliveryMode,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				isDTC ? Core.Constants.DeliveryTypes.DirectToCNE : string.Empty);
		}

		DeliveryDueDateCalculator CreateCalculatorWithEmptyValues(string deliveryMode = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, bool isDTC = false)
		{
			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				ZString.Empty,
				deliveryMode,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				ZString.Empty,
				isDTC ? Core.Constants.DeliveryTypes.DirectToCNE : string.Empty);
			return DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
		}

		DeliveryDueDateCalculator CreateCalculatorWithRealValues(bool isDTC = false)
		{
			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(Factory,
				ZDateTime.Now,
				"STD",
				Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
				"WTG",
				"357/4 Bindon Place",
				"MEL",
				"323/4 Bindon Place",
				"NSA",
				"21/8 Camillo St.",
				"KGB",
				"20/8 Camillo St.",
				"ALL",
				isDTC ? Core.Constants.DeliveryTypes.DirectToCNE : string.Empty);
			return DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
		}

		#endregion
	}
}
