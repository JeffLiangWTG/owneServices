using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickFinalisationErrorReportingHelperTest : WhsTestCaseWithFactory
	{
		public void TestReportPickFinalisationErrorMessage_IncludeUnfinalisableReason_SecurityRight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();

			Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			pick.WP_PickStatus = PickStatus.Codes.PickSlip;
			Env.Security.WhsReleaseFinalise.IsAllowed = false;

			var errorReportingHelper = new WhsPickFinalisationErrorReportingHelper(pick);
			var expectedErrorMessage = @"
Failed to Finalize Pick.
Login user is not allowed to finalize Warehouse Pick. Please check user's security rights settings for Warehouse Release Finalize
";
			AssertEquals(expectedErrorMessage.Trim(), errorReportingHelper.ReportPickFinalisationErrorMessage());
		}

		public void TestReportPickFinalisationErrorMessage_IncludeUnfinalisableReason_ErrorMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();

			Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			pick.WP_PickStatus = PickStatus.Codes.PickSlip;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m);
			pick.Orders.Add(order2);

			var errorReportingHelper = new WhsPickFinalisationErrorReportingHelper(pick);
			var expectedErrorMessage = @"
Failed to Finalize Pick.
Error Message: Please finalize all the orders on the pick before finalizing the pick.
";
			AssertEquals(expectedErrorMessage.Trim(), errorReportingHelper.ReportPickFinalisationErrorMessage());
		}

		public void TestReportPickFinalisationErrorMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			pick.FinaliseOrder(order);
			pick.FinalisePick();

			Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			pick.WP_PickStatus = PickStatus.Codes.PickSlip;
			var orderLine = order.Lines[0];
			orderLine.AddRowError("Something is wrong, that's the reason why pick is not finalised.");

			var errorReportingHelper = new WhsPickFinalisationErrorReportingHelper(pick);
			var expectedErrorMessage = @"
Failed to Finalize Pick.
Order Line Product: P1
Error - Docket Line: Something is wrong, that's the reason why pick is not finalised.
";
			AssertEquals(expectedErrorMessage.Trim(), errorReportingHelper.ReportPickFinalisationErrorMessage());
		}
	}
}
