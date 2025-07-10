using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Stocktake_GetNextUnfinalizedStocktakeTest : WhsStocktakeSecureServiceTestCase
	{
		#region TestStocktake_GetNextUnfinalizedStocktake

		public void TestStocktake_GetNextUnfinalizedStocktake()
		{
			SetupEnvironmentDataForStocktake();
			LoadStocktakes();

			var area = "ANY";
			var pickMethod = "ANY";

			var stocktakeNumber1 = TestData.Stocktake1.WS_StocktakeNumber;
			var webService1 = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			var response1 = webService1.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber1, area, pickMethod);
			AssertStocktakeWebServiceResponse("Stocktake 1", stocktakeNumber1, 6, 1, webService1, response1);
			AssertEquals("Should have added a log.", 1, Helper.FindLogs(TestData.Stocktake1.Logs, Events.ServiceCommenced).Length);

			var stocktakeNumber2 = TestData.Stocktake2.WS_StocktakeNumber;
			var webService2 = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			var response2 = webService2.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber2, area, pickMethod);
			AssertStocktakeWebServiceResponse("Stocktake 2", stocktakeNumber2, 6, 1, webService2, response2);
			AssertEquals("Should have added a log.", 1, Helper.FindLogs(TestData.Stocktake2.Logs, Events.ServiceCommenced).Length);

			var stocktakeNumber3 = TestData.Stocktake3.WS_StocktakeNumber;
			var webService3 = GetNewWebService(TestData.Warehouse2, TestData.Staff1);
			var response3 = webService3.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber3, area, pickMethod);
			AssertStocktakeWebServiceResponse("Stocktake 3", stocktakeNumber3, 1, 3, webService3, response3);
			AssertEquals("Should have added a log.", 1, Helper.FindLogs(TestData.Stocktake3.Logs, Events.ServiceCommenced).Length);

			var stocktakeNumber4 = TestData.Stocktake1.WS_StocktakeNumber;
			var webService4 = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			var response4 = webService4.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber4, area, pickMethod);
			AssertStocktakeWebServiceResponse("Stocktake 1", stocktakeNumber4, 6, 1, webService4, response4);
			AssertEquals("Should have added another log.", 2, Helper.FindLogs(TestData.Stocktake1.Logs, Events.ServiceCommenced).Length);
		}

		public void TestStocktake_GetNextUnfinalizedStocktake_NoLoadedStocktake()
		{
			SetupEnvironmentDataForStocktake();

			var area = "ANY";
			var pickMethod = "ANY";
			var stocktakeNumber = string.Empty;

			var webService1 = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			AssertBusinessValidationError(webService1, "Warehouse 1 should not have any available stocktake", "No stocktake available.", webService1.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber, area, pickMethod));

			var webService2 = GetNewWebService(TestData.Warehouse2, TestData.Staff1);
			AssertBusinessValidationError(webService2, "Warehouse 2 should not have any available stocktake", "No stocktake available.", webService2.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber, area, pickMethod));
		}

		public void TestGetNextUnfinalizedStocktake_UnfoundStocktake()
		{
			SetupEnvironmentDataForStocktake();

			var area = "ANY";
			var pickMethod = "ANY";
			var stocktakeNumber = "xxx";

			var webService = GetNewWebService(TestData.Warehouse1, TestData.Staff1);
			AssertBusinessValidationError(webService, "Warehouse 1 should not have any available stocktake 'xxx'", "Stocktake 'xxx' does not exist or is not available.", webService.Stocktake_GetNextUnfinalizedStocktake(stocktakeNumber, area, pickMethod));
		}
		#endregion
	}
}
