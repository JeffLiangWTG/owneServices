using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(FinaliseReceivesActionMethodApplicator))]
	public class FinaliseReceivesActionMethodApplicatorTest : FinaliseDocketActionMethodApplicatorTest<WhsReceive, FinaliseReceivesActionMethodApplicator>
	{
		#region TestApplicator_CannotSaveError

		public void TestApplicator_CannotSaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receiveLine.WE_TransactionQuantity = 5m;
			Factory.Save();

			var applicator = new FinaliseReceivesActionMethodApplicator(Factory);
			applicator.UserModifyDataInDBAction += (f) =>
			{
				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var helper = new WhsTestHelperFunctions(otherFactory);
				var receiveLineInOtherFactory = otherFactory.Load<WhsReceiveLine>(receiveLine.PK);
				var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
				helper.CreateReservePickLine(orderLine, receiveLineInOtherFactory.Inventory[0], 10m);
				AssertEquals("Precondition: reserved quantity for the line should be 10.", 10m, orderLine.ReservedPickLines.Sum(l => l.WZ_Units));
				otherFactory.Save();
			};

			var log = new DummyOperationalActionSectionLog();
			AssertNoExceptionThrown("Should not throw exception", () => applicator.Apply(log, new[] { receive }));

			AssertEquals($@"WARNING: Receive [HL W00000001] - Another user has modified this Receive and prevented finalization from succeeding.
Review reservations on the Receive before reattempting to finalize.",
				log.MessagesString().Trim());
		}

		#endregion

		#region TestApplicator_CannotFinaliseWhenCreatedFromPickByBOM

		public void TestApplicator_CannotFinaliseWhenCreatedFromPickByBOM()
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
			var applicator = new FinaliseReceivesActionMethodApplicator(Factory);
			var log = new DummyOperationalActionSectionLog();
			AssertNoExceptionThrown("Should not throw exception", () => applicator.Apply(log, new[] { createdReceive }));

			AssertEquals("WARNING: Receive [HL W00000003] - You cannot finalize receives created from Pick Orders.",
				log.MessagesString().Trim());
		}

		#endregion

		#region Implementation

		protected override WhsReceive GetNewDocket(WhsTestHelperFunctions helper, OrgHeader client, WhsWarehouse warehouse, string reference = "1")
		{
			return helper.CreateWhsReceive(client, warehouse, reference);
		}

		protected override WhsDocketLine GetNewDocketLine(WhsTestHelperFunctions helper, WhsReceive docket, OrgSupplierPart part, ZDecimal quantity)
		{
			return helper.CreateWhsReceiveLine(docket, part, quantity, docket.Warehouse.DefaultLocation);
		}

		#endregion
	}
}
