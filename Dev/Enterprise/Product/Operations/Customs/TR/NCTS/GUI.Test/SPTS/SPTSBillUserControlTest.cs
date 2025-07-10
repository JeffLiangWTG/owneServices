using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class SPTSBillUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeGrid()
		{
			using (var form = new ZForm())
			using (var control = new SPTSBillUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var billsGrid = control.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions("Test for Existence of The Grid", () =>
				{
					AssertEquals("B0_MasterBillNumber", ((ZGridColumnInfo)billsGrid.ColumnStyles[0]).ColumnName);
					AssertEquals("B0_ReferenceQualifier", ((ZGridColumnInfo)billsGrid.ColumnStyles[1]).ColumnName);
					AssertEquals("B0_ReferenceID", ((ZGridColumnInfo)billsGrid.ColumnStyles[2]).ColumnName);
					AssertEquals("B0_ServiceType", ((ZGridColumnInfo)billsGrid.ColumnStyles[3]).ColumnName);
				});
			}
		}
	}
}
