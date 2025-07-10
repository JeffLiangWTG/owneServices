using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetCartonSizeForToteByCodeTest : WhsSecureServiceTestCase
	{
		#region TestGetCartonSizeForToteByCode_CartonSizeCodeIsEmpty

		public void TestGetCartonSizeForToteByCode_CartonSizeCodeIsEmpty()
		{
			var expectedErrorMessage = "Please provide a Carton Size Code.";
			var webService = GetNewWebService();
			var response = webService.GetCartonSizeForToteByCode(null);
			AssertNull("CartonSizeInfo should be null", response.CartonSizeInfo);
			AssertEquals("Should Error as no Carton Size Code given.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(expectedErrorMessage, response.ErrorMessage);

			var response2 = webService.GetCartonSizeForToteByCode("");
			AssertNull("CartonSizeInfo should be null", response2.CartonSizeInfo);
			AssertEquals("Should Error as no Carton Size Code given.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals(expectedErrorMessage, response2.ErrorMessage);
		}

		#endregion

		#region TestGetCartonSizeForToteByCode_CartonSizeIsInvalid

		public void TestGetCartonSizeForToteByCode_CartonSizeIsInvalid()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var cartonSize1 = helper.CreateWhsCartonSize("BIG");
			var cartonSize2 = helper.CreateWhsCartonSize("SML");

			var response = webService.GetCartonSizeForToteByCode("AAA");
			AssertNull("CartonSizeInfo should be null", response.CartonSizeInfo);
			AssertEquals("Should Error as no Carton Size not found.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Carton Size 'AAA' cannot be found.", response.ErrorMessage);
		}

		#endregion

		#region TestGetCartonSizeForToteByCode

		public void TestGetCartonSizeForToteByCode()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var cartonSize1 = helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 10m, 10, 80, "CM", "KG");
			var cartonSize2 = helper.CreateWhsCartonSize("SML", 5m, 5m, 5m, 0m, 5m, 5, 50, "CM", "KG");

			var response = webService.GetCartonSizeForToteByCode("SML");
			var cartonSizeInfo = response.CartonSizeInfo;
			AssertNotNull("CartonSizeInfo should not be null", cartonSizeInfo);
			AssertEquals("Should have no error.", ErrorTypes.None, response.Error);

			AssertEquals("SML", cartonSizeInfo.Code);
			AssertEquals(5m, cartonSizeInfo.Length);
			AssertEquals(5m, cartonSizeInfo.Width);
			AssertEquals(5m, cartonSizeInfo.Height);
			AssertEquals("KG", cartonSizeInfo.WeightUQ);
			AssertEquals("CM", cartonSizeInfo.DimensionUQ);
		}

		#endregion
	}
}
