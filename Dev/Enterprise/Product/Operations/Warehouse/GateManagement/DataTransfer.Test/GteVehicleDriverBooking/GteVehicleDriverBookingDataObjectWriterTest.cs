using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.GateManagement.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleDriverBookingDataObjectWriterTest))]
	class GteVehicleDriverBookingDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestDefaultCrewType()
		{
			var vehicleType1 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType1.RC_Code = "Type 1";
			vehicleType1.RC_Description = "Existing RefContainer";

			var bookingBO = Factory.NewWithValidTestData<GteBooking>();
			bookingBO.GBK_ReferenceNumber = "BookingReference";

			var vehicleMovementBooking0 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking0.GBV_GBK_Booking = bookingBO.PK;
			vehicleMovementBooking0.GBV_VehicleRegistration = "Vehicle 0";
			vehicleMovementBooking0.GBV_RC_VehicleType = vehicleType1.PK;

			var vehicleDriverBooking1 = CreateVehicleDriverBookingForTest(bookingBO.PK.ToGuid(), "12345678", "Thomas Jefferson");

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingBO)))
				.GetDataObject(bookingBO);

			CombineAssertions(() =>
			{
				AssertEquals("Expect there to be a single crew member", 1,
					dataObject.VehicleRun?.CrewCollection.Count);
				AssertEquals("Expect Driver CrewType to be correctly filled", CrewType.Driver,
					dataObject.VehicleRun?.CrewCollection?[0].CrewType);
			});
		}

		public void TestWriteToDataObject()
		{
			var vehicleType1 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType1.RC_Code = "Type 1";
			vehicleType1.RC_Description = "Existing RefContainer";

			var bookingBO = Factory.NewWithValidTestData<GteBooking>();
			bookingBO.GBK_ReferenceNumber = "BookingReference";

			var vehicleMovementBooking0 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking0.GBV_GBK_Booking = bookingBO.PK;
			vehicleMovementBooking0.GBV_VehicleRegistration = "Vehicle 0";
			vehicleMovementBooking0.GBV_RC_VehicleType = vehicleType1.PK;

			var vehicleDriverBooking1 = CreateVehicleDriverBookingForTest(bookingBO.PK.ToGuid(), "12345678", "Thomas Jefferson");

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingBO)))
				.GetDataObject(bookingBO);

			var dataObjectLicenseNumber = dataObject.VehicleRun?.CrewCollection?[0].LicenseNumber;
			var dataObjectFullName = dataObject.VehicleRun?.CrewCollection?[0].FullName;

			CombineAssertions(() =>
			{
				AssertEquals("Expect Driver License Number to be correctly filled", "12345678",
					dataObjectLicenseNumber ?? string.Empty);
				AssertEquals("Expect Driver Name to be correctly filled", "Thomas Jefferson",
					dataObjectFullName ?? string.Empty);
			});
		}

		public void TestGivenMultipleDrivers_WhenWriteToDataObject_ThenCreateMultipleCrewInUXML()
		{
			var vehicleType1 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType1.RC_Code = "Type 1";
			vehicleType1.RC_Description = "Existing RefContainer";

			var bookingBO = Factory.NewWithValidTestData<GteBooking>();
			bookingBO.GBK_ReferenceNumber = "BookingReference";

			var vehicleMovementBooking0 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking0.GBV_GBK_Booking = bookingBO.PK;
			vehicleMovementBooking0.GBV_VehicleRegistration = "Vehicle 0";
			vehicleMovementBooking0.GBV_RC_VehicleType = vehicleType1.PK;

			var vehicleDriverBooking1 =
				CreateVehicleDriverBookingForTest(bookingBO.PK.ToGuid(), "12121212", "Thomas Jefferson");
			var vehicleDriverBooking2 =
				CreateVehicleDriverBookingForTest(bookingBO.PK.ToGuid(), "34343434", "George Washington");
			var vehicleDriverBooking3 =
				CreateVehicleDriverBookingForTest(bookingBO.PK.ToGuid(), "56565656", "Benjamin Franklin");

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingBO)))
				.GetDataObject(bookingBO);

			CombineAssertions(() =>
			{
				AssertEquals("Expect there to be three crew members", 3, dataObject.VehicleRun?.CrewCollection.Count);

				AssertCollectionContains(dataObject.VehicleRun?.CrewCollection, c => c.FullName.ToString() == "Thomas Jefferson" && c.LicenseNumber.ToString() == "12121212");
				AssertCollectionContains(dataObject.VehicleRun?.CrewCollection, c => c.FullName.ToString() == "George Washington" && c.LicenseNumber.ToString() == "34343434");
				AssertCollectionContains(dataObject.VehicleRun?.CrewCollection, c => c.FullName.ToString() == "Benjamin Franklin" && c.LicenseNumber.ToString() == "56565656");
			});
		}

		public void TestGivenEmptyFullNameAndLicenseNumber_WhenWriteToDataObject_ThenCreateVehicleDriverBookingWithEmptyFullNameAndLicenseNumber()
		{
			var vehicleType1 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType1.RC_Code = "Type 1";
			vehicleType1.RC_Description = "Existing RefContainer";

			var bookingBO = Factory.NewWithValidTestData<GteBooking>();
			bookingBO.GBK_ReferenceNumber = "BookingReference";

			var vehicleMovementBooking0 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking0.GBV_GBK_Booking = bookingBO.PK;
			vehicleMovementBooking0.GBV_VehicleRegistration = "Vehicle 0";
			vehicleMovementBooking0.GBV_RC_VehicleType = vehicleType1.PK;

			var vehicleDriverBooking =
				CreateVehicleDriverBookingForTest(bookingBO.PK.ToGuid(), "", "");

			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingBO)))
				.GetDataObject(bookingBO);

			var dataObjectLicenseNumber = dataObject.VehicleRun?.CrewCollection?[0].LicenseNumber;
			var dataObjectFullName = dataObject.VehicleRun?.CrewCollection?[0].FullName;

			CombineAssertions(() =>
			{
				AssertEquals("Expect Driver License Number to be correctly filled", string.Empty,
					dataObjectLicenseNumber);
				AssertEquals("Expect Driver Name to be correctly filled", string.Empty,
					dataObjectFullName);
			});
		}

		GteVehicleDriverBooking CreateVehicleDriverBookingForTest(Guid bookingPK, string licenseNumber, string name)
		{
			var vehicleDriverBooking = Factory.NewWithValidTestData<GteVehicleDriverBooking>();
			vehicleDriverBooking.GBD_GBK_Booking = bookingPK;
			vehicleDriverBooking.GBD_DriverLicenseNumber = licenseNumber;
			vehicleDriverBooking.GBD_DriverName = name;

			return vehicleDriverBooking;
		}
	}
}
