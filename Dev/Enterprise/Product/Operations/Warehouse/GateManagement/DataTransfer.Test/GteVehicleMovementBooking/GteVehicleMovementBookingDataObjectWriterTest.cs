using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.GateManagement.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleMovementBookingDataObjectWriterTest))]
	class GteVehicleMovementBookingDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteToDataObject()
		{
			//Arrange
			var vehicleType1 = Factory.NewWithValidTestData<RefContainer>();
			vehicleType1.RC_Code = "Type 1";
			vehicleType1.RC_Description = "Existing RefContainer";

			var bookingBO = Factory.NewWithValidTestData<GteBooking>();
			bookingBO.GBK_ReferenceNumber = "BookingReference";

			var vehicleMovementBooking0 = Factory.NewWithValidTestData<GteVehicleMovementBooking>();
			vehicleMovementBooking0.GBV_GBK_Booking = bookingBO.PK;
			vehicleMovementBooking0.GBV_VehicleRegistration = "Vehicle 0";
			vehicleMovementBooking0.GBV_RC_VehicleType = vehicleType1.PK;

			//Act
			var dataObject = new GteBookingDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingBO))).GetDataObject(bookingBO);

			//Assert
			AssertEquals("Booking UXML should contain vehicleMovement in PreCarriageShipmentCollection", 1, dataObject.PreCarriageShipmentCollection.Count);

			var vehicleMovement = dataObject.PreCarriageShipmentCollection.First();
			CombineAssertions(() =>
			{
				AssertEquals("Expect Vehicle Registration to be correctly filled", "Vehicle 0", vehicleMovement.VehicleRun?.Vehicle?.Registration?.Number.Value ?? string.Empty);
				AssertEquals("Expect Vehicle Type Code to be correctly filled", "Type 1", vehicleMovement.VehicleRun?.Vehicle?.VehicleType?.Code ?? string.Empty);
				AssertEquals("Expect Vehicle Type Description to be correctly filled", "Existing RefContainer", vehicleMovement.VehicleRun?.Vehicle?.VehicleType?.Description ?? string.Empty);
			});
		}
	}
}
