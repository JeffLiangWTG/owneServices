using System;
using System.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveLineCollection))]
	public class WhsReceiveLineCollectionTestCase : WhsDocketLineCollectionTestCase<WhsReceiveLineCollection>
	{
		protected override WhsReceiveLineCollection GetCollectionToTest()
		{
			return new WhsReceiveLineCollection(Factory.New<WhsReceive>());
		}

		protected override void TestLocationSortedProperlyCore(string locationPropertyToSort)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-3"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, data.Whs1.FindLocation("A-5"));
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-10"));

			// Check sort ascending
			receive.Lines.ApplySort(locationPropertyToSort, ListSortDirection.Ascending);
			AssertEquals("A-1", receive.Lines[0].LocationString);
			AssertEquals("A-2", receive.Lines[1].LocationString);
			AssertEquals("A-3", receive.Lines[2].LocationString);
			AssertEquals("A-5", receive.Lines[3].LocationString);
			AssertEquals("A-10", receive.Lines[4].LocationString);

			// Check sort descending
			receive.Lines.ApplySort(locationPropertyToSort, ListSortDirection.Descending);
			AssertEquals("A-10", receive.Lines[0].LocationString);
			AssertEquals("A-5", receive.Lines[1].LocationString);
			AssertEquals("A-3", receive.Lines[2].LocationString);
			AssertEquals("A-2", receive.Lines[3].LocationString);
			AssertEquals("A-1", receive.Lines[4].LocationString);
		}

		protected override void TestTransferFromLocationStringSortedProperlyCore(string locationPropertyToCompare)
		{
			// receive lines do not have transfer from location
			Assert(true);
		}

		public void TestAllowNew_PickByBOM()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var collection = new WhsReceiveLineCollection(receive);
			AssertEquals("Receive should allow Lines to be added by default.", true, ((IBindingList)collection).AllowNew);

			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;
			AssertEquals("Pick by BOM Receive should not allow Lines to be added.", false, ((IBindingList)collection).AllowNew);
		}

		public void TestAllowNew_UnloadComplete()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			Factory.Save();

			var collection = new WhsReceiveLineCollection(receive);
			AssertEquals("Receive should allow Lines to be added by default even with TM2.", true, ((IBindingList)collection).AllowNew);

			receive.WD_UnloadCompletedTime = DateTimeOffset.Now;
			AssertEquals("Setting WD_UnloadCompletedTime should not allow Lines to be added.", false, ((IBindingList)collection).AllowNew);

			receive.WD_HoldPalletIDPutaway = true;
			AssertEquals("Setting WD_UnloadCompletedTime with WD_HoldPalletIDPutaway set should allow Lines to be added.", true, ((IBindingList)collection).AllowNew);
		}

		public void TestAllowRemove_UnloadComplete()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			Factory.Save();

			var collection = new WhsReceiveLineCollection(receive);
			AssertEquals("Receive should allow Lines to be removed by default even with TM2.", true, ((IBindingList)collection).AllowRemove);

			receive.WD_UnloadCompletedTime = DateTimeOffset.Now;
			AssertEquals("Setting WD_UnloadCompletedTime should not allow Lines to be removed.", false, ((IBindingList)collection).AllowRemove);

			receive.WD_HoldPalletIDPutaway = true;
			AssertEquals("Setting WD_UnloadCompletedTime without with WD_HoldPalletIDPutaway set should allow Lines to be removed.", true, ((IBindingList)collection).AllowRemove);
		}

		public void TestDestLocationSortedProperly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "ABC1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockDoorLocation, "ABC2");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, data.Whs1.FindLocation("A-2"), "ABC1", 10m);
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, data.Whs1.FindLocation("A-10"), "ABC2", 20m);
			transferLine1.RunPreSaveValidation();
			transferLine2.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("DestLocation should be from putaway transferLine.", "A-2", receiveLine1.DestLocation);
			AssertEquals("DestLocation should be from putaway transferLine.", "A-10", receiveLine2.DestLocation);

			// Check sort ascending
			receive.Lines.ApplySort(WhsReceiveLine.Schema.DestLocation, ListSortDirection.Ascending);
			AssertEquals("A-2", receive.Lines[0].DestLocation);
			AssertEquals("A-10", receive.Lines[1].DestLocation);

			// Check sort descending
			receive.Lines.ApplySort(WhsReceiveLine.Schema.DestLocation, ListSortDirection.Descending);
			AssertEquals("A-10", receive.Lines[0].DestLocation);
			AssertEquals("A-2", receive.Lines[1].DestLocation);
		}

		public void TestDestLocationSortedProperly_NoPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "ABC1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, dockDoorLocation, "ABC2");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, data.Whs1.FindLocation("A-2"), "ABC1", 10m);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("DestLocation should be from putaway transferLine.", "A-2", receiveLine1.DestLocation);
			AssertEquals("DestLocation should be none.", true, receiveLine2.DestLocation.IsEmpty);

			// Check sort ascending
			receive.Lines.ApplySort(WhsReceiveLine.Schema.DestLocation, ListSortDirection.Ascending);
			AssertEquals("", receive.Lines[0].DestLocation);
			AssertEquals("A-2", receive.Lines[1].DestLocation);

			// Check sort descending
			receive.Lines.ApplySort(WhsReceiveLine.Schema.DestLocation, ListSortDirection.Descending);
			AssertEquals("A-2", receive.Lines[0].DestLocation);
			AssertEquals("", receive.Lines[1].DestLocation);
		}

		protected override bool ExpectedAllowAddOrRemoveForPutawayDocket => true;
	}
}
