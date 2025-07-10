using System.Collections.Generic;
using System.Linq;
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
	[TestedType(typeof(GteVehicleDriverBookingDataObjectReaderTest))]
	public class GteVehicleDriverBookingDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadDataObject()
		{
			var shipment = SetupShipmentForTest();
			var vehicleRun = shipment.VehicleRun;
			var crew = CreateCrewForTest("Thomas Jefferson", "12345678");

			vehicleRun.CrewCollection.Add(crew);

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var vehicleDriverBooking = (GteVehicleDriverBooking)booking.VehicleDriverBookings.FirstOrDefault();

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Expected Driver name to be populated correctly", "Thomas Jefferson", vehicleDriverBooking.GBD_DriverName);
				AssertEquals("Expected License Number to be populated correctly", "12345678", vehicleDriverBooking.GBD_DriverLicenseNumber);
			});
		}

		public void TestGivenMultipleCrew_WhenReadDataObject_ThenCrewMultipleVehicleDriverBookings()
		{
			var shipment = SetupShipmentForTest();
			var vehicleRun = shipment.VehicleRun;

			var crew1 = CreateCrewForTest("Thomas Jefferson", "12121212");
			vehicleRun.CrewCollection.Add(crew1);
			var crew2 = CreateCrewForTest("George Washington", "34343434");
			vehicleRun.CrewCollection.Add(crew2);
			var crew3 = CreateCrewForTest("Benjamin Franklin", "56565656");
			vehicleRun.CrewCollection.Add(crew3);

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var vehicleDriverBookings = booking.VehicleDriverBookings;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Expect there to be three crew members", 3,
					vehicleDriverBookings.Count);

				AssertEquals("First Driver is present", 1, vehicleDriverBookings.Find(db => db.GBD_DriverName == "Thomas Jefferson" && db.GBD_DriverLicenseNumber == "12121212").Count());
				AssertEquals("Second Driver is present", 1, vehicleDriverBookings.Find(db => db.GBD_DriverName == "George Washington" && db.GBD_DriverLicenseNumber == "34343434").Count());
				AssertEquals("Third Driver is present", 1, vehicleDriverBookings.Find(db => db.GBD_DriverName == "Benjamin Franklin" && db.GBD_DriverLicenseNumber == "56565656").Count());
			});
		}

		public void TestWhenNewShipmentIsRead_DeletePreviousVehicleDriverBookings()
		{
			var shipment = SetupShipmentForTest();
			var vehicleRun = shipment.VehicleRun;
			var crew = CreateCrewForTest("Thomas Jefferson", "12345678");

			vehicleRun.CrewCollection.Add(crew);

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			vehicleRun.CrewCollection.Clear();
			vehicleRun.CrewCollection.Add(CreateCrewForTest("George Washington", "87654321"));

			Factory.SaveForTesting();

			booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var vehicleDriverBooking = (GteVehicleDriverBooking)booking.VehicleDriverBookings.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("Expected a single vehicleDriverBooking", 1, booking.VehicleDriverBookings.Count);

				AssertEquals("Expected Driver name to be populated correctly", "George Washington", vehicleDriverBooking.GBD_DriverName);
				AssertEquals("Expected License Number to be populated correctly", "87654321", vehicleDriverBooking.GBD_DriverLicenseNumber);
			});
		}

		UniversalShipment SetupShipmentForTest()
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

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>());

			return shipment;
		}

		Crew CreateCrewForTest(string name, string licenseNumber)
		{
			var crew = new Crew();
			crew.FullName = name;
			crew.LicenseNumber = licenseNumber;
			crew.CrewType = CrewType.Driver;

			return crew;
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
