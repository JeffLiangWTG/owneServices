using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
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
using static Enterprise.Core.Constants.GateManagementConstants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleMovementBookingDataObjectReaderTest))]
	public class GteVehicleMovementBookingDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateBusinessObjectWithVehicleType_GivenValidUXML()
		{
			//Arrange
			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refContainer1.RC_Code = "VALID";
			refContainer1.RC_Description = "Existing RefContainer";

			Factory.SaveForTesting();

			var shipment1 = SetupShipment();
			var vehicle1 = shipment1.PreCarriageShipmentCollection.First().VehicleRun.Vehicle;

			vehicle1.VehicleType = new CodeDescriptionPair10Char()
			{
				Code = "VALID",
				Description = "Existing RefContainer"
			};

			var shipment2 = SetupShipment();
			var vehicle2 = shipment2.PreCarriageShipmentCollection.First().VehicleRun.Vehicle;

			vehicle2.VehicleType = new CodeDescriptionPair10Char()
			{
				Code = "INVALID",
				Description = "Non-existant refContainer"
			};

			//Act
			var booking1 = new GteBookingDataObjectReader(shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var vehicleMovementBooking1 = (GteVehicleMovementBooking)booking1.VehicleMovementBookings.FirstOrDefault();

			Factory.SaveForTesting();

			var booking2 = new GteBookingDataObjectReader(shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var vehicleMovementBooking2 = (GteVehicleMovementBooking)booking2.VehicleMovementBookings.FirstOrDefault();

			//Assert
			CombineAssertions(() =>
			{
				AssertEquals("Expected Vehicle Registration to be populated correctly", "TESTING123", vehicleMovementBooking1.GBV_VehicleRegistration);
				AssertEquals("Expected refContainer to be found matching on RC_Code", refContainer1.PK, vehicleMovementBooking1.GBV_RC_VehicleType);

				AssertEquals("Expected Vehicle Registration to be populated correctly", "TESTING123", vehicleMovementBooking2.GBV_VehicleRegistration);
				AssertEquals("Expected no matching Vehicle Types to be found", ZGuid.Empty, vehicleMovementBooking2.GBV_RC_VehicleType);
			});
		}

		public void TestPopulateBusinessObject_GivenUXMLMissingInformation_ThenThrowNoExceptions()
		{
			//Arrange
			var shipment1 = SetupShipment();
			shipment1.SetPreCarriageShipmentCollection(() => null);

			var shipment2 = SetupShipment();
			var vehicle2 = shipment2.PreCarriageShipmentCollection.First().VehicleRun.Vehicle;
			vehicle2.Registration = null;

			var shipment3 = SetupShipment();
			var vehicle3 = shipment3.PreCarriageShipmentCollection.First().VehicleRun.Vehicle;
			vehicle3.Registration.Number = string.Empty;

			//Act + Assert
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Should accept all UXML's sent by VBS", () => new GteBookingDataObjectReader(shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject());
				AssertNoExceptionThrown("Should accept all UXML's sent by VBS", () => new GteBookingDataObjectReader(shipment3, new DummyLogger(), Factory).ReadIntoBusinessObject());
			});
		}

		public void TestGivenExistingGBVs_WhenReadingUXMLWithGBVs_ThenUpdateAddDeleteGBVs()
		{
			// Arrange
			var vehicleType1 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType1.RC_Code = "Type 1";
			vehicleType1.RC_Description = "Existing RefContainer";

			var vehicleType2 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType2.RC_Code = "Type 2";
			vehicleType2.RC_Description = "Existing RefContainer";

			var ogBooking = Factory.NewWithValidTestData<GteBooking>();
			ogBooking.GBK_ReferenceNumber = "BookingReference";

			var vehicleMovementBooking0 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking0.GBV_GBK_Booking = ogBooking.PK;
			vehicleMovementBooking0.GBV_VehicleRegistration = "Vehicle 0";
			vehicleMovementBooking0.GBV_RC_VehicleType = vehicleType1.PK;

			var vehicleMovementBooking1 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking1.GBV_GBK_Booking = ogBooking.PK;
			vehicleMovementBooking1.GBV_VehicleRegistration = "Vehicle 1";
			vehicleMovementBooking1.GBV_RC_VehicleType = vehicleType1.PK;

			Factory.SaveForTesting();

			var shipment = SetupShipment();

			var vehicleMovementData1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var vehicleRun1 = new VehicleRun();
			var vehicle1 = new Vehicle();
			var vehicleRegistration1 = new Registration();

			vehicleRegistration1.Number = "Vehicle 1";
			vehicle1.Registration = vehicleRegistration1;
			vehicleRun1.Vehicle = vehicle1;
			vehicleMovementData1.VehicleRun = vehicleRun1;

			vehicle1.VehicleType = new CodeDescriptionPair10Char()
			{
				Code = "Type 2",
				Description = "Existing RefContainer"
			};

			var vehicleMovementData2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var vehicleRun2 = new VehicleRun();
			var vehicle2 = new Vehicle();
			var vehicleRegistration2 = new Registration();

			vehicleRegistration2.Number = "Vehicle 2";
			vehicle2.Registration = vehicleRegistration2;
			vehicleRun2.Vehicle = vehicle2;
			vehicleMovementData2.VehicleRun = vehicleRun2;

			shipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment> { vehicleMovementData1, vehicleMovementData2 });

			// Act
			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var vehicleMovement0 = booking.VehicleMovementBookings.Find(x => x.GBV_VehicleRegistration == "Vehicle 0").FirstOrDefault();
			var vehicleMovement1 = booking.VehicleMovementBookings.Find(x => x.GBV_VehicleRegistration == "Vehicle 1").First();
			var vehicleMovement2 = booking.VehicleMovementBookings.Find(x => x.GBV_VehicleRegistration == "Vehicle 2").FirstOrDefault();

			Factory.SaveForTesting();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("Booking should only have 2 vehicle movement bookings.", 2, booking.VehicleMovementBookings.Count);
				AssertNull("Unmatched vehicle movement booking should be deleted", vehicleMovement0);
				AssertEquals("Existing vehicle movement booking should be updated", vehicleType2.PK, vehicleMovement1.GBV_RC_VehicleType);
				AssertNotNull("New vehicle movement booking should be added", vehicleMovement2);
			});
		}

		UniversalShipment SetupShipment()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.GateBooking, string.Empty);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress
				{
					AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress),
					OrganizationCode = "ABC"
				},
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.LocalCartageYard),
					Address1 = "1 main st",
					City = "Sydney",
					Postcode = "2020",
					State = "NSW",
					Country = new Country() { Code = "AU" }
				}
			});

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC123" };
			shipment.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard)).SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.BookingConfirmationReference = "GateMovementBooking1";
			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			var vehicleMovement = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var vehicleRun = new VehicleRun();
			var vehicle = new Vehicle();
			var vehicleRegistration = new Registration();

			vehicleRegistration.Number = "TESTING123";
			vehicle.Registration = vehicleRegistration;
			vehicleRun.Vehicle = vehicle;
			vehicleMovement.VehicleRun = vehicleRun;

			shipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment> { vehicleMovement });

			var crew = new Crew();
			crew.FullName = "Thomas Jefferson";
			crew.LicenseNumber = "12345678";
			crew.CrewType = CrewType.Driver;

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew> { crew });

			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			var facilityCompany = Factory.NewWithValidTestData<OrgHeader>();
			facilityCompany.OH_Code = "XYZ";
			facilityCompany.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC123", string.Empty);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = facilityCompany.MainAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse.WW_IsActive = true;

			Factory.SaveForTesting();
		}
	}
}
