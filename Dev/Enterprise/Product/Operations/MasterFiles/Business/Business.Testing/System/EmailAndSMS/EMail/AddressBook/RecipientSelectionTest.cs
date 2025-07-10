using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RecipientSelection))]
	sealed class RecipientSelectionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RecipientSelection(new AddressBookSelection());
		}

		public void TestAppendedAddressLines()
		{
			var addressBook = AddressBookSelection;
			var recipientSelection = new RecipientSelection(addressBook);
			var rec1 = new AddressBookRecipient(recipient1);

			AssertEquals("Precondition", "", recipientSelection.ToEmailAddress);
			recipientSelection.AppendRecipientsEmailToEmailAddress(new[] { rec1, rec1, rec1 });
			AssertEquals(rec1.Email, recipientSelection.ToEmailAddress);

			AssertEquals("Precondition", "", recipientSelection.Cc);
			recipientSelection.AppendRecipientsEmailToCc(new[] { rec1, rec1, rec1 });
			AssertEquals(rec1.Email, recipientSelection.Cc);

			AssertEquals("Precondition", "", recipientSelection.Bcc);
			recipientSelection.AppendRecipientsEmailToBcc(new[] { rec1, rec1, rec1 });
			AssertEquals(rec1.Email, recipientSelection.Bcc);
		}

		[ExpectNoExceptions]
		public void TestAppendedAddressLines_NotExceedMaximumLength()
		{
			var addressBook = AddressBookSelection;
			var recipientSelection = new RecipientSelection(addressBook);
			var rec1 = new AddressBookRecipient(recipient1);

			var expectedAddressLine = new string('t', recipientSelection.ToEmailAddressInfo.MaxLength);
			recipientSelection.ToEmailAddress = expectedAddressLine;
			recipientSelection.AppendRecipientsEmailToEmailAddress(new[] { rec1 });
			AssertEquals(expectedAddressLine, recipientSelection.ToEmailAddress);

			expectedAddressLine = new string('c', recipientSelection.CcInfo.MaxLength);
			recipientSelection.Cc = expectedAddressLine;
			recipientSelection.AppendRecipientsEmailToCc(new[] { rec1 });
			AssertEquals(expectedAddressLine, recipientSelection.Cc);

			expectedAddressLine = new string('b', recipientSelection.BccInfo.MaxLength);
			recipientSelection.Bcc = expectedAddressLine;
			recipientSelection.AppendRecipientsEmailToBcc(new[] { rec1 });
			AssertEquals(expectedAddressLine, recipientSelection.Bcc);
		}

		public void TestUpdatingAvailableRecipientsByAddressBook()
		{
			var addressBook = AddressBookSelection;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "Org1";
			var orgRecipient1 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), org);
			var orgRecipient2 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), org);
			addressBook.AddRecipient(orgRecipient1);
			addressBook.AddRecipient(orgRecipient2);

			var recipientSelection = new RecipientSelection(addressBook);
			AssertEquals("Precondition", AddressBookSelection.AddressBookCodes.All, recipientSelection.AddressBook);
			AssertEquals("Precondition", 11, recipientSelection.AvailableRecipients.Count);

			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org", 2, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgRecipient1, recipientSelection.AvailableRecipients);
			AssertCollectionContains(orgRecipient2, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments)", 9, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient1, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient2, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient3, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient4, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient7, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.All;
			AssertEquals(11, recipientSelection.AvailableRecipients.Count);
		}

		public void TestUpdatingAvailableRecipientsByAddressBook_SearchFilterOn()
		{
			var addressBook = AddressBookSelection;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "Org1";
			var orgRecipient1 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), org, name: "aaa1", phone: "", location: "", title: "", role: "", email: "");
			var orgRecipient2 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), org, name: "eee1", phone: "", location: "", title: "", role: "", email: "");
			addressBook.AddRecipient(orgRecipient1);
			addressBook.AddRecipient(orgRecipient2);

			var recipientSelection = new RecipientSelection(addressBook);
			AssertEquals("Precondition", AddressBookSelection.AddressBookCodes.All, recipientSelection.AddressBook);
			AssertEquals("Precondition", 11, recipientSelection.AvailableRecipients.Count);

			recipientSelection.SearchQuery = "aaa";
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with search == aaa", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgRecipient1, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with search == aaa", 3, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient4, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);

			recipientSelection.SearchQuery = "eee";
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with search == eee", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgRecipient2, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with search == eee", 4, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient2, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient3, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient7, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
		}

		public void TestUpdatingAvailableRecipientsByAddressBook_AdvancedSearchFilterOn()
		{
			var addressBook = AddressBookSelection;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "Org1";
			var orgRecipient = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), org, name: "aaa1", phone: "12", location: "street", title: "", role: "", email: "");
			var orgStaffRecipient = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), org, name: "eee1", phone: "98", location: "road", title: "nnooo title", role: StaffAssignmentRoles.Codes.SalesRep, email: "");
			addressBook.AddRecipient(orgRecipient);
			addressBook.AddRecipient(orgStaffRecipient);

			var recipientSelection = new RecipientSelection(addressBook);
			AssertEquals("Precondition", AddressBookSelection.AddressBookCodes.All, recipientSelection.AddressBook);
			AssertEquals("Precondition", 11, recipientSelection.AvailableRecipients.Count);

			recipientSelection.LocationQuery = "stree";
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with Location == stree", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgRecipient, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with Location == stree", 5, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient7, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = "nn";
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with Title == nn", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgStaffRecipient, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with Title == nn", 4, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = string.Empty;
			recipientSelection.PhoneQuery = "12";
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with Phone == 12", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgRecipient, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with Phone == 12", 3, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient1, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient2, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient3, recipientSelection.AvailableRecipients);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = string.Empty;
			recipientSelection.PhoneQuery = string.Empty;
			recipientSelection.RoleQuery = StaffAssignmentRoles.Codes.SalesRep;
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with Role == SalesRep", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgStaffRecipient, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with Role == SalesRep", 2, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = string.Empty;
			recipientSelection.PhoneQuery = string.Empty;
			recipientSelection.RoleQuery = string.Empty;
			recipientSelection.NameQuery = "aaa";
			recipientSelection.AddressBook = org.OH_Code;
			AssertEquals("Number of recipents in org with name == aaa", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(orgRecipient, recipientSelection.AvailableRecipients);

			recipientSelection.AddressBook = AddressBookSelection.AddressBookCodes.Staff;
			AssertEquals("Number of recipients not in orgs (e.g. staff who are not org staff assignments) with name == aaa", 3, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient4, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
		}

		public void TestSearch()
		{
			var recipientSelection = new RecipientSelection(AddressBookSelection);
			AssertEquals("Precondition", 9, recipientSelection.AvailableRecipients.Count);

			recipientSelection.SearchQuery = "aaa";
			recipientSelection.Search();
			AssertEquals("Name: aaa", 3, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient4, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);

			recipientSelection.SearchQuery = "EEE";
			recipientSelection.Search();
			AssertEquals("Name: EEE", 4, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient2, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient3, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient7, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);

			recipientSelection.SearchQuery = "";
			recipientSelection.Search();
			AssertEquals("Blank simple criteria", 9, recipientSelection.AvailableRecipients.Count);
		}

		public void TestSearch_ResetAdvancedSearch()
		{
			var recipientSelection = new RecipientSelection(AddressBookSelection);

			recipientSelection.LocationQuery = "stree";
			recipientSelection.SearchQuery = "something else";
			recipientSelection.Search();
			AssertEquals(string.Empty, recipientSelection.LocationQuery);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = "nn";
			recipientSelection.SearchQuery = "something else";
			recipientSelection.Search();
			AssertEquals(string.Empty, recipientSelection.TitleQuery);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = string.Empty;
			recipientSelection.PhoneQuery = "98";
			recipientSelection.SearchQuery = "something else";
			recipientSelection.Search();
			AssertEquals(string.Empty, recipientSelection.PhoneQuery);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = string.Empty;
			recipientSelection.PhoneQuery = string.Empty;
			recipientSelection.RoleQuery = StaffAssignmentRoles.Codes.SalesRep;
			recipientSelection.SearchQuery = "something else";
			recipientSelection.Search();
			AssertEquals(string.Empty, recipientSelection.RoleQuery);

			recipientSelection.LocationQuery = string.Empty;
			recipientSelection.TitleQuery = string.Empty;
			recipientSelection.PhoneQuery = string.Empty;
			recipientSelection.RoleQuery = string.Empty;
			recipientSelection.NameQuery = "EE";
			recipientSelection.SearchQuery = "something else";
			recipientSelection.Search();
			AssertEquals(string.Empty, recipientSelection.NameQuery);
		}

		public void TestAdvancedSearch()
		{
			var recipientSelection = new RecipientSelection(AddressBookSelection);
			AssertEquals("Precondition", 9, recipientSelection.AvailableRecipients.Count);

			recipientSelection.LocationQuery = "stree";
			recipientSelection.AdvancedSearch();
			AssertEquals("Location == stree", 5, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient7, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.TitleQuery = "nn";
			recipientSelection.AdvancedSearch();
			AssertEquals("Location == stree && Title == nn", 4, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient5, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.PhoneQuery = "98";
			recipientSelection.AdvancedSearch();
			AssertEquals("Location == stree && Title == nn && Phone == 98", 3, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient6, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.RoleQuery = StaffAssignmentRoles.Codes.SalesRep;
			recipientSelection.AdvancedSearch();
			AssertEquals("Location == stree && Title == nn && Phone == 98 && Role == SalesRep", 2, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);
			AssertCollectionContains(recipient9, recipientSelection.AvailableRecipients);

			recipientSelection.NameQuery = "EE";
			recipientSelection.AdvancedSearch();
			AssertEquals("Location == stree && Title == nn && Phone == 98 && Role == SalesRep && Name == EE", 1, recipientSelection.AvailableRecipients.Count);
			AssertCollectionContains(recipient8, recipientSelection.AvailableRecipients);

			recipientSelection.LocationQuery = "";
			recipientSelection.TitleQuery = "";
			recipientSelection.PhoneQuery = "";
			recipientSelection.RoleQuery = "";
			recipientSelection.NameQuery = "";
			recipientSelection.AdvancedSearch();
			AssertEquals("Blank advanced criteria", 9, recipientSelection.AvailableRecipients.Count);
		}

		public void TestAdvancedSearch_ResetSearch()
		{
			var recipientSelection = new RecipientSelection(AddressBookSelection);

			recipientSelection.SearchQuery = "name";
			recipientSelection.LocationQuery = "stree";
			recipientSelection.AdvancedSearch();
			AssertEquals(string.Empty, recipientSelection.SearchQuery);

			recipientSelection.SearchQuery = "name";
			recipientSelection.TitleQuery = "nn";
			recipientSelection.AdvancedSearch();
			AssertEquals(string.Empty, recipientSelection.SearchQuery);

			recipientSelection.SearchQuery = "name";
			recipientSelection.PhoneQuery = "98";
			recipientSelection.AdvancedSearch();
			AssertEquals(string.Empty, recipientSelection.SearchQuery);

			recipientSelection.SearchQuery = "name";
			recipientSelection.RoleQuery = StaffAssignmentRoles.Codes.SalesRep;
			recipientSelection.AdvancedSearch();
			AssertEquals(string.Empty, recipientSelection.SearchQuery);

			recipientSelection.SearchQuery = "name";
			recipientSelection.NameQuery = "EE";
			recipientSelection.AdvancedSearch();
			AssertEquals(string.Empty, recipientSelection.SearchQuery);
		}

		public void TestResetAdvancedSearchQueries()
		{
			var recipientSelection = new RecipientSelection(AddressBookSelection);

			recipientSelection.LocationQuery = "stree";
			recipientSelection.ResetAdvancedSearchQueries();
			AssertEquals(string.Empty, recipientSelection.LocationQuery);

			recipientSelection.TitleQuery = "nn";
			recipientSelection.ResetAdvancedSearchQueries();
			AssertEquals(string.Empty, recipientSelection.TitleQuery);
			recipientSelection.PhoneQuery = "98";
			recipientSelection.ResetAdvancedSearchQueries();
			AssertEquals(string.Empty, recipientSelection.PhoneQuery);

			recipientSelection.RoleQuery = StaffAssignmentRoles.Codes.SalesRep;
			recipientSelection.ResetAdvancedSearchQueries();
			AssertEquals(string.Empty, recipientSelection.RoleQuery);

			recipientSelection.NameQuery = "EE";
			recipientSelection.ResetAdvancedSearchQueries();
			AssertEquals(string.Empty, recipientSelection.NameQuery);
		}

		public void TestRetainPreviousAdvancedSearchQueries_RestorePreviousAdvancedSearchQueriesIfCancelled()
		{
			var recipientSelection = new RecipientSelection(AddressBookSelection);
			recipientSelection.LocationQuery = "previous location";
			recipientSelection.TitleQuery = "previous title";
			recipientSelection.PhoneQuery = "previous phone";
			recipientSelection.RoleQuery = StaffAssignmentRoles.Codes.SalesRep;
			recipientSelection.NameQuery = "previous name";

			recipientSelection.RetainPreviousAdvancedSearchQueries();

			recipientSelection.LocationQuery = "location";
			recipientSelection.TitleQuery = "title";
			recipientSelection.PhoneQuery = "phone";
			recipientSelection.RoleQuery = "";
			recipientSelection.NameQuery = "name";
			AssertEquals("Precondition", "location", recipientSelection.LocationQuery);
			AssertEquals("Precondition", "title", recipientSelection.TitleQuery);
			AssertEquals("Precondition", "phone", recipientSelection.PhoneQuery);
			AssertEquals("Precondition", "", recipientSelection.RoleQuery);
			AssertEquals("Precondition", "name", recipientSelection.NameQuery);

			recipientSelection.RestorePreviousAdvancedSearchQueriesIfCancelled();
			AssertEquals("previous location", recipientSelection.LocationQuery);
			AssertEquals("previous title", recipientSelection.TitleQuery);
			AssertEquals("previous phone", recipientSelection.PhoneQuery);
			AssertEquals(StaffAssignmentRoles.Codes.SalesRep, recipientSelection.RoleQuery);
			AssertEquals("previous name", recipientSelection.NameQuery);
		}

		public void TestRemoveRecipientsEmailFromEmailAddress()
		{
			var recipients = new[] { recipient1, recipient2 };
			var recipientSelection = new RecipientSelection(AddressBookSelection);
			recipientSelection.AppendRecipientsEmailToEmailAddress(recipients);
			AssertEquals("1@email.com;2@email.com", recipientSelection.ToEmailAddress);

			recipientSelection.RemoveRecipientsEmailFromEmailAddress(recipients);
			AssertEquals("", recipientSelection.ToEmailAddress);
		}

		public void TestRemoveRecipientsEmailFromCc()
		{
			var recipients = new[] { recipient1, recipient2 };
			var recipientSelection = new RecipientSelection(AddressBookSelection);
			recipientSelection.AppendRecipientsEmailToCc(recipients);
			AssertEquals("1@email.com;2@email.com", recipientSelection.Cc);

			recipientSelection.RemoveRecipientsEmailFromCc(recipients);
			AssertEquals("", recipientSelection.Cc);
		}

		public void TestRemoveRecipientsEmailFromBcc()
		{
			var recipients = new[] { recipient1, recipient2 };
			var recipientSelection = new RecipientSelection(AddressBookSelection);
			recipientSelection.AppendRecipientsEmailToBcc(recipients);
			AssertEquals("1@email.com;2@email.com", recipientSelection.Bcc);

			recipientSelection.RemoveRecipientsEmailFromBcc(recipients);
			AssertEquals("", recipientSelection.Bcc);
		}

		AddressBookSelection AddressBookSelection
		{
			get
			{
				if (addressBookSelection == null)
				{
					addressBookSelection = new AddressBookSelection();
					addressBookSelection.AddRecipient(recipient1);
					addressBookSelection.AddRecipient(recipient2);
					addressBookSelection.AddRecipient(recipient3);
					addressBookSelection.AddRecipient(recipient4);
					addressBookSelection.AddRecipient(recipient5);
					addressBookSelection.AddRecipient(recipient6);
					addressBookSelection.AddRecipient(recipient7);
					addressBookSelection.AddRecipient(recipient8);
					addressBookSelection.AddRecipient(recipient9);
				}
				return addressBookSelection;
			}
		}
		AddressBookSelection addressBookSelection;
		// Make test recipients look different by having a different PK so they can be added to the
		// AddressBookRecipientCollection (because it does not add the same recipient (via PK & Address Book) twice).
		readonly AddressBookRecipient recipient1 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "123", "a`a`a", "1 Road", "zzz", "", "1@email.com");
		readonly AddressBookRecipient recipient2 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "123", "EEE", "1 ROAD", "ZZZ", "", "2@email.com");
		readonly AddressBookRecipient recipient3 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "123", "EEE", "1 Road", "XXX", "", "3@email.com");
		readonly AddressBookRecipient recipient4 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "041", "AAA", "1 road", "xxx", "", "4@email.com");
		readonly AddressBookRecipient recipient5 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "041", "aaa", "Street", "nNn", "", "5@email.com");
		readonly AddressBookRecipient recipient6 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "987", "AAA", "street", "NnN", "", "6@email.com");
		readonly AddressBookRecipient recipient7 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "987", "eee", "STREET", "n~nn", StaffAssignmentRoles.Codes.ProjectManager, "7@email.com");
		readonly AddressBookRecipient recipient8 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "987", "eee", "Street", "NNN", StaffAssignmentRoles.Codes.SalesRep, "8@email.com");
		readonly AddressBookRecipient recipient9 = AddressBookRecipientHelper.CreateRecipient(ZGuid.NewZGuid(), null, "987", "e?ee", "Street", "nnn", StaffAssignmentRoles.Codes.SalesRep, "9@email.com");
	}
}
