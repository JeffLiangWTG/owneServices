using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class UNDGSubstanceMultiColumnsManagerTest : TestCaseWithFactory
	{
		public void TestAddColumns()
		{
			using (var zGrid = new ZGrid())
			{
				var manager = new UNDGSubstanceMultiColumnsManager(zGrid);
				manager.AddColumns("Name");
				AssertEquals("Grid should have no columns.", 0, zGrid.ColumnStyles.Count);

				manager.AddColumns(GroupColumnConstant.LimitedQuantities);
				AssertEquals("Grid should have 3 columns.", 3, zGrid.ColumnStyles.Count);
				AssertEquals("Grid should have a column named DG_LQMaxAmt", true, zGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == "DG_LQMaxAmt"));

				manager.AddColumns(GroupColumnConstant.CargoAircraftOnly);
				AssertEquals("Grid should have 6 columns.", 6, zGrid.ColumnStyles.Count);
				AssertEquals("Grid should have a column named DG_CargoMaxAmtUQ", true, zGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == "DG_CargoMaxAmtUQ"));

				manager.AddColumns(GroupColumnConstant.PassengerCargoAircraft);
				AssertEquals("Grid should have 9 columns.", 9, zGrid.ColumnStyles.Count);
				AssertEquals("Grid should have a column named DG_PaxPackIns", true, zGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == "DG_PaxPackIns"));

				manager = null;
				zGrid.Dispose();
			}
		}
	}
}
