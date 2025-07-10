using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EmailContactItemCollection))]
	sealed class EmailContactItemCollectionTest : ContactItemProxyCollectionTestCase<EmailContactItemCollection, EmailContactItem>
	{
		#region Add

		public void TestAddNew_WithDescription()
		{
			var contact = Factory.New<OrgContact>();
			var collection = contact.EmailContactItems;
			var mainEmail = collection.AddNew(EmailContactItemDescriptionList.Codes.Main);
			var otherEmail = collection.AddNew(EmailContactItemDescriptionList.Codes.Other);

			AssertEquals(EmailContactItemDescriptionList.Codes.Main, mainEmail.OI_Description);
			AssertEquals(EmailContactItemDescriptionList.Codes.Other, otherEmail.OI_Description);
		}

		#endregion

		#region AddItemsForOrgContactColumns

		public void TestAddItemsForOrgContactColumns()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "andrew@wisetechglobal.com";
			Factory.Save();

			var emailCollection = contact.EmailContactItems;
			AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"MAI: andrew@wisetechglobal.com"
					},
					emailCollection.Items.Select(item => string.Format("{0}: {1}", item.OI_Description, item.OI_Address))
				);

			AssertEquals(false, contact.HasChanges);

			Factory.Save();

			emailCollection = contact.EmailContactItems;
			AssertContainsExactElementsInAnyOrder("ContactItems for OrgContactColumns should exist after save",
					new[]
					{
						"MAI: andrew@wisetechglobal.com"
					},
					emailCollection.Items.Select(item => string.Format("{0}: {1}", item.OI_Description, item.OI_Address))
				);

			var otherFactory = new BusinessObjectFactory();
			var orgContactItemsQuery = new ZQuery(OrgContactItemSchema.OI_OC, contact.PK);
			AssertContainsExactElementsInAnyOrder("Should not have saved OrgContactItems for OrgContactColumn",
				System.Array.Empty<string>(),
				otherFactory.Load<OrgContactItem>(orgContactItemsQuery).Select(x => string.Format("{0}: {1}", x.OI_Description, x.OI_Address)));
		}

		#endregion

		#region Implementation

		protected override EmailContactItemCollection GetCollectionToTest()
		{
			var contact = Factory.New<OrgContact>();
			return new EmailContactItemCollection(contact);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgContactItem = Factory.New<OrgContactItem>();
			orgContactItem.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			return new EmailContactItem(orgContactItem);
		}

		#endregion
	}
}
