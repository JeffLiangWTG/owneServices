using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PhoneContactItemCollection))]
	sealed class PhoneContactItemCollectionTest : ContactItemProxyCollectionTestCase<PhoneContactItemCollection, PhoneContactItem>
	{
		#region Add

		public void TestAddNew_WithDescription()
		{
			var contact = Factory.New<OrgContact>();
			var collection = contact.PhoneContactItems;
			var workPhoneItem = collection.AddNew(PhoneContactItemDescriptionList.Codes.Work);
			var homePhoneItem = collection.AddNew(PhoneContactItemDescriptionList.Codes.Home);

			AssertEquals(PhoneContactItemDescriptionList.Codes.Work, workPhoneItem.OI_Description);
			AssertEquals(PhoneContactItemDescriptionList.Codes.Home, homePhoneItem.OI_Description);
		}

		#endregion

		#region AddItemsForOrgContactColumns

		public void TestAddItemsForOrgContactColumns()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.Person.UpdateFromContact(contact);
			contact.OC_Fax = "03 12345678";
			contact.OC_HomePhone = "02 12345678";
			contact.OC_Mobile = "04 12345678";
			contact.OC_OtherPhone = "000";
			contact.OC_Pager = "01 12345678";
			contact.OC_Phone = "02 11114444";
			contact.OC_PhoneExtension = "123";
			Factory.Save();

			var phoneCollection = contact.PhoneContactItems;
			AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"FAX: 03 12345678",
						"HOM: 02 12345678",
						"MOB1: 04 12345678",
						"OTH: 000",
						"PGR: 01 12345678",
						"WRK1: 02 11114444",
						"EXT: 123"
					},
					phoneCollection.Items.Select(item => string.Format("{0}: {1}", item.OI_Description, item.OI_Address))
				);

			AssertEquals(false, contact.HasChanges);

			Factory.Save();

			phoneCollection = contact.PhoneContactItems;
			AssertContainsExactElementsInAnyOrder("ContactItems for OrgContactColumns should exist after save",
					new[]
					{
						"FAX: 03 12345678",
						"HOM: 02 12345678",
						"MOB1: 04 12345678",
						"OTH: 000",
						"PGR: 01 12345678",
						"WRK1: 02 11114444",
						"EXT: 123"
					},
					phoneCollection.Items.Select(item => string.Format("{0}: {1}", item.OI_Description, item.OI_Address))
				);

			var otherFactory = new BusinessObjectFactory();
			var orgContactItemsQuery = new ZQuery(OrgContactItemSchema.OI_OC, contact.PK);
			AssertContainsExactElementsInAnyOrder("Should not have saved OrgContactItems for OrgContactColumn",
				System.Array.Empty<string>(),
				otherFactory.Load<OrgContactItem>(orgContactItemsQuery).Select(x => string.Format("{0}: {1}", x.OI_Description, x.OI_Address)));
		}

		#endregion

		#region Implementation

		protected override PhoneContactItemCollection GetCollectionToTest()
		{
			var contact = Factory.New<OrgContact>();
			return new PhoneContactItemCollection(contact);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgContactItem = Factory.New<OrgContactItem>();
			orgContactItem.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
			return new PhoneContactItem(orgContactItem);
		}

		#endregion
	}
}
