using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PackingDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsBoundToGrid()
		{
			using (var packingDetailsControl = new PackingDetailsUserControl())
			{
				Assert("Grid should include JL_ExportRefNumber column", ColumnExists(packingDetailsControl.ContainerDetailsGrid, "JL_ExportRefNumber"));
				Assert("Grid should include JL_ImportRefNumber column", ColumnExists(packingDetailsControl.ContainerDetailsGrid, "JL_ImportRefNumber"));
				Assert("Grid should include JL_InspectionTypeCode column", ColumnExists(packingDetailsControl.ContainerDetailsGrid, "JL_InspectionTypeCode"));
			}
		}

		public void TestDisableImportDataMenuItem()
		{
			using (var form = new ZForm())
			using (var control = new PackingDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("PackingDetailsUserControl DisableImportDataMenuItem", control.ContainerDetailsGrid.DisableImportDataMenuItem);
			}
		}

		#region Implementation

		bool ColumnExists(ZGrid grid, string columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == columnName);
		}

		#endregion
	}
}
