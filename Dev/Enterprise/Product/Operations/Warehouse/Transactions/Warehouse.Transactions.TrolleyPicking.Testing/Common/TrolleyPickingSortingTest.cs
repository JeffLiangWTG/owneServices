using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	public class TrolleyPickingSortingTest : WhsTestCaseWithFactory
	{
		#region SortPickLinesForTrolleyPicking

		public void TestSortPickLinesForTrolleyPicking()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0]);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locations[1]);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, locations[1]);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, locations[1]);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, locations[1]);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 1m); // priority location but not trolley slot
			var pickLine2 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 2m);
			var pickLine3 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 3m); // priority slot
			var pickLine4 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 4m);
			var pickLine5 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 5m); // not packed

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box); // pick line 1 & 2
			var pkg2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Box); // pick line 3
			var pkg3 = packingHelper.CreatePackage(pkgJob, "PKG3", 1, Constants.PkgUnit.Box); // pick line 4
			packingHelper.CreatePackageDivot(pkg1, (IPackableItem)((BusinessObject)pickLine1));
			packingHelper.CreatePackageDivot(pkg1, (IPackableItem)((BusinessObject)pickLine2));
			packingHelper.CreatePackageDivot(pkg2, (IPackableItem)((BusinessObject)pickLine3));
			packingHelper.CreatePackageDivot(pkg3, (IPackableItem)((BusinessObject)pickLine4));

			var pickLinesSorted1 = new[] { pickLine4, pickLine3, pickLine1, pickLine2, pickLine5 };
			Array.Sort(pickLinesSorted1, new SortPickLinesForTrolleyPicking());
			// First pick line have priority location therefore should be first.
			// All other lines are equal since they do not have trolley slots associated with them, should be ordered in PK order.
			var sortedPickLines = new[] { pickLine2, pickLine3, pickLine4, pickLine5 }.OrderBy(l => l.PK).ToArray();
			AssertEquals(pickLine1.PK, pickLinesSorted1[0].PK);
			AssertEquals(sortedPickLines[0].PK, pickLinesSorted1[1].PK);
			AssertEquals(sortedPickLines[1].PK, pickLinesSorted1[2].PK);
			AssertEquals(sortedPickLines[2].PK, pickLinesSorted1[3].PK);
			AssertEquals(sortedPickLines[3].PK, pickLinesSorted1[4].PK);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 4);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			var trolleySlot3 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg3, 3);

			var pickLinesSorted2 = new[] { pickLine4, pickLine3, pickLine1, pickLine2, pickLine5 };
			Array.Sort(pickLinesSorted2, new SortPickLinesForTrolleyPicking());
			// First pick line have priority location therefore should be first.
			// All other lines should be ordered based on trolley slot number
			AssertEquals(pickLine1.PK, pickLinesSorted2[0].PK); // priority location
			AssertEquals(pickLine3.PK, pickLinesSorted2[1].PK); // slot 2
			AssertEquals(pickLine4.PK, pickLinesSorted2[2].PK); // slot 3
			AssertEquals(pickLine2.PK, pickLinesSorted2[3].PK); // slot 4
			AssertEquals(pickLine5.PK, pickLinesSorted2[4].PK); // not assigned to any slots
		}

		#endregion
	}
}

