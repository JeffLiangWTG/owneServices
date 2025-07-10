using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContactItemProxy))]
	sealed class ContactItemProxyTest : NonPersistentBusinessObjectTestCase
	{
		#region IReadOnlySecurity

		public void TestReadOnlySecurityMembers()
		{
			var orgContactItem = Factory.NewWithValidTestData<OrgContactItem>();
			orgContactItem.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			var contactItem = new EmailContactItem(orgContactItem);

			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			AssertEquals(false, contactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(false, contactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(false, contactItem.OI_IsPrimaryInfo.ReadOnly);

			Env.Security.OrgContactModifyContactDetails.IsAllowed = false;
			AssertEquals(true, contactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(true, contactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(true, contactItem.OI_IsPrimaryInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contactItem.OI_OC = contact.PK;

			AssertEquals("Precondition", true, org.SecurityProvider.HasModifyContactContactDetailsSecurity);
			AssertEquals(false, contactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(false, contactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(false, contactItem.OI_IsPrimaryInfo.ReadOnly);

			Factory.Save();
			AssertEquals("Precondition", false, org.SecurityProvider.HasModifyContactContactDetailsSecurity);
			AssertEquals(true, contactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(true, contactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(true, contactItem.OI_IsPrimaryInfo.ReadOnly);
		}

		#endregion

		#region Properties

		#region OI_IsPrimary

		public void TestOI_IsPrimary()
		{
			var contactItem = GetNewContactItem();
			contactItem.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			contactItem.OI_Address = "aaa@a.com";
			AssertNotNull("Precondition", contactItem.OrgContactItem);

			contactItem.OI_IsPrimary = true;
			AssertEquals(true, contactItem.OrgContactItem.OI_IsPrimary);

			contactItem.OI_IsPrimary = false;
			AssertEquals(false, contactItem.OrgContactItem.OI_IsPrimary);

			contactItem.OrgContactItem.OI_IsPrimary = true;
			AssertEquals(true, contactItem.OI_IsPrimary);
		}

		#endregion

		#region OI_ContactItemType

		public void TestOI_ContactItemType()
		{
			var contactItem = GetNewContactItem();
			contactItem.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			contactItem.OI_Address = "aaa@a.com";
			AssertNotNull("Precondition", contactItem.OrgContactItem);

			contactItem.OI_ContactItemType = "AAA";
			AssertEquals("AAA", contactItem.OrgContactItem.OI_ContactItemType);

			contactItem.OI_ContactItemType = "BBB";
			AssertEquals("BBB", contactItem.OrgContactItem.OI_ContactItemType);

			contactItem.OrgContactItem.OI_ContactItemType = "CCC";
			AssertEquals("CCC", contactItem.OI_ContactItemType);
		}

		#endregion

		#region OI_OC

		public void TestOI_OC()
		{
			var contact1 = Factory.New<OrgContact>();
			var contact2 = Factory.New<OrgContact>();
			var contact3 = Factory.New<OrgContact>();

			var contactItem = GetNewContactItem();
			contactItem.OI_Description = EmailContactItemDescriptionList.Codes.Other;
			contactItem.OI_Address = "aaa@a.com";
			AssertNotNull("Precondition", contactItem.OrgContactItem);

			contactItem.OI_OC = contact1.PK;
			AssertEquals(contact1.PK, contactItem.OrgContactItem.OI_OC);

			contactItem.OI_OC = contact2.PK;
			AssertEquals(contact2.PK, contactItem.OrgContactItem.OI_OC);

			contactItem.OrgContactItem.OI_OC = contact3.PK;
			AssertEquals(contact3.PK, contactItem.OI_OC);
		}

		#endregion

		#region OI_Address

		public void TestOI_Address()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Email = "";
			var contactItemCollection = contact.EmailContactItems;

			var contactItem1 = contactItemCollection.AddNew();
			contactItem1.OI_Description = EmailContactItemDescriptionList.Codes.Main;
			contactItem1.OI_Address = "andrew@cargowise.com";
			AssertEquals("Precondition", contact.OC_EmailInfo, contactItem1.AddressInfoOnOrgContact);
			AssertEquals("andrew@cargowise.com", contact.OC_Email);

			var contactItem2 = contactItemCollection.AddNew();
			contactItem2.OI_Description = EmailContactItemDescriptionList.Codes.Main;
			contactItem2.OI_Address = "andrew@wistechglobal.com";
			AssertNull("Precondition", contactItem2.AddressInfoOnOrgContact);
			AssertNotNull("Precondition", contactItem2.OrgContactItem);
			AssertEquals("andrew@cargowise.com", contact.OC_Email);
			AssertEquals("andrew@wistechglobal.com", contactItem2.OrgContactItem.OI_Address);
		}

		#endregion

		#region OI_Description

		public void TestOI_Description()
		{
			var contact = Factory.New<OrgContact>();
			var contactItemCollection = contact.PhoneContactItems;

			var contactItem1 = contactItemCollection.AddNew(PhoneContactItemDescriptionList.Codes.Fax);
			contactItem1.OI_Address = "03 12345678";
			var contactItem2 = contactItemCollection.AddNew(PhoneContactItemDescriptionList.Codes.Fax);
			contactItem2.OI_Address = "03 98765432";

			AssertEquals("Precondition", contact.OC_FaxInfo, contactItem1.AddressInfoOnOrgContact);
			AssertNull("Precondition", contactItem2.AddressInfoOnOrgContact);
			AssertEquals("Precondition", "03 12345678", contact.OC_Fax);

			contactItem1.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			AssertEquals("03 12345678", contact.OC_HomePhone);
			AssertEquals("", contact.OC_Fax);
			AssertEquals(contact.OC_HomePhoneInfo, contactItem1.AddressInfoOnOrgContact);

			contactItem2.OI_Description = PhoneContactItemDescriptionList.Codes.Home;
			AssertEquals("03 12345678", contact.OC_HomePhone);
			AssertEquals("", contact.OC_Fax);
			AssertNull(contactItem2.AddressInfoOnOrgContact);
		}

		public void TestDisplayDescription()
		{
			var contact = Factory.New<OrgContact>();
			var phoneItem = contact.PhoneContactItems.AddNew();
			phoneItem.OI_Description = "";
			AssertEquals("", phoneItem.DisplayDescription);

			phoneItem.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
			AssertEquals(PhoneContactItemDescriptionList.Descriptions.Fax, phoneItem.DisplayDescription);

			phoneItem.OI_Description = "Custom Description";
			AssertEquals("Custom Description", phoneItem.DisplayDescription);
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewContactItem();
		}

		ContactItemProxy GetNewContactItem()
		{
			return new EmailContactItem(Contact);
		}

		OrgContact Contact
		{
			get { return contact ?? (contact = Factory.New<OrgContact>()); }
		}
		OrgContact contact;

		#endregion
	}
}
