using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	class USExportManifestSelectionDialogTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { splitBillSelectionItem });
			using (var chooserDialog = new USExportManifestSelectionDialog(chooser, "Test"))
			{
				chooserDialog.Show();
				var grid = chooserDialog.FindSingle<ZGrid>("ItemsGrid");
				var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertContainsExactElementsInAnyOrder(new string[] { "Checked", "Description", "ActionType", "ManifestQty" }, columnNames);
			}
		}
	}
}
