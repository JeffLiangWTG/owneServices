using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceLevelPackingPivotControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			using (var frm = new ZForm(declaration))
			using (var ctr = new InvoiceLevelPackingPivotControl())
			{
				frm.Controls.Add(ctr);
				frm.Show();
				Application.DoEvents();
				var grid = (ZArchitecture.ZGrid)ctr.Controls.Find("PackageInvoiceGrid", true).First();
				var expectedColumns = new[] { "IsLinked", "PackageNumber", "PackQty" };
				var actualColumns = grid.Columns.Select(c => c.ColumnName).ToArray();
				AssertArrayEqualsByElements(expectedColumns, actualColumns);
			}
		}
	}
}
