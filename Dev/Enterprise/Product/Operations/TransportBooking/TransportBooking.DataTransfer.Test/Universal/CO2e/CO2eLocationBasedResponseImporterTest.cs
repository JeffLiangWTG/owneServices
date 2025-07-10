using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	sealed class CO2eLocationBasedResponseImporterTest : TestCaseWithFactory
	{
		public void TestImportGreenHouseGasEmission_ValidData()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();

			// Act
			var importer = new CO2eLocationBasedResponseImporter(Factory);
			var result = importer.ImportGreenHouseGasEmission(dataObject, booking, booking.HumanReadableName);

			// Assert
			Assert(result);
			AssertEquals(CO2eStatusList.Codes.Current, booking.GetCO2eStatus());
			AssertEquals(10000m, booking.GetTotalCO2e());
			AssertEquals(1500m, booking.GetCO2eDistanceInKM());
			CO2eTestHelper.AssertGHGEvent(booking.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
		}

		public void TestImportGreenHouseGasEmission_LogsOldCO2eValue()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			var previousCO2eValue = (TotalCO2e: 2000m, Transports: Enumerable.Empty<BusinessObject>());

			// Act
			var importer = new CO2eLocationBasedResponseImporter(Factory);
			var result = importer.ImportGreenHouseGasEmission(dataObject, booking, booking.HumanReadableName, previousCO2eValue);

			// Assert
			Assert(result);
			AssertEquals(CO2eStatusList.Codes.Current, booking.GetCO2eStatus());
			AssertEquals(10000m, booking.GetTotalCO2e());
			AssertEquals(1500m, booking.GetCO2eDistanceInKM());
			CO2eTestHelper.AssertGHGEvent(booking.Logs, "|NEW=10000|OLD=2000|TYP=Updated");
		}

		public void TestImportGreenHouseGasEmission_InvalidCO2eValues()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.GreenhouseGasEmission.CO2e = 10000000000000000000m;

			// Act
			var importer = new CO2eLocationBasedResponseImporter(Factory);
			var result = importer.ImportGreenHouseGasEmission(dataObject, booking, booking.HumanReadableName);

			// Assert
			Assert(!result);
			AssertEquals(0m, booking.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Rejected, booking.GetCO2eStatus());
			AssertContains("Warning - Total CO2e 10000000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded", importer.LogMessages);
			CO2eTestHelper.AssertGHGEvent(booking.Logs, "|RES=Total CO2e 10000000000000000000 is greater than maximum limit 99999999999999.9999999, it will be discarded|TYP=Rejected");
		}

		public void TestImportGreenHouseGasEmission_NotApplicable()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();

			// Act
			var importer = new CO2eLocationBasedResponseImporter(Factory, new XmlSessionTracker(new SimpleLogger()));
			var result = importer.ImportGreenHouseGasEmission(dataObject, booking, booking.HumanReadableName);

			// Assert
			Assert(!result);
			AssertEquals(0m, booking.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, booking.GetCO2eStatus());
			AssertContains("Warning - Cannot populate CO2e for Transport Booking TB00009231 because CO2e Calculation input parameters have been changed", importer.LogMessages);
			CO2eTestHelper.AssertGHGEvent(booking.Logs, "|RES=Input value(s) have changed|TYP=Rejected");
		}

		public void TestImportGreenHouseGasEmission_SkipApplicableCheck()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			booking.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();

			// Act
			var importer = new CO2eLocationBasedResponseImporter(Factory);
			var result = importer.ImportGreenHouseGasEmission(dataObject, booking, booking.HumanReadableName);

			// Assert
			Assert(result);
			AssertEquals(CO2eStatusList.Codes.Current, booking.GetCO2eStatus());
			AssertEquals(10000m, booking.GetTotalCO2e());
			AssertEquals(1500m, booking.GetCO2eDistanceInKM());
			CO2eTestHelper.AssertGHGEvent(booking.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
		}

		public void TestImportGreenHouseGasEmission_MissingGHGTag()
		{
			// Arrange
			var booking = CO2eTestHelper.CreateBasicBooking(Factory);
			var dataObject = CO2eTestHelper.CreateSampleCO2eResponse();
			dataObject.GreenhouseGasEmission = null;

			// Act
			var importer = new CO2eLocationBasedResponseImporter(Factory);
			var result = importer.ImportGreenHouseGasEmission(dataObject, booking, booking.HumanReadableName);

			// Assert
			Assert(!result);
			AssertEquals(0m, booking.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Rejected, booking.GetCO2eStatus());
			AssertContains("Warning - Missing CO2e value in the response.", importer.LogMessages);
			CO2eTestHelper.AssertGHGEvent(booking.Logs, "|RES=Missing CO2e value in the response.|TYP=Rejected");
		}
	}
}
