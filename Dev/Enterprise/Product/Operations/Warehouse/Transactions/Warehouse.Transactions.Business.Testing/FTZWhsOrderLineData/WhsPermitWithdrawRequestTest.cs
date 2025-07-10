using System;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPermitWithdrawRequestTest : WhsTestCaseWithFactory
	{
		#region TestWhsPermitWithdrawRequest_ArgumentIsNull

		public void TestWhsPermitWithdrawRequest_ArgumentIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: orderLine",
				() => new WhsPermitWithdrawRequest(null));

			var orderLine = Factory.New<WhsOrderLine>();
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: Order",
				() => new WhsPermitWithdrawRequest(orderLine));

			var order = Factory.New<WhsOrder>();
			orderLine.WE_WD = order.PK;
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: Warehouse",
				() => new WhsPermitWithdrawRequest(orderLine));

			order.WD_WW_Whs = Helper.CreateWarehouse("WHS").PK;
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: Client",
				() => new WhsPermitWithdrawRequest(orderLine));
		}

		#endregion

		#region TestIPermitWithdrawRequest

		public void TestIPermitWithdrawRequest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			data.Part1.OP_StockKeepingUnit = "CTN";
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m, 8, 6);
			orderLine.CustomsData.WB_Tariff = "AAA";
			orderLine.CustomsData.WB_AddInfo = "AddInfo1";
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = "US";
			var manufacturer = Helper.CreateClient("MAN", "Manufacturer");
			orderLine.CustomsData.WB_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var whsPermitWithdrawRequest = new WhsPermitWithdrawRequest(orderLine);
			AssertEquals("WhsPermitWithdrawRequest should have wrapped the OrderLine passed in.", "P1",
				whsPermitWithdrawRequest.ProductCode);
			AssertEquals("WhsPermitWithdrawRequest should have passed Ordered Quantity in.", 5m,
				whsPermitWithdrawRequest.Qty);
			AssertEquals("WhsPermitWithdrawRequest should have wrapped the OrderLine passed in.",
				order.WD_DocketID + "_8", whsPermitWithdrawRequest.PermitTransactionRefNumber);
			AssertEquals("WhsPermitWithdrawRequest.Owner", data.Org1.MainAddress, whsPermitWithdrawRequest.Owner);
			AssertEquals("WhsPermitWithdrawRequest.Manufacturer", manufacturer.MainAddress,
				whsPermitWithdrawRequest.Manufacturer);
			AssertEquals("WhsPermitWithdrawRequest.Warehouse", whs.WarehouseAddress,
				whsPermitWithdrawRequest.Warehouse);
			AssertEquals("WhsPermitWithdrawRequest.DetailedTrackingEnabled", true,
				whsPermitWithdrawRequest.DetailedTrackingEnabled);
			AssertEquals("WhsPermitWithdrawRequest.PermitType FTZ",
				Enterprise.MasterFiles.Integration.Customs.PermitService.PermitType.FTZ,
				whsPermitWithdrawRequest.PermitType);
			AssertEquals("WhsPermitWithdrawRequest.Tariff", "AAA", whsPermitWithdrawRequest.Tariff);
			AssertEquals("WhsPermitWithdrawRequest.CountryOfOrigin", "US", whsPermitWithdrawRequest.CountryOfOrigin);
			AssertEquals("WhsPermitWithdrawRequest.ProductCode", "P1", whsPermitWithdrawRequest.ProductCode);
			AssertEquals("WhsPermitWithdrawRequest.UQ", "CTN", whsPermitWithdrawRequest.UQ);
		}

		#endregion
	}
}
