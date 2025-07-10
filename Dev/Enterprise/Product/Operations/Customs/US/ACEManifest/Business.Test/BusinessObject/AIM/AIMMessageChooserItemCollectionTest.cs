using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AIMMessageChooserItemCollection))]
	public sealed class AIMMessageChooserItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AIMMessageChooserItemCollection>
	{
		public void TestAddNew_Parameters()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "10000001";
			var chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRI);
			var collection = new AIMMessageChooserItemCollection();
			var item = collection.AddNew(chooser, bill, true);
			AssertEquals(typeof(AIMMessageChooserItem), item.GetType());
			AssertEquals(1, collection.Count);
			AssertCollectionContains(item, collection);
		}

		protected override AIMMessageChooserItemCollection GetCollectionToTest()
		{
			return new AIMMessageChooserItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "10000001";
			var chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRI);
			return chooser.ChooserItems[0];
		}
	}
}
