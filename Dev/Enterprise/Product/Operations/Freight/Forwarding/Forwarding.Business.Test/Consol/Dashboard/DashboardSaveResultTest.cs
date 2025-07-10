using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DashboardSaveResultTest : TestCaseWithFactory
	{
		public void TestCtor()
		{
			var consol1 = CreateConsolSnapshot();
			var consol2 = CreateConsolSnapshot();
			var consol3 = CreateConsolSnapshot();

			var snapshots = new[] { consol1, consol2, consol3 };

			var result = new DashboardSaveResult(snapshots);
			AssertContainsExactElementsInAnyOrder(snapshots, result.ConsolSnapshots);
		}

		public void TestIsEmpty()
		{
			var result = new DashboardSaveResult(System.Array.Empty<DashboardConsolSnapshot>());
			AssertEquals(true, result.IsEmpty);

			var consol1 = CreateConsolSnapshot();
			var snapshots = new[] { consol1 };

			result = new DashboardSaveResult(snapshots);
			AssertEquals(false, result.IsEmpty);
		}

		public void TestAllConsolsHaveBeenSaved()
		{
			var result = new DashboardSaveResult(System.Array.Empty<DashboardConsolSnapshot>());
			AssertEquals(true, result.AllConsolsHaveBeenSaved);

			var consol1 = CreateConsolSnapshot();
			var consol2 = CreateConsolSnapshot();
			var consol3 = CreateConsolSnapshot();

			var snapshots = new[] { consol1, consol2, consol3 };

			result = new DashboardSaveResult(snapshots);
			AssertEquals(false, result.AllConsolsHaveBeenSaved);

			consol1.SetState(DashboardConsolSnapshot.SnapshotState.Saved);
			consol2.SetState(DashboardConsolSnapshot.SnapshotState.Saved);
			consol3.SetState(DashboardConsolSnapshot.SnapshotState.ValidationErrors);
			AssertEquals(false, result.AllConsolsHaveBeenSaved);

			consol3.SetState(DashboardConsolSnapshot.SnapshotState.Saved);
			AssertEquals(true, result.AllConsolsHaveBeenSaved);
		}

		public void TestPropertiesFilteringByState()
		{
			var consol1 = CreateConsolSnapshot();
			var consol2 = CreateConsolSnapshot();
			var consol3 = CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState.Saved);
			var consol4 = CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState.Saved);
			var consol5 = CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState.ValidationErrors);
			var consol6 = CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState.ValidationErrors);
			var consol7 = CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState.SaveExceptions);
			var consol8 = CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState.ModifiedByAnotherUser);

			var snapshots = new[] { consol1, consol2, consol3, consol4, consol5, consol6, consol7, consol8 };

			var result = new DashboardSaveResult(snapshots);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, result.Unprocessed);
			AssertContainsExactElementsInAnyOrder(new[] { consol3, consol4 }, result.Saved);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol5, consol6, consol7, consol8 }, result.Unsaved);
			AssertContainsExactElementsInAnyOrder(new[] { consol5, consol6 }, result.ValidationErrors);
			AssertContainsExactElementsInAnyOrder(new[] { consol7 }, result.SaveExceptions);
			AssertContainsExactElementsInAnyOrder(new[] { consol8 }, result.ModifiedByAnotherUser);
		}

		#region Implementation

		DashboardConsolSnapshot CreateConsolSnapshot(DashboardConsolSnapshot.SnapshotState state = DashboardConsolSnapshot.SnapshotState.Unprocessed)
		{
			var consolPK = ZGuid.NewZGuid();
			var consolHumanReadableName = consolPK.ToString().Substring(0, 4);
			var shipment1PK = ZGuid.NewZGuid();
			var shipment2PK = ZGuid.NewZGuid();
			var notifications = System.Array.Empty<Notification>();

			var snapshot = new DashboardConsolSnapshot(consolPK, consolHumanReadableName, new[] { shipment1PK, shipment2PK }, notifications);
			snapshot.SetState(state);

			return snapshot;
		}

		#endregion
	}
}
