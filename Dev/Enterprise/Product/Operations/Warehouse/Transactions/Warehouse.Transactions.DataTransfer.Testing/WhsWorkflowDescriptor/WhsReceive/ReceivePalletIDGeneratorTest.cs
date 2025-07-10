using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class ReceivePalletIDGeneratorTest : ReceiveActionProcessorTest
	{
		protected override void TestProcessCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20);

			AssertNullOrEmpty("Precondition", receiveLine1.WE_PalletID);
			AssertNullOrEmpty("Precondition", receiveLine2.WE_PalletID);

			var generator = GetProcessor(receive);
			var notifications = new NotificationBuffer();
			generator.Process(notifications);

			AssertNotNullOrEmpty(receiveLine1.WE_PalletID);
			AssertNotNullOrEmpty(receiveLine2.WE_PalletID);
			AssertEquals("Notification buffer should not have any errors or warnings.", NotificationType.Information, notifications.Events.Single().Type);
			AssertEquals($"Operation {ExpectedActionName} for receive {receive.WD_DocketID} completed.", notifications.Events.Single().Message);
		}
		protected override ReceiveActionProcessor GetProcessor(WhsReceive receive)
		{
			return new ReceivePalletIDGenerator(receive);
		}

		protected override ZString ExpectedActionName => "generate pallet IDs";
	}
}
