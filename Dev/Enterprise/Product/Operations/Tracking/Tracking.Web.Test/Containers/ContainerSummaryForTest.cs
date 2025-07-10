using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ContainerSummaryForTest : ContainerSummary
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void SetupSummaryGridForTest(ZDataGrid grid)
		{
			SetupSummaryGrid(grid);
		}

		public DataGrid SummaryGrid
		{
			get { return SelectedContainersGrid; }
		}
	}
}
