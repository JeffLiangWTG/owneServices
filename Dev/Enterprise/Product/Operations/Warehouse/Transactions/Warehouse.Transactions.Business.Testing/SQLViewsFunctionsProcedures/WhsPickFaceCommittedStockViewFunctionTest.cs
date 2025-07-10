using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickFaceCommittedStockViewFunctionTest : WhsTestCaseWithFactory
	{
		#region TestView_WhsPickFaceCommittedStockView_GeneralColumns

		public void TestView_WhsPickFaceCommittedStockView_GeneralColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			var pickFace3 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location2);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 60m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 30m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 70m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part2, 50m);

			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3, order4, order5);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(5, list.Length);
			var pickFace1Order1 = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == order1.WD_ExternalReference && l.WCP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickFace1Order1.WCP_WP), pick1.PK, pickFace1Order1.WCP_WP);
			AssertEquals(nameof(pickFace1Order1.WCP_QuantityCommitted), 40m, pickFace1Order1.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order1.WCP_PickPriority), (byte)0, pickFace1Order1.WCP_PickPriority);

			var pickFace1Order2 = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == order2.WD_ExternalReference && l.WCP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickFace1Order2.WCP_WP), pick1.PK, pickFace1Order2.WCP_WP);
			AssertEquals(nameof(pickFace1Order2.WCP_QuantityCommitted), 60m, pickFace1Order2.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order2.WCP_PickPriority), (byte)0, pickFace1Order2.WCP_PickPriority);

			var pickFace2Order3 = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == order3.WD_ExternalReference && l.WCP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickFace2Order3.WCP_WP), pick2.PK, pickFace2Order3.WCP_WP);
			AssertEquals(nameof(pickFace2Order3.WCP_QuantityCommitted), 30m, pickFace2Order3.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order3.WCP_PickPriority), (byte)0, pickFace2Order3.WCP_PickPriority);

			var pickFace2Order4 = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == order4.WD_ExternalReference && l.WCP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickFace2Order4.WCP_WP), pick2.PK, pickFace2Order4.WCP_WP);
			AssertEquals(nameof(pickFace2Order4.WCP_QuantityCommitted), 70m, pickFace2Order4.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order4.WCP_PickPriority), (byte)0, pickFace2Order4.WCP_PickPriority);

			var pickFace3Order5 = list.Single(l => l.WCP_WF_PickFace == pickFace3.PK && l.WCP_OrderNo == order5.WD_ExternalReference && l.WCP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickFace3Order5.WCP_WP), pick2.PK, pickFace3Order5.WCP_WP);
			AssertEquals(nameof(pickFace3Order5.WCP_QuantityCommitted), 50m, pickFace3Order5.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace3Order5.WCP_PickPriority), (byte)0, pickFace3Order5.WCP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_NoCommittedQuantity

		public void TestView_WhsPickFaceCommittedStockView_NoCommittedQuantity_NoOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 50m);
			var pick1 = Helper.CreatePickNew(order1);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(1, list.Length);
			AssertEquals("There are no orders/picks refering pickFace1.", 0, list.Count(l => l.WCP_WF_PickFace == pickFace1.PK));
			AssertEquals("There is 1 orders refering pickFace2.", 1, list.Count(l => l.WCP_WF_PickFace == pickFace2.PK));

			var pickFace2Order1 = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK);
			AssertEquals(nameof(pickFace2Order1.WCP_PickNo), pick1.WP_PickNo, pickFace2Order1.WCP_PickNo);
			AssertEquals(nameof(pickFace2Order1.WCP_OrderNo), order1.WD_ExternalReference, pickFace2Order1.WCP_OrderNo);
			AssertEquals(nameof(pickFace2Order1.WCP_QuantityCommitted), 50m, pickFace2Order1.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order1.WCP_PickPriority), (byte)0, pickFace2Order1.WCP_PickPriority);
		}

		public void TestView_WhsPickFaceCommittedStockView_NoCommittedQuantity_UnpickedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			Factory.Save();

			AssertNull("Order has no pick.", order.Pick);
			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());
			AssertEquals(0, list.Length);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_ReservedStocks

		public void TestView_WhsPickFaceCommittedStockView_ReservedStocks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			var inventory1 = Factory.Load<WhsInventoryView>(receiveLine1.PK);
			order1.Lines[0].ReserveStockIfAbleTo(inventory1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 60m);
			var inventory2 = Factory.Load<WhsInventoryView>(receiveLine2.PK);
			order2.Lines[0].ReserveStockIfAbleTo(inventory2);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(2, list.Length);
			var pickFace1Order1 = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == "O1");
			AssertEquals("No pick.", string.Empty, pickFace1Order1.WCP_PickNo);
			AssertEquals(nameof(pickFace1Order1.WCP_QuantityCommitted), 40m, pickFace1Order1.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order1.WCP_PickPriority), ZByte.Zero, pickFace1Order1.WCP_PickPriority);

			var pickFace2Order2 = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == "O2");
			AssertEquals("No pick.", string.Empty, pickFace2Order2.WCP_PickNo);
			AssertEquals(nameof(pickFace2Order2.WCP_QuantityCommitted), 60m, pickFace2Order2.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order2.WCP_PickPriority), ZByte.Zero, pickFace2Order2.WCP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_PickPriority_NoPick

		public void TestView_WhsPickFaceCommittedStockView_PickPriority_NoPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			order1.WD_PickPriority = 5;

			var inventory1 = Factory.Load<WhsInventoryView>(receiveLine1.PK);
			order1.Lines[0].ReserveStockIfAbleTo(inventory1);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(1, list.Length);
			var pickFace1Order1 = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == order1.WD_ExternalReference);
			AssertEquals(nameof(pickFace1Order1.WCP_PickNo), string.Empty, pickFace1Order1.WCP_PickNo);
			AssertEquals(nameof(pickFace1Order1.WCP_QuantityCommitted), 50m, pickFace1Order1.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order1.WCP_PickPriority), (byte)5, pickFace1Order1.WCP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_PickPriority_HasPick

		public void TestView_WhsPickFaceCommittedStockView_PickPriority_HasPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			var location3 = data.Whs1.FindLocation("A-3");			
			var pickFace3 = Helper.CreateProductPickFace(data.Part2, data.Org1, location1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location2);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			order1.WD_PickPriority = 2;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 60m);
			order2.WD_PickPriority = 3;
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 30m);
			order3.WD_PickPriority = 4;
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 70m);
			order4.WD_PickPriority = 0;
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part2, 20m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part2, 80m);

			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3, order4);
			var pick3 = Helper.CreatePickNew(order5, order6);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(6, list.Length);

			var pickFace1Order1 = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == order1.WD_ExternalReference && l.WCP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickFace1Order1.WCP_QuantityCommitted), 40m, pickFace1Order1.WCP_QuantityCommitted);
			AssertEquals("Min of WD_PickPriority between order1/order2 is 2.", (byte)2, pickFace1Order1.WCP_PickPriority);

			var pickFace1Order2 = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == order2.WD_ExternalReference && l.WCP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickFace1Order2.WCP_QuantityCommitted), 60m, pickFace1Order2.WCP_QuantityCommitted);
			AssertEquals("Min of WD_PickPriority between order1/order2 is 2.", (byte)2, pickFace1Order2.WCP_PickPriority);

			var pickFace2Order3 = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == order3.WD_ExternalReference && l.WCP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickFace2Order3.WCP_QuantityCommitted), 30m, pickFace2Order3.WCP_QuantityCommitted);
			AssertEquals("Min of WD_PickPriority among order3/order4 is 4.", (byte)4, pickFace2Order3.WCP_PickPriority);

			var pickFace2Order4 = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == order4.WD_ExternalReference && l.WCP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickFace2Order4.WCP_QuantityCommitted), 70m, pickFace2Order4.WCP_QuantityCommitted);
			AssertEquals("Min of WD_PickPriority among order3/order4 is 4.", (byte)4, pickFace2Order4.WCP_PickPriority);

			var pickFace3Order5 = list.Single(l => l.WCP_WF_PickFace == pickFace3.PK && l.WCP_OrderNo == order5.WD_ExternalReference && l.WCP_PickNo == pick3.WP_PickNo);
			AssertEquals(nameof(pickFace3Order5.WCP_QuantityCommitted), 20m, pickFace3Order5.WCP_QuantityCommitted);
			AssertEquals("WD_PickPriority of order5/order6 haven't been set, so it's 0.", (byte)0, pickFace3Order5.WCP_PickPriority);

			var pickFace3Order6 = list.Single(l => l.WCP_WF_PickFace == pickFace3.PK && l.WCP_OrderNo == order6.WD_ExternalReference && l.WCP_PickNo == pick3.WP_PickNo);
			AssertEquals(nameof(pickFace3Order6.WCP_QuantityCommitted), 80m, pickFace3Order6.WCP_QuantityCommitted);
			AssertEquals("WD_PickPriority of order5/order6 haven't been set, so it's 0.", (byte)0, pickFace3Order6.WCP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_NotPickingFromPickFace

		public void TestView_WhsPickFaceCommittedStockView_NotPickingFromPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());
			AssertEquals("Orders not picking from a pick face should not be included.", 0, list.Length);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_WhenThereAreOutboundDockDoorTransfers

		public void TestView_WhsPickFaceCommittedStockView_WhenThereAreOutboundDockDoorTransfers()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staff = Helper.CreateGlbStaff("ABC", "ABC");
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var location1 = data.Whs1.FindLocation("A-1");
				var location2 = data.Whs1.FindLocation("A-2");
				var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());
				AssertEquals(1, list.Length);

				var pickFace1Order1 = list.Single(l => l.WCP_WF_PickFace == pickFace.PK && l.WCP_OrderNo == order.WD_ExternalReference);
				AssertEquals(nameof(pickFace1Order1.WCP_PickNo), pick.WP_PickNo, pickFace1Order1.WCP_PickNo);
				AssertEquals(nameof(pickFace1Order1.WCP_QuantityCommitted), 100m, pickFace1Order1.WCP_QuantityCommitted);
				AssertEquals(nameof(pickFace1Order1.WCP_PickPriority), (byte)0, pickFace1Order1.WCP_PickPriority);

				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
				Factory.Save();

				list = new BusinessObjectFactory().Load<WhsPickFaceCommittedStockView>(new ZQuery());
				AssertEquals("Stock picked, not committed anymore.", 0, list.Length);
			}
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_NonPickableDocket

		public void TestView_WhsPickFaceCommittedStockView_NonPickableDocket()
		{
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var inventory = Factory.Load<WhsInventoryView>(receiveLine1.PK);
			var transferFromPickFace = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transferFromPickFace, data.Part1, location1, staff.GS_Code);
			Helper.CreateWhsPickLine(transferLine, inventory, 10m);
			Factory.Save();

			AssertEquals("Precondition: 1 Pick Line created.", 1, transferLine.PickLines.Count);
			AssertEquals("Precondition: not picked.", ZDateTimeOffset.Empty, transferLine.PickLines[0].WZ_PickedDateTime);

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());
			AssertEquals("Non pickable docket will not be counted.", 0, list.Length);
		}

		#endregion

		#region TestView_WhsPickFaceCommittedStockView_DifferentClients

		public void TestView_WhsPickFaceCommittedStockView_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var org2 = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, org2, location2);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 100m, location1);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 100m, location2);
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 60m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(2, list.Length);
			var pickFace1Order = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == order1.WD_ExternalReference && l.WCP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickFace1Order.WCP_QuantityCommitted), 40m, pickFace1Order.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order.WCP_PickPriority), (byte)0, pickFace1Order.WCP_PickPriority);

			var pickFace2Order = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == order2.WD_ExternalReference && l.WCP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickFace2Order.WCP_QuantityCommitted), 60m, pickFace2Order.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order.WCP_PickPriority), (byte)0, pickFace2Order.WCP_PickPriority);
		}

		public void TestView_WhsPickFaceCommittedStockView_DifferentClients_SinglePick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var org2 = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, org2, location2);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 100m, location1);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 100m, location2);
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 60m);

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(2, list.Length);
			var pickFace1Order = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == "O1" && l.WCP_PickNo == pick.WP_PickNo);
			AssertEquals(nameof(pickFace1Order.WCP_QuantityCommitted), 40m, pickFace1Order.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order.WCP_PickPriority), (byte)0, pickFace1Order.WCP_PickPriority);

			var pickFace2Order = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == "O2" && l.WCP_PickNo == pick.WP_PickNo);
			AssertEquals(nameof(pickFace2Order.WCP_QuantityCommitted), 60m, pickFace2Order.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order.WCP_PickPriority), (byte)0, pickFace2Order.WCP_PickPriority);
		}

		public void TestView_WhsPickFaceCommittedStockView_DifferentClients_SinglePick_SameLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var org2 = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, org2, location1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 100m, location1);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 100m, location1);
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 60m);

			var pick = Helper.CreatePickNew(order1, order2);
			Factory.Save();

			var list = Factory.Load<WhsPickFaceCommittedStockView>(new ZQuery());

			AssertEquals(2, list.Length);
			var pickFace1Order = list.Single(l => l.WCP_WF_PickFace == pickFace1.PK && l.WCP_OrderNo == "O1" && l.WCP_PickNo == pick.WP_PickNo);
			AssertEquals(nameof(pickFace1Order.WCP_QuantityCommitted), 40m, pickFace1Order.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace1Order.WCP_PickPriority), (byte)0, pickFace1Order.WCP_PickPriority);

			var pickFace2Order = list.Single(l => l.WCP_WF_PickFace == pickFace2.PK && l.WCP_OrderNo == "O2" && l.WCP_PickNo == pick.WP_PickNo);
			AssertEquals(nameof(pickFace2Order.WCP_QuantityCommitted), 60m, pickFace2Order.WCP_QuantityCommitted);
			AssertEquals(nameof(pickFace2Order.WCP_PickPriority), (byte)0, pickFace2Order.WCP_PickPriority);
		}

		#endregion
	}
}
