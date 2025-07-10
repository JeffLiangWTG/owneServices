using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleMovementDataObjectReader))]
	public class GteVehicleMovementDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		readonly ZDateTimeOffset GateInTime = new ZDateTimeOffset(new ZDateTime(2024, 04, 18, 12, 00, 00), DateTimeKind.Local);
		readonly ZDateTimeOffset GateOutTime = new ZDateTimeOffset(new ZDateTime(2024, 04, 19, 12, 00, 00), DateTimeKind.Local);

		#region VehicleMovement matching tests
		public void TestGivenMatchingVehicleEntry_WhenGetExistingBusinessObject_ThenCorrectVehicleMovementReturned()
		{
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_IsIncoming = true;
			Factory.NewWithValidTestData<GteVehicleEntry>();
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest(gateIn.GVE_GateActionNumber, null, true, true);
			var reader = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory);
			var matchedObject = reader.TryGetExistingBusinessObject();

			AssertEquals("Should return correct VehicleMovement", gateIn.VehicleMovement.PK, matchedObject.PK);
		}

		public void TestGivenNoMatchingVehicleEntryForGivenKey_WhenGetExistingBusinessObject_ThenExceptionThrown()
		{
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_IsIncoming = true;
			Factory.NewWithValidTestData<GteVehicleEntry>();
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest(gateIn.GVE_GateActionNumber + "0", null, true, true);
			var reader = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory);

			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.TryGetExistingBusinessObject());
		}

		public void TestGivenMatchByMovementBookingNumber_WhenGetExistingBusinessObject_ThenCorrectObjectReturned()
		{
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_IsIncoming = true;
			Factory.NewWithValidTestData<GteVehicleEntry>();

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = gateIn.GVE_GVM_VehicleMovement;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest(null, gateMovement.GateMovementBooking.GBM_MovementBookingNumber, true, true);
			var reader = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory);
			var matchedObject = reader.TryGetExistingBusinessObject();

			AssertEquals("Should return correct VehicleMovement", gateIn.VehicleMovement.PK, matchedObject.PK);
		}
		#endregion

		#region Booking creation tests

		public void TestGivenNoExistingBookings_WhenImportUXML_ThenNewEntitiesCreated()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: false);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Expect there to be no existing GteBookings", 0, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovementBookings", 0, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleMovements", 0, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleEntries", 0, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});

			_ = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect there to be a new GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteGateMovement", 1, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleMovement", 1, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleEntry", 1, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect the new GteVehicleEntry has correct entry time", GateInTime, Factory.Load<GteVehicleEntry>(new ZQuery())[0].GVE_EntryTime);
			});
		}

		public void TestGivenExistingMatchingBooking_WhenImportUXML_ThenNoNewBookingCreated()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: false);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Expect there to be an existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be an existing GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleMovements", 0, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleEntries", 0, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});

			_ = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect there to be no new GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be no new GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteGateMovement", 1, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleMovement", 1, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleEntry", 1, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect the new GteVehicleEntry has correct entry time", GateInTime, Factory.Load<GteVehicleEntry>(new ZQuery())[0].GVE_EntryTime);
			});
		}

		public void TestGivenExistingNonMatchingBooking_WhenImportUXML_ThenNewBookingAndMovementCreated()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			var booking = movementBooking.Booking;
			booking.GBK_OH_TransportCompany = TransportCompanyForTest.PK;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", null, hasGateIn: true, hasGateOut: false);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Expect there to be an existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be an existing GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleMovements", 0, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleEntries", 0, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});

			_ = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect there to be a new GteBooking", 2, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteGateMovementBooking", 2, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteGateMovement", 1, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleMovement", 1, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleEntry", 1, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect the new GteVehicleEntry has correct entry time", GateInTime, Factory.Load<GteVehicleEntry>(new ZQuery())[0].GVE_EntryTime);
			});
		}

		public void TestGivenExistingMatchingBookingAndMovement_WhenImportUXML_ThenNoNewMovementCreated()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = booking.PK;
			vehicleMovement.FillWithValidTestData();

			var gateMovement = Factory.New<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement.GGM_GBM_MovementBooking = movementBooking.PK;
			gateMovement.FillWithValidTestData();

			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: false);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Expect there to be an existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be an existing GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be an existing GteGateMovement", 1, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be an existing GteVehicleMovement", 1, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleEntry", 0, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});

			_ = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect there to be no new GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be no new GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be no new GteGateMovement", 1, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be no new GteVehicleMovement", 1, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect there to be a new GteVehicleEntry", 1, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect the new GteVehicleEntry has correct entry time", GateInTime, Factory.Load<GteVehicleEntry>(new ZQuery())[0].GVE_EntryTime);
			});
		}

		#endregion

		#region GteVehicleMovement field update tests

		public void TestGivenNoMatchingBooking_WhenImportFullySpecifiedUXML_ThenVehicleMovementFieldsAreSetCorrectly()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: false);

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleRegistration to match", shipment.VehicleRun.Vehicle.Registration.Number, vehicleMovement.GVM_VehicleRegistration);
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_RC_VehicleType to match", shipment.VehicleRun.Vehicle.VehicleType.Code, vehicleMovement.VehicleType.RC_Code);
				AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to match", WarehouseLocationForTest.PK, vehicleMovement.GVM_WL_Location);
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_GBK_MainBooking to match", vehicleMovement.GateMovements[0].GateMovementBooking.Booking.PK, vehicleMovement.GVM_GBK_MainBooking);
			});
		}

		public void TestGivenNoMatchingBooking_WhenImportUXMLWithIncorrectWarehouseLocation_ThenWarehouseLocationIsNull()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: false);
			shipment.WarehouseLocation = null;

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to be null when WarehouseLocation is null", ZGuid.Empty, vehicleMovement.GVM_WL_Location);

			shipment.WarehouseLocation = "";
			vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to be null when WarehouseLocation is blank", ZGuid.Empty, vehicleMovement.GVM_WL_Location);

			shipment.WarehouseLocation = "XXXX-XXXXX-XXX";
			vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to be null when WarehouseLocation does not match an existing Warehouse", ZGuid.Empty, vehicleMovement.GVM_WL_Location);
		}

		public void TestGivenNoMatchingBooking_WhenImportUXMLWithBlankOrEmptyVehicleRegistration_ThenThrowError()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: false);
			shipment.VehicleRun.Vehicle.Registration.Number = null;

			AssertExceptionThrown<DataObjectReadFailureException>("Vehicle Registration cannot be empty", () =>
			{
				new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			});

			shipment.VehicleRun.Vehicle.Registration.Number = "";
			AssertExceptionThrown<DataObjectReadFailureException>("Vehicle Registration cannot be empty", () =>
			{
				new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			});
		}

		public void TestGivenMatchingVehicleMovementBooking_WhenImportUXMLWithNoVehicleRegistrationOrType_ThenFieldsSetFromBooking()
		{
			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS-001";
			gateMovementBooking.GBM_Source = "VBS";
			var booking = gateMovementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovementBooking = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking.GBV_GBK_Booking = booking.PK;
			vehicleMovementBooking.GBV_VehicleRegistration = "REG-001";
			vehicleMovementBooking.GBV_RC_VehicleType = Factory.NewWithValidTestData<RefContainer>().PK;
			vehicleMovementBooking.VehicleType.RC_Code = "20FR";

			var shipment = GetVehicleMovementShipmentForTest("", gateMovementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: false);
			shipment.VehicleRun = null;

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("VehicleMovement.GVM_VehicleRegistration should be set from VehicleMovementBooking", "REG-001", vehicleMovement.GVM_VehicleRegistration);
				AssertEquals("VehicleMovement.GVM_RC_VehicleType should be set from VehicleMovementBooking", vehicleMovementBooking.GBV_RC_VehicleType, vehicleMovement.GVM_RC_VehicleType);
			});
		}

		public void TestGivenNoMatchingBooking_WhenImportUXMLWithIncorrectVehicleType_ThenVehicleTypeIsEmpty()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: false);
			shipment.VehicleRun.Vehicle.VehicleType.Code = null;

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleType to be null when when VehicleType Code is null", ZGuid.Empty, vehicleMovement.GVM_RC_VehicleType);

			shipment.VehicleRun.Vehicle.VehicleType.Code = "";
			vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleType to be null when VehicleType Code is empty", ZGuid.Empty, vehicleMovement.GVM_RC_VehicleType);

			shipment.VehicleRun.Vehicle.VehicleType.Code = "XXX";
			vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleType to be null when VehicleType Code does not map to a type", ZGuid.Empty, vehicleMovement.GVM_RC_VehicleType);
		}

		public void TestGivenMatchingMovement_WhenImportFullySpecifiedUXML_ThenVehicleMovementFieldsAreUpdatedCorrectly()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();

			var vehicleMovement = gateMovement.VehicleMovement;
			vehicleMovement.GVM_RC_VehicleType = VehicleTypeForTest.PK;
			vehicleMovement.GVM_VehicleRegistration = "REG-001";
			vehicleMovement.GVM_WL_Location = WarehouseLocationForTest.PK;
			var movementBooking = gateMovement.GateMovementBooking;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);

			var newVehicleType = Factory.NewWithValidTestData<RefContainer>();
			newVehicleType.RC_Code = "MIATA";
			shipment.VehicleRun.Vehicle.VehicleType.Code = newVehicleType.RC_Code;
			shipment.VehicleRun.Vehicle.Registration.Number = "REG-002";

			var newWarehouse = Factory.NewWithValidTestData<WhsLocation>();
			newWarehouse.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			newWarehouse.Warehouse.WW_IsActive = true;
			newWarehouse.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var address = newWarehouse.Warehouse.WarehouseAddress;
			address.Address1 = "Address 11";
			address.Address2 = "Address 12";
			address.City = "SYD";
			address.Postcode = "0000";
			address.OA_Code = "BLANK SYD 3";
			address.Header.OH_Code = "ZX";

			Factory.SaveForTesting();

			shipment.WarehouseLocation = newWarehouse.ToLocationString();
			shipment.OrganizationAddressCollection.Remove(shipment.OrganizationAddressCollection.First(a => a.AddressType.ToString() == nameof(DocAddressType.LocalCartageYard)));
			shipment.OrganizationAddressCollection.Add(new OrganizationAddress()
			{
				AddressType = nameof(DocAddressType.LocalCartageYard),
				Address1 = address.Address1,
				Address2 = address.Address2,
				City = address.City,
				Postcode = address.Postcode,
				AddressShortCode = address.OA_Code,
				OrganizationCode = address.Header.OH_Code,
			});

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleRegistration to match", shipment.VehicleRun.Vehicle.Registration.Number, vehicleMovementFromShipment.GVM_VehicleRegistration);
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_RC_VehicleType to match", shipment.VehicleRun.Vehicle.VehicleType.Code, vehicleMovementFromShipment.VehicleType.RC_Code);
				AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to match", newWarehouse.PK, vehicleMovementFromShipment.GVM_WL_Location);
			});
		}

		public void TestGivenMatchingMovement_WhenImportUXMLWithBlankValues_ThenVehicleMovementFieldsAreUnchanged()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var vehicleMovement = gateMovement.VehicleMovement;
			vehicleMovement.GVM_RC_VehicleType = VehicleTypeForTest.PK;
			vehicleMovement.GVM_VehicleRegistration = "REG-001";
			vehicleMovement.GVM_WL_Location = WarehouseLocationForTest.PK;
			var movementBooking = gateMovement.GateMovementBooking;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);
			shipment.VehicleRun.Vehicle.Registration.Number = "";
			shipment.VehicleRun.Vehicle.VehicleType.Code = "";
			shipment.WarehouseLocation = "";

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleRegistration to be unchanged", "REG-001", vehicleMovement.GVM_VehicleRegistration);
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_RC_VehicleType to be unchanged", VehicleTypeForTest.PK, vehicleMovement.GVM_RC_VehicleType);
				AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to be unchanged", WarehouseLocationForTest.PK, vehicleMovement.GVM_WL_Location);
			});
		}

		public void TestGivenMatchingMovement_WhenImportUXMLWithNullValues_ThenVehicleMovementFieldsAreUnchanged()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var vehicleMovement = gateMovement.VehicleMovement;
			vehicleMovement.GVM_RC_VehicleType = VehicleTypeForTest.PK;
			vehicleMovement.GVM_VehicleRegistration = "REG-001";
			vehicleMovement.GVM_WL_Location = WarehouseLocationForTest.PK;
			var movementBooking = gateMovement.GateMovementBooking;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);
			shipment.VehicleRun.Vehicle.Registration.Number = null;
			shipment.VehicleRun.Vehicle.VehicleType.Code = null;
			shipment.WarehouseLocation = null;

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_VehicleRegistration to be unchanged", "REG-001", vehicleMovement.GVM_VehicleRegistration);
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_RC_VehicleType to be unchanged", VehicleTypeForTest.PK, vehicleMovement.GVM_RC_VehicleType);
				AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to be unchanged", WarehouseLocationForTest.PK, vehicleMovement.GVM_WL_Location);
			});
		}

		public void TestGivenMatchingMovement_WhenImportUXMLWithInvalidValues_ThenVehicleMovementFieldsAreUnchanged()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var vehicleMovement = gateMovement.VehicleMovement;
			vehicleMovement.GVM_RC_VehicleType = VehicleTypeForTest.PK;
			vehicleMovement.GVM_VehicleRegistration = "REG-001";
			vehicleMovement.GVM_WL_Location = WarehouseLocationForTest.PK;
			var movementBooking = gateMovement.GateMovementBooking;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);
			shipment.VehicleRun.Vehicle.VehicleType.Code = "XXXX";
			shipment.WarehouseLocation = "XXXXX";

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement.GVM_RC_VehicleType to be unchanged", VehicleTypeForTest.PK, vehicleMovement.GVM_RC_VehicleType);
				AssertEquals("Postcondition: Expect GteVehicleMovement.WL_Location to be unchanged", WarehouseLocationForTest.PK, vehicleMovement.GVM_WL_Location);
			});
		}

		public void TestGivenMatchingMovement_WhenImportValidUXML_ThenMainBookingUnchanged()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var vehicleMovement = gateMovement.VehicleMovement;
			vehicleMovement.GVM_RC_VehicleType = VehicleTypeForTest.PK;
			vehicleMovement.GVM_VehicleRegistration = "REG-001";
			vehicleMovement.GVM_WL_Location = WarehouseLocationForTest.PK;
			var movementBooking = gateMovement.GateMovementBooking;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			var originalVehicleMovementMainBooking = vehicleMovement.GVM_GBK_MainBooking;
			AssertNotEquals("MainBooking should be unrelated to GVM data", movementBooking.GBM_GBK_Booking.ToString(), originalVehicleMovementMainBooking.ToString());

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("GVM_GBK_MainBooking should be unchanged", originalVehicleMovementMainBooking.ToString(), vehicleMovementFromShipment.GVM_GBK_MainBooking.ToString());
		}

		#endregion

		#region UXML without missing or invalid actions compared to existing

		public void TestGivenFullyMatchedBookingWithNoActions_WhenImportGateInUXML_ThenBookingIsGatedIn()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: false);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Expect there to be an existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be an existing GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleMovements", 0, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleEntries", 0, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect there to be a new GteVehicleMovement", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one GteGateMovement", 1, vehicleMovement.GateMovements.Count);
				AssertEquals("Postcondition: Expect GteGateMovement to have correct MovementBooking", movementBooking, vehicleMovement.GateMovements[0].GateMovementBooking);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleEntry to be incoming", true, vehicleMovement.VehicleEntries[0].GVE_IsIncoming);
				AssertEquals("Postcondition: Expect the new GteVehicleEntry has correct entry time", GateInTime, vehicleMovement.VehicleEntries[0].GVE_EntryTime);
			});
		}

		public void TestGivenNoExistingBooking_WhenImportGateInUXML_ThenBookingIsCreatedAndGatedIn()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: false);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Expect there to be no existing GteBooking", 0, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovementBooking", 0, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleMovements", 0, Factory.Load<GteVehicleMovement>(new ZQuery()).Length);
				AssertEquals("Precondition: Expect there to be no existing GteVehicleEntries", 0, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect there to be a new GteVehicleMovement", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one GteGateMovement", 1, vehicleMovement.GateMovements.Count);
				AssertNotNull("Postcondition: Expect GteGateMovement to have a MovementBooking", vehicleMovement.GateMovements[0].GateMovementBooking);
				AssertNotNull("Postcondition: Expect GteGateMovement to have a GteBooking", vehicleMovement.GateMovements[0].GateMovementBooking.Booking);
				AssertEquals("Postcondition: Expect GteBooking to have be ad-hoc", GateManagementConstants.BookingTypes.AdHoc, vehicleMovement.GateMovements[0].GateMovementBooking.Booking.GBK_BookingType);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleEntry to be incoming", true, vehicleMovement.VehicleEntries[0].GVE_IsIncoming);
				AssertEquals("Postcondition: Expect the new GteVehicleEntry has correct entry time", GateInTime, vehicleMovement.VehicleEntries[0].GVE_EntryTime);
			});
		}

		public void TestGivenFullyMatchedBookingWithReversedGateIn_WhenImportGateInUXML_ThenBookingIsGatedIn()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = movementBooking.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var cancelledGateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledGateIn.GVE_CancelledReason = "TEST";
			cancelledGateIn.GVE_CancelledTime = DateTime.Now;
			cancelledGateIn.GVE_GS_NKCancelledBy = "~BP";
			cancelledGateIn.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: false);

			var newVehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement to match", vehicleMovement.PK, newVehicleMovement.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one GteGateMovement", 1, vehicleMovement.GateMovements.Count);
				AssertEquals("Postcondition: Expect GteGateMovement to have correct MovementBooking", movementBooking, vehicleMovement.GateMovements[0].GateMovementBooking);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two incoming GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one cancelled GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_CancelledReason != string.Empty));
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one uncancelled GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_CancelledReason == string.Empty));
				AssertEquals("Postcondition: Expect the uncancelled GteVehicleEntry has correct entry time", GateInTime, vehicleMovement.VehicleEntries.First(entry => entry.GVE_CancelledReason == string.Empty).GVE_EntryTime);
			});
		}

		public void TestGivenFullyMatchedBookingWithGateIn_WhenImportFullGateOutUXML_ThenGateOutIsCreated()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var movementBooking = gateMovement.GateMovementBooking;
			var vehicleMovement = gateMovement.VehicleMovement;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleEntry = Factory.New<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.FillWithValidTestData();
			vehicleEntry.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: true);

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect VehicleMovement to match", vehicleMovement.PK, vehicleMovementFromShipment.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the incoming GteVehicleEntry has correct entry time", GateInTime, vehicleMovement.VehicleEntries.First(entry => entry.GVE_IsIncoming).GVE_EntryTime);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one outgoing GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the outgoing GteVehicleEntry has correct entry time", GateOutTime, vehicleMovement.VehicleEntries.First(entry => !entry.GVE_IsIncoming).GVE_EntryTime);
			});
		}

		public void TestGivenFullyMatchedBookingWithNoActions_WhenImportFullGateOutUXML_ThenBookingIsGatedOut()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: true);

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect VehicleMovement to have correct parent booking", booking.PK, vehicleMovement.GateMovements[0].GateMovementBooking.Booking.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the incoming GteVehicleEntry has correct entry time", GateInTime, vehicleMovement.VehicleEntries.First(entry => entry.GVE_IsIncoming).GVE_EntryTime);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one outgoing GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the outgoing GteVehicleEntry has correct entry time", GateOutTime, vehicleMovement.VehicleEntries.First(entry => !entry.GVE_IsIncoming).GVE_EntryTime);
			});
		}

		public void TestGivenNoExistingBooking_WhenImportFullGateOutUXML_ThenBookingIsCreatedAndGatedOut()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: true, hasGateOut: true);

			var vehicleMovement = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the incoming GteVehicleEntry has correct entry time", GateInTime, vehicleMovement.VehicleEntries.First(entry => entry.GVE_IsIncoming).GVE_EntryTime);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one outgoing GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the outgoing GteVehicleEntry has correct entry time", GateOutTime, vehicleMovement.VehicleEntries.First(entry => !entry.GVE_IsIncoming).GVE_EntryTime);
			});
		}

		public void TestGivenFullyMatchedBookingWithGateIn_WhenImportGateOutUXML_ThenBookingIsGatedOut()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var movementBooking = gateMovement.GateMovementBooking;
			var vehicleMovement = gateMovement.VehicleMovement;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleEntry = Factory.New<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.FillWithValidTestData();
			vehicleEntry.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: true);

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect VehicleMovement to match", vehicleMovement.PK, vehicleMovementFromShipment.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one outgoing GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect the outgoing GteVehicleEntry has correct entry time", GateOutTime, vehicleMovement.VehicleEntries.First(entry => !entry.GVE_IsIncoming).GVE_EntryTime);
			});
		}

		public void TestGivenFullyMatchedBookingReversedGateOut_WhenImportGateOutUXML_ThenBookingIsGatedOut()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var movementBooking = gateMovement.GateMovementBooking;
			var vehicleMovement = gateMovement.VehicleMovement;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleEntry = Factory.New<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.FillWithValidTestData();
			vehicleEntry.GVE_IsIncoming = true;

			var cancelledVehicleExit = Factory.New<GteVehicleEntry>();
			cancelledVehicleExit.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledVehicleExit.FillWithValidTestData();
			cancelledVehicleExit.GVE_IsIncoming = false;
			cancelledVehicleExit.GVE_EntryTime = DateTime.UtcNow - TimeSpan.FromDays(1);
			cancelledVehicleExit.GVE_CancelledReason = "Test";
			cancelledVehicleExit.GVE_CancelledTime = DateTime.UtcNow;
			cancelledVehicleExit.GVE_GS_NKCancelledBy = "~BP";
			Factory.SaveForTesting();

			var entries = vehicleMovement.VehicleEntries;

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: true, hasGateOut: true);

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect VehicleMovement to match", vehicleMovement.PK, vehicleMovementFromShipment.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have three GteVehicleEntries", 3, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two outgoing GteVehicleEntry", 2, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one current outgoing GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming && entry.GVE_CancelledReason == ""));
				AssertEquals("Postcondition: Expect the current outgoing GteVehicleEntry has correct entry time", GateOutTime, vehicleMovement.VehicleEntries.First(entry => !entry.GVE_IsIncoming && entry.GVE_CancelledReason == "").GVE_EntryTime);
			});
		}

		public void TestGivenFullyMatchedBookingWithGateIn_WhenImportUXMLWithoutGateIn_ThenGateInIsUnmodified()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var movementBooking = gateMovement.GateMovementBooking;
			var vehicleMovement = gateMovement.VehicleMovement;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleEntry = Factory.New<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.FillWithValidTestData();
			vehicleEntry.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect VehicleMovement to match", vehicleMovement.PK, vehicleMovementFromShipment.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("PostconditioN: Expect no new GteVehicleEntry to have been created", 1, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});
		}

		public void TestGivenFullyMatchedBookingWithGateOut_WhenImportUXMLWithoutGateOut_ThenGateOutIsUnmodified()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var movementBooking = gateMovement.GateMovementBooking;
			var vehicleMovement = gateMovement.VehicleMovement;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			var booking = movementBooking.Booking;

			var vehicleEntry = Factory.New<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.FillWithValidTestData();
			vehicleEntry.GVE_IsIncoming = true;

			var vehicleExit = Factory.New<GteVehicleEntry>();
			vehicleExit.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleExit.FillWithValidTestData();
			vehicleExit.GVE_IsIncoming = false;

			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expect VehicleMovement to match", vehicleMovement.PK, vehicleMovementFromShipment.PK);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have two GteVehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one incoming GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => entry.GVE_IsIncoming));
				AssertEquals("Postcondition: Expect GteVehicleMovement to have one outgoing GteVehicleEntry", 1, vehicleMovement.VehicleEntries.Count(entry => !entry.GVE_IsIncoming));
				AssertEquals("PostconditioN: Expect no new GteVehicleEntry to have been created", 2, Factory.Load<GteVehicleEntry>(new ZQuery()).Length);
			});
		}

		public void TestGivenFullyMatchedBookingWithNoActions_WhenImportUXMLWithNoVehicleRun_ThenValuesSetFromVehicleBooking()
		{
			var movementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var booking = movementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleBooking = Factory.New<GteVehicleMovementBooking>();
			vehicleBooking.GBV_GBK_Booking = booking.PK;
			vehicleBooking.GBV_VehicleRegistration = "REG001";
			vehicleBooking.GBV_RC_VehicleType = VehicleTypeForTest.PK;
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";
			Factory.SaveForTesting();

			var shipment = GetVehicleMovementShipmentForTest("", movementBooking.GBM_MovementBookingNumber, hasGateIn: false, hasGateOut: false);
			shipment.VehicleRun = null;

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: Expected GVM_VehicleRegistration to be set from the first VehicleMovementBooking", "REG001", vehicleMovementFromShipment.GVM_VehicleRegistration);
				AssertEquals("Postcondition: Expected GVM_VehicleType to be set from the first VehicleMovementBooking", VehicleTypeForTest.PK, vehicleMovementFromShipment.GVM_RC_VehicleType);
			});
		}

		#endregion

		#region Invalid UXML

		[ExpectException(expectedException: typeof(DataObjectReadFailureException))]
		public void TestGivenUXMLWithNoGateMovements_WhenImportUXML_ThenThrowsError()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: false, hasGateOut: false);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
		}

		[ExpectException(expectedException: typeof(DataObjectReadFailureException))]
		public void TestGivenUXMLWithNoAddress_WhenImportUXML_ThenThrowsError()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: false, hasGateOut: false);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
		}

		[ExpectException(expectedException: typeof(DataObjectReadFailureException))]
		public void TestGivenUXMLWithInvalidAddress_WhenImportUXML_ThenThrowsError()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: false, hasGateOut: false);
			shipment.OrganizationAddressCollection[0].OrganizationCode = "XXXX";

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
		}

		[ExpectException(expectedException: typeof(DataObjectReadFailureException))]
		public void TestGivenUXMLWithoutVehicleRegistration_WhenImportUXML_ThenThrowsError()
		{
			var shipment = GetVehicleMovementShipmentForTest("", "VBS-001", hasGateIn: false, hasGateOut: false);
			shipment.VehicleRun.Vehicle.Registration = null;

			var vehicleMovementFromShipment = new GteVehicleMovementDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
		}

		#endregion

		#region Helpers

		Shipment GetVehicleMovementShipmentForTest(ZString? gateInNumber, ZString? movementBookingNumber, bool hasGateIn, bool hasGateOut)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.GateVehicleMovement, gateInNumber);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress() { AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress), OrganizationCode = TransportCompanyForTest.OH_Code }
			});

			shipment.WarehouseLocation = WarehouseLocationForTest.ToLocationString();
			var address = WarehouseLocationForTest.Warehouse.WarehouseAddress;
			shipment.AddOrgAddress(new OrganizationAddress()
			{
				AddressType = nameof(DocAddressType.LocalCartageYard),
				AddressShortCode = address.OA_Code,
				Address1 = address.Address1,
				Address2 = address.Address2,
				City = address.City,
				Postcode = address.Postcode,
				OrganizationCode = address.Header.OH_Code,
			});

			shipment.VehicleRun = new VehicleRun()
			{
				Vehicle = new Vehicle() { Registration = new Registration() { Number = "REG-123" }, VehicleType = new CodeDescriptionPair10Char() { Code = VehicleTypeForTest.RC_Code } },
			};

			if (hasGateIn || hasGateOut)
			{
				shipment.SetRelatedShipmentCollection(() => new List<Shipment>());
				shipment.SetDateCollection(() => new List<Date>());

				if (hasGateIn)
				{
					shipment.RelatedShipmentCollection.Add(GetVehicleEntryShipmentForTest(isGateIn: true));
					shipment.DateCollection.Add(Date.New(DateType.Start, ZBool.False, GateInTime));
				}

				if (hasGateOut)
				{
					shipment.RelatedShipmentCollection.Add(GetVehicleEntryShipmentForTest(isGateIn: false));
					shipment.DateCollection.Add(Date.New(DateType.End, ZBool.False, GateOutTime));
				}
			}

			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			shipment.SubShipmentCollection.Add(GetGateMovementShipmentForTest(movementBookingNumber, isPickup: true));

			return shipment;
		}

		Shipment GetVehicleEntryShipmentForTest(bool isGateIn)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TotalWeight = 0;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = "KG" };
			shipment.WarehouseLocation = LaneForTest.Gate.GTE_Code + "|" + LaneForTest.GLN_Code;

			shipment.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = "IsIncoming", Value = isGateIn ? "true" : "false" }
			});

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>()
			{
				new Crew() { FullName = "Jimothy", LicenseNumber = "1234 5678" }
			});

			return shipment;
		}

		Shipment GetGateMovementShipmentForTest(ZString? movementBookingNumber, bool isPickup)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.BookingConfirmationReference = "BRN001";

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			if (!string.IsNullOrEmpty(movementBookingNumber))
			{
				shipment.AdditionalReferenceCollection.Add(new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = GateManagementConstants.ReferenceTypes.Codes.MovementBookingNumber,
						Description = GateManagementConstants.ReferenceTypes.Descriptions.MovementBookingNumber
					},
					ReferenceNumber = movementBookingNumber
				});
			}
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference()
			{
				Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.TransportReference, Description = AdditionalReferenceTypes.Descriptions.TransportReference },
				ReferenceNumber = "TRF001"
			});

			shipment.TransportBookingDirection = new TransportBookingDirection()
			{
				Code = isPickup
					? GateManagementConstants.TransportBookingDirections.Codes.Pickup
					: GateManagementConstants.TransportBookingDirections.Codes.Delivery
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

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				new PackingLine() { Commodity = new Commodity() { Code = "AABT" }, PackType = new PackageType() { Code = "BAG" } }
			});

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "UNT-001", ContainerType = new ContainerType() { Code = "20FR" } }
			});

			return shipment;
		}

		OrgHeader TransportCompanyForTest
		{
			get
			{
				if (transportCompanyForTest == null)
				{
					transportCompanyForTest = Factory.NewWithValidTestData<OrgHeader>();
				}
				return transportCompanyForTest;
			}
			set => transportCompanyForTest = value;
		}
		OrgHeader transportCompanyForTest;

		GteLane LaneForTest
		{
			get
			{
				if (laneForTest == null)
				{
					laneForTest = Factory.NewWithValidTestData<GteLane>();
					laneForTest.GLN_Code = "LNE";
					laneForTest.Gate.GTE_Code = "GTE";
					laneForTest.Gate.GTE_GB_Branch = BranchForTest.PK;
					laneForTest.Gate.GTE_WW_Facility = WarehouseForTest.PK;
					Factory.SaveForTesting();
				}
				return laneForTest;
			}
			set => laneForTest = value;
		}
		GteLane laneForTest;

		GlbBranch BranchForTest
		{
			get
			{
				if (branchForTest == null)
				{
					branchForTest = Factory.NewWithValidTestData<GlbBranch>();
					Factory.SaveForTesting();
				}
				return branchForTest;
			}
			set => branchForTest = value;
		}
		GlbBranch branchForTest;

		WhsWarehouse WarehouseForTest
		{
			get
			{
				if (warehouseForTest == null)
				{
					warehouseForTest = Factory.NewWithValidTestData<WhsWarehouse>();
					warehouseForTest.WW_GB_RelatedCompanyBranch = BranchForTest.PK;
					warehouseForTest.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
					warehouseForTest.WW_IsActive = true;
					var address = warehouseForTest.WarehouseAddress;
					address.Address1 = "Address 01";
					address.Address2 = "Address 02";
					address.City = "SYD";
					address.Postcode = "0000";
					address.OA_Code = "BLANK SYD";
					address.Header.OH_Code = "ZY";

					Factory.SaveForTesting();
				}
				return warehouseForTest;
			}
			set => warehouseForTest = value;
		}
		WhsWarehouse warehouseForTest;

		WhsLocation WarehouseLocationForTest
		{
			get
			{
				if (warehouseLocationForTest == null)
				{
					warehouseLocationForTest = Factory.NewWithValidTestData<WhsLocation>();
					warehouseLocationForTest.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
					warehouseLocationForTest.WLV_WW_Whs = WarehouseForTest.PK;
					Factory.SaveForTesting();
				}
				return warehouseLocationForTest;
			}
			set => warehouseLocationForTest = value;
		}
		WhsLocation warehouseLocationForTest;

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

		RefContainer VehicleTypeForTest
		{
			get
			{
				if (vehicleTypeForTest == null)
				{
					vehicleTypeForTest = Factory.NewWithValidTestData<RefContainer>();
					vehicleTypeForTest.RC_Code = "RTRK";
					Factory.SaveForTesting();
				}
				return vehicleTypeForTest;
			}
			set => vehicleTypeForTest = value;
		}
		RefContainer vehicleTypeForTest;

		#endregion
	}
}
