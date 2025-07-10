using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPermitWithdrawalRequestDetailTest : WhsTestCaseWithFactory
	{
		#region TestWhsPermitWithdrawalRequestDetail_ArgumentIsNull

		public void TestWhsPermitWithdrawalRequestDetail_ArgumentIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: orderLine",
				() => new WhsPermitWithdrawalRequestDetail(null, 1));

			var orderLine = Factory.New<WhsOrderLine>();
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: Order",
				() => new WhsPermitWithdrawalRequestDetail(orderLine, 1));
		}

		#endregion

		#region TestGetPermitTransactionRefNumber

		public void TestGetPermitTransactionRefNumber()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: orderLine",
				() => WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(null));

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketID = "O1";

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.WE_LineNo = 5;
			AssertEquals("Permit Number should be DocketID + Line No.", "O1_5",
				WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine));

			orderLine.WE_WD = ZGuid.Empty;
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: Order",
				() => WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine));
		}

		#endregion

		#region IPermitWithdrawalRequestDetail Members

		public void TestIPermitWithdrawalRequestDetail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m, 8, 6);
			orderLine.CustomsData.WB_CustomsQty = 2m;
			orderLine.CustomsData.WB_BondedWhsQty = 5m;

			var permitWithdrawalRequestDetail = new WhsPermitWithdrawalRequestDetail(orderLine, 7m);
			AssertEquals("IPermitWithdrawalRequestDetail.Qty", 7m, permitWithdrawalRequestDetail.Qty);
			AssertEquals("IPermitWithdrawalRequestDetail.ReceiveTotalQty", 5m,
				permitWithdrawalRequestDetail.ReceiveTotalQty);
			AssertEquals("IPermitWithdrawalRequestDetail.ReceiveTotalCustomsValue", 2m,
				permitWithdrawalRequestDetail.ReceiveTotalCustomsValue);
			AssertEquals("IPermitWithdrawalRequestDetail.PermitTransactionRefNumber", order.WD_DocketID + "_8",
				permitWithdrawalRequestDetail.PermitTransactionRefNumber);
			AssertEquals("IPermitWithdrawalRequestDetail.Warehouse", whs.WarehouseAddress,
				permitWithdrawalRequestDetail.Warehouse);
			AssertEquals("IPermitWithdrawalRequestDetail.PermitType FTZ", PermitType.FTZ,
				permitWithdrawalRequestDetail.PermitType);
		}

		#endregion
	}
}
