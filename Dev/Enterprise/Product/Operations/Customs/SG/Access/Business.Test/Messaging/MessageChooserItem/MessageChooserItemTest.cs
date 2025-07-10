using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(MessageChooserItem))]
	sealed class MessageChooserItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRequiresCycleFields()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			var chooserItem = chooser.ChooserItems[0];
			AssertEquals(true, chooserItem.RequiresCycleFields);
			chooser = new MessageChooser(header, new ISelectionItem[] { bill }, false);
			chooserItem = chooser.ChooserItems[0];
			AssertEquals(false, chooserItem.RequiresCycleFields);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
			return chooser.ChooserItems[0];
		}
	}
}
