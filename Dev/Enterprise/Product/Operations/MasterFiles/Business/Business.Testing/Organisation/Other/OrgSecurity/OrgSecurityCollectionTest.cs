using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityCollection))]
	sealed class OrgSecurityCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals("No new items can be added to the security collection as it has all security items in it", false, org.SecurityRights.AllowNew);
		}

		public void TestUnspecifiedSecurityRightsDefaultCorrectly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestXXX";
			AssertEquals("WebQuotes is granted by default", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("WebInvoicingAndStatements is granted by default", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebInvoicingAndStatements));
			AssertEquals("WebBookingsSelectSchedules is denied by default", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebBookingsSelectSchedules));
			AssertEquals("WebISFView is denied by default", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebISFView));
			org.Factory.Save();
			org = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			AssertEquals("After save and load in 2nd factory, no web security changes", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("After save and load in 2nd factory, no web security changes", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebInvoicingAndStatements));
			AssertEquals("After save and load in 2nd factory, no web security changes", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebBookingsSelectSchedules));
			AssertEquals("After save and load in 2nd factory, no web security changes", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebISFView));

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebBookingsSelectSchedules.Code))[0];
			AssertEquals("default OrgSecurities aren't saved", false, orgSecurity1.IsSavedByFactory);
			AssertEquals("default OrgSecurities aren't saved", false, orgSecurity2.IsSavedByFactory);
			orgSecurity1.OX_Granted = false;
			orgSecurity2.OX_Granted = true;
			AssertEquals("changed OrgSecurities are saved", true, orgSecurity1.IsSavedByFactory);
			AssertEquals("changed OrgSecurities are saved", true, orgSecurity2.IsSavedByFactory);
			AssertEquals("WebQuotes granted by default, denied by rights changes", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("WebInvoicingAndStatements granted by default, unchanged", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebInvoicingAndStatements));
			AssertEquals("WebBookingsSelectSchedules denied by default, granted by rights changes", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebBookingsSelectSchedules));
			AssertEquals("WebISFView denied by default, unchanged", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebISFView));
			AssertEquals("OrgSecurity not yet in database", false, orgSecurity1.IsInDatabase);
			AssertEquals("OrgSecurity not yet in database", false, orgSecurity2.IsInDatabase);
			org.Factory.Save();
			AssertEquals("OrgSecurity successfully saved and in database", true, orgSecurity1.IsInDatabase);
			AssertEquals("OrgSecurity successfully saved and in database", true, orgSecurity2.IsInDatabase);
			org = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			AssertEquals("OrgSecurity settings survive DB roundtrip", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("OrgSecurity settings survive DB roundtrip", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebInvoicingAndStatements));
			AssertEquals("OrgSecurity settings survive DB roundtrip", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebBookingsSelectSchedules));
			AssertEquals("OrgSecurity settings survive DB roundtrip", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebISFView));
		}

		public void TestReplacementSecurityRightShouldHaveAValidSecurityContactWithOrgContact()
		{
			//setup
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestXXX";
			var contact = Factory.New<OrgContact>();
			OrgSecurity security = org.SecurityRights.First() as OrgSecurity;
			security.OX_Granted = false;
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1");
			org.Contacts.Add(contact);
			var contactSecurity = Factory.New<OrgSecurityContacts>();
			contactSecurity.OZ_OC = contact.PK;
			contactSecurity.OZ_Granted = true;
			security.ContactSecurityRights.Add(contactSecurity);
			Factory.Save();

			//Act
			var testCollection = new OrgSecurityCollectionForTest(org);
			var replacement = testCollection.GetReplacementForDeletedSecurityRightExposed(security);
			var contacts = replacement.ContactSecurityRights.Select(x => x.Contact).Where(x => x != null);
			//Assert
			AssertEquals("Contacts should not be null", 2, contacts.Count());
		}

		public void TestIsRightGranted()
		{
			var report = GetTestReport();

			var org = Factory.New<OrgHeader>();
			AssertEquals("by default should be granted", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = true;
			AssertEquals("should be granted", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("should be granted", true, org.SecurityRights.IsRightGranted(ReportsWebSecurityRights.GetSecurityRightForReport(report)));

			orgSecurity1.OX_Granted = false;
			orgSecurity2.OX_Granted = false;
			AssertEquals("should NOT be granted", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			AssertEquals("should NOT be granted", false, org.SecurityRights.IsRightGranted(ReportsWebSecurityRights.GetSecurityRightForReport(report)));
		}

		public void TestWebSecurityRights()
		{
			var report = GetTestReport();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var securityRights = WebSecurityRightsList.New();
			var reportSecurityRights = ReportsWebSecurityRights.New(Factory);
			var docSecurityRights = new DocumentWebSecurityRights(Factory);

			AssertEquals("Security right exists on the org for each system-defined security right and Web Published report", securityRights.Count + reportSecurityRights.Count + docSecurityRights.Count, org.SecurityRights.Count);

			CheckWebSecurityRightsForOrg(securityRights, org);
			CheckWebSecurityRightsForOrg(reportSecurityRights, org);
			CheckWebSecurityRightsForOrg(docSecurityRights, org);

			AssertEquals("Collection should not have changes, even though dummy items were added", false, org.SecurityRights.HasChanges);

			foreach (OrgSecurity orgSec in org.SecurityRights)
			{
				AssertEquals("Since nothing has changed this security right should not get saved to the DB", false, orgSec.IsSavedByFactory);
			}

			Factory.Save();

			foreach (OrgSecurity orgSec in org.SecurityRights)
			{
				AssertEquals("Since nothing has changed no security rights should have been saved", false, orgSec.IsInDatabase);
			}

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];

			orgSecurity1.OX_Granted = false;
			orgSecurity2.OX_Granted = true;
			AssertEquals("Security right is no longer granted, so should be saved to the DB", true, orgSecurity1.IsSavedByFactory);
			AssertEquals("Security right is no longer denied, so should be saved to the DB", true, orgSecurity2.IsSavedByFactory);
			Factory.Save();
			AssertEquals("Security right is no longer granted, so is now in DB", true, orgSecurity1.IsInDatabase);
			AssertEquals("Security right is no longer denied, so is now in DB", true, orgSecurity2.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);

			orgInNewFactory.SecurityRights.Load();

			orgSecurity1 = (OrgSecurity)orgInNewFactory.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			orgSecurity1.OX_Granted = true;
			CheckWebSecurityRightsForOrg(securityRights, orgInNewFactory);
		}

		public void TestDummyRowsCreatedOnLoadAndSavedCorrectly()
		{
			var report = GetTestReport();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var securityRights = WebSecurityRightsList.New();
			var reportSecurityRights = ReportsWebSecurityRights.New(Factory);
			var docSecurityRights = new DocumentWebSecurityRights(Factory);

			AssertEquals("Security right exists on the org for each system-defined security right and Web Published report", securityRights.Count + reportSecurityRights.Count + docSecurityRights.Count, org.SecurityRights.Count);

			CheckWebSecurityRightsForOrg(securityRights, org);
			CheckWebSecurityRightsForOrg(reportSecurityRights, org);
			CheckWebSecurityRightsForOrg(docSecurityRights, org);

			AssertEquals("Collection should not have changes, even though dummy items were added", false, org.SecurityRights.HasChanges);

			foreach (OrgSecurity orgSec in org.SecurityRights)
			{
				AssertEquals("Since nothing has changed this security right should not get saved to the DB", false, orgSec.IsSavedByFactory);
			}

			Factory.Save();

			foreach (OrgSecurity orgSec in org.SecurityRights)
			{
				AssertEquals("Since nothing has changed no security rights should have been saved", false, orgSec.IsInDatabase);
			}

			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];

			orgSecurity1.OX_Granted = false;
			orgSecurity2.OX_Granted = true;
			AssertEquals("Security right is no longer granted, so should be saved to the DB", true, orgSecurity1.IsSavedByFactory);
			AssertEquals("Security right is no longer denied, so should be saved to the DB", true, orgSecurity2.IsSavedByFactory);
			Factory.Save();
			AssertEquals("Security right is no longer granted, so is now in DB", true, orgSecurity1.IsInDatabase);
			AssertEquals("Security right is no longer denied, so is now in DB", true, orgSecurity2.IsInDatabase);

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = false;
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;

			var securityContact1 = (OrgSecurityContacts)orgSecurity1.ContactSecurityRights.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity1.PK))[0];
			var securityContact2 = (OrgSecurityContacts)orgSecurity2.ContactSecurityRights.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurity2.PK))[0];
			securityContact1.OZ_Granted = false;
			securityContact2.OZ_Granted = true;
			AssertEquals("Security right is granted again, BUT a security contact has denied security, so should be saved to the DB", true, orgSecurity1.IsSavedByFactory);
			AssertEquals("Security right is denied again, BUT a security contact has granted security, so should be saved to the DB", true, orgSecurity2.IsSavedByFactory);
			Factory.Save();
			AssertEquals("Security right is granted again, BUT a security contact has denied security, so should be in the DB", true, orgSecurity1.IsInDatabase);
			AssertEquals("Security right is denied again, BUT a security contact has granted security, so should be in the DB", true, orgSecurity2.IsInDatabase);

			securityContact1.OZ_Granted = true;
			securityContact2.OZ_Granted = false;
			AssertEquals("Security right is granted again, and child has same security, so should NOT be saved to the DB", false, orgSecurity1.IsSavedByFactory);
			AssertEquals("Security right is denied again, and child has same security, so should NOT be saved to the DB", false, orgSecurity2.IsSavedByFactory);
			AssertEquals("Child should not be saved", false, securityContact1.IsSavedByFactory);
			AssertEquals("Child should not be saved", false, securityContact2.IsSavedByFactory);
			Factory.Save();
			AssertEquals("Security right is granted again, and child has same security, so is not in the DB", false, orgSecurity1.IsInDatabase);
			AssertEquals("Security right is denied again, and child has same security, so is not in the DB", false, orgSecurity2.IsInDatabase);
			AssertEquals("Child should not be in DB", false, securityContact1.IsInDatabase);
			AssertEquals("Child should not be in DB", false, securityContact2.IsInDatabase);
		}

		public void TestLoadSetsCorrectProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var securityRights = AllWebSecurityRights.New(Factory);

			Assert(securityRights.Count > 0);

			org.SecurityRights.Load();
			AssertEquals("Security right exists on the org for each system-defined security right and Web Published report", securityRights.Count, org.SecurityRights.Count);

			foreach (var securityRight in securityRights)
			{
				AssertContainsSecurityRight(org, securityRight);
			}
		}

		public void TestReplaceDeletedSecurityRights_ContactSecurityInDatabase()
		{
			var report = GetTestReport();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
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
			ZString securityItemName1 = orgSecurity1.OX_SecurityItemName;

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

			orgSecurity1.OX_Granted = isGrantedByDefault1;
			orgSecurity2.OX_Granted = isGrantedByDefault2;
			contactSecurity1.OZ_Granted = isGrantedByDefault1;
			contactSecurity2.OZ_Granted = isGrantedByDefault2;
			Factory.Save();
			AssertEquals("Should contain same number of security rights", securityCount, org.SecurityRights.Count);
			AssertEquals("Should contain same number of security rights", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("Org security right is deleted because granted value is same as default", true, orgSecurity1.IsDeleted);
			AssertEquals("Org security right is deleted because granted value is same as default", true, orgSecurity2.IsDeleted);
			AssertEquals("Contact security right is deleted because granted value is same as org security right", true, contactSecurity1.IsDeleted);
			AssertEquals("Contact security right is deleted because granted value is same as org security right", true, contactSecurity2.IsDeleted);

			var orgSecurityReplacement1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, securityItemName1))[0];
			var orgSecurityReplacement2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];
			AssertEquals("Replacement is in memory only", false, orgSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, orgSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", isGrantedByDefault1, orgSecurityReplacement1.OX_Granted);
			AssertEquals("Replacement has same granted value", isGrantedByDefault2, orgSecurityReplacement2.OX_Granted);

			var contactSecurityReplacement1 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurityReplacement1.PK))[0];
			var contactSecurityReplacement2 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurityReplacement2.PK))[0];
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", isGrantedByDefault1, contactSecurityReplacement1.OZ_Granted);
			AssertEquals("Replacement has same granted value", isGrantedByDefault2, contactSecurityReplacement2.OZ_Granted);
		}

		public void TestReplaceDeletedSecurityRights_ContactSecurityNotInDatabase()
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

			orgSecurity1.OX_Granted = !isGrantedByDefault1;
			orgSecurity2.OX_Granted = !isGrantedByDefault2;
			contactSecurity1.OZ_Granted = !isGrantedByDefault1;
			contactSecurity2.OZ_Granted = !isGrantedByDefault2;

			Factory.Save();
			AssertEquals("Pre-condition: contact has same security rights as org", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("Pre-condition: org security right is saved", true, orgSecurity1.IsInDatabase);
			AssertEquals("Pre-condition: org security right is saved", true, orgSecurity2.IsInDatabase);
			AssertEquals("Pre-condition: contact security right is not saved", false, contactSecurity1.IsInDatabase);
			AssertEquals("Pre-condition: contact security right is not saved", false, contactSecurity2.IsInDatabase);

			orgSecurity1.OX_Granted = isGrantedByDefault1;
			orgSecurity2.OX_Granted = isGrantedByDefault2;
			contactSecurity1.OZ_Granted = isGrantedByDefault1;
			contactSecurity2.OZ_Granted = isGrantedByDefault2;
			Factory.Save();
			AssertEquals("Should contain same number of security rights", securityCount, org.SecurityRights.Count);
			AssertEquals("Should contain same number of security rights", securityCount, contact.SecurityRightsForBindingOnly.Count);
			AssertEquals("Org security right is deleted because granted value is same as default", true, orgSecurity1.IsDeleted);
			AssertEquals("Org security right is deleted because granted value is same as default", true, orgSecurity2.IsDeleted);
			AssertEquals("Contact security right is not deleted because it is not in database", false, contactSecurity1.IsDeleted);
			AssertEquals("Contact security right is not deleted because it is not in database", false, contactSecurity2.IsDeleted);

			var orgSecurityReplacement1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, securityItemName))[0];
			var orgSecurityReplacement2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];
			AssertEquals("Replacement is in memory only", false, orgSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, orgSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", isGrantedByDefault1, orgSecurityReplacement1.OX_Granted);
			AssertEquals("Replacement has same granted value", isGrantedByDefault2, orgSecurityReplacement2.OX_Granted);

			var contactSecurityReplacement1 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurityReplacement1.PK))[0];
			var contactSecurityReplacement2 = (OrgSecurityContacts)contact.SecurityRightsForBindingOnly.Find(new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgSecurityReplacement2.PK))[0];
			AssertEquals("Replacement is same as original", contactSecurity1, contactSecurityReplacement1);
			AssertEquals("Replacement is same as original", contactSecurity2, contactSecurityReplacement2);
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement1.IsInDatabase);
			AssertEquals("Replacement is in memory only", false, contactSecurityReplacement2.IsInDatabase);
			AssertEquals("Replacement has same granted value", isGrantedByDefault1, contactSecurityReplacement1.OZ_Granted);
			AssertEquals("Replacement has same granted value", isGrantedByDefault2, contactSecurityReplacement2.OZ_Granted);
		}

		public void TestSetOrgSecurities()
		{
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var profiles = new OrgSecurityProfileCollection();
			var profile = profiles.AddNew();
			profile.Default = true;
			profile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = true; x.CustomerManaged = true; });
			AssertEquals(true, profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().All(x => x.CustomerManaged && x.Granted));

			org.SecurityRights.SetOrgSecurities(profile);
			AssertEquals(true, org.SecurityRightsView.OfType<OrgSecurity>().All(x => x.OX_Granted && x.OX_IsCustomerManaged));

			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = false; x.CustomerManaged = false; });
			org.SecurityRights.SetOrgSecurities(profile);
			AssertEquals(true, profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().All(x => !x.CustomerManaged && !x.Granted));
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
		}

		public void TestSetOrgSecurities_EnableSecurityGroupsForContactsInGLOW()
		{
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var profiles = new OrgSecurityProfileCollection();
			var profile = profiles.AddNew();
			profile.Default = true;
			profile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = true; x.CustomerManaged = true; });

			org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == WebSecurityRightsList.ContainerYardShippingLinePortal.SecurityItemName).OX_Granted = false;
			org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == WebSecurityRightsList.WebQuotes.SecurityItemName).OX_Granted = false;
			Factory.Save();

			using (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				org.SecurityRights.SetOrgSecurities(profile, true, false, shouldMaintainExistingContacts: true);
				AssertEquals("ContainerYardShippingLinePortal should not be granted", false, org.SecurityRights.IsRightGranted(WebSecurityRightsList.ContainerYardShippingLinePortal));
				AssertEquals("WebQuotes should be granted", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			}

			using (GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				org.SecurityRights.SetOrgSecurities(profile, true, false, shouldMaintainExistingContacts: true);
				AssertEquals("ContainerYardShippingLinePortal should be granted", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.ContainerYardShippingLinePortal));
				AssertEquals("WebQuotes should be granted", true, org.SecurityRights.IsRightGranted(WebSecurityRightsList.WebQuotes));
			}
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
		}

		public void TestSetOrgSecurities_MaintainContactRights()
		{
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var profiles = new OrgSecurityProfileCollection();
			var profile = profiles.AddNew();
			profile.Default = true;
			profile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = true; x.CustomerManaged = true; });

			var securityNames = new string[] { "Actual Milestones (Update)", "Container (Edit) Client Reference", "Container Actual De-hire (Edit)", "Container Actual Delivery (Edit)" };

			var contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			var orgSecurity1 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[0]);
			var orgSecurity2 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[1]);
			var orgSecurity3 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[2]);
			var orgSecurity4 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[3]);
			var contactSecurity1 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity1.PK);
			var contactSecurity2 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity2.PK);
			var contactSecurity3 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity3.PK);
			var contactSecurity4 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity4.PK);

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = false;
			orgSecurity3.OX_Granted = true;
			orgSecurity4.OX_Granted = false;
			contactSecurity1.OZ_Granted = true;
			contactSecurity2.OZ_Granted = true;
			contactSecurity3.OZ_Granted = false;
			contactSecurity4.OZ_Granted = false;

			org.SecurityRights.SetOrgSecurities(profile, true, false, shouldMaintainExistingContacts: true);
			org.Factory.Save();
			orgSecurity1 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[0]);
			orgSecurity2 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[1]);
			orgSecurity3 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[2]);
			orgSecurity4 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[3]);
			contactSecurity1 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity1.PK);
			contactSecurity2 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity2.PK);
			contactSecurity3 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity3.PK);
			contactSecurity4 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity4.PK);
			AssertEquals(true, orgSecurity1.OX_Granted);
			AssertEquals(true, orgSecurity2.OX_Granted);
			AssertEquals(true, orgSecurity3.OX_Granted);
			AssertEquals(true, orgSecurity4.OX_Granted);
			AssertEquals(true, contactSecurity1.OZ_Granted);
			AssertEquals(true, contactSecurity2.OZ_Granted);
			AssertEquals(false, contactSecurity3.OZ_Granted);
			AssertEquals(false, contactSecurity4.OZ_Granted);

			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = false; x.CustomerManaged = false; });

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = false;
			orgSecurity3.OX_Granted = true;
			orgSecurity4.OX_Granted = false;
			contactSecurity1.OZ_Granted = true;
			contactSecurity2.OZ_Granted = true;
			contactSecurity3.OZ_Granted = false;
			contactSecurity4.OZ_Granted = false;

			org.SecurityRights.SetOrgSecurities(profile, true, false, shouldMaintainExistingContacts: true);
			org.Factory.Save();
			orgSecurity1 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[0]);
			orgSecurity2 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[1]);
			orgSecurity3 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[2]);
			orgSecurity4 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[3]);
			contactSecurity1 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity1.PK);
			contactSecurity2 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity2.PK);
			contactSecurity3 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity3.PK);
			contactSecurity4 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity4.PK);
			AssertEquals(false, orgSecurity1.OX_Granted);
			AssertEquals(false, orgSecurity2.OX_Granted);
			AssertEquals(false, orgSecurity3.OX_Granted);
			AssertEquals(false, orgSecurity4.OX_Granted);
			AssertEquals(true, contactSecurity1.OZ_Granted);
			AssertEquals(true, contactSecurity2.OZ_Granted);
			AssertEquals(false, contactSecurity3.OZ_Granted);
			AssertEquals(false, contactSecurity4.OZ_Granted);
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
		}

		public void TestSetOrgSecurities_EraseContactRights()
		{
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var profiles = new OrgSecurityProfileCollection();
			var profile = profiles.AddNew();
			profile.Default = true;
			profile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = true; x.CustomerManaged = true; });

			var securityNames = new string[] { "Actual Milestones (Update)", "Container (Edit) Client Reference", "Container Actual De-hire (Edit)", "Container Actual Delivery (Edit)" };

			var contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			var orgSecurity1 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[0]);
			var orgSecurity2 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[1]);
			var orgSecurity3 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[2]);
			var orgSecurity4 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[3]);
			var contactSecurity1 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity1.PK);
			var contactSecurity2 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity2.PK);
			var contactSecurity3 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity3.PK);
			var contactSecurity4 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity4.PK);

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = false;
			orgSecurity3.OX_Granted = true;
			orgSecurity4.OX_Granted = false;
			contactSecurity1.OZ_Granted = true;
			contactSecurity2.OZ_Granted = true;
			contactSecurity3.OZ_Granted = false;
			contactSecurity4.OZ_Granted = false;

			org.SecurityRights.SetOrgSecurities(profile, true, false, shouldEraseExistingContacts: true);
			org.Factory.Save();
			orgSecurity1 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[0]);
			orgSecurity2 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[1]);
			orgSecurity3 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[2]);
			orgSecurity4 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[3]);
			contactSecurity1 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity1.PK);
			contactSecurity2 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity2.PK);
			contactSecurity3 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity3.PK);
			contactSecurity4 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity4.PK);
			AssertEquals(true, orgSecurity1.OX_Granted);
			AssertEquals(true, orgSecurity2.OX_Granted);
			AssertEquals(true, orgSecurity3.OX_Granted);
			AssertEquals(true, orgSecurity4.OX_Granted);
			AssertEquals(true, contactSecurity1.OZ_Granted);
			AssertEquals(true, contactSecurity2.OZ_Granted);
			AssertEquals(true, contactSecurity3.OZ_Granted);
			AssertEquals(true, contactSecurity4.OZ_Granted);

			orgSecurity1.OX_Granted = true;
			orgSecurity2.OX_Granted = false;
			orgSecurity3.OX_Granted = true;
			orgSecurity4.OX_Granted = false;
			contactSecurity1.OZ_Granted = true;
			contactSecurity2.OZ_Granted = true;
			contactSecurity3.OZ_Granted = false;
			contactSecurity4.OZ_Granted = false;

			Array.ForEach(profile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = false; x.CustomerManaged = false; });

			org.SecurityRights.SetOrgSecurities(profile, true, false, shouldEraseExistingContacts: true);
			org.Factory.Save();
			orgSecurity1 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[0]);
			orgSecurity2 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[1]);
			orgSecurity3 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[2]);
			orgSecurity4 = org.SecurityRightsView.OfType<OrgSecurity>().First(x => x.OX_SecurityItemName == securityNames[3]);
			contactSecurity1 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity1.PK);
			contactSecurity2 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity2.PK);
			contactSecurity3 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity3.PK);
			contactSecurity4 = contact.SecurityRightsView.OfType<OrgSecurityContacts>().First(x => x.OZ_OX == orgSecurity4.PK);
			AssertEquals(false, orgSecurity1.OX_Granted);
			AssertEquals(false, orgSecurity2.OX_Granted);
			AssertEquals(false, orgSecurity3.OX_Granted);
			AssertEquals(false, orgSecurity4.OX_Granted);
			AssertEquals(false, contactSecurity1.OZ_Granted);
			AssertEquals(false, contactSecurity2.OZ_Granted);
			AssertEquals(false, contactSecurity3.OZ_Granted);
			AssertEquals(false, contactSecurity4.OZ_Granted);
			OrgSecurityProfileSettingCollection.ResetDefaultValues();
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

		void AssertContainsSecurityRight(OrgHeader org, WebSecurityRight webSecurityRight)
		{
			var containsRight = false;

			foreach (OrgSecurity securityRight in org.SecurityRights)
			{
				if (securityRight.SecurityItemNameForDisplay.TrimEnd() == webSecurityRight.Description.TrimEnd())
				{
					AssertEquals(webSecurityRight.SecurityItemName, securityRight.OX_SecurityItemName);
					AssertEquals(webSecurityRight.SecurityGuid, securityRight.OX_SU);
					AssertEquals(webSecurityRight.IsGrantedByDefault, securityRight.OX_Granted);

					containsRight = true;
					break;
				}
			}

			Assert(containsRight);
		}

		void CheckWebSecurityRightsForOrg(IWebSecurityRightProvider rightsList, OrgHeader org)
		{
			foreach (WebSecurityRight right in rightsList)
			{
				bool rightIsGrantedByDefault = rightsList is DocumentWebSecurityRights ||
						(rightsList is WebSecurityRightsList &&
								right.Code != WebSecurityRightsList.WebPublishLayouts.Code &&
								right.Code != WebSecurityRightsList.WebShipmentDeliveryAdd.Code &&
								right.Code != WebSecurityRightsList.WebShipmentDeliveryEdit.Code &&
								right.Code != WebSecurityRightsList.WebBookingsSelectSchedules.Code &&
								right.Code != WebSecurityRightsList.WebDocumentsAdd.Code &&
								right.Code != WebSecurityRightsList.WebMAWBEdit.Code &&
								right.Code != WebSecurityRightsList.WebMAWBView.Code &&
								right.Code != WebSecurityRightsList.WebMAWBSend.Code &&
								right.Code != WebSecurityRightsList.WebMAWBAdmin.Code &&
								right.Code != WebSecurityRightsList.WebHAWBEdit.Code &&
								right.Code != WebSecurityRightsList.WebHAWBView.Code &&
								right.Code != WebSecurityRightsList.WebHAWBSend.Code &&
								right.Code != WebSecurityRightsList.WebHAWBAdmin.Code &&
								right.Code != WebSecurityRightsList.WebActualMilestonesUpdate.Code &&
								right.Code != WebSecurityRightsList.WebEstimatedMilestonesUpdate.Code &&
								right.Code != WebSecurityRightsList.WebEventsView.Code &&
								right.Code != WebSecurityRightsList.TransportCustomerPortal.Code &&
								right.Code != WebSecurityRightsList.TransportSubContractorPortal.Code &&
								right.Code != WebSecurityRightsList.WebTranslationFeedback.Code &&
								right.Code != WebSecurityRightsList.MapEntitiesViaDataWizard.Code &&
								right.Code != WebSecurityRightsList.NettingParticipantPortal.Code &&
								right.Code != WebSecurityRightsList.USAMSPortal.Code &&
								right.Code != WebSecurityRightsList.USAMSAddEdit.Code &&
								right.Code != WebSecurityRightsList.USAMSDelete.Code &&
								right.Code != WebSecurityRightsList.USAMSSend.Code &&
								right.Code != WebSecurityRightsList.USAMSView.Code &&
								right.Code != WebSecurityRightsList.SlotManagementClientPortal.Code &&
								right.Code != WebSecurityRightsList.SlotManagementStaffPortal.Code &&
								right.Code != WebSecurityRightsList.ContainerYardShippingLinePortal.Code &&
								right.Code != WebSecurityRightsList.eCommerceShipperPortal.Code &&
								right.Code != WebSecurityRightsList.eCommerceOriginDepotPortal.Code &&
								right.Code != WebSecurityRightsList.eCommerceDestinationDepotPortal.Code &&
								right.Code != WebSecurityRightsList.eCommerceViewCarriersAndDepots.Code &&
								right.Code != WebSecurityRightsList.eCommerceConfirmHVLVBookingHeader.Code &&
								right.Code != WebSecurityRightsList.eCommerceReceiveHVLVBookingHeader.Code &&
								right.Code != WebSecurityRightsList.eCommerceLodgeHVLVOriginLoadList.Code &&
								right.Code != WebSecurityRightsList.eCommerceCalculateDepotAndLastMileCarrierDetails.Code &&
								right.Code != WebSecurityRightsList.eCommerceLastMileCarrierBooking.Code &&
								right.Code != WebSecurityRightsList.eCommerceAutomatedBookingHeaderCreation.Code &&
								right.Code != WebSecurityRightsList.WebISFAddEdit.Code &&
								right.Code != WebSecurityRightsList.WebISFDelete.Code &&
								right.Code != WebSecurityRightsList.WebISFSend.Code &&
								right.Code != WebSecurityRightsList.WebISFView.Code &&
								right.Code != WebSecurityRightsList.TransitWarehouseClientPortal.Code &&
								right.Code != WebSecurityRightsList.MapConsignmentViaDataWizard.Code);

				AssertEquals("Security right " + right.Code + " is" + (rightIsGrantedByDefault ? "" : " not") + " granted by default for a new org", rightIsGrantedByDefault, org.SecurityRights.IsRightGranted(right));
				AssertCollectionContains("Security right found for the right code", right.Code, org.SecurityRights.Select(orgSec => orgSec.SecurityKey));
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(OrgSecurity));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader org = OrgHeader.New(Factory);

			// Below two lines of code are for standard business object collection tests
			// as items are added automatically which make the standard tests fail.
			// By removing these auto-added items, we are testing as if this collection was "normal".
			org.SecurityRights.RemoveAndDeleteAll();
			org.SecurityRights.HasChanges = false;

			return org.SecurityRights;
		}

		#endregion
	}
}
