using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	[TestedType(typeof(USExportManifestSelectionDialog))]
	class USExportManifestSelectionDialogBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			bill.ABL_ManifestQty = 2;
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { splitBillSelectionItem });
			var result = new USExportManifestSelectionDialog(chooser, "Test");
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();
			return result;
		}
	}
}
