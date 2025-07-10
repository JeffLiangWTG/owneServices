using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickFaceAwaitingReplenishmentViewFunctionTest : WhsTestCaseWithFactory
	{
		#region TestView_WhsPickFaceAwaitingReplenishmentView_GeneralColumns

		public void TestView_WhsPickFaceAwaitingReplenishmentView_GeneralColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);
			var location3 = data.Whs1.FindLocation("A-3");
			
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, location3, location2);
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 200m);
			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 300m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: pick2 is awaiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 2 records.", 2, list.Length);

			var pickView1 = list.Single(v => v.WWP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickView1.WWP_WP), pick1.PK, pickView1.WWP_WP);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 100m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickView2.WWP_WP), pick2.PK, pickView2.WWP_WP);
			AssertEquals(nameof(pickView2.WWP_OH_Client), data.Org1.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part2.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), data.Whs1.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 200m, pickView2.WWP_QuantityRequired);
			AssertEquals(nameof(pickView2.WWP_PickPriority), (byte)0, pickView2.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_PickPriority

		public void TestView_WhsPickFaceAwaitingReplenishmentView_PickPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var part4 = Helper.CreateProduct("P4", data.Org1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);
			var location3 = data.Whs1.FindLocation("A-3");
			var pickFace3 = Helper.CreateProductPickFace(part3, data.Org1, location3);
			var location4 = data.Whs1.FindLocation("A-4");
			
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 90m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 91m, location2);
			Helper.CreateWhsReceiveLine(receive, part3, 92m, location3);
			Helper.CreateWhsReceiveLine(receive, part4, 50m, location4);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location4);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location4);
			Helper.CreateWhsReceiveLine(receive, part3, 10m, location4);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location4, location1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, location4, location2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, part3, 10m, location4, location3);
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			transferLine3.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 40m);
			order1.WD_PickPriority = 2;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 60m);
			order2.WD_PickPriority = 3;
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 30m);
			order3.WD_PickPriority = 4;
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 70m);
			order4.WD_PickPriority = 0;
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", part3, 20m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", part3, 80m);
			var order7 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O7", part4, 50m);
			order7.WD_PickPriority = 1;

			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3, order4, order7);
			var pick3 = Helper.CreatePickNew(order5, order6);

			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick2 is awaiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick3 is awaiting replenishment.", true, pick3.WP_IsAwaitingReplenishment);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 3 records.", 3, list.Length);

			var pickView1 = list.Single(v => v.WWP_PickNo == pick1.WP_PickNo);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_PickNo), pick1.WP_PickNo, pickView1.WWP_PickNo);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 10m, pickView1.WWP_QuantityRequired);
			AssertEquals("Min of WD_PickPriority between order1/order2 is 2.", (byte)2, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_PickNo == pick2.WP_PickNo);
			AssertEquals(nameof(pickView2.WWP_OH_Client), data.Org1.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part2.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), data.Whs1.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_PickNo), pick2.WP_PickNo, pickView2.WWP_PickNo);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 9m, pickView2.WWP_QuantityRequired);
			AssertEquals("Min of WD_PickPriority among order3/order4/order7 is 1.", (byte)1, pickView2.WWP_PickPriority);

			var pickView3 = list.Single(v => v.WWP_PickNo == pick3.WP_PickNo);
			AssertEquals(nameof(pickView3.WWP_OH_Client), data.Org1.PK, pickView3.WWP_OH_Client);
			AssertEquals(nameof(pickView3.WWP_OP), part3.PK, pickView3.WWP_OP);
			AssertEquals(nameof(pickView3.WWP_WW_Whs), data.Whs1.PK, pickView3.WWP_WW_Whs);
			AssertEquals(nameof(pickView3.WWP_PickNo), pick3.WP_PickNo, pickView3.WWP_PickNo);
			AssertEquals(nameof(pickView3.WWP_QuantityRequired), 8m, pickView3.WWP_QuantityRequired);
			AssertEquals("WD_PickPriority of order5/order6 haven't been set, so it's 0.", (byte)0, pickView3.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_WhenThereAreDifferentPickFacesForSameWhsAndProductAndClient

		public void TestView_WhsPickFaceAwaitingReplenishmentView_WhenThereAreDifferentPickFacesForSameWhsAndProductAndClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			var location3 = data.Whs1.FindLocation("A-3");
			var pickFace3 = Helper.CreateProductPickFace(data.Part2, data.Org1, location1);
			var pickFace4 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, location3, location1);
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 300m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 400m);
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 2 records.", 2, list.Length);

			var pickView1 = list.Single(v => v.WWP_OP == data.Part1.PK);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 100m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_OP == data.Part2.PK);
			AssertEquals(nameof(pickView2.WWP_OH_Client), data.Org1.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part2.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), data.Whs1.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 200m, pickView2.WWP_QuantityRequired);
			AssertEquals(nameof(pickView2.WWP_PickPriority), (byte)0, pickView2.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_OneOrderLineHasMultiplePickLines

		public void TestView_WhsPickFaceAwaitingReplenishmentView_OneOrderLineHasMultiplePickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location1);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 300m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: pick is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView1 = list.Single();
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 200m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_MultipleOrderLines

		public void TestView_WhsPickFaceAwaitingReplenishmentView_MultipleOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 60m);
			Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: pick is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView1 = list.Single();
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 50m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);
		}

		public void TestView_WhsPickFaceAwaitingReplenishmentView_MultipleOrderLines_OtherLineNotOnPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Helper.CreateWhsOrderLine(order, data.Part2, 50m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition: pick is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView1 = list.Single();
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 50m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_NoPickLines

		public void TestView_WhsPickFaceAwaitingReplenishmentView_NoPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine1.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: no pick lines.", 0, pick.GetAllPickLines().Count());

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView = list[0];
			AssertEquals(nameof(pickView.WWP_OH_Client), data.Org1.PK, pickView.WWP_OH_Client);
			AssertEquals(nameof(pickView.WWP_OP), data.Part1.PK, pickView.WWP_OP);
			AssertEquals(nameof(pickView.WWP_WW_Whs), data.Whs1.PK, pickView.WWP_WW_Whs);
			AssertEquals(nameof(pickView.WWP_PickNo), pick.WP_PickNo, pickView.WWP_PickNo);
			AssertEquals(nameof(pickView.WWP_QuantityRequired), 100m, pickView.WWP_QuantityRequired);
			AssertEquals(nameof(pickView.WWP_PickPriority), (byte)0, pickView.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_OnePickWaitingOnTwoDifferentKindOfPickFaces_NoPickLines

		public void TestView_WhsPickFaceAwaitingReplenishmentView_OnePickWaitingOnDifferentKindOfPickFaces_NoPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, location2, location1);
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 90m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 70m);
			var pick1 = Helper.CreatePickNew(order1, order2);

			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: no pick lines.", 0, pick1.GetAllPickLines().Count());

			Factory.Save();

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 2 records.", 2, list.Length);

			var pickView1 = list.Single(v => v.WWP_OP == data.Part1.PK);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_PickNo), pick1.WP_PickNo, pickView1.WWP_PickNo);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 90m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_OP == data.Part2.PK);
			AssertEquals(nameof(pickView2.WWP_OH_Client), data.Org1.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part2.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), data.Whs1.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_PickNo), pick1.WP_PickNo, pickView2.WWP_PickNo);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 70m, pickView2.WWP_QuantityRequired);
			AssertEquals(nameof(pickView2.WWP_PickPriority), (byte)0, pickView2.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_OnePickWaitingOnTwoDifferentKindOfPickFaces_HavePickLines

		public void TestView_WhsPickFaceAwaitingReplenishmentView_OnePickWaitingOnTwoDifferentKindOfPickFaces_HavePickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location2);
			var location3 = data.Whs1.FindLocation("A-3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 50m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, location3, location2);
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 200m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 300m);
			var pick1 = Helper.CreatePickNew(order1, order2);

			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 2 records.", 2, list.Length);

			var pickView1 = list.Single(v => v.WWP_OP == data.Part1.PK);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_PickNo), pick1.WP_PickNo, pickView1.WWP_PickNo);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 150m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_OP == data.Part2.PK);
			AssertEquals(nameof(pickView2.WWP_OH_Client), data.Org1.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part2.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), data.Whs1.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_PickNo), pick1.WP_PickNo, pickView2.WWP_PickNo);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 250m, pickView2.WWP_QuantityRequired);
			AssertEquals(nameof(pickView2.WWP_PickPriority), (byte)0, pickView2.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_DifferentWarehouses

		public void TestView_WhsPickFaceAwaitingReplenishmentView_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");

			var whs2 = Helper.CreateWarehouse("WW2", "B", 2, 1);
			Factory.Save();
			var location3 = whs2.FindLocation("B-1");
			var location4 = whs2.FindLocation("B-2");
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location3);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, location3);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, location4);
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, location2, location1);
			transferLine1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, whs2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, location4, location3);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer1.IsFinalised);
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer2.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 90m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "O2", data.Part1, 100m);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save();
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick2 is awaiting replenishment.", true, pick2.WP_IsAwaitingReplenishment);

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 2 records.", 2, list.Length);

			var pickView1 = list.Single(v => v.WWP_WW_Whs == data.Whs1.PK);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_PickNo), pick1.WP_PickNo, pickView1.WWP_PickNo);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 40m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_WW_Whs == whs2.PK);
			AssertEquals(nameof(pickView2.WWP_OH_Client), data.Org1.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part1.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), whs2.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_PickNo), pick2.WP_PickNo, pickView2.WWP_PickNo);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 50m, pickView2.WWP_QuantityRequired);
			AssertEquals(nameof(pickView2.WWP_PickPriority), (byte)0, pickView2.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_DifferentClients

		public void TestView_WhsPickFaceAwaitingReplenishmentView_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);

			var location2 = data.Whs1.FindLocation("A-2");
			var org2 = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, org2, location2);

			var location3 = data.Whs1.FindLocation("A-3");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location3);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, location2);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, location3);
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, location3, location1);
			transferLine1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(org2, data.Whs1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, location3, location2);
			transferLine2.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer1.IsFinalised);
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer2.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 90m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 100m);
			var pick1 = Helper.CreatePickNew(order1, order2);

			Factory.Save();
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 2 records.", 2, list.Length);

			var pickView1 = list.Single(v => v.WWP_OH_Client == data.Org1.PK);
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_PickNo), pick1.WP_PickNo, pickView1.WWP_PickNo);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 40m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);

			var pickView2 = list.Single(v => v.WWP_OH_Client == org2.PK);
			AssertEquals(nameof(pickView2.WWP_OH_Client), org2.PK, pickView2.WWP_OH_Client);
			AssertEquals(nameof(pickView2.WWP_OP), data.Part1.PK, pickView2.WWP_OP);
			AssertEquals(nameof(pickView2.WWP_WW_Whs), data.Whs1.PK, pickView2.WWP_WW_Whs);
			AssertEquals(nameof(pickView2.WWP_PickNo), pick1.WP_PickNo, pickView2.WWP_PickNo);
			AssertEquals(nameof(pickView2.WWP_QuantityRequired), 50m, pickView2.WWP_QuantityRequired);
			AssertEquals(nameof(pickView2.WWP_PickPriority), (byte)0, pickView2.WWP_PickPriority);
		}

		public void TestView_WhsPickFaceAwaitingReplenishmentView_DifferentClients_SingleOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);

			var location2 = data.Whs1.FindLocation("A-2");
			var org2 = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, org2, location2);

			var location3 = data.Whs1.FindLocation("A-3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location1);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 90m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition: pick is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView = list.Single(v => v.WWP_OH_Client == data.Org1.PK);
			AssertEquals(nameof(pickView.WWP_OH_Client), data.Org1.PK, pickView.WWP_OH_Client);
			AssertEquals(nameof(pickView.WWP_OP), data.Part1.PK, pickView.WWP_OP);
			AssertEquals(nameof(pickView.WWP_WW_Whs), data.Whs1.PK, pickView.WWP_WW_Whs);
			AssertEquals(nameof(pickView.WWP_PickNo), pick.WP_PickNo, pickView.WWP_PickNo);
			AssertEquals(nameof(pickView.WWP_QuantityRequired), 40m, pickView.WWP_QuantityRequired);
			AssertEquals(nameof(pickView.WWP_PickPriority), (byte)0, pickView.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoProducts_OneIsNotInFixedPickFace

		public void TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoProducts_OneIsNotInFixedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");			
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 100m, location2);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 200m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 300m);
			var pick1 = Helper.CreatePickNew(order1, order2);

			Factory.Save();
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick1 has 2 pick lines.", 2, pick1.GetAllPickLines().Count());

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should only have 1 record for the Fixed Pick Face.", 1, list.Length);

			var pickView = list.Single();
			AssertEquals(nameof(pickView.WWP_OH_Client), data.Org1.PK, pickView.WWP_OH_Client);
			AssertEquals(nameof(pickView.WWP_OP), data.Part1.PK, pickView.WWP_OP);
			AssertEquals(nameof(pickView.WWP_WW_Whs), data.Whs1.PK, pickView.WWP_WW_Whs);
			AssertEquals(nameof(pickView.WWP_QuantityRequired), 100m, pickView.WWP_QuantityRequired);
			AssertEquals(nameof(pickView.WWP_PickPriority), (byte)0, pickView.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoClients_OneIsNotInFixedPickFace

		public void TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoClients_OneIsNotInFixedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");

			var org2 = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 100m, location1);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location2);
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location1);
			transferLine1.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 100m, location2);
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 400m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 500m);
			var pick1 = Helper.CreatePickNew(order1, order2);

			Factory.Save();
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick1.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick1 has 2 pick lines.", 2, pick1.GetAllPickLines().Count());

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should only have 1 record for the Fixed Pick Face.", 1, list.Length);

			var pickView1 = list.Single();
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 300m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoLocations_BothFixedPickFaces

		public void TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoLocations_BothFixedPickFaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			var location3 = data.Whs1.FindLocation("A-3");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location1);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 200m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 300m);
			var pick = Helper.CreatePickNew(order1, order2);

			Factory.Save();
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick1 has 2 pick lines.", 2, pick.GetAllPickLines().Count());

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView1 = list.Single();
			AssertEquals(nameof(pickView1.WWP_OH_Client), data.Org1.PK, pickView1.WWP_OH_Client);
			AssertEquals(nameof(pickView1.WWP_OP), data.Part1.PK, pickView1.WWP_OP);
			AssertEquals(nameof(pickView1.WWP_WW_Whs), data.Whs1.PK, pickView1.WWP_WW_Whs);
			AssertEquals(nameof(pickView1.WWP_QuantityRequired), 300m, pickView1.WWP_QuantityRequired);
			AssertEquals(nameof(pickView1.WWP_PickPriority), (byte)0, pickView1.WWP_PickPriority);
		}

		#endregion

		#region TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoLocations_OneIsNotInFixedPickFace

		public void TestView_WhsPickFaceAwaitingReplenishmentView_PickHasTwoLocations_OneIsNotInFixedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location2);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location3, location2);
			transferLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition: create an unfinalised Transfer so the pick will awaiting replenishment.", false, transfer.IsFinalised);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 200m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 300m);
			var pick = Helper.CreatePickNew(order1, order2);

			Factory.Save();
			AssertEquals("Precondition: pick1 is awaiting replenishment.", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals("Precondition: pick1 has 2 pick lines.", 2, pick.GetAllPickLines().Count());

			var list = Factory.Load<WhsPickFaceAwaitingReplenishmentView>(new ZQuery());
			AssertEquals("Should have 1 record.", 1, list.Length);

			var pickView = list.Single();
			AssertEquals(nameof(pickView.WWP_OH_Client), data.Org1.PK, pickView.WWP_OH_Client);
			AssertEquals(nameof(pickView.WWP_OP), data.Part1.PK, pickView.WWP_OP);
			AssertEquals(nameof(pickView.WWP_WW_Whs), data.Whs1.PK, pickView.WWP_WW_Whs);
			AssertEquals(nameof(pickView.WWP_QuantityRequired), 300m, pickView.WWP_QuantityRequired);
			AssertEquals(nameof(pickView.WWP_PickPriority), (byte)0, pickView.WWP_PickPriority);
		}

		#endregion
	}
}
