using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

static class ContactPersonTestHelper
{
	internal const string TestContactName = "Contact Name";
	internal const string TestContactEmail = "email@email.com";
	internal const string TestContactPhone = "98765";

	internal static void FillWithValidData(OrgContact contact)
	{
		contact.OC_ContactName = TestContactName;
		contact.OC_Email = TestContactEmail;
		contact.OC_Mobile = TestContactPhone;
	}

	internal static void TestNewOrNull_OnePerson(OrgHeader orgHeader, Func<IContactPerson> test)
	{
		orgHeader.Contacts.RemoveAll();
		AssertNull("No contacts available", test());

		var contact = orgHeader.Contacts.AddNew();
		FillWithValidData(contact);
		AssertNotNull("Contact with valid data", test());

		contact.OC_ContactName = ZString.Empty;
		AssertNull("Contact name is empty", test());

		contact.OC_Mobile = ZString.Empty;
		contact.OC_Phone = ZString.Empty;
		contact.OC_HomePhone = ZString.Empty;
		contact.OC_OtherPhone = ZString.Empty;
		AssertNull("Contact phones are empty", test());

		FillWithValidData(contact);
		AssertNotNull("Valid 1 contact in Contacts", test());
	}

	internal static void TestNewOrNull_MultiplePersonsWithAllocations(OrgHeader orgHeader, Func<IContactPerson> test)
	{
		orgHeader.Contacts.RemoveAll();

		var contact = orgHeader.Contacts.AddNew();
		FillWithValidData(contact);

		const string testContactName2 = "Test Contact 2";
		var secondContact = orgHeader.Contacts.AddNew();
		secondContact.OC_ContactName = testContactName2;
		secondContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		secondContact.OC_IsActive = ZBool.False;
		AssertNotNull("first contact valid; second is invalid, CUS, inactive. Check NewOrNull not null.", test());
		AssertEquals("first contact valid; second is invalid, CUS, inactive. Check NewOrNull result name is first contact name", TestContactName, test().Name);

		secondContact.OC_Phone = "7777";
		AssertNotNull("first contact valid; second is valid, CUS, inactive. Check NewOrNull not null.", test());
		AssertEquals("first contact valid; second is valid, CUS, inactive. Check NewOrNull result name is first contact name", TestContactName, test().Name);

		secondContact.OC_IsActive = ZBool.True;
		AssertNotNull("first contact valid; second is valid, CUS, active. Check NewOrNull not null.", test());
		AssertEquals("first contact valid; second is valid, CUS, active. Check NewOrNull result name is second contact name", testContactName2, test().Name);

		secondContact.Allocations[0].PC_Type = OrgConstants.ContactAllocationType.HAZ;
		AssertNull("first contact valid; second is valid, non-CUS, active", test());

		const string testContactName3 = "Test Contact 3";
		var thirdContact = orgHeader.Contacts.AddNew();
		thirdContact.OC_ContactName = testContactName3;
		thirdContact.OC_Phone = "8888";
		thirdContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		AssertNotNull("first contact valid; second is valid, non-CUS, active; third is valid, CUS, active. Check NewOrNull not null.", test());
		AssertEquals("first contact valid; second is valid, non-CUS, active; third is valid, CUS, active. Check NewOrNull result name is third contact name", testContactName3, test().Name);

		thirdContact.OC_IsActive = ZBool.False;
		AssertNull("first contact valid; second is valid, non-CUS, active; third is valid, CUS, inactive", test());
	}
}
