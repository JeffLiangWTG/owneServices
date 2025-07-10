using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgContactSupersedeHelperTest : TestCaseWithFactory
	{
		public void TestDeactivateWithRedirection()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 2";
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 1";
			contact3.OC_IsActive = true;
			contact3.OC_PER = contact1.OC_PER;
			contact3.OC_WebAccessEnabled = true;
			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "Mr 2";
			contact4.OC_IsActive = false;
			contact4.OC_PER = contact2.OC_PER;
			contact4.OC_WebAccessEnabled = true;
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2 });

			AssertEquals("Should not be deactivated with redirection since no was selected", false, contact1.WebAccessSuperseded);
			AssertEquals("Should not be deactivated with redirection since no was selected", true, contact1.OC_IsActive);
			AssertEquals("Should not be deactivated with redirection since no was selected", false, contact2.WebAccessSuperseded);
			AssertEquals("Should not be deactivated with redirection since no was selected", true, contact2.OC_IsActive);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2 });

			AssertEquals("Should be deactivated with redirection since yes was selected", true, contact1.WebAccessSuperseded);
			AssertEquals("Should be deactivated with redirection since yes was selected", false, contact1.OC_IsActive);
			AssertEquals("Should be hard deactivated since its other web access contact was inactive", false, contact2.WebAccessSuperseded);
			AssertEquals("Should be hard deactivated since its other web access contact was inactive", false, contact2.OC_IsActive);
			AssertEquals("Should display message for contacts without other active web access contacts", FormattableString.Invariant($"The following contacts will be deactivated (without superseding their web access) because there are no other active web access contacts for the Person:\r\n{contact2.OC_ContactName}"), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeactivateWithRedirectionShouldSkipInactiveContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 3";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = false;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 4";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;
			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_ContactName = "Mr 1";
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_PER = contact1.OC_PER;
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2, contact3 });

			AssertEquals("Nothing should change since cancel was clicked", false, contact1.WebAccessSuperseded);
			AssertEquals("Nothing should change since cancel was clicked", true, contact1.OC_IsActive);
			AssertEquals("Nothing should change since cancel was clicked", false, contact2.WebAccessSuperseded);
			AssertEquals("Nothing should change since cancel was clicked", false, contact2.OC_IsActive);
			AssertEquals("Nothing should change since cancel was clicked", false, contact3.WebAccessSuperseded);
			AssertEquals("Nothing should change since cancel was clicked", false, contact3.OC_IsActive);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2, contact3 });

			AssertEquals("Should be deactivated with redirection", true, contact1.WebAccessSuperseded);
			AssertEquals("Should be deactivated with redirection", false, contact1.OC_IsActive);
			AssertEquals("Should stay hard deactivated", false, contact2.WebAccessSuperseded);
			AssertEquals("Should stay hard deactivated", false, contact2.OC_IsActive);
			AssertEquals("Should stay hard deactivated", false, contact3.WebAccessSuperseded);
			AssertEquals("Should stay hard deactivated", false, contact3.OC_IsActive);

			AssertEquals("Should display message for skipped inactive contacts", FormattableString.Invariant($"The following contacts will be skipped because they are already inactive:\r\n{contact2.OC_ContactName}\r\n{contact3.OC_ContactName}"), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeactivateWithRedirectionShouldDeactivateContactsWithNoOtherWebAccessContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Mr 2";
			contact2.OC_WebAccessEnabled = true;
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 3";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2, contact3 });
			AssertEquals("Nothing should change since cancel was clicked", false, contact1.WebAccessSuperseded);
			AssertEquals("Nothing should change since cancel was clicked", true, contact1.OC_IsActive);
			AssertEquals("Nothing should change since cancel was clicked", false, contact2.WebAccessSuperseded);
			AssertEquals("Nothing should change since cancel was clicked", true, contact2.OC_IsActive);
			AssertEquals("Nothing should change since cancel was clicked", false, contact3.WebAccessSuperseded);
			AssertEquals("Nothing should change since cancel was clicked", false, contact3.OC_IsActive);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2, contact3 });

			AssertEquals("Should be hard deactivated", false, contact1.WebAccessSuperseded);
			AssertEquals("Should be hard deactivated", false, contact1.OC_IsActive);
			AssertEquals("Should be hard deactivated", false, contact2.WebAccessSuperseded);
			AssertEquals("Should be hard deactivated", false, contact2.OC_IsActive);
			AssertEquals("Nothing should change since contact is inactive", false, contact3.WebAccessSuperseded);
			AssertEquals("Nothing should change since contact is inactive", false, contact3.OC_IsActive);

			AssertEquals("Should display message for contacts without other web access contacts", FormattableString.Invariant($"The following contacts will be deactivated (without superseding their web access) because there are no other active web access contacts for the Person:\r\n{contact1.OC_ContactName}\r\n{contact2.OC_ContactName}"), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSupersedeContactsAsPerDebtorTypeOrgRequireARContact()
		{
			var arDocument1 = Factory.NewWithValidTestData<OrgDocument>();
			arDocument1.OD_DocumentGroup = ContactType.Receivables.Code;

			var arDocument2 = Factory.NewWithValidTestData<OrgDocument>();
			arDocument2.OD_DocumentGroup = ContactType.Receivables.Code;

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			arDocument1.OD_OC = contact1.PK;
			contact1.Documents.Add(arDocument1);
			contact1.OC_IsActive = true;

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			arDocument2.OD_OC = contact2.PK;
			contact2.Documents.Add(arDocument2);
			contact2.OC_IsActive = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = false;
			org.OH_IsTempAccount = true;
			org.Contacts.Add(contact1);
			org.Contacts.Add(contact2);

			CombineAssertions("Preconditions: ", () =>
			{
				AssertEquals(true,
					contact1.Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == ContactType.Receivables.Code)
					&& contact2.Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == ContactType.Receivables.Code));
				AssertEquals(false, org.OH_IsDebtor);
				AssertEquals(true, org.OH_IsTempAccount);
				AssertEquals(org, contact1.ParentOrg);
				AssertEquals(org, contact2.ParentOrg);
				AssertEquals(2, org.GetActiveContacts().Count);
				AssertEquals(true, contact1.OC_IsActive && contact2.OC_IsActive);
			});

			using (RawDataRegistry.Instance.TempOrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, true).GetRegistryValue()))
			{
				AssertSupersedeContactsIfNotRequireARContact(contact1, contact2, org);
			}

			contact1.OC_IsActive = true;
			contact2.OC_IsActive = true;
			org.OH_IsDebtor = true;
			AssertEquals("Preconditions: ", true, contact1.OC_IsActive && contact2.OC_IsActive && org.OH_IsDebtor);

			using (RawDataRegistry.Instance.TempOrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, true).GetRegistryValue()))
			{
				AssertSupersedeContactsIfRequireARContact(contact1, contact2, org);
			}

			contact1.OC_IsActive = true;
			contact2.OC_IsActive = true;
			AssertEquals("Preconditions: ", true, contact1.OC_IsActive && contact2.OC_IsActive);

			using (RawDataRegistry.Instance.TempOrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false).GetRegistryValue()))
			{
				AssertSupersedeContactsIfNotRequireARContact(contact1, contact2, org);
			}

			org.OH_IsTempAccount = false;
			contact1.OC_IsActive = true;
			contact2.OC_IsActive = true;
			AssertEquals("Preconditions: ", true, contact1.OC_IsActive && contact2.OC_IsActive && !org.OH_IsTempAccount);

			using (RawDataRegistry.Instance.OrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false).GetRegistryValue()))
			{
				AssertSupersedeContactsIfNotRequireARContact(contact1, contact2, org);
			}

			contact1.OC_IsActive = true;
			contact2.OC_IsActive = true;
			AssertEquals("Preconditions: ", true, contact1.OC_IsActive && contact2.OC_IsActive);

			using (RawDataRegistry.Instance.OrgDebtorRequiredFields.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, true).GetRegistryValue()))
			{
				AssertSupersedeContactsIfRequireARContact(contact1, contact2, org);
			}
		}

		void AssertSupersedeContactsIfRequireARContact(OrgContact contact1, OrgContact contact2, OrgHeader org)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1, contact2 });
			AssertEquals("There should always be at least one active A/R Contact on Debtor Organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("If there are 2 active contacts, cannot supersede both.", 2, org.GetActiveContacts().Count);

			contact1.OC_IsActive = true;
			contact2.OC_IsActive = true;
			AssertEquals(true, contact1.OC_IsActive && contact2.OC_IsActive);

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1 });
			AssertContains("The following contacts will be deactivated ", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("If there are 2 active contacts, can supersede 1 active contact.", 1, org.GetActiveContacts().Count);

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1 });
			AssertContains("The following contacts will be skipped because they are already inactive:", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Can supersede 1 inactive contact.", 1, org.GetActiveContacts().Count);

			UnitTestUserNotification.Instance.ClearMessages();
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact2 });
			AssertEquals("There should always be at least one active A/R Contact on Debtor Organizations.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("If there is 1 active contact, cannot supersede it.", 1, org.GetActiveContacts().Count);
		}

		void AssertSupersedeContactsIfNotRequireARContact(OrgContact contact1, OrgContact contact2, OrgHeader org)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact1 });
			AssertContains("The following contacts will be deactivated", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("If there are 2 active contacts, can supersede 1 active contact.", 1, org.GetActiveContacts().Count);

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			OrgContactSupersedeHelper.SupersedeContacts(new[] { contact2 });
			AssertContains("The following contacts will be deactivated", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("If there is 1 active contacts, can supersede it.", 0, org.GetActiveContacts().Count);
		}
	}
}
