using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurity))]
	sealed class OrgSecurityTest : EnterpriseBusinessObjectTestCase
	{
		#region ReadOnly Security

		public void TestWebSecurityRightsCollectionIsReadOnly()
		{
			bool oldWebSecurityRightsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;

			try
			{
				OrgSecurity testSecurity = OrgInDB.SecurityRights.AddNew();
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testSecurity.ContactSecurityRights.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				ResetOrgInDB();
				testSecurity = OrgInDB.SecurityRights.AddNew();
				Assert("Access Disallowed - ReadOnly", testSecurity.ContactSecurityRights.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldWebSecurityRightsValue;
			}
		}

		public void TestWebSecurityRightIsCached()
		{
			var testSecurity = Factory.New<OrgSecurity>();
			var webSecurity = testSecurity.WebSecurityRight;
			AssertNotNull("Web Security Right Is Cached", Factory.GetCachedValue<AllWebSecurityRights>("OrgSecurity.WebSecurityRights", () => null));
		}

		public void TestReadOnlySecurity()
		{
			bool oldWebDetailsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;
			bool oldNewWebDetailsValue = Env.Security.OrgDetailsNewWebSecurity.IsAllowed;

			try
			{
				OrgSecurity security = OrgInDB.SecurityRights.AddNew();

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !security.OX_GrantedInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", security.OX_GrantedInfo.ReadOnly);

				security = Factory.NewWithValidTestData<OrgHeader>().SecurityRights.AddNew();

				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !security.OX_GrantedInfo.ReadOnly);

				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", security.OX_GrantedInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldWebDetailsValue;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = oldNewWebDetailsValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region IsSavedByFactory

		public void TestIsSavedByFactory()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;

			OrgSecurity security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_Granted, true))[0];
			OrgSecurityContacts contactSecurity = security.ContactSecurityRights[0];

			AssertEquals("Security is not saved as it is granted", false, security.IsSavedByFactory);
			AssertEquals("Contact Security is not saved as it is granted", false, contactSecurity.IsSavedByFactory);

			security.OX_Granted = false;
			AssertEquals("Security is saved as it is denied", true, security.IsSavedByFactory);
			AssertEquals("Contact Security is NOT saved as it is the same as the parent security", false, contactSecurity.IsSavedByFactory);

			contactSecurity.OZ_Granted = true;
			AssertEquals("Security is saved as the child record needs to be saved", true, security.IsSavedByFactory);
			AssertEquals("Contact Security is saved as it has different security to the parent", true, contactSecurity.IsSavedByFactory);

			security.OX_Granted = true;
			AssertEquals("Security is not saved as it is granted", false, security.IsSavedByFactory);
			AssertEquals("Contact Security is not saved as it is granted", false, contactSecurity.IsSavedByFactory);

			security.OX_SecurityItemName = "Some Dynamic Item";
			security.OX_Granted = false;
			AssertEquals("Dynamic Security is not saved as it is not granted", false, security.IsSavedByFactory);

			security.OX_Granted = true;
			AssertEquals("Dynamic Security is saved as it is granted", true, security.IsSavedByFactory);
		}

		public void TestIsSavedByFactory_PredefinedRights()
		{
			WebSecurityRightsListForTest.UseWebSecurityRightsListForTest();

			OrgSecurity security1 = Factory.New<OrgSecurity>();
			security1.OX_SecurityItemName = "Test 1";
			security1.OX_Granted = true;
			Assert(!security1.IsDifferentToDefaultRight);
			Assert("The same as default, does not need to be saved", !security1.IsSavedByFactory);

			security1.OX_Granted = false;
			Assert(security1.IsDifferentToDefaultRight);
			Assert("Differs from the default value, should be saved", security1.IsSavedByFactory);

			OrgSecurity security2 = Factory.New<OrgSecurity>();
			security2.OX_SecurityItemName = "Test 2";
			security2.OX_Granted = true;
			Assert(security2.IsDifferentToDefaultRight);
			Assert("Differs from the default value, should be saved", security2.IsSavedByFactory);

			security2.OX_Granted = false;
			Assert(!security2.IsDifferentToDefaultRight);
			Assert("The same as default, does not need to be saved", !security2.IsSavedByFactory);

			security2.OX_IsCustomerManaged = true;
			Assert(security2.IsDifferentToDefaultRight);
			Assert("Differs from the default value, should be saved", security2.IsSavedByFactory);
		}

		public void TestIsSavedByFactory_ReportRights()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuType = "WEB";
			menuItem.SU_MenuName = "Report";
			menuItem.SU_IsPublished = true;

			OrgSecurity security = Factory.New<OrgSecurity>();
			security.OX_SecurityItemName = ": Report";
			security.OX_Granted = false;
			Assert(!security.IsDifferentToDefaultRight);
			Assert("The same as default, does not need to be saved", !security.IsSavedByFactory);

			security.OX_Granted = true;
			Assert(security.IsDifferentToDefaultRight);
			Assert("Differs from the default value, should be saved", security.IsSavedByFactory);
		}

		class WebSecurityRightsListForTest : WebSecurityRightsList
		{
			public static void UseWebSecurityRightsListForTest()
			{
				OverridableNewDelegate.Value = delegate
				{ return new WebSecurityRightsListForTest(); };
			}

			public static WebSecurityRight TestRight1 = new WebSecurityRight("Test 1", (NoResString)"", WebSecurityApplication.EdiWebTracker, true);
			public static WebSecurityRight TestRight2 = new WebSecurityRight("Test 2", (NoResString)"", WebSecurityApplication.EdiWebTracker, false);
		}

		#endregion

		public void TestSecurityItemNameIsReadOnly()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgSecurity security = org.SecurityRights[0];

			AssertEquals("Security Item Name is always readonly", true, security.OX_SecurityItemNameInfo.ReadOnly);
		}

		public void TestIsGrantedByDefault()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepTest";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgSecurity1 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			var orgSecurity2 = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SU, report.PK))[0];

			Assert(orgSecurity1.IsGrantedByDefault);
			Assert(!orgSecurity2.IsGrantedByDefault);
		}

		public void TestGrantedFiltersDownToDummyRowsWhenContactHasWebAccess()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			contact.OC_ContactName = "Hello";
			OrgSecurity security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_Granted, true))[0];

			AssertEquals("Precondition: Security is granted", true, security.OX_Granted);
			AssertEquals("Precondition: Contact is granted", true, security.ContactSecurityRights[0].OZ_Granted);

			security.OX_Granted = false;
			AssertEquals("Security is not granted", false, security.OX_Granted);
			AssertEquals("Contact is not granted", false, security.ContactSecurityRights[0].OZ_Granted);
			security.ContactSecurityRights[0].OZ_Granted = true;

			Factory.Save();
			AssertEquals("Precondition: Contact security is in the DB", true, security.ContactSecurityRights[0].IsInDatabase);
			security.OX_Granted = false;
			AssertEquals("Security is granted", false, security.OX_Granted);
			AssertEquals("Contact is STILL granted as it was in the database, so not updated", true, security.ContactSecurityRights[0].OZ_Granted);
		}

		public void TestGrantedFiltersDownToDummyRowsWhenContactHasNoWebAccess()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = false;
			contact.OC_ContactName = "Hello";
			OrgSecurity security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_Granted, true))[0];

			AssertEquals("Precondition: Security is granted", true, security.OX_Granted);
			AssertEquals("Contact is NOT granted as contact has no web access", false, security.ContactSecurityRights[0].OZ_Granted);

			security.OX_Granted = false;
			AssertEquals("Security is not granted", false, security.OX_Granted);
			AssertEquals("Contact is still not granted", false, security.ContactSecurityRights[0].OZ_Granted);
		}

		public void TestChildrenWithDifferentSecurity()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Hello";
			OrgSecurity security = org.SecurityRights[0];

			security.OX_Granted = false;
			security.ContactSecurityRights[0].OZ_Granted = false;

			Assert(security.NoChildrenExistWithDifferentSecurity);

			security.ContactSecurityRights[0].OZ_Granted = true;
			Assert("Different security, but web access disabled so it returns true", security.NoChildrenExistWithDifferentSecurity);

			contact.OC_WebAccessEnabled = true;
			org.SecurityRights[0].ContactSecurityRights[0].OZ_Granted = true;
			Assert("Different security, and web access enabed", !security.NoChildrenExistWithDifferentSecurity);

			security.OX_Granted = false;
			Assert("Since the child is not in the DB, it will set the child's security to also denied", !contact.SecurityRightsForBindingOnly[0].OZ_Granted);
			Assert(security.NoChildrenExistWithDifferentSecurity);

			contact.SecurityRightsForBindingOnly[0].OZ_Granted = true;
			Assert("Explicitly set, child's security is granted", contact.SecurityRightsForBindingOnly[0].OZ_Granted);
			Assert("Child has different security to parent", !security.NoChildrenExistWithDifferentSecurity);
		}

		public void TestDontLoadDummyContactRecords()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Hello";
			OrgSecurity security = org.SecurityRights[0];

			security.OX_Granted = false;
			Assert(security.NoChildrenExistWithDifferentSecurity);
			var amount1 = security.fContactSecurityRights.Count;
			var dummy = security.ContactSecurityRights;
			Assert("loads dummy contact records only when accessed outside of NoChildrenExistWithDifferentSecurity", security.ContactSecurityRights.Count > amount1);
		}

		public void TestSecurityKey()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var security = org.SecurityRights[0];

			var securityItemName = "Blah";
			security.OX_SecurityItemName = securityItemName;
			security.OX_SU = ZGuid.Empty;
			AssertEquals(securityItemName, security.SecurityKey);

			var menuItemGuid = ZGuid.NewZGuid();
			security.OX_SecurityItemName = string.Empty;
			security.OX_SU = menuItemGuid;
			AssertEquals(menuItemGuid.ToString(), security.SecurityKey);
		}

		public void TestOX_GrantedForWeb()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;
			var orgSecurity = org.SecurityRights[0];
			var contactSecurity = contact.SecurityRightsView.Cast<OrgSecurityContacts>().Single(x => x.OZ_OX == orgSecurity.PK);
			AssertNotNull(orgSecurity.ContactSecurityRights);

			orgSecurity.OX_Granted = true;
			AssertEquals(true, contactSecurity.OZ_Granted);
			orgSecurity.OX_Granted = false;
			AssertEquals(false, contactSecurity.OZ_Granted);

			orgSecurity.OX_GrantedForWeb = true;
			AssertEquals(false, contactSecurity.OZ_Granted);
			contactSecurity.OZ_Granted = true;
			orgSecurity.OX_GrantedForWeb = false;
			AssertEquals(true, contactSecurity.OZ_Granted);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return org.SecurityRights.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			OrgSecurity result = org.SecurityRights[0];
			result.OX_SecurityItemName = WebSecurityRightsList.WebBookingsAddEdit.Code;
			result.OX_Granted = false;

			return result;
		}

		#endregion
	}
}
