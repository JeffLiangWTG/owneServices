using System.Linq;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Stocktake_GetStocktakeLinesForLocationTest : WhsStocktakeSecureServiceTestCase
	{
		#region TestStocktake_GetStocktakeLinesForLocation

		public void TestStocktake_GetStocktakeLinesForLocation()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var area = "ANY";
			var pickMethod = "ANY";
			var stocktakeNumber = TestData.Stocktake1.WS_StocktakeNumber;

			var webService = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			var response = webService.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber, area, pickMethod);
			AssertEquals("Precondition: Added SVC log on loading stocktake.", 1, Helper.FindLogs(TestData.Stocktake1.Logs, Events.ServiceCommenced).Length);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Stocktake", response.Stocktake);
			AssertEquals("Stocktake number", stocktakeNumber, response.Stocktake.Number);
			AssertEquals("LocationsToCount Count", 6, response.LocationsToCount.Count);

			foreach (var location in response.LocationsToCount)
			{
				webService = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
				response = webService.Stocktake_GetStocktakeLinesForLocation(stocktakeNumber, area, pickMethod, location);
				AssertSuccessfulResponse(response, webService);
				AssertEquals("LinesToCount Count", 1, response.LinesToCount.Count);
				AssertEquals("Lines Location", location, response.LinesToCount.Single().LocationString);
				AssertEquals("Should not have added another service commenced log when loading lines.", 1, Helper.FindLogs(TestData.Stocktake1.Logs, Events.ServiceCommenced).Length);
			}
		}

		public void TestStocktake_GetStocktakeLinesForLocation_NullReference()
		{
			SetupEnvironmentDataForStocktake();

			var webService = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			AssertBusinessValidationError(webService, "Should not accept null reference", "reference cannot be null.", webService.Stocktake_GetStocktakeLinesForLocation(null, "ANY", "ANY", "L1"));
		}

		#endregion
	}
}
