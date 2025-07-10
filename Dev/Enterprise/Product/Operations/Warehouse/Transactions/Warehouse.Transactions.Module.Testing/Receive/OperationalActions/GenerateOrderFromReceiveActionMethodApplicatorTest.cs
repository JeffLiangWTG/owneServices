using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateOrderFromReceiveActionMethodApplicator))]
	public class GenerateOrderFromReceiveActionMethodApplicatorTest : GenerateOrderFromExistingInventoryActionMethodApplicatorTest
	{
		#region TestApplicator_CannotGenerateOrderWhenCreatedFromPickByBOM

		public void TestApplicator_CannotGenerateOrderWhenCreatedFromPickByBOM()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = helper.CreateWhsOrderLine(order, bike, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			Factory.Save();

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var expectedLogText = @"
WARNING: Warehouse Receipt W00000003 [HL W00000003] - You cannot generate Orders for Receives created from Pick Orders.
<-- Summary -->";
			ApplyApplicator(new WhsReceive[] { createdReceive }, expectedLogText);
		}

		#endregion

		#region Implementation

		protected override BusinessObject[] GetInventoryOrReceives()
		{
			return Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
		}

		protected override WhsOperationalActionSupporter GetOperationalActionSupporter()
		{
			return new ReceiveOperationalActionSupporter();
		}

		protected override GenerateOrderFromExistingInventoryActionMethodApplicator GetApplicator(BusinessObjectFactory factory)
		{
			return new GenerateOrderFromReceiveActionMethodApplicator(factory);
		}

		#endregion
	}
}
