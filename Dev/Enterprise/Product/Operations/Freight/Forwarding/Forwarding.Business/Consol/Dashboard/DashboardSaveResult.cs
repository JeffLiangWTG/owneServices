using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DashboardSaveResult
	{
		public DashboardSaveResult(IEnumerable<DashboardConsolSnapshot> consolSnapshots)
		{
			ConsolSnapshots = consolSnapshots ?? System.Array.Empty<DashboardConsolSnapshot>();
		}

		public readonly IEnumerable<DashboardConsolSnapshot> ConsolSnapshots;

		public bool IsEmpty
		{
			get { return !ConsolSnapshots.Any(); }
		}

		public bool AllConsolsHaveBeenSaved
		{
			get { return ConsolSnapshots.All(snapshot => snapshot.State == DashboardConsolSnapshot.SnapshotState.Saved); }
		}

		public IEnumerable<DashboardConsolSnapshot> Unprocessed
		{
			get { return ConsolSnapshots.Where(snapshot => snapshot.State == DashboardConsolSnapshot.SnapshotState.Unprocessed); }
		}

		public IEnumerable<DashboardConsolSnapshot> Saved
		{
			get { return ConsolSnapshots.Where(snapshot => snapshot.State == DashboardConsolSnapshot.SnapshotState.Saved); }
		}

		public IEnumerable<DashboardConsolSnapshot> Unsaved
		{
			get { return ConsolSnapshots.Where(snapshot => snapshot.State != DashboardConsolSnapshot.SnapshotState.Saved); }
		}

		public IEnumerable<DashboardConsolSnapshot> ValidationErrors
		{
			get { return ConsolSnapshots.Where(snapshot => snapshot.State == DashboardConsolSnapshot.SnapshotState.ValidationErrors); }
		}

		public IEnumerable<DashboardConsolSnapshot> ModifiedByAnotherUser
		{
			get { return ConsolSnapshots.Where(snapshot => snapshot.State == DashboardConsolSnapshot.SnapshotState.ModifiedByAnotherUser); }
		}

		public IEnumerable<DashboardConsolSnapshot> SaveExceptions
		{
			get { return ConsolSnapshots.Where(snapshot => snapshot.State == DashboardConsolSnapshot.SnapshotState.SaveExceptions); }
		}
	}
}
