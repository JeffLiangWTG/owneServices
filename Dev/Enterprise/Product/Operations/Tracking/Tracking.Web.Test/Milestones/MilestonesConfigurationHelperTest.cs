using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class MilestonesConfigurationHelperTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestIfSiteUserIsNull()
		{
			var dGrid = new ZDataGrid();
			var columns = new DataGridColumn[5];
			var quantityColumn = new ZCalcEditColumn("headertext", "bindTo", "bindToDecimals");
			quantityColumn.ID = "ADate_Edit";

			for (int i = 0; i < 5; i++)
			{
				columns[i] = quantityColumn;
			}

			foreach (DataGridColumn col in columns)
			{
				dGrid.Columns.Add(col);
			}

			AssertNoExceptionThrown(() => MilestonesConfigurationHelper.ConfigureMilestonesGridColumnsVisibility(dGrid, true));
		}
	}
}
