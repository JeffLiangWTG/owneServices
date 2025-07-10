using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(TransferHeaderMessageChooserItemCollection))]
	class TransferHeaderMessageChooserItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransferHeaderMessageChooserItemCollection>
	{
		public void TestAddNew_Parameters()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			var collection = new TransferHeaderMessageChooserItemCollection();
			var item = collection.AddNew(chooser, transferHeaderSelectionItem, true);
			AssertEquals(typeof(TransferHeaderMessageChooserItem), item.GetType());
			AssertEquals(1, collection.Count);
			AssertCollectionContains(item, collection);
		}

		protected override TransferHeaderMessageChooserItemCollection GetCollectionToTest()
		{
			return new TransferHeaderMessageChooserItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			return chooser.ChooserItems[0];
		}
	}
}
