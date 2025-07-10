using System;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetTransportCompanyFromRMALinkedOrderTest : WhsSecureServiceTestCase
	{
		public void TestGetTransportCompany_ReceiveNotFound()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Receive could not be found. Please restart the operation and try again.", webService.GetTransportCompanyFromRMALinkedOrder(new Guid()));
		}

		public void TestGetTransportCompany_LinkedOrderNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.Factory.Save();
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Linked order could not be found. Please restart the operation and try again.", webService.GetTransportCompanyFromRMALinkedOrder(receive.PK.ToGuid()));
		}

		public void TestGetTransportCompany_LinkedOrderHasNoTransportCompanyRecorded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_WD_ParentDocket = order.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Linked order does not have transport company info. Please scan / enter the company code directly.", webService.GetTransportCompanyFromRMALinkedOrder(receive.PK.ToGuid()));
		}

		public void TestGetTransportCompany()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.TransportCoPK = data.Org1.PK;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_WD_ParentDocket = order.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.GetTransportCompanyFromRMALinkedOrder(receive.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals(response.Organisations.Length, 1);
			AssertEquals(data.Org1.OH_Code, response.Organisations[0].Code);
		}
	}
}
