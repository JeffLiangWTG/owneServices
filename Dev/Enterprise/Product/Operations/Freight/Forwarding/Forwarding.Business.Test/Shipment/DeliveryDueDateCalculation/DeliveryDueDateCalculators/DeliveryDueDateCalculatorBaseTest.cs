using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public abstract class DeliveryDueDateCalculatorBaseTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			AdditionalBasicSetUp();
			Factory.Save();
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(AssertMessage, ExpectedDeliveryDueDate, result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}

		[ExpectNoExceptions]
		public void TestCalculate_InsufficientFactors()
		{
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals(false, result.IsSuccess);
		}

		public void TestCalculateFallBackToPickupRequiredBy()
		{
			AdditionalBasicSetUp();
			AdditionalFallBackSetUp();
			Factory.Save();
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			CombineAssertions("Pre-conditions", () =>
			{
				if (ShouldSetupPickupAddress && ShouldSetupOriginZoneFromPickupAddress && ShouldSetupOriginZoneActive)
				{
					AssertEquals(OriginZone.PK, calculator.DeliveryDueDateCalculationContext.OriginZoneItemFromPickupAddress.Zone.PK);
				}
				if (ShouldSetupDeliveryAddress && ShouldSetupDestinationZoneFromDeliveryAddress && ShouldSetupDestinationZoneActive)
				{
					AssertEquals(DestinationZone.PK, calculator.DeliveryDueDateCalculationContext.DestinationZoneItemFromDeliveryAddress.Zone.PK);
				}
			});
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(AssertMessage, ExpectedDeliveryDueDate, result.DeliveryDueDate);
			Assert(result.IsSuccess);
		}

		public void TestCalculateWhenBothActualPickupAndPickupRequiredByAreEmpty()
		{
			AdditionalBasicSetUp();
			EmptyReadyDateSetUp();
			Factory.Save();
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals(false, result.IsSuccess);
		}

		#region Implementation

		protected abstract ZString Mode { get; }

		protected abstract ZString AssertMessage { get; }

		protected abstract ZDateTime ExpectedDeliveryDueDate { get; }

		protected virtual bool ShouldSetupDTC { get; }
		protected virtual bool ShouldSetupPickupAddress => true;
		protected virtual bool ShouldSetupDeliveryAddress => true;
		protected virtual bool ShouldSetupOriginZoneFromPickupAddress => true;
		protected virtual bool ShouldSetupDestinationZoneFromDeliveryAddress => true;
		protected virtual bool ShouldSetupOriginZoneActive => true;
		protected virtual bool ShouldSetupDestinationZoneActive => true;
		protected virtual bool ShouldSetupTransitTime => true;

		protected virtual bool ShouldSetupDummyOriginZone => false;
		protected virtual bool ShouldSetupDummyDestinationZone => false;

		protected abstract void AdditionalBasicSetUp();
		protected abstract void AdditionalFallBackSetUp();
		protected abstract void EmptyReadyDateSetUp();

		protected DeliveryDueDateCalculator GetNewDeliveryDueDateCalculatorForTest()
		{
			var deliveryDueDateCalculationContext = new DeliveryDueDateCalculationContext(Shipment);
			return DeliveryDueDateCalculatorFactory.GetDeliveryDueDateCalculatorByDeliveryMode(deliveryDueDateCalculationContext);
		}

		protected ForwardingShipment Shipment;
		protected OrgAddress PickupCFSAddress;
		protected OrgAddress DeliveryCFSAddress;
		protected OrgAddress DeliveryAgentAddress;
		protected RefTransitTime TransitTime;
		protected RateTransportZone OriginZone;
		protected RateTransportZone DestinationZone;
		protected RefServiceLevel ServiceLevel;
		protected RateTransportZone DummyOriginZone;
		protected RateTransportZone DummyDestinationZone;

		protected override void SetUp()
		{
			base.SetUp();
			BaseSetUp();
		}

		void BaseSetUp()
		{
			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Shipment.JS_RL_NKLoadPort = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";
			Shipment.JS_HBLContainerPackModeOverride = Mode;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var pickupCFS = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCFS = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();

			var pickupAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(consignor, "Pickup Address", "2000", "AU", "Sydney", "NSW", "AUSYD");
			PickupCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(pickupCFS, "Pickup CFS Address", "2222", "AU", "Sydney", "NSW", "AUSYD");
			DeliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFS, "Delivery CFS Address", "2222", "NZ", "Auckland", "AUK", "NZAKL");
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(consignee, "Delivery Address", "2000", "NZ", "Auckland", "AUK", "NZAKL");

			Shipment.ConsignorPickupAddress.E2_OA_Address = ShouldSetupPickupAddress ? pickupAddress.PK : Shipment.ConsignorPickupAddress.E2_OA_Address;
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = ShouldSetupDeliveryAddress ? deliveryAddress.PK : Shipment.ConsigneeDeliveryAddress.E2_OA_Address;

			Shipment.JS_OA_ExportReceivingDepot = PickupCFSAddress.PK;
			Shipment.JS_OA_ImportReleaseDepot = DeliveryCFSAddress.PK;
			Shipment.JS_RS_NKServiceLevel = "STD";

			if (ShouldSetupDummyOriginZone)
			{
				DummyOriginZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFS, "3000", ZString.Empty, PickupCFSAddress.OA_RN_NKCountryCode);
			}

			OriginZone = !ShouldSetupOriginZoneFromPickupAddress ? DeliveryDueDateCalculationTestHelper.SetupTransportZoneWithName(Factory, pickupCFS, "TS", "ALL", "TestOriginZone")
				: ShouldSetupPickupAddress ? DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFS, pickupAddress.Postcode, ZString.Empty, pickupAddress.OA_RN_NKCountryCode)
				: DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, pickupCFS, PickupCFSAddress.OA_RN_NKCountryCode);

			OriginZone.TZ_IsActive = ShouldSetupOriginZoneActive;

			if (ShouldSetupDummyDestinationZone)
			{
				DummyDestinationZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFS, "3000", ZString.Empty, "US");
				DummyDestinationZone.TransportProvider.TP_DefaultDeliveryDueTime = TimeSpan.FromHours(15);
			}

			if (ShouldSetupDTC)
			{
				DeliveryAgentAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryAgent, "Delivery Agent Address", "2222", "NZ", "Auckland", "AUK", "NZAKL");
				DeliveryAgentAddress.AddAddressType(OrgAddressType.Delivery);
				Shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

				Shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
				Assert("Precondition", Shipment.IsDTC);

				DestinationZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryAgent, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode);
			}
			else
			{
				DestinationZone = !ShouldSetupDestinationZoneFromDeliveryAddress ? DeliveryDueDateCalculationTestHelper.SetupTransportZoneWithName(Factory, deliveryCFS, "TS", "ALL", "TestDestinationZone")
					: ShouldSetupDeliveryAddress ? DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFS, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode)
					: DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFS, DeliveryCFSAddress.OA_RN_NKCountryCode);
			}

			DestinationZone.TZ_IsActive = ShouldSetupDestinationZoneActive;
			ServiceLevel = DeliveryDueDateCalculationTestHelper.GetOrCreateRefServiceLevelIfNotExist(Factory, "STD");
			TransitTime = ShouldSetupTransitTime ? DeliveryDueDateCalculationTestHelper.SetupTransitTime(Factory, OriginZone.PK, DestinationZone.PK, "STD", "ALL", 10) : TransitTime;
		}

		protected void AssertDeliveryDueDate(string message, ZDateTime expectedDateTime)
		{
			var calculator = GetNewDeliveryDueDateCalculatorForTest();
			var result = calculator.CalculateDeliveryDueDate();
			AssertEquals(message, expectedDateTime, result.DeliveryDueDate);
			AssertEquals(true, result.IsSuccess);
		}

		#endregion
	}
}
