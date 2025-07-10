using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AIMMessageChooserItem))]
	public sealed class AIMMessageChooserItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDescription()
		{
			var item = (AIMMessageChooserItem)GetNewBusinessObject();
			AssertEquals("Bill Number - 10000001", item.Description);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "10000001";
			var chooser = new AIMMessageChooser(header, header.Bills, AIMMessageSubTypes.FRI);
			return new AIMMessageChooserItem(chooser, bill, true);
		}
	}
}
