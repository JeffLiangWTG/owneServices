using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(UEMMessageChooser))]
	sealed class UEMMessageChooserTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			return new UEMMessageChooser(header, new ISelectionItem[] { new SplitBillSelectionItem(bill) });
		}

		public void TestCheckedBoxIsTrueByDefault()
		{
			var chooser = (UEMMessageChooser)GetNewBusinessObject();
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals(true, chooserItem.Checked);
		}
	}
}
