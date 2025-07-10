using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class WhsTransferExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestCheckTransferIsMasterTransfer

		public void TestCheckTransferIsMasterTransfer()
		{
			var response1 = new WebServiceResponse();
			AssertExceptionThrown<ArgumentNullException>(() => ((WhsTransfer)null).CheckTransferIsMasterTransfer(response1));

			var masterTransfer = Factory.New<WhsTransfer>();
			var childTransfer = Factory.New<WhsTransfer>();
			childTransfer.WD_WD_ParentDocket = masterTransfer.PK;
			childTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			var response2 = new WebServiceResponse();
			childTransfer.CheckTransferIsMasterTransfer(response2);
			AssertEquals("Cannot transfer an Inter-Warehouse Transfer using the child job.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals(false, response2.NoError());

			var response3 = new WebServiceResponse();
			masterTransfer.CheckTransferIsMasterTransfer(response3);
			AssertEquals(true, response3.NoError());
		}

		#endregion
	}
}
