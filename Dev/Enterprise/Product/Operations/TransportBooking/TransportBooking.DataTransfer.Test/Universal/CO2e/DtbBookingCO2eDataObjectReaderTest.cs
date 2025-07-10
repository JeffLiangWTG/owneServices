using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace EEnterprise.TransportBookings.DataTransfer.Universal.Testing
{
	sealed class DtbBookingCO2eDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestPopulateBusinessObject()
		{
			// Arrange
			const string key = "TB0000012";
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = key;
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, key);

			// Act
			var reader = GetDataObjectReader(dataObject, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();

			// Assert
			AssertEquals(10000m, bookingReadIn.GetTotalCO2e());
			AssertEquals("CUR", bookingReadIn.GetCO2eStatus());
			AssertEquals(1500m, bookingReadIn.GetCO2eDistanceInKM());
			CO2eTestHelper.AssertGHGEvent(bookingReadIn.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
		}

		public void TestPopulateBusinessObject_LogsOldCO2eValue()
		{
			// Arrange
			const string key = "TB0000012";
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = key;
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, key);

			// Act
			var reader = GetDataObjectReader(dataObject, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();

			// Assert
			AssertEquals(10000m, bookingReadIn.GetTotalCO2e());
			AssertEquals("CUR", bookingReadIn.GetCO2eStatus());
			CO2eTestHelper.AssertGHGEvent(bookingReadIn.Logs, "|NEW=10000|OLD=NA|TYP=Updated");

			dataObject.GreenhouseGasEmission.CO2e = 30000m;
			Factory.SaveForTesting();
			reader = GetDataObjectReader(dataObject, booking);
			bookingReadIn = reader.ReadIntoBusinessObject();

			AssertEquals(30000m, bookingReadIn.GetTotalCO2e());
			AssertEquals("CUR", bookingReadIn.GetCO2eStatus());
			CO2eTestHelper.AssertGHGEvent(bookingReadIn.Logs, "|NEW=30000|OLD=10000|TYP=Updated", 2);
		}

		public void TestPopulateBusinessObject_BookingIsNull()
		{
			// Arrange
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, "TB000012");

			// Act & Assert
			var reader = GetDataObjectReader(dataObject, null);
			AssertExceptionThrown<DataObjectReadFailureException>("Match couldn't be found for TransportBooking with Key TB000012", () => reader.ReadIntoBusinessObject());
		}

		DtbBookingCO2eDataObjectReader GetDataObjectReader(Shipment shipment, DtbBooking booking)
		{
			return new DtbBookingCO2eDataObjectReader(shipment, Logger, Factory, booking);
		}
	}
}
