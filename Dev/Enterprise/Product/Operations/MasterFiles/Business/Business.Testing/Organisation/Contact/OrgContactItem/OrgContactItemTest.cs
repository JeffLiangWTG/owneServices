using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactItem))]
	sealed class OrgContactItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var contactItem = Factory.New<OrgContactItem>();
			AssertEquals(true, contactItem.OI_IsPrimary);
		}

		#region IReadOnlySecurity

		public void TestReadOnlySecurityMembers()
		{
			var orgContactItem = Factory.NewWithValidTestData<OrgContactItem>();

			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			AssertEquals(false, orgContactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(false, orgContactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(false, orgContactItem.OI_IsPrimaryInfo.ReadOnly);

			Env.Security.OrgContactModifyContactDetails.IsAllowed = false;
			AssertEquals(true, orgContactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(true, orgContactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(true, orgContactItem.OI_IsPrimaryInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			orgContactItem.OI_OC = contact.PK;

			AssertEquals("Precondition", true, org.SecurityProvider.HasModifyContactContactDetailsSecurity);
			AssertEquals(false, orgContactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(false, orgContactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(false, orgContactItem.OI_IsPrimaryInfo.ReadOnly);

			Factory.Save();
			AssertEquals("Precondition", false, org.SecurityProvider.HasModifyContactContactDetailsSecurity);
			AssertEquals(true, orgContactItem.OI_DescriptionInfo.ReadOnly);
			AssertEquals(true, orgContactItem.OI_AddressInfo.ReadOnly);
			AssertEquals(true, orgContactItem.OI_IsPrimaryInfo.ReadOnly);
		}

		public void TestOI_Address_IsManuallyVerified()
		{
			var orgContactItem1 = Factory.NewWithValidTestData<OrgContactItem>();
			var orgContactItem2 = Factory.NewWithValidTestData<OrgContactItem>();
			PhoneNumberTestHelper.AssertPropertyIsManuallyVerified(Factory, OrgContactItem.Schema.OI_Address_IsManuallyVerified, OrgContactItemSchema.Constants.Prefix, OrgContactItemSchema.Constants.OI_Address, orgContactItem1, orgContactItem2);
		}

		public void TestDelete()
		{
			var orgContactItem1 = Factory.NewWithValidTestData<OrgContactItem>();
			var orgContactItem2 = Factory.NewWithValidTestData<OrgContactItem>();
			orgContactItem1.OI_Address_IsManuallyVerified = true;
			orgContactItem2.OI_Address_IsManuallyVerified = true;

			var acks1 = new GenCustomAddOnRuleAckCollection(orgContactItem1);
			var acks2 = new GenCustomAddOnRuleAckCollection(orgContactItem2);
			AssertEquals("Precondition", 1, acks1.Count);
			AssertEquals("Precondition", 1, acks2.Count);

			orgContactItem1.Delete();
			AssertEquals(0, acks1.Count);
			AssertEquals(1, acks2.Count);
		}

		#endregion
	}
}
