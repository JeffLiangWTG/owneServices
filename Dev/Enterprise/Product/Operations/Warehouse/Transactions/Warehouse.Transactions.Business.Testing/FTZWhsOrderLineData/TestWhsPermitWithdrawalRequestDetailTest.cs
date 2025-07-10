using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Customs.US.Business.Testing
{
	public class TestWhsPermitWithdrawalRequestDetailTest : TestCaseWithFactory
	{
		public void TestWhsPermitWithdrawalRequestDetail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var whs = helper.CreateFTZWarehouseInUS();
			var order = helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m, 8, 6);
			orderLine.CustomsData.WB_CustomsQty = 2m;
			orderLine.CustomsData.WB_BondedWhsQty = 3m;
			var permitWithdrawalRequestDetail = new WhsPermitWithdrawalRequestDetail(orderLine, 7m);
			AssertEquals("IPermitWithdrawalRequestDetail.Qty", 7m, permitWithdrawalRequestDetail.Qty);
			AssertEquals("IPermitWithdrawalRequestDetail.ReceiveTotalQty", 3m,
				permitWithdrawalRequestDetail.ReceiveTotalQty);
		}
	}
}
