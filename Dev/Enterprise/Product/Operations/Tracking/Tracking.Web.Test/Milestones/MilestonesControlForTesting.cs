using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class MilestonesControlForTesting : MilestonesControl
	{
		public void SetupForTesting()
		{
			MilestonesGrid = new ZDataGrid();
		}

		protected override bool IsBindableDataSource(object dataSource) => true;

		public new bool AllowUpdate => base.AllowUpdate;

		public ZDataGrid MilestonesGridForTesting => MilestonesGrid;
		public void SetIsInEditModeForTesting(bool value) => IsInEditMode = value;
	}
}
