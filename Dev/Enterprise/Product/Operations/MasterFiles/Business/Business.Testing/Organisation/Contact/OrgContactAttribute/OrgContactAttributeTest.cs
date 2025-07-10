using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactAttribute))]
	sealed class OrgContactAttributeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlySecurity()
		{
			bool oldContactAttributeValue = Env.Security.OrgContactModify.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				OrgContactAttribute attribute = testContact.Attributes.AddNew();

				Env.Security.OrgContactModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !attribute.PC_TypeInfo.ReadOnly);

				Env.Security.OrgContactModify.IsAllowed = false;
				Assert("Access Disallowed - ReadOnly", attribute.PC_TypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModify.IsAllowed = oldContactAttributeValue;
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

		public void TestNoAuditLog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			var attribute = contact.Attributes.AddNew();
			attribute.PC_Type = "ADX";
			Factory.Save();
			AssertNull(attribute.Logs.AutoCreatedLog);
		}

		public void TestAttributeDescription()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			OrgInDB.Contacts.Add(contact);
			OrgContactAttribute attribute = contact.Attributes.AddNew();

			attribute.PC_Type = "ART";
			AssertEquals("Arts", attribute.AttributeDescription);

			attribute.PC_Type = "XYZ";
			AssertEquals("", attribute.AttributeDescription);

			attribute.PC_Type = "BAS";
			AssertEquals("Baseball", attribute.AttributeDescription);

			attribute.PC_Type = "";
			AssertEquals("", attribute.AttributeDescription);
		}

		public void TestURLAttribute()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContactAttribute attribute = contact.Attributes.AddNew();

			attribute.PC_URL = "http://www.example.com";
			attribute.PC_Type = "ART";
			Assert("PC_URL is ReadOnly when PC_Type is not equal to 'SNL'", attribute.PC_URLInfo.ReadOnly);
			Assert("PC_URL is empty when PC_URL is ReadOnly", attribute.PC_URL.IsEmpty);

			attribute.PC_URL = "http://www.example.com";
			attribute.PC_Type = "SNL";
			Assert("PC_URL is not ReadOnly when PC_Type is equal to 'SNL'", !attribute.PC_URLInfo.ReadOnly);
			Assert("PC_URL is not empty when PC_URL is not ReadOnly", !attribute.PC_URL.IsEmpty);

			attribute.PC_URL = "http://www.example.com";
			attribute.PC_Type = "";
			Assert("PC_URL is ReadOnly when PC_Type is not equal to 'SNL'", attribute.PC_URLInfo.ReadOnly);
			Assert("PC_URL is empty when PC_URL is ReadOnly", attribute.PC_URL.IsEmpty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<OrgHeader>();
			return header.Contacts.AddNew().Attributes.AddNew();
		}
	}
}
