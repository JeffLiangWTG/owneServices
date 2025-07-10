using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(TransferHeaderMessageChooser))]
	sealed class TransferHeaderMessageChooserTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			return new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
		}

		public void TestCheckedBoxIsNotTrueByDefault()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IMP";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var transferHeader = arrivalHeader.TransferHeaders.AddNew();
			var transferHeaderSelectionItem = new TransferHeaderSelectionItem(transferHeader);
			var chooser = new TransferHeaderMessageChooser(header, new ISelectionItem[] { transferHeaderSelectionItem }, true, true);
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals(false, chooserItem.Checked);
		}
	}
}
