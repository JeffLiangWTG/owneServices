using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactDependentCollection))]
	sealed class OrgContactDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNewAndRemove()
		{
			bool oldContactSecurityValue = Env.Security.OrgContactModifyContactDetails.IsAllowed;
			Org.FillWithValidTestData();
			Factory.Save();
			try
			{
				Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", TestCollection.AllowNew);
				Assert("Access Allowed - Not ReadOnly", TestCollection.AllowRemove);

				Env.Security.OrgContactModifyContactDetails.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", !TestCollection.AllowNew);
				Assert("Access NOT Allowed - ReadOnly", !TestCollection.AllowRemove);
			}
			finally
			{
				Env.Security.OrgContactModifyContactDetails.IsAllowed = oldContactSecurityValue;
			}
		}

		public void TestModifyingContactListUpdatesSecurityListing()
		{
			var org = OrgHeader.New(Factory);
			var rightsList = AllWebSecurityRights.New(Factory);
			AssertEquals("Number of security items is correct", rightsList.Count, org.SecurityRights.Count);

			foreach (OrgSecurity security in org.SecurityRights)
			{
				AssertEquals("Each security right has NO contact rights as no contacts exist", 0, security.ContactSecurityRights.Count);
			}

			OrgContact contact = org.Contacts.AddNew();
			foreach (OrgSecurity security in org.SecurityRights)
			{
				AssertEquals("Each security right has correct 1 contact right as 1 contact exists", 1, security.ContactSecurityRights.Count);
			}

			AssertEquals("Contact has correct number of security rights", rightsList.Count, contact.SecurityRightsForBindingOnly.Count);
		}

		public void TestAddNew_ShouldNotLoadSecurityRightsIfWasntPreviouslyLoaded()
		{
			Org.FillWithValidTestData();
			var securityRight = Org.SecurityRights.AddNew();
			securityRight.FillWithValidTestData();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var orgInOtherFactory = anotherFactory.Load<OrgHeader>(Org.PK);

			AssertTableHitCount("Precondition", 0, OrgSecuritySchema.Constants.TableName, anotherFactory);

			orgInOtherFactory.Contacts.AddNew();

			AssertTableHitCount(0, OrgSecuritySchema.Constants.TableName, anotherFactory);
		}

		public void TestSetDocGroupAsDefault()
		{
			OrgContact contact1 = TestCollection.AddNew();
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_DefaultContact = true;
			contact1.Documents.AddNew();
			contact1.Documents[1].OD_DocumentGroup = ContactType.Consignee.Code;
			contact1.Documents[1].OD_DefaultContact = false;

			OrgContact contact2 = TestCollection.AddNew();
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact2.Documents[0].OD_DefaultContact = true;

			Assert("No Notify Party, so set as default", TestCollection.SetDocGroupAsDefault(ContactType.NotifyParty.Code));
			Assert("Consignor already defaulted, so don't set as default", !TestCollection.SetDocGroupAsDefault(ContactType.Consignor.Code));
			Assert("Consignee already defaulted, so don't set as default", !TestCollection.SetDocGroupAsDefault(ContactType.Consignee.Code));

			contact2.Documents[0].OD_DefaultContact = false;
			Assert("Neither Consignee defaulted, so set as default", TestCollection.SetDocGroupAsDefault(ContactType.Consignee.Code));
		}

		public void TestDescriptionFromCode()
		{
			OrgContact nonRelatedContact = Factory.New<OrgContact>();
			nonRelatedContact.OC_ContactName = "MATT";
			nonRelatedContact.OC_Title = "CEO 1";

			OrgContact relatedContact = Factory.New<OrgContact>();
			relatedContact.OC_ContactName = "MATT";
			relatedContact.OC_Title = "CEO 2";
			relatedContact.OC_OH = Org.PK;

			string result = ((IFindBoxListProvider)TestCollection).DescriptionFromCode("MATT");
			AssertEquals("Filtered Description Found", "CEO 2", result);
		}

		public void TestNearestMatch()
		{
			OrgContact nonRelatedContact = Factory.New<OrgContact>();
			nonRelatedContact.OC_ContactName = "MATT 1";

			OrgContact relatedContact = Factory.New<OrgContact>();
			relatedContact.OC_ContactName = "MATT 2";
			relatedContact.OC_OH = Org.PK;

			string result = ((IFindBoxListProvider)TestCollection).NearestMatch("MATT", true, -1).Item1;
			AssertEquals("Filtered Description Found", "MATT 2", result);
		}

		public void TestGetPKFromCode()
		{
			OrgHeader someOtherOrg = Factory.New<OrgHeader>();

			OrgContact relatedContact = Factory.New<OrgContact>();
			relatedContact.OC_ContactName = "MATT";
			relatedContact.OC_OH = Org.PK;

			OrgContact nonRelatedContact = Factory.New<OrgContact>();
			nonRelatedContact.OC_ContactName = "MATT";
			nonRelatedContact.OC_OH = someOtherOrg.PK;

			ZGuid result = ((IFindBoxListProvider)TestCollection).PrimaryKeyFromCode("MATT");
			AssertEquals("Correct PK Found", relatedContact.PK, result);
		}

		public void TestFormatAllContactPhoneNumbers()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(false);

			OrgContact contact1 = TestCollection.AddNew();
			contact1.OC_ContactName = "Apple 201508";
			contact1.OC_Mobile = "0414 123 987";
			contact1.OC_Phone = "02 2200 3300";
			contact1.OC_Fax = "61 2 9025 1100";

			OrgContact contact2 = TestCollection.AddNew();
			contact2.OC_ContactName = "Banana 201508";
			contact2.OC_Mobile = "0414 123 981";
			contact2.OC_Phone = "02 2200 3301";
			contact2.OC_Fax = "61 2 9025 1101";

			TestCollection.FormatAllContactPhoneNumbers();
			AssertEquals("Number should be not changed", "0414 123 987", contact1.OC_Mobile);
			AssertEquals("Number should be not changed", "02 2200 3300", contact1.OC_Phone);
			AssertEquals("Number should be not changed", "61 2 9025 1100", contact1.OC_Fax);
			AssertEquals("Number should be not changed", "0414 123 981", contact2.OC_Mobile);
			AssertEquals("Number should be not changed", "02 2200 3301", contact2.OC_Phone);
			AssertEquals("Number should be not changed", "61 2 9025 1101", contact2.OC_Fax);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			TestCollection.FormatAllContactPhoneNumbers();
			AssertEquals("Number should be formatted", "+61 (414) 123-987", contact1.OC_Mobile);
			AssertEquals("Number should be formatted", "+61 (2) 2200-3300", contact1.OC_Phone);
			AssertEquals("Number should be formatted", "+61 (2) 9025-1100", contact1.OC_Fax);
			AssertEquals("Number should be formatted", "+61 (414) 123-981", contact2.OC_Mobile);
			AssertEquals("Number should be formatted", "+61 (2) 2200-3301", contact2.OC_Phone);
			AssertEquals("Number should be formatted", "+61 (2) 9025-1101", contact2.OC_Fax);
		}

		public void TestGetContactForAllocation()
		{
			var contactFSV = TestCollection.AddNew();
			var alloc = contactFSV.Allocations.AddNew();
			alloc.PC_Type = OrgConstants.ContactAllocationType.USFSV;

			var contactPGA = TestCollection.AddNew();
			var alloc2 = contactPGA.Allocations.AddNew();
			alloc2.PC_Type = OrgConstants.ContactAllocationType.USPGA;

			AssertEquals(contactFSV, TestCollection.GetContactForAllocation(OrgConstants.ContactAllocationType.USFSV));
			AssertEquals(contactPGA, TestCollection.GetContactForAllocation(OrgConstants.ContactAllocationType.USPGA));
		}

		#region Implementation

		OrgHeader Org;
		OrgContactDependentCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			TestCollection = Org.Contacts;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Org = Factory.New<OrgHeader>();
			TestCollection = Org.Contacts;
			return TestCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(OrgContact));
		}

		#endregion
	}
}
