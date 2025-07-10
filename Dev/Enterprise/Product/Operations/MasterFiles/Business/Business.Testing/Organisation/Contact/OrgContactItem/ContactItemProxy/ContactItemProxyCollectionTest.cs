using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContactItemProxyCollectionTest : TestCaseWithFactory
	{
		#region GetMostImportantItem

		public void TestGetMostImportantItem()
		{
			var contact = Factory.New<OrgContact>();
			var collection = new ContactItemProxyCollectionForTesting(contact);
			var xxxItem1 = collection.AddNew();
			var xxxItem2 = collection.AddNew();
			var xxxItem3 = collection.AddNew();
			var yyyItem = collection.AddNew();

			xxxItem1.OI_Description = "XXX";
			xxxItem1.OI_Address = "02 23456789";

			xxxItem2.OI_Description = "XXX";
			xxxItem2.OI_Address = "02 12345678";

			xxxItem3.OI_Description = "XXX";
			xxxItem3.OI_Address = "02 34567890";

			yyyItem.OI_Description = "YYY";
			yyyItem.OI_Address = "000";

			AssertEquals(xxxItem2, collection.GetMostImportantItem("XXX"));
			AssertEquals(yyyItem, collection.GetMostImportantItem("YYY"));
			AssertEquals(null, collection.GetMostImportantItem("ZZZ"));
		}

		#endregion

		#region Implementation

		class ContactItemProxyCollectionForTesting : ContactItemProxyCollection<ContactItemProxyForTesting>
		{
			public ContactItemProxyCollectionForTesting(OrgContact contact)
				: base(contact, ContactItemProxyForTesting.Type)
			{
			}

			protected override ContactItemProxyForTesting GetNewContactItemProxy(OrgContactItem orgContactItem)
			{
				return new ContactItemProxyForTesting(orgContactItem);
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new ContactItemProxyForTesting(Contact);
			}
		}

		class ContactItemProxyForTesting : ContactItemProxy
		{
			public ContactItemProxyForTesting(ZString contactItemType, ZString description, ZPropertyInfo addressInfoOnOrgContact)
				: base(Type, description, addressInfoOnOrgContact)
			{
			}

			public ContactItemProxyForTesting(OrgContactItem orgContactItem)
				: base(orgContactItem)
			{
			}

			public ContactItemProxyForTesting(OrgContact contact)
				: base(contact, Type)
			{
			}

			public static string Type = "TST";

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				OI_ContactItemType = Type;
			}
		}

		#endregion
	}
}
