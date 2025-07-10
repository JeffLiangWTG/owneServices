using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	class ReceiveAutoPalletiseProcessorTest : ReceiveActionProcessorTest
	{
		protected override void TestProcessCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "PLT", 5);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 9);

			var processor = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			AssertEquals("Processor should auto palletise the receive line", 2, receive.Lines.Count);
			AssertEquals("9 units should now be 5", 5m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Should have new line of 4 units", 4m, receive.Lines.Single(l => l.PK != receiveLine.PK).WE_TransactionQuantity);
			AssertEquals("Notification buffer should not have any errors or warnings.", NotificationType.Information, notifications.Events.Single().Type);
			AssertEquals($"Operation {ExpectedActionName} for receive {receive.WD_DocketID} completed.", notifications.Events.Single().Message);
		}

		public void TestProcess_AutoPalletiseFailure_HasWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "PLT", 5);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);
			receiveLine.WE_OP = ZGuid.Empty;

			AssertNull("Precondition", receiveLine.SupplierPart);

			var processor = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			AssertEquals("Notification buffer should have warning when no product in receive line.", NotificationType.Warning, notifications.Events.Single().Type);
			AssertEquals($"Unable to {ExpectedActionName} for all receive lines of receive {receive.WD_DocketID} because at least one of its lines does not have a product or the product does not have a pallet definition.", notifications.Events.Single().Message);
		}

		protected override ReceiveActionProcessor GetProcessor(WhsReceive receive)
		{
			return new ReceiveAutoPalletiseProcessor(receive);
		}

		protected override ZString ExpectedActionName => "auto palletize";
	}
}
