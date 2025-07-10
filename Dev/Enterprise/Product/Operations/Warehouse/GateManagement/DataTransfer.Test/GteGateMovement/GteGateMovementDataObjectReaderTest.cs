using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementDataObjectReader))]
	public class GteGateMovementDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Matching Tests

		public void TestGivenGateMovementBookingHasNoGateMovements_WhenFindBusinessObject_ThenNullReturned()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);
			var gteGateMovement = reader.TryGetExistingBusinessObject();

			AssertNull("Should not find match when no GateMovements exist on GateMovementBooking", gteGateMovement);
		}

		public void TestGivenGateMovementBookingHasUncancelledGateMovement_WhenFindBusinessObject_ThenExistingBusinessObjectReturned()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = movementBooking.PK;
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);
			var gteGateMovement = reader.TryGetExistingBusinessObject();

			AssertEquals("Should find match for GteGateMovement", gateMovement.PK, gteGateMovement.PK);
		}

		public void TestGivenGateMovementBookingHasCancelledGateMovement_WhenFindBusinessObject_ThenNullReturned()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = movementBooking.PK;
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_CancelledReason = "Test";
			gateMovement.GGM_CancelledTime = DateTime.Now;
			gateMovement.GGM_GS_NKCancelledBy = "~BP";

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);
			var gteGateMovement = reader.TryGetExistingBusinessObject();

			AssertNull("Should not find match for GteGateMovement", gteGateMovement);
		}

		public void TestGivenGateMovementBookingHasCancelledAndNonCancelledGateMovement_WhenFindBusinessObject_ThenCorrectMovementReturned()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var cancelledGateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			cancelledGateMovement.GGM_GBM_MovementBooking = movementBooking.PK;
			cancelledGateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledGateMovement.GGM_CancelledReason = "Test";
			cancelledGateMovement.GGM_CancelledTime = DateTime.Now;
			cancelledGateMovement.GGM_GS_NKCancelledBy = "~BP";
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = movementBooking.PK;
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);
			var gteGateMovement = reader.TryGetExistingBusinessObject();

			AssertEquals("Should find correct match for GteGateMovement", gateMovement.PK, gteGateMovement.PK);
		}

		#endregion

		#region Import for new GateMovement tests without exception

		public void TestGivenNoMatchingGateMovement_WhenImportFullUXMLForPickup_ThenFieldsAreCorrectlySet()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportFullUXMLForDropoff_ThenFieldsAreCorrectlySet()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.TransportBookingDirection.Code = GateManagementConstants.TransportBookingDirections.Codes.Delivery;
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", false, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithNoUnitNumberOrType_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.SetContainerCollection(() => null);
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", ZGuid.Empty, gateMovement.GGM_RC_UnitType);
				AssertEquals("GateMovement should have correct UnitNumber", "", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithBlankUnitNumberOrType_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.ContainerCollection[0].ContainerNumber = "";
			shipment.ContainerCollection[0].ContainerType.Code = "";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", ZGuid.Empty, gateMovement.GGM_RC_UnitType);
				AssertEquals("GateMovement should have correct UnitNumber", "", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithInvalidUnitType_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.ContainerCollection[0].ContainerType.Code = "XXX";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", ZGuid.Empty, gateMovement.GGM_RC_UnitType);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithInvalidCargoType_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.PackingLineCollection[0].Commodity.Code = "XXX";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "XXX", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithInvalidPackType_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.PackingLineCollection[0].PackType.Code = "XXX";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "XXX", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithoutDock_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.WarehouseLocation = null;
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", ZGuid.Empty, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithInvalidDock_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.WarehouseLocation = "XXX";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", ZGuid.Empty, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithoutDirection_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.TransportBookingDirection.Code = null;
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct Pickup Direction", false, gateMovement.GGM_IsPickup);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithBlankDirection_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.TransportBookingDirection.Code = "";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct Pickup Direction", false, gateMovement.GGM_IsPickup);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithoutInvalidDirection_DoesNotCauseError()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.TransportBookingDirection.Code = "NIL";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovement = reader.ReadIntoBusinessObject();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotNull("GateMovement should not be null after read", gateMovement);
				AssertNotNull("GateMovement should have parent GateMovementBooking", gateMovement.GateMovementBooking);
				AssertEquals("GateMovement should have correct Pickup Direction", false, gateMovement.GGM_IsPickup);
			});
		}

		#endregion

		#region Import for existing matching GateMovement tests

		public void TestGivenMatchingGateMovement_WhenImportFullUXML_ThenFieldsUpdated()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_TransportReference = "TRF000";
			gateMovement.GGM_IsPickup = false;
			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			gateMovement.GGM_WL_Dock = dock.PK;
			gateMovement.GGM_RH_NKCargoType = "AAA";
			gateMovement.GGM_F3_NKPackageType = "AAA";
			gateMovement.GGM_RC_UnitType = Factory.NewWithValidTestData<RefContainer>().PK;
			gateMovement.UnitType.RC_Code = "AAA";
			gateMovement.GGM_UnitNumber = "";

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS001";
			gateMovementBooking.GBM_Source = "VBS";

			var oldDockPK = dock.PK;
			var oldUnitTypePK = gateMovement.GGM_RC_UnitType;

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var reader = new GteGateMovementDataObjectReader(gateMovementBooking, shipment, new DummyLogger(), Factory);

			var gateMovementFromShipment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Shipment should have matched with existing GateMovement", gateMovement.PK, gateMovementFromShipment.PK);
				AssertEquals("GateMovementBooking should have matched with existing", gateMovementBooking.PK, gateMovementFromShipment.GGM_GBM_MovementBooking);
				AssertEquals("GateMovement should have correct TransportReference", "TRF001", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement should have correct Pickup Direction", true, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement should have correct Dock", DockForTest.PK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement should have correct CargoType", "AABT", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement should have correct PackType", "BAG", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement should have correct UnitType", "20FR", gateMovement.UnitType.RC_Code);
				AssertEquals("GateMovement should have correct UnitNumber", "UNT-001", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenMatchingGateMovement_WhenImportBlankUXML_ThenFieldsNotChanged()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_TransportReference = "TRF000";
			gateMovement.GGM_IsPickup = false;
			gateMovement.GGM_WL_Dock = DockForTest.PK;
			gateMovement.GGM_RH_NKCargoType = "AAA";
			gateMovement.GGM_F3_NKPackageType = "AAA";
			gateMovement.GGM_RC_UnitType = UnitTypeForTest.PK;
			gateMovement.GGM_UnitNumber = "";

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS001";
			gateMovementBooking.GBM_Source = "VBS";

			var oldDockPK = gateMovement.GGM_WL_Dock;
			var oldUnitTypePK = gateMovement.GGM_RC_UnitType;

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.BookingConfirmationReference = "BRN001";

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.BookingPartyReference, AdditionalReferenceTypes.Descriptions.BookingPartyReference, "VBS001");
			var reader = new GteGateMovementDataObjectReader(gateMovementBooking, shipment, new DummyLogger(), Factory);

			var gateMovementFromShipment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Shipment should have matched with existing GateMovement", gateMovement.PK, gateMovementFromShipment.PK);
				AssertEquals("GateMovementBooking should have matched with existing", gateMovementBooking.PK, gateMovementFromShipment.GGM_GBM_MovementBooking);
				AssertEquals("GateMovement.TransportReference should be unchanged", "TRF000", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement.PickupDirection should be unchanged", false, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement.Dock should be unchanged", oldDockPK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement.CargoType should be unchanged", "AAA", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement.PackType should be unchanged", "AAA", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement.UnitType should be unchanged", oldUnitTypePK, gateMovement.GGM_RC_UnitType);
				AssertEquals("GateMovement.UnitNumber should be unchanged", "", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenMatchingGateMovement_WhenImportUXMLWithEmptyValues_ThenFieldsNotChanged()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_TransportReference = "TRF000";
			gateMovement.GGM_IsPickup = false;
			gateMovement.GGM_WL_Dock = DockForTest.PK;
			gateMovement.GGM_RH_NKCargoType = "AAA";
			gateMovement.GGM_F3_NKPackageType = "AAA";
			gateMovement.GGM_RC_UnitType = UnitTypeForTest.PK;
			gateMovement.GGM_UnitNumber = "";

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS001";
			gateMovementBooking.GBM_Source = "VBS";

			var oldDockPK = gateMovement.GGM_WL_Dock;
			var oldUnitTypePK = gateMovement.GGM_RC_UnitType;

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.AdditionalReferenceCollection.First(reference => (string)reference.Type.Code == AdditionalReferenceTypes.Codes.TransportReference).ReferenceNumber = "";
			shipment.TransportBookingDirection.Code = "";
			shipment.WarehouseLocation = "";
			shipment.DateCollection[0].Value = null;
			shipment.DateCollection[1].Value = null;
			shipment.PackingLineCollection[0].Commodity.Code = "";
			shipment.PackingLineCollection[0].PackType.Code = "";
			shipment.ContainerCollection[0].ContainerType.Code = "";
			shipment.ContainerCollection[0].ContainerNumber = "";

			var reader = new GteGateMovementDataObjectReader(gateMovementBooking, shipment, new DummyLogger(), Factory);

			var gateMovementFromShipment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Shipment should have matched with existing GateMovement", gateMovement.PK, gateMovementFromShipment.PK);
				AssertEquals("GateMovementBooking should have matched with existing", gateMovementBooking.PK, gateMovementFromShipment.GGM_GBM_MovementBooking);
				AssertEquals("GateMovement.TransportReference should be unchanged", "TRF000", gateMovement.GGM_TransportReference);
				AssertEquals("GateMovement.PickupDirection should be unchanged", false, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement.Dock should be unchanged", oldDockPK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement.CargoType should be unchanged", "AAA", gateMovement.GGM_RH_NKCargoType);
				AssertEquals("GateMovement.PackType should be unchanged", "AAA", gateMovement.GGM_F3_NKPackageType);
				AssertEquals("GateMovement.UnitType should be unchanged", oldUnitTypePK, gateMovement.GGM_RC_UnitType);
				AssertEquals("GateMovement.UnitNumber should be unchanged", "", gateMovement.GGM_UnitNumber);
			});
		}

		public void TestGivenMatchingGateMovement_WhenImportUXMLWithInvalidValues_ThenFieldsNotChanged()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_TransportReference = "TRF000";
			gateMovement.GGM_IsPickup = false;
			gateMovement.GGM_WL_Dock = DockForTest.PK;
			gateMovement.GGM_RH_NKCargoType = "AAA";
			gateMovement.GGM_F3_NKPackageType = "AAA";
			gateMovement.GGM_RC_UnitType = UnitTypeForTest.PK;
			gateMovement.GGM_UnitNumber = "";

			var gateMovementBooking = gateMovement.GateMovementBooking;
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS001";
			gateMovementBooking.GBM_Source = "VBS";

			var oldDockPK = gateMovement.GGM_WL_Dock;
			var oldUnitTypePK = gateMovement.GGM_RC_UnitType;

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			shipment.TransportBookingDirection.Code = "NIL";
			shipment.WarehouseLocation = "XXXXX";
			shipment.ContainerCollection[0].ContainerType.Code = "XXX";

			var reader = new GteGateMovementDataObjectReader(gateMovementBooking, shipment, new DummyLogger(), Factory);

			var gateMovementFromShipment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Shipment should have matched with existing GateMovement", gateMovement.PK, gateMovementFromShipment.PK);
				AssertEquals("GateMovementBooking should have matched with existing", gateMovementBooking.PK, gateMovementFromShipment.GGM_GBM_MovementBooking);
				AssertEquals("GateMovement.PickupDirection should be unchanged", false, gateMovement.GGM_IsPickup);
				AssertEquals("GateMovement.Dock should be unchanged", oldDockPK, gateMovement.GGM_WL_Dock);
				AssertEquals("GateMovement.UnitType should be unchanged", oldUnitTypePK, gateMovement.GGM_RC_UnitType);
			});
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithoutTransportReference_ThenThrowsNoErrors()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var transportReference = shipment.AdditionalReferenceCollection.First(reference => (string)reference.Type.Code == AdditionalReferenceTypes.Codes.TransportReference);
			transportReference.ReferenceNumber = null;
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovementFromShipment = reader.ReadIntoBusinessObject();
			gateMovementFromShipment.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();
			AssertEquals(gateMovementFromShipment.GGM_TransportReference, "");
		}

		public void TestGivenNoMatchingGateMovement_WhenImportUXMLWithBlankTransportReference_ThenThrowsNoErrors()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			var shipment = GetGateMovementShipmentForTest("VBS001");
			var transportReference = shipment.AdditionalReferenceCollection.First(reference => (string)reference.Type.Code == AdditionalReferenceTypes.Codes.TransportReference);
			transportReference.ReferenceNumber = "";
			var reader = new GteGateMovementDataObjectReader(movementBooking, shipment, new DummyLogger(), Factory);

			var gateMovementFromShipment = reader.ReadIntoBusinessObject();
			gateMovementFromShipment.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			Factory.SaveForTesting();
			AssertEquals(gateMovementFromShipment.GGM_TransportReference, "");
		}

		#endregion

		#region Helpers

		Shipment GetGateMovementShipmentForTest(ZString? sourceReference)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.BookingConfirmationReference = "BRN001";

			if (!string.IsNullOrEmpty(sourceReference))
			{
				shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.BookingPartyReference, AdditionalReferenceTypes.Descriptions.BookingPartyReference, sourceReference);
			}
			shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.TransportReference, AdditionalReferenceTypes.Descriptions.TransportReference, "TRF001");

			shipment.TransportBookingDirection = new TransportBookingDirection()
			{
				Code = GateManagementConstants.TransportBookingDirections.Codes.Pickup
			};
			shipment.WarehouseLocation = DockForTest.ToLocationString();
			var address = DockForTest.Warehouse.WarehouseAddress;
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress()
				{
					AddressType = nameof(DocAddressType.LocalCartageYard),
					Address1 = address.Address1,
					Address2 = address.Address2,
					City = address.City,
					Postcode = address.Postcode,
					AddressShortCode = address.OA_Code,
					OrganizationCode = address.Header.OH_Code,
				}
			});

			shipment.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.Start, Value = new ZDateTime(2024, 04, 18, 12, 0, 0) },
				new Date() { Type = DateType.End, Value = new ZDateTime(2024, 04, 19, 12, 0, 0) }
			});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				new PackingLine() { Commodity = new Commodity() { Code = "AABT" }, PackType = new PackageType() { Code = "BAG" } }
			});

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "UNT-001", ContainerType = new ContainerType() { Code = UnitTypeForTest.RC_Code } }
			});

			return shipment;
		}

		WhsLocation DockForTest
		{
			get
			{
				if (dockForTest == null)
				{
					dockForTest = Factory.NewWithValidTestData<WhsLocation>();
					dockForTest.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
					dockForTest.Warehouse.WW_IsActive = true;
					dockForTest.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
					var address = dockForTest.Warehouse.WarehouseAddress;
					address.Address1 = "Address 1";
					address.Address2 = "Address 2";
					address.City = "SYD";
					address.Postcode = "0000";
					address.OA_Code = "BLANK SYD";
					address.Header.OH_Code = "ZZ";
					Factory.SaveForTesting();
				}
				return dockForTest;
			}
			set => dockForTest = value;
		}
		WhsLocation dockForTest;

		RefContainer UnitTypeForTest
		{
			get
			{
				if (unitTypeForTest == null)
				{
					unitTypeForTest = Factory.NewWithValidTestData<RefContainer>();
					unitTypeForTest.RC_Code = "20FR";
					Factory.SaveForTesting();
				}
				return unitTypeForTest;
			}
			set => unitTypeForTest = value;
		}
		RefContainer unitTypeForTest;

		#endregion
	}
}
