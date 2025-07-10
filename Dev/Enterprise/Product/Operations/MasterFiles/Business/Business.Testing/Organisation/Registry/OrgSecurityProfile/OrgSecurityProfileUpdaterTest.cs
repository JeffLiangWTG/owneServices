using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfileUpdater))]
	sealed class OrgSecurityProfileUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdate()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_Email = "a@cw1.com";
			contact1.OC_WebAccessEnabled = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = "a@cw2.com";
			contact2.OC_WebAccessEnabled = true;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_Email = "a@cw3.com";
			contact3.OC_WebAccessEnabled = true;

			Factory.Save();

			var profiles = new OrgSecurityProfileCollection();
			var userProfile = profiles.AddNew();
			userProfile.Default = true;
			userProfile.Name = "User";
			userProfile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(userProfile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = false; x.CustomerManaged = false; });

			var adminrProfile = profiles.AddNew();
			adminrProfile.Default = false;
			adminrProfile.Name = "Admin";
			adminrProfile.OrgSecuritySettings.PopulateDefaultSettings();
			Array.ForEach(adminrProfile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().ToArray(), x => { x.Granted = true; x.CustomerManaged = true; });

			OrganisationRegistry.Instance.WebSecurityDefaultValues.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profiles);

			var securities = new HashSet<ZString>(userProfile.OrgSecuritySettings.OfType<OrgSecurityProfileSetting>().Select(x => x.SecurityKey));

			var updater1 = new OrgSecurityProfileUpdater(org1)
			{ ShouldApplyGranted = true, ShouldApplyIsCustomerManaged = true, ProfileName = "User" };
			AssertEquals(1, updater1.Update());
			AssertSecurity(securities, false, org1);

			var updater2 = new OrgSecurityProfileUpdater(new[] { org2.PK, org3.PK })
			{ ShouldApplyGranted = true, ShouldApplyIsCustomerManaged = true, ProfileName = "Admin" };
			AssertEquals(2, updater2.Update());

			var newFactory = new BusinessObjectFactory();
			var org2InNewFactory = newFactory.Load<OrgHeader>(org2.PK);
			var org3InNewFactory = newFactory.Load<OrgHeader>(org3.PK);
			AssertSecurity(securities, true, org2);
			AssertSecurity(securities, true, org3);
		}

		void AssertSecurity(HashSet<ZString> securities, ZBool granted, OrgHeader org)
		{
			AssertEquals(true, org.SecurityRightsView.OfType<OrgSecurity>().Where(x => securities.Contains(x.SecurityKey)).All(x => x.OX_Granted == granted && x.OX_IsCustomerManaged == granted));

			foreach (OrgContact contact in org.Contacts)
			{
				AssertEquals(true, contact.SecurityRightsView.OfType<OrgSecurityContacts>().Where(x => securities.Contains(x.Security.SecurityKey)).All(x => x.OZ_Granted == granted));
			}
		}
	}
}
