using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityContactsCollection))]
	sealed class OrgSecurityContactsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestInheritParentRights()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;

			var security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_Granted, true))[0];
			var contactSecurity = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, security.PK))[0];

			AssertEquals("Precondition: Security Right Granted", true, security.OX_Granted);
			AssertEquals("Precondition: Contact Security Right Granted", true, contactSecurity.OZ_Granted);

			security.ContactSecurityRights.Load();

			security.OX_Granted = false;
			contactSecurity.OZ_Granted = true;

			AssertEquals("Security Right Denied", false, security.OX_Granted);
			AssertEquals("Contact Security Right Granted", true, contactSecurity.OZ_Granted);

			contact.SecurityRightsForBindingOnly.InheritParentRights();
			AssertEquals("Security Right Denied", false, security.OX_Granted);
			AssertEquals("Contact Security Right Denied", false, contactSecurity.OZ_Granted);
		}

		public void TestDenyAll()
		{
			var org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;

			var grantedSecurityRights = contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_Granted, true));
			var contactSecurity1 = grantedSecurityRights[0] as OrgSecurityContacts;
			var contactSecurity2 = grantedSecurityRights[1] as OrgSecurityContacts;

			AssertEquals("Precondition: Security Right Granted", true, contactSecurity1.OZ_Granted);
			AssertEquals("Precondition: Security Right Granted", true, contactSecurity2.OZ_Granted);

			contact.SecurityRightsForBindingOnly.DenyAll();

			AssertEquals("Security Right Denied", false, contactSecurity1.OZ_Granted);
			AssertEquals("Security Right Denied", false, contactSecurity2.OZ_Granted);
		}

		public void TestIsRightGranted_DeniedByDefaultInRegistry()
		{
			using (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var report = Factory.New<StmMenuItem>();
				report.SU_BusinessContext = "RepTest";
				report.SU_MenuName = "Test Report";
				report.SU_MenuType = "WEB";

				var org = Factory.New<OrgHeader>();
				OrgContact contact = org.Contacts.AddNew();

				AssertEquals("Web access is turned off, so not granted", false, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));

				contact.OC_WebAccessEnabled = true;
				AssertEquals("by default should be denied due to registry setting", false, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));

				var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
				var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];

				orgSecurity1.ContactSecurityRights.Load();
				orgSecurity2.ContactSecurityRights.Load();

				orgSecurity1.OX_Granted = true;
				orgSecurity2.OX_Granted = true;
				AssertEquals("should be granted", true, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));
				AssertEquals("should be granted", true, contact.SecurityRightsForBindingOnly.IsRightGranted(ReportsWebSecurityRights.GetSecurityRightForReport(report)));

				orgSecurity1.OX_Granted = false;
				orgSecurity2.OX_Granted = false;
				AssertEquals("should NOT be granted", false, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));
				AssertEquals("should NOT be granted", false, contact.SecurityRightsForBindingOnly.IsRightGranted(ReportsWebSecurityRights.GetSecurityRightForReport(report)));
			}
		}

		public void TestIsRightGranted()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepTest";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";

			var org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			AssertEquals("Web access is turned off, so not granted", false, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));

			contact.OC_WebAccessEnabled = true;
			AssertEquals("by default should be granted as web access is now on", true, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];

			orgSecurity1.ContactSecurityRights.Load();
			orgSecurity2.ContactSecurityRights.Load();

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = true;
			AssertEquals("should be granted", true, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("should be granted", true, contact.SecurityRightsForBindingOnly.IsRightGranted(ReportsWebSecurityRights.GetSecurityRightForReport(report)));

			orgSecurity1.OX_Granted = false;
			orgSecurity2.OX_Granted = false;
			AssertEquals("should NOT be granted", false, contact.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("should NOT be granted", false, contact.SecurityRightsForBindingOnly.IsRightGranted(ReportsWebSecurityRights.GetSecurityRightForReport(report)));
		}

		public void TestLoadingFromContactAndSecurityRight()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			contact.OC_ContactName = "John";

			var securityRights = AllWebSecurityRights.New(Factory);

			AssertEquals("Same number of rights as system defined rights", securityRights.Count, contact.SecurityRightsForBindingOnly.Count);

			CheckWebSecurityRightsForOrgContact(securityRights, contact);

			foreach (var securityContact in contact.SecurityRightsForBindingOnly)
			{
				AssertEquals("Each security item should not be saved to the DB", false, securityContact.IsSavedByFactory);
			}

			Factory.Save();

			AssertEquals("Same number of rights as system defined rights and Web Reports, even after saving", securityRights.Count, contact.SecurityRightsForBindingOnly.Count);
			foreach (OrgSecurityContacts securityContact in contact.SecurityRightsForBindingOnly)
			{
				AssertEquals("Each Security item should not have been saved to the DB", false, securityContact.IsInDatabase);
			}

			foreach (OrgSecurityContacts securityContact in contact.SecurityRightsForBindingOnly)
			{
				securityContact.OZ_Granted = !securityContact.Security.OX_Granted;
				AssertEquals("Each item SHOULD be saved to the DB as it has a different security to it's parent", true, securityContact.IsSavedByFactory);
			}

			Factory.Save();
			foreach (OrgSecurityContacts securityContact in contact.SecurityRightsForBindingOnly)
			{
				AssertEquals("Each Security item SHOULD have been saved to the DB", true, securityContact.IsInDatabase);
			}
		}

		public void TestAllowNew()
		{
			var org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			AssertEquals("No new items can be added to the contact security collection as it has all security items in it", false, contact.SecurityRightsForBindingOnly.AllowNew);
		}

		public void TestFactorySaveShouldNotLoadExtraContactSecurityRights()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = org.Contacts.AddNew();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "Test 1";
			contact1.OC_Email = "email1@zzz.zz";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "Test 2";
			contact2.OC_Email = "email2@zzz.zz";

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedContact1 = loadFactory.Load<OrgContact>(contact1.PK);
			var loadedContact2 = loadFactory.Load<OrgContact>(contact2.PK);

			WebSecurityRightsList definedRights = WebSecurityRightsList.New();
			ReportsWebSecurityRights reportRights = ReportsWebSecurityRights.New(Factory);
			CheckWebSecurityRightsForOrgContact(definedRights, loadedContact1);
			CheckWebSecurityRightsForOrgContact(reportRights, loadedContact1);

			loadFactory.Save();

			AssertNull("Contact 2 should not load any security rights when changes only made to contact 1", loadedContact2.SecurityRightsNoCreate);
		}

		public void TestReplaceDeletedSecurityRights()
		{
			var report = GetTestReport();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			contact.OC_ContactName = "Samuel";

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];
			var contactSecurity1 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity1.PK))[0];
			var contactSecurity2 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity2.PK))[0];

			int securityCount = org.SecurityRights.Count;
			bool isGrantedByDefault1 = orgSecurity1.IsGrantedByDefault;
			bool isGrantedByDefault2 = orgSecurity2.IsGrantedByDefault;

			orgSecurity1.OX_Granted = !isGrantedByDefault1;
			orgSecurity2.OX_Granted = !isGrantedByDefault2;
			contactSecurity1.OZ_Granted = isGrantedByDefault1;
			contactSecurity2.OZ_Granted = isGrantedByDefault2;

			Factory.Save();
			AssertEquals("Pre-condition: contact has same security rights as org", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("Pre-condition: org security right is saved", true, orgSecurity1.IsInDatabase);
			AssertEquals("Pre-condition: org security right is saved", true, orgSecurity2.IsInDatabase);
			AssertEquals("Pre-condition: contact security right is saved", true, contactSecurity1.IsInDatabase);
			AssertEquals("Pre-condition: contact security right is saved", true, contactSecurity2.IsInDatabase);

			contactSecurity1.OZ_Granted = orgSecurity1.OX_Granted;
			contactSecurity2.OZ_Granted = orgSecurity2.OX_Granted;
			Factory.Save();
			AssertEquals("Should contain same number of security rights", securityCount, org.SecurityRights.Count);
			AssertEquals("Should contain same number of security rights", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("No change to org security right", true, orgSecurity1.IsInDatabase);
			AssertEquals("No change to org security right", true, orgSecurity2.IsInDatabase);
			AssertEquals("Contact security right is deleted because granted value is same as org security right", true, contactSecurity1.IsDeleted);
			AssertEquals("Contact security right is deleted because granted value is same as org security right", true, contactSecurity2.IsDeleted);

			var contactSecurityReplacement1 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity1.PK))[0];
			var contactSecurityReplacement2 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity2.PK))[0];
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", orgSecurity1.OX_Granted, contactSecurityReplacement1.OZ_Granted);
			AssertEquals("Replacement has same granted value", orgSecurity2.OX_Granted, contactSecurityReplacement2.OZ_Granted);
			AssertEquals("Parent security should exist", orgSecurity1, contactSecurityReplacement1.Security);
			AssertEquals("Parent security should exist", orgSecurity2, contactSecurityReplacement2.Security);
		}

		public void TestReplaceDeletedSecurityRights_CascadeDeleteOrgSecurity()
		{
			var report = GetTestReport();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			contact.OC_ContactName = "Samuel";

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];
			var contactSecurity1 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity1.PK))[0];
			var contactSecurity2 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity2.PK))[0];

			int securityCount = org.SecurityRights.Count;
			bool isGrantedByDefault1 = orgSecurity1.IsGrantedByDefault;
			bool isGrantedByDefault2 = orgSecurity2.IsGrantedByDefault;
			ZString securityItemName = orgSecurity1.OX_SecurityItemName;

			orgSecurity1.OX_Granted = isGrantedByDefault1;
			orgSecurity2.OX_Granted = isGrantedByDefault2;
			contactSecurity1.OZ_Granted = !isGrantedByDefault1;
			contactSecurity2.OZ_Granted = !isGrantedByDefault2;

			Factory.Save();
			AssertEquals("Pre-condition: contact has same security rights as org", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("Pre-condition: org security right is saved", true, orgSecurity1.IsInDatabase);
			AssertEquals("Pre-condition: org security right is saved", true, orgSecurity2.IsInDatabase);
			AssertEquals("Pre-condition: contact security right is saved", true, contactSecurity1.IsInDatabase);
			AssertEquals("Pre-condition: contact security right is saved", true, contactSecurity2.IsInDatabase);

			contactSecurity1.OZ_Granted = orgSecurity1.OX_Granted;
			contactSecurity2.OZ_Granted = orgSecurity2.OX_Granted;
			Factory.Save();
			AssertEquals("Should contain same number of security rights", securityCount, org.SecurityRights.Count);
			AssertEquals("Should contain same number of security rights", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("Org security right is deleted because granted value is same as default", true, orgSecurity1.IsDeleted);
			AssertEquals("Org security right is deleted because granted value is same as default", true, orgSecurity2.IsDeleted);
			AssertEquals("Contact security right is deleted because granted value is same as org security right", true, contactSecurity1.IsDeleted);
			AssertEquals("Contact security right is deleted because granted value is same as org security right", true, contactSecurity1.IsDeleted);

			var orgSecurityReplacement1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, securityItemName))[0];
			var orgSecurityReplacement2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];
			AssertEquals("Replacement is in memory only", false, orgSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, orgSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", isGrantedByDefault1, orgSecurityReplacement1.OX_Granted);
			AssertEquals("Replacement has same granted value", isGrantedByDefault2, orgSecurityReplacement2.OX_Granted);

			var contactSecurityReplacement1 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurityReplacement1.PK))[0];
			var contactSecurityReplacement2 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurityReplacement2.PK))[0];
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", orgSecurityReplacement1.OX_Granted, contactSecurityReplacement1.OZ_Granted);
			AssertEquals("Replacement has same granted value", orgSecurityReplacement2.OX_Granted, contactSecurityReplacement2.OZ_Granted);
			AssertEquals("Parent security should exist", orgSecurityReplacement1, contactSecurityReplacement1.Security);
			AssertEquals("Parent security should exist", orgSecurityReplacement2, contactSecurityReplacement2.Security);
		}

		#region Implementation

		StmMenuItem GetTestReport()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepTest";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";
			return report;
		}

		void CheckWebSecurityRightsForOrgContact(IWebSecurityRightProvider rightsList, OrgContact contact)
		{
			foreach (CodeDescriptionPair right in rightsList)
			{
				OrgSecurityContacts foundSecurityOnContact = null;
				foreach (OrgSecurityContacts securityContact in contact.SecurityRightsForBindingOnly)
				{
					if (right.Code == securityContact.SecurityItemName || right.Description == securityContact.SecurityItemName)
					{
						foundSecurityOnContact = securityContact;
					}
				}

				AssertNotNull("Each defined security right should have been created for this contact", foundSecurityOnContact);

				OrgSecurityContacts foundSecurityOnSecurityRight = null;
				foreach (OrgSecurity security in contact.ParentOrg.SecurityRights)
				{
					foreach (OrgSecurityContacts securityContact in security.ContactSecurityRights)
					{
						if (right.Code == securityContact.SecurityItemName || right.Description == securityContact.SecurityItemName)
						{
							foundSecurityOnSecurityRight = securityContact;
						}
					}
				}

				AssertNotNull("Each defined security right should have been created for this contact", foundSecurityOnSecurityRight);
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(OrgSecurityContacts));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org = OrgHeader.New(Factory);
			OrgSecurity security = org.SecurityRights.AddNew();
			return security.ContactSecurityRights;
		}

		#endregion
	}
}
