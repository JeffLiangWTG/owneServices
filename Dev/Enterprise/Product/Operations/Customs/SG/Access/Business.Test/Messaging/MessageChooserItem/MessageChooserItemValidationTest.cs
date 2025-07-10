using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class MessageChooserItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckChecked()
		{
			const string expectedWarning = "The Bill Cycle details do not match the Submission Cycle details.";
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			bill.CycleDate = new ZDateTime(2017, 6, 7);
			bill.CycleNumber = "2";
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			chooser.CycleDate = new ZDateTime(2018, 6, 7);
			chooser.CycleNumber = "2";
			var chooserItem = chooser.ChooserItems[0];
			chooserItem.Checked = true;
			AssertHasWarning(chooserItem.CheckedInfo, expectedWarning);
			bill.CycleDate = new ZDateTime(2018, 6, 7);
			bill.CycleNumber = "1";
			chooserItem.Checked = true;
			AssertHasWarning(chooserItem.CheckedInfo, expectedWarning);
			bill.CycleDate = new ZDateTime(2018, 6, 7);
			bill.CycleNumber = "2";
			chooserItem.Checked = true;
			AssertNoWarning(chooserItem.CheckedInfo, expectedWarning);
		}
	}
}
