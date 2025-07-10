using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(UEMMessageChooserItemCollection))]
	class UEMMessageChooserItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UEMMessageChooserItemCollection>
	{
		public void TestAddNew_Parameters()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { splitBillSelectionItem });
			var collection = new UEMMessageChooserItemCollection();
			var item = collection.AddNew(chooser, splitBillSelectionItem, true);
			AssertEquals(typeof(UEMMessageChooserItem), item.GetType());
			AssertEquals(1, collection.Count);
			AssertCollectionContains(item, collection);
		}

		protected override UEMMessageChooserItemCollection GetCollectionToTest()
		{
			return new UEMMessageChooserItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { new SplitBillSelectionItem(bill) });
			return chooser.ChooserItems[0];
		}
	}
}
