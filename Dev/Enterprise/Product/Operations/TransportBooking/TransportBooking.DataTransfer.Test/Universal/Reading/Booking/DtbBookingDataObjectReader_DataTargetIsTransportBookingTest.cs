using System;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	public class DtbBookingDataObjectReader_DataTargetIsTransportBookingTest : OrganizationAddressTestHelper
	{
		public void TestDataContextType()
		{
			var reader = GetDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), new UniversalObjectFactory());
			AssertEquals(DataContextType.TransportBooking, reader.DataContextType);
		}

		public void TestPopulateBusinessObject()
		{
			var shipmentForTransport = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();

			var reader = (ITopLevelDataObjectReader)GetDataObjectReader(shipmentForTransport, Logger, Factory);
			var transportRead = reader.ReadIntoTopLevelBusinessObject();
			AssertNotNull(transportRead);
			AssertNotNull(((DtbBooking)transportRead).ConsolidationSingleJob);
		}

		public void TestReadIntoBusinessObjectThrows()
		{
			var shipmentForTransport = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();

			var reader = GetDataObjectReader(shipmentForTransport, Logger, Factory);
			AssertExceptionThrown<InvalidOperationException>("This method should always throw", "This reader should never be used to populate a business object, it should only be used to redirect to the consolidation reader.", () => reader.ReadIntoBusinessObject());
		}

		public void TestCO2eImport()
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
			var reader = (ITopLevelDataObjectReader)GetDataObjectReader(dataObject, Logger, Factory);
			var bookingReadIn = (DtbBooking)reader.ReadIntoTopLevelBusinessObject();

			// Assert
			AssertEquals(10000m, bookingReadIn.GetTotalCO2e());
			AssertEquals("CUR", bookingReadIn.GetCO2eStatus());
			AssertEquals(1500m, bookingReadIn.GetCO2eDistanceInKM());
			CO2eTestHelper.AssertGHGEvent(bookingReadIn.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
		}

		public void TestCO2eImport_NoMatch()
		{
			// Arrange
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = "TB0000099";
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, "TB0000012");

			// Act & Assert
			var reader = (ITopLevelDataObjectReader)GetDataObjectReader(dataObject, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Match couldn't be found for TransportBooking with Key TB000012", () => reader.ReadIntoTopLevelBusinessObject());
		}

		public void TestCO2eImport_RejectedWhenStatusIsNotCurrent()
		{
			// Arrange
			const string key = "TB0000012";
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = key;
			booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, key);

			// Act
			var reader = (ITopLevelDataObjectReader)GetDataObjectReader(dataObject, Logger, Factory);
			var bookingReadIn = (DtbBooking)reader.ReadIntoTopLevelBusinessObject();

			// Assert
			AssertEquals(0m, bookingReadIn.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, bookingReadIn.GetCO2eStatus());
			AssertEquals(0m, bookingReadIn.GetCO2eDistanceInKM());
			AssertContains("Cannot populate CO2e for DtbBooking because CO2e Calculation input parameters have been changed", Logger.GetWarnings());
			CO2eTestHelper.AssertGHGEvent(bookingReadIn.Logs, "|RES=Input value(s) have changed|TYP=Rejected");
		}

		public void TestCO2eImport_RejectedWhenGHGTagMissing()
		{
			// Arrange
			const string key = "TB0000012";
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = key;
			Factory.SaveForTesting();

			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, key);
			dataObject.GreenhouseGasEmission = null;

			// Act
			var reader = (ITopLevelDataObjectReader)GetDataObjectReader(dataObject, Logger, Factory);
			var bookingReadIn = (DtbBooking)reader.ReadIntoTopLevelBusinessObject();

			// Assert
			AssertEquals(0m, bookingReadIn.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Rejected, bookingReadIn.GetCO2eStatus());
			AssertEquals(0m, bookingReadIn.GetCO2eDistanceInKM());
			AssertContains("Missing CO2e value in the response.", Logger.GetWarnings());
			CO2eTestHelper.AssertGHGEvent(bookingReadIn.Logs, "|RES=Missing CO2e value in the response.|TYP=Rejected");
		}

		DtbBookingDataObjectReader_DataTargetIsTransportBooking GetDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new DtbBookingDataObjectReader_DataTargetIsTransportBooking(shipment, logger, factory);
		}
	}
}
