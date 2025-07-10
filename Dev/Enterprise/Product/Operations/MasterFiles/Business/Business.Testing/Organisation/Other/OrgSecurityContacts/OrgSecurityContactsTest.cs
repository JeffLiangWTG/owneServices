using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityContacts))]
	public class OrgSecurityContactsTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;

			org.SecurityRights[0].OX_Granted = false;
			OrgSecurityContacts securityContact = org.SecurityRights[0].ContactSecurityRights[0];
			securityContact.OZ_Granted = true;

			return securityContact;
		}

		public void TestIsSavedByFactory()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = true;

			org.SecurityRights[0].OX_Granted = false;
			OrgSecurityContacts securityContact = org.SecurityRights[0].ContactSecurityRights[0];

			securityContact.OZ_Granted = false;
			AssertEquals("Security contact should not be saved as it has the same granted flag as the parent", false, securityContact.IsSavedByFactory);

			securityContact.OZ_Granted = true;
			AssertEquals("Security contact should be saved as it has the different granted flag as the parent", true, securityContact.IsSavedByFactory);

			securityContact.OZ_Granted = false;
			AssertEquals("Security contact should not be saved as it has the same granted flag as the parent", false, securityContact.IsSavedByFactory);

			securityContact.Delete();
			AssertEquals("Security contact should be saved as it has been deleted", true, securityContact.IsSavedByFactory);
		}

		public void TestIsSavedByFactoryWhenContactHasNoWebAccess()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_WebAccessEnabled = false;

			OrgSecurityContacts securityContact = org.SecurityRights[0].ContactSecurityRights[0];

			AssertEquals("Security rights for this contact are never saved as they do not have web access", false, securityContact.IsSavedByFactory);

			securityContact.OZ_Granted = false;
			AssertEquals("Security rights for this contact are never saved as they do not have web access", false, securityContact.IsSavedByFactory);

			securityContact.OZ_Granted = true;
			AssertEquals("Security rights for this contact are never saved as they do not have web access", false, securityContact.IsSavedByFactory);

			securityContact.OZ_Granted = false;
			AssertEquals("Security rights for this contact are never saved as they do not have web access", false, securityContact.IsSavedByFactory);
		}

		public void TestDifferentSecurityToParent()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			org.SecurityRights[0].OX_Granted = false;
			org.SecurityRights[0].ContactSecurityRights[0].OZ_Granted = false;
			AssertEquals("Contact has same security to parent", false, org.SecurityRights[0].ContactSecurityRights[0].HasDifferentSecurityToParent);

			org.SecurityRights[0].ContactSecurityRights[0].OZ_Granted = true;
			AssertEquals("Contact does not have different security to parent as web access is denied", false, org.SecurityRights[0].ContactSecurityRights[0].HasDifferentSecurityToParent);

			contact.OC_WebAccessEnabled = true;
			org.SecurityRights[0].ContactSecurityRights[0].OZ_Granted = true;
			AssertEquals("Contact has different security to parent", true, org.SecurityRights[0].ContactSecurityRights[0].HasDifferentSecurityToParent);

			OrgSecurityContacts contactSecurityWithNoParent = Factory.New<OrgSecurityContacts>();
			AssertEquals("Contact has SAME security as parent if parent is null", false, contactSecurityWithNoParent.HasDifferentSecurityToParent);
		}

		public void TestSecurityItemName()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.Contacts.AddNew();
			org.SecurityRights[0].SecurityItemNameForDisplay = "";
			org.SecurityRights[0].OX_SecurityItemName = "Testing";

			AssertEquals("Precondition", "Testing", org.SecurityRights[0].OX_SecurityItemName);
			AssertEquals("Contact security has correct name", "Testing", org.SecurityRights[0].ContactSecurityRights[0].SecurityItemName);
			AssertEquals("Security Item Name is always readonly", true, org.SecurityRights[0].ContactSecurityRights[0].SecurityItemNameInfo.ReadOnly);

			OrgSecurityContacts contactSecurityWithNoParent = Factory.New<OrgSecurityContacts>();
			AssertEquals("Contact security has blank name", "", contactSecurityWithNoParent.SecurityItemName);

			AssertEquals("Security Item Name is always readonly", true, contactSecurityWithNoParent.SecurityItemNameInfo.ReadOnly);

			org.SecurityRights[0].SecurityItemNameForDisplay = "Testing For Display";
			AssertEquals("Testing For Display", org.SecurityRights[0].ContactSecurityRights[0].SecurityItemName);
		}

		public void TestContactIsReadOnly()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.Contacts.AddNew();
			org.SecurityRights[0].OX_SecurityItemName = "Testing";
			OrgSecurityContacts securityContact = org.SecurityRights[0].ContactSecurityRights[0];

			AssertEquals("Contact is always readonly", true, securityContact.OZ_OCInfo.ReadOnly);
		}

		public void TestReadOnlySecurity()
		{
			bool oldWebDetailsValue = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;
			bool oldNewWebDetailsValue = Env.Security.OrgDetailsNewWebSecurity.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				OrgSecurityContacts security = testContact.SecurityRightsForBindingOnly.AddNew();

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !security.OZ_GrantedInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", security.OZ_GrantedInfo.ReadOnly);

				testContact = Factory.NewWithValidTestData<OrgHeader>().Contacts.AddNew();
				security = testContact.SecurityRightsForBindingOnly.AddNew();

				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !security.OZ_GrantedInfo.ReadOnly);

				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", security.OZ_GrantedInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldWebDetailsValue;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = oldNewWebDetailsValue;
			}
		}

		public void TestOrgSecurityCanSaveWhenRelatedOrgSecurityContactsHasTheSameGrantedValue()
		{
			//setup
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestXXX";
			var contact = Factory.New<OrgContact>();

			var security = org.SecurityRights.OfType<OrgSecurity>().First(x => x.OX_Granted);
			security.OX_Granted = false;
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1");
			org.Contacts.Add(contact);
			Factory.Save();

			//act
			var contactSecurityPK = ZGuid.NewZGuid();
			var sql = $"INSERT INTO dbo.OrgSecurityContacts(OZ_PK, OZ_OC, OZ_OX, OZ_Granted) SELECT '{contactSecurityPK}', '{contact.PK}', '{security.PK}', 0";
			Db.Connection.ExecuteNonQuery(sql);
			Factory.ReloadAll<OrgSecurityContacts>();
			var contactSecurity = security.ContactSecurityRights.Where(x => x.OZ_OC == contact.PK).First();

			//assert
			AssertEquals("OrgSecurity default Granted should be false", false, security.OX_Granted);
			AssertEquals("OrgSecurityContact contactSecurity1 Granted should be false", false, contactSecurity.OZ_Granted);

			//act
			security.OX_Granted = true;
			AssertEquals(true, contactSecurity.IsSavedByFactory);
			Factory.Save();

			//assert
			AssertEquals(true, security.OX_Granted);
			AssertEquals(false, contactSecurity.OZ_Granted);
			AssertEquals(true, security.IsInDatabase);
			AssertEquals(true, contactSecurity.IsInDatabase);

			//act
			contactSecurity.OZ_Granted = true;
			Factory.Save();

			//assert
			AssertEquals(false, security.IsInDatabase);
			AssertEquals(false, contactSecurity.IsInDatabase);
		}

		public void TestSupportsNotes()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var security = contact.SecurityRightsView.Cast<OrgSecurityContacts>().First();
			AssertEquals(false, security.SupportsNotes);
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

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSecurity security = org.SecurityRights.AddNew();
			return security.ContactSecurityRights.AddNew();
		}

		#endregion
	}
}
