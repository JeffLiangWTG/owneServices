using System.ComponentModel;
using System.Linq;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferLineCollection))]
	class WhsTransferLineCollectionTestCase : WhsDocketLineCollectionTestCase<WhsTransferLineCollection>
	{
		#region TestIBindingList_AllowNew

		#region TestIBindingList_AllowNew_MasterTransfer

		public void TestIBindingList_AllowNew_MasterTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			AssertEquals(true, ((IBindingList)transfer.Lines).AllowNew);

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			transferLine.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);

			AssertEquals(true, ((IBindingList)transfer.Lines).AllowNew);
			AssertEquals(false, ((IBindingList)transfer.ChildTransfers.ElementAt(0).Lines).AllowNew);
		}

		#endregion

		#region TestIBindingList_AllowNew_VASOrderTransfer

		public void TestIBindingList_AllowNew_VASOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var intoServiceAreaTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull(intoServiceAreaTransfer);
			AssertEquals(false, ((IBindingList)intoServiceAreaTransfer.Lines).AllowNew);
		}

		#endregion

		#region TestIBindingList_AllowNew_OutboundDockDoorTransfer

		public void TestIBindingList_AllowNew_OutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			AssertEquals(false, ((IBindingList)transfer.Lines).AllowNew);
		}

		#endregion

		#region TestIBindingList_AllowNew_PutawayTransfer

		public void TestIBindingList_AllowNew_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.FindLocation("A-1"), "");
			transfer.WD_IsPutawayTransfer = true;

			AssertEquals(false, ((IBindingList)transfer.Lines).AllowNew);
		}

		#endregion

		#region TestIBindingList_AllowNew_PlanningStatus

		public void TestIBindingList_AllowNew_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.FindLocation("A-1"), "");
			AssertEquals(true, ((IBindingList)transfer.Lines).AllowNew);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(false, ((IBindingList)transfer.Lines).AllowNew);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals(true, ((IBindingList)transfer.Lines).AllowNew);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals(false, ((IBindingList)transfer.Lines).AllowNew);
		}

		#endregion

		#endregion

		protected override void TestLocationSortedProperlyCore(string locationPropertyToCompare)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var sourceLocation1 = data.Whs1.FindLocation("A-4");
			var sourceLocation2 = data.Whs1.FindLocation("A-5");
			var sourceLocation3 = data.Whs1.FindLocation("A-6");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, sourceLocation2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 30m, sourceLocation3, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation1.ToLocationString(), data.Whs1.FindLocation("A-1").ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, sourceLocation2.ToLocationString(), data.Whs1.FindLocation("A-2").ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, sourceLocation3.ToLocationString(), data.Whs1.FindLocation("A-10").ToLocationString());

			// Check sort ascending
			transfer.Lines.ApplySort(locationPropertyToCompare, ListSortDirection.Ascending);
			AssertEquals("A-1", transfer.Lines[0].LocationString);
			AssertEquals("A-2", transfer.Lines[1].LocationString);
			AssertEquals("A-10", transfer.Lines[2].LocationString);

			// Check sort descending
			transfer.Lines.ApplySort(locationPropertyToCompare, ListSortDirection.Descending);
			AssertEquals("A-10", transfer.Lines[0].LocationString);
			AssertEquals("A-2", transfer.Lines[1].LocationString);
			AssertEquals("A-1", transfer.Lines[2].LocationString);
		}

		protected override void TestTransferFromLocationStringSortedProperlyCore(string locationPropertyToSort)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var sourceLocation1 = data.Whs1.FindLocation("A-1");
			var sourceLocation2 = data.Whs1.FindLocation("A-2");
			var sourceLocation3 = data.Whs1.FindLocation("A-10");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, sourceLocation2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 30m, sourceLocation3, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation1.ToLocationString(), data.Whs1.FindLocation("A-4").ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, sourceLocation2.ToLocationString(), data.Whs1.FindLocation("A-5").ToLocationString());
			Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, sourceLocation3.ToLocationString(), data.Whs1.FindLocation("A-6").ToLocationString());

			// Check sort ascending
			transfer.Lines.ApplySort(locationPropertyToSort, ListSortDirection.Ascending);
			AssertEquals("A-1", transfer.Lines[0].TransferFromLocationString);
			AssertEquals("A-2", transfer.Lines[1].TransferFromLocationString);
			AssertEquals("A-10", transfer.Lines[2].TransferFromLocationString);

			// Check sort descending
			transfer.Lines.ApplySort(locationPropertyToSort, ListSortDirection.Descending);
			AssertEquals("A-10", transfer.Lines[0].TransferFromLocationString);
			AssertEquals("A-2", transfer.Lines[1].TransferFromLocationString);
			AssertEquals("A-1", transfer.Lines[2].TransferFromLocationString);
		}

		#region Implementations

		protected override WhsTransferLineCollection GetCollectionToTest()
		{
			return new WhsTransferLineCollection(Factory.New<WhsTransfer>());
		}

		#endregion
	}
}
