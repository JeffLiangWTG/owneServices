using System;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetWhsReceiveASNUnloadStateTest : WhsSecureServiceTestCase
	{
		#region GetWhsASNState

		#region TestGetWhsReceiveASNUnloadState_InvalidReceivePK

		public void TestGetWhsReceiveASNUnloadState_InvalidReceivePK()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Receive record could not be found.", webService.GetWhsReceiveASNUnloadState(Guid.Empty));
		}

		#endregion

		#region TestGetWhsReceiveASNUnloadState_FinalisedReceive

		public void TestGetWhsReceiveASNUnloadState_FinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Receive record has been finalized.", webService.GetWhsReceiveASNUnloadState(receive.PK.ToGuid()));
		}

		#endregion

		#region TestGetWhsReceiveASNUnloadState

		public void TestGetWhsReceiveASNUnloadState()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			// receive has no receive lines and ASN lines, ASNUnloadState: ASNState.NoPallets
			// other conditions tested in TestGetWhsReceives_ASNUnloadState
			var docketResponse = webService.GetWhsReceiveASNUnloadState(receive.PK.ToGuid());
			AssertSuccessfulResponse(docketResponse, webService);

			var docket = docketResponse.Docket;
			AssertNotNull(docketResponse.Docket);
			AssertEquals("Should find out existing receive", receive.PK, docket.PK);
			AssertEquals("ASNState", ASNState.NoPallets, docket.ASNUnloadState);
		}

		#endregion

		#endregion
	}
}
