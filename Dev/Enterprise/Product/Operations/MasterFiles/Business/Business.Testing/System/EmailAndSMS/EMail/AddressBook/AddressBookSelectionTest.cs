using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AddressBookSelection))]
	sealed class AddressBookSelectionTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddressBookSelection();
		}

		#endregion

		public void TestAddRecipient_Contact()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgContact contact1 = header.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			OrgContact contact2 = header.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			OrgContact contact3 = header.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";
			contact3.OC_IsActive = false;

			AddressBookSelection selection = new AddressBookSelection();
			selection.AddRecipient(contact1);
			selection.AddRecipient(contact2);
			selection.AddRecipient(contact3);

			AssertEquals("Active contacts are added only", 2, selection.Recipients.Count);
			AssertEquals(contact1.OC_ContactName, selection.Recipients[0].Name);
			AssertEquals(contact1.OC_Email, selection.Recipients[0].Email);
			AssertEquals(contact2.OC_ContactName, selection.Recipients[1].Name);
			AssertEquals(contact2.OC_Email, selection.Recipients[1].Email);
		}

		public void TestAddRecipient_Org()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "staff1";
			staff1.GS_EmailAddress = "staff1@mail.ru";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "staff2";
			staff2.GS_EmailAddress = "staff2@mail.ru";
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_FullName = "staff3";
			staff3.GS_EmailAddress = "staff3@mail.ru";
			staff3.GS_IsActive = false;
			OrgHeader header = Factory.New<OrgHeader>();
			OrgContact contact1 = header.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			OrgContact contact2 = header.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			OrgContact contact3 = header.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";
			contact3.OC_IsActive = false;
			OrgStaffAssignments assignment1 = header.StaffAssignments.AddNew();
			assignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			OrgStaffAssignments assignment2 = header.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			OrgStaffAssignments assignment3 = header.StaffAssignments.AddNew();
			assignment3.O8_GS_NKPersonResponsible = staff3.GS_Code;
			OrgStaffAssignments assignment4 = header.StaffAssignments.AddNew();
			AssertEquals("Precondition: 3 Contacts (inc. inactive)", 3, header.Contacts.Count);
			AssertEquals("Precondition: 4 StaffAssignments (inc. inactive and w/ no person responsible) in Assignments List", 4, header.StaffAssignments.Count);

			AddressBookSelection selection = new AddressBookSelection();
			selection.AddRecipient(header);
			AssertEquals(4, selection.Recipients.Count);
			AssertEquals(contact1.OC_ContactName, selection.Recipients[0].Name);
			AssertEquals(contact1.OC_Email, selection.Recipients[0].Email);
			AssertEquals(contact2.OC_ContactName, selection.Recipients[1].Name);
			AssertEquals(contact2.OC_Email, selection.Recipients[1].Email);
			AssertEquals(staff1.GS_FullName, selection.Recipients[2].Name);
			AssertEquals(staff1.GS_EmailAddress, selection.Recipients[2].Email);
			AssertEquals(staff2.GS_FullName, selection.Recipients[3].Name);
			AssertEquals(staff2.GS_EmailAddress, selection.Recipients[3].Email);
		}
	}
}
