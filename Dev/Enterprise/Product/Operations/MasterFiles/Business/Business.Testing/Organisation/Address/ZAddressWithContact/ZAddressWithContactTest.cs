using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ZAddressWithContact))]
	sealed class ZAddressWithContactTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContactFKDefaultedWhenOrgChanged()
		{
			var differentOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var differentAddress = differentOrgHeader.Addresses.AddNew();

			var testObj = new ObjectForTest(Factory);
			testObj.TheContact = contact.PK;
			testObj.TheAddress = address.PK;
			var addressWContact = new ZAddressWithContact(testObj.TheContactInfo, testObj.TheAddressInfo);
			AssertEquals(orgHeader.PK, addressWContact.OrgPK);
			AssertEquals(address.PK, addressWContact.AddressFK);
			AssertEquals(contact.PK, addressWContact.ContactFK);

			addressWContact.OrgPK = differentOrgHeader.PK;
			AssertEquals(differentOrgHeader.PK, addressWContact.OrgPK);
			AssertEquals(ZGuid.Empty, addressWContact.AddressFK);
			AssertEquals(ZGuid.Empty, addressWContact.ContactFK);
		}

		public void TestContactFK_ReadOnly()
		{
			var testObj = new ObjectForTest(Factory);
			testObj.TheContact = ZGuid.Empty;
			testObj.TheAddress = ZGuid.Empty;
			testObj.TheContact_ReadOnly = false;
			var addressWContact = new ZAddressWithContact(testObj.TheContactInfo, testObj.TheAddressInfo);
			AssertEquals("Read only due to no address therefore no org", true, addressWContact.ContactFK_ReadOnly);
			testObj.TheContact_ReadOnly = true;
			AssertEquals("Read only due to no address therefore no org AND underlying property", true, addressWContact.ContactFK_ReadOnly);
			testObj.TheAddress = address.PK;
			AssertEquals("Read only due to underlying property dispite address being supplied and therefore org", true, addressWContact.ContactFK_ReadOnly);
			testObj.TheContact_ReadOnly = false;
			AssertEquals("Not read only due to address supplied and underlying property", false, addressWContact.ContactFK_ReadOnly);
		}

		public void TestContactChange()
		{
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Dick Smith";
			contact2.OC_Phone = "7";
			contact2.OC_Email = "hello@world.com";
			contact2.OC_Fax = "3";

			var testObj = new ObjectForTest(Factory);
			testObj.TheContact = contact.PK;
			testObj.TheAddress = address.PK;

			AssertContactDetails(
@"Ph: 12348
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y", testObj);

			testObj.TheContact = contact2.PK;

			AssertContactDetails(
@"Ph: 7
Fax: 3
Em: hello@world.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y", testObj);
		}

		public void TestContactDetailsFields()
		{
			AssertContactDetails(
@"Ph: 12348
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");
			contact.OC_Phone = "12348";
			contact.OC_Mobile = "77777";

			AssertContactDetails(
@"Ph: 12348, (M) 77777
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");
			contact.OC_Phone = ZString.Empty;
			contact.OC_Mobile = "77777";

			AssertContactDetails(
@"Ph: 77777
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");
			contact.OC_Phone = ZString.Empty;
			contact.OC_Mobile = ZString.Empty;
			contact.OC_Fax = ZString.Empty;
			contact.OC_Email = ZString.Empty;

			AssertContactDetails(
@"Ph: *NOT FOUND*
Fax: *NOT FOUND*
Em: *NOT FOUND*
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");
			orgHeader.OrgWebURLs[0].Delete();

			AssertContactDetails(
@"Ph: *NOT FOUND*
Fax: *NOT FOUND*
Em: *NOT FOUND*
Web: *NOT FOUND*

Y
Y
Y
Y
N");
			var testObj = new ObjectForTest(Factory);
			testObj.TheContact = ZGuid.Empty;
			testObj.TheAddress = ZGuid.Empty;

			AssertContactDetails(
@"Ph: *NOT FOUND*
Fax: *NOT FOUND*



Y
Y
N
N
N", testObj);

			testObj = new ObjectForTest(Factory);
			testObj.TheContact = ZGuid.Empty;
			testObj.TheAddress = address.PK;

			AssertContactDetails(
@"Ph: *NOT FOUND*
Fax: *NOT FOUND*



Y
Y
N
N
N", testObj);
		}

		#region TestGetDefaultContact

		public void TestGetDefaultContact_OnOrgChangedOnAddressChanged()
		{
			var testObj = new ObjectForTest(Factory);
			var addressWContact = new ZAddressWithContact(testObj.TheContactInfo, testObj.TheAddressInfo);
			addressWContact.GetDefaultAddress += (IOrgHeader myOrgHeader) => myOrgHeader.Identifier == orgHeader.PK ? address.PK : ZGuid.Empty;
			addressWContact.GetDefaultContact += (IOrgHeader myOrgHeader, IOrgAddress myAddress) => myAddress.PK == address.PK ? contact.PK : ZGuid.Empty;
			addressWContact.OrgPK = orgHeader.PK;

			AssertEquals("address is defaulted", address.PK, addressWContact.AddressFK);
			AssertEquals("contact is defaulted", contact.PK, addressWContact.ContactFK);
			AssertEquals("contact details", "Ph: " + contact.OC_Phone, addressWContact.ContactDetails_Phone);
		}

		public void TestGetDefaultContact_OnAddressChanged()
		{
			var testObj = new ObjectForTest(Factory);
			var addressWContact = new ZAddressWithContact(testObj.TheContactInfo, testObj.TheAddressInfo);
			addressWContact.GetDefaultContact += (IOrgHeader myOrgHeader, IOrgAddress myAddress) => myAddress.PK == address.PK ? contact.PK : ZGuid.Empty;

			addressWContact.AddressFK = address.PK;
			AssertEquals("contact is defaulted", contact.PK, addressWContact.ContactFK);
			AssertEquals("contact details", "Ph: " + contact.OC_Phone, addressWContact.ContactDetails_Phone);
		}

		#endregion

		public void TestContactDetails_Phone()
		{
			SetupAddressesPhone();
			AssertContactDetails(
@"Ph: 12348
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");

			contact.OC_Phone = ZString.Empty;
			contact.OC_OA_OrgAddress = address.PK;
			AssertContactDetails(
@"Ph: 22222
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");

			contact.OC_OA_OrgAddress = ZGuid.Empty;
			AssertContactDetails(
@"Ph: 11111
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");

			var anotherAddress = orgHeader.Addresses.AddNew();
			anotherAddress.OA_Phone = "33333";
			contact.OC_OA_OrgAddress = anotherAddress.PK;
			AssertContactDetails(
@"Ph: 11111
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");
		}

		public void TestContactDetailsFax()
		{
			SetupAddressesPhone();
			AssertContactDetails(
@"Ph: 12348
Fax: 12349
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");

			contact.OC_Fax = ZString.Empty;
			contact.OC_OA_OrgAddress = address.PK;
			AssertContactDetails(
@"Ph: 12348
Fax: 22222
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");

			contact.OC_OA_OrgAddress = ZGuid.Empty;
			AssertContactDetails(
@"Ph: 12348
Fax: 11111
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");

			var anotherAddress = orgHeader.Addresses.AddNew();
			anotherAddress.OA_Phone = "33333";
			contact.OC_OA_OrgAddress = anotherAddress.PK;
			AssertContactDetails(
@"Ph: 12348
Fax: 11111
Em: a@b.com
Web: 
http://www.cargowise.com
Y
Y
Y
Y
Y");
		}

		#region Implementation

		void AssertContactDetails(string expectedCombinedString, ObjectForTest testObj = null)
		{
			if (testObj == null)
			{
				testObj = new ObjectForTest(Factory);
				testObj.TheContact = contact.PK;
				testObj.TheAddress = address.PK;
			}
			var addressWContact = new ZAddressWithContact(testObj.TheContactInfo, testObj.TheAddressInfo);

			string[] elements = new string[] { addressWContact.ContactDetails_Phone,
				addressWContact.ContactDetails_Fax,
				addressWContact.ContactDetails_Email,
				addressWContact.ContactDetails_Web,
				addressWContact.ContactDetails_WebLink,
				addressWContact.ContactDetails_PhoneEnabled ? "Y" : "N",
				addressWContact.ContactDetails_FaxVisible ? "Y" : "N",
				addressWContact.ContactDetails_EmailVisible ? "Y" : "N",
				addressWContact.ContactDetails_WebVisible ? "Y" : "N",
				addressWContact.ContactDetails_WebLinkVisible ? "Y" : "N" };

			AssertEquals(expectedCombinedString, String.Join(System.Environment.NewLine, elements));
		}

		class ObjectForTest : NonPersistentBusinessObject
		{
			public ObjectForTest(BusinessObjectFactory factory) : base(factory, null) { }

			#region TheContact

			ZGuid theContact;
			public virtual ZGuid TheContact
			{
				get { return theContact; }
				set { SetNonPersistentPropertyValue(TheContactInfo, ref theContact, value); }
			}
			public virtual ZPropertyInfo TheContactInfo
			{
				get { return GetZPropertyInfo(nameof(TheContact)); }
			}
			public bool TheContact_ReadOnly { get; set; }

			#endregion

			#region TheAddress

			ZGuid theAddress;
			public virtual ZGuid TheAddress
			{
				get { return theAddress; }
				set { SetNonPersistentPropertyValue(TheAddressInfo, ref theAddress, value); }
			}
			public virtual ZPropertyInfo TheAddressInfo
			{
				get { return GetZPropertyInfo(nameof(TheAddress)); }
			}

			#endregion
		}

		OrgHeader orgHeader;
		OrgContact contact;
		OrgAddress address;

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var url = orgHeader.OrgWebURLs.AddNew();
			url.PU_URL = "http://www.cargowise.com";
			url.PU_IsPrimary = true;
			url.PU_Description = "Main Website";
			url.PU_Type = OrgWebUrlList.Codes.MainWebsite;
			address = orgHeader.Addresses.AddNew();
			address.OA_Code = "Alexandria";
			address.OA_Address1 = "51 Some Street";
			contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Chris Noth";
			contact.OC_Phone = "12348";
			contact.OC_Email = "a@b.com";
			contact.OC_Fax = "12349";
			Factory.Save();
		}

		void SetupAddressesPhone()
		{
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Phone = "11111";
			mainAddress.OA_Fax = "11111";

			address.OA_Phone = "22222";
			address.OA_Fax = "22222";

			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testObj = new ObjectForTest(Factory);
			testObj.TheContact = contact.PK;
			testObj.TheAddress = address.PK;
			return new ZAddressWithContact(testObj.TheContactInfo, testObj.TheAddressInfo);
		}

		#endregion

	}
}
