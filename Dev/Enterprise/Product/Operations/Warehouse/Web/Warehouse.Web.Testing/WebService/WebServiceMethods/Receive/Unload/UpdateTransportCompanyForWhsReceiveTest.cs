using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UpdateTransportCompanyForWhsReceiveTest : WhsSecureServiceTestCase
	{
		public void TestUpdate()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_IsShippingProvider = true;
			Helper.Factory.Save();
			AssertNull(receive.TransportCo);

			// Act
			var webService = GetNewWebService();
			var response = webService.UpdateTransportCompanyForWhsReceive(receive.PK.ToGuid(), data.Org1.OH_Code);

			// Assert
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);

			receive = Helper.Factory.Load<WhsReceive>(receive.PK);
			AssertNotNull(receive.TransportCo);
			AssertEquals("TransportCoPK as expected", data.Org1.PK, receive.TransportCoPK);
		}

		public void TestUpdate_RemoveTransportCompany()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_IsShippingProvider = true;
			receive.TransportCoPK = data.Org1.PK;
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService();
			var response = webService.UpdateTransportCompanyForWhsReceive(receive.PK.ToGuid(), "");

			// Assert
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);

			receive = Helper.Factory.Load<WhsReceive>(receive.PK);
			AssertNull(receive.TransportCo);
		}

		public void TestUpdate_Error_ReceiveNotFound()
		{
			var webService = GetNewWebService();
			var response = webService.UpdateTransportCompanyForWhsReceive(new System.Guid(), "");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Receive could not be found. Please restart the Unload and try again.", response.ErrorMessage);
		}

		public void TestUpdate_Error_TransportCompanyNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.UpdateTransportCompanyForWhsReceive(receive.PK.ToGuid(), "NotACompany");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Transport company could not be found. Please scan the correct company code.", response.ErrorMessage);
		}

		public void TestUpdate_Error_TransportCompanyNotCarrier()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_IsShippingProvider = false;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.UpdateTransportCompanyForWhsReceive(receive.PK.ToGuid(), data.Org1.OH_Code);

			AssertSuccessfulResponse(response, webService);
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "The company is not a Carrier. Please scan the correct company code.", response.ErrorMessage);
		}
	}
}
