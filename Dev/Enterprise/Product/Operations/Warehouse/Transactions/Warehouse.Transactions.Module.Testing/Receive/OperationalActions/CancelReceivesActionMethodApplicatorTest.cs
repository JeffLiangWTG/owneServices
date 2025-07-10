using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CancelReceivesActionMethodApplicator))]
	class CancelReceivesActionMethodApplicatorTest : CancelDocketsActionMethodApplicatorTest<CancelReceivesActionMethodApplicator, WhsReceive>
	{
		#region TestCancelDocket

		protected override string GetExpectedMessage()
		{
			return string.Format(@"INFO: Warehouse Receipt W1 [HL W1] - is canceled successfully.
WARNING: Warehouse Receipt W2 [HL W2] - You can only cancel dockets with Entered (Saved) status
WARNING: Warehouse Receipt W3 [HL W3] - You can only cancel dockets with Entered (Saved) status
WARNING: Warehouse Receipt W4 [HL W4] - You can only cancel dockets with Entered (Saved) status
WARNING: Warehouse Receipt W5 [HL W5] - Warehouse Receipt W5 cannot be deactivated.
Accounting Transaction(s) have been saved against this Invoicing Job Header (JobNum01) in the company EDI.
");
		}

		#endregion

		#region TestApplicator_CannotCancelWhenCreatedFromPickByBOM

		public void TestApplicator_CannotCancelWhenCreatedFromPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			Factory.Save();

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));

			ApplyApplicator(new WhsDocket[] { createdReceive }, "WARNING: Warehouse Receipt W00000003 [HL W00000003] - You cannot cancel receives created from Pick Orders.");
		}

		#endregion

		protected override WhsDocketLine GetNewDocketLine(WhsReceive docket, OrgSupplierPart part, ZDecimal quantity)
		{
			return Helper.CreateWhsReceiveLine(docket, part, quantity);
		}

		protected override void AddCustomFetchHint(Dictionary<string, int> expectedDbHits)
		{
			expectedDbHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
		}
	}
}
