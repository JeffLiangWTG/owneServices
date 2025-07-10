using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRRecruitmentJobCampaignLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobRoles()
		{
			AssertNotNull("Job Roles lookups should not be null", Campaign.Lookups.JobRoles);
		}

		public void TestClientAccounts()
		{
			AssertNotNull("Client Accounts lookups should not be null", Campaign.Lookups.ClientAccounts);
		}

		public void TestClientContacts()
		{
			OrgHeader clientOrg = Factory.New<OrgHeader>();
			OrgContact clientContact1 = clientOrg.Contacts.AddNew();

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			OrgContact otherContact1 = otherOrg.Contacts.AddNew();

			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			AssertEquals("Client account not set, Client contacts lookups should not contain any contacts", 0, campaign.Lookups.ClientContacts.Count);

			campaign.HV_OH_ClientAccount = clientOrg.PK;
			AssertEquals("Client account is set, should be able to select contacts", 1, campaign.Lookups.ClientContacts.Count);
			Assert("Client account is set, client contact lookups should contain client contact", campaign.Lookups.ClientContacts.Contains(clientContact1));
			Assert("Client account is set, client contact lookups should NOT contain contacts of other clients", !campaign.Lookups.ClientContacts.Contains(otherContact1));

			campaign.HV_OH_ClientAccount = otherOrg.PK;
			Assert("Client account is set to OtherOrg, client contact lookups should contain other contact", campaign.Lookups.ClientContacts.Contains(otherContact1));
			Assert("Client account is set to OtherOrg, client contact lookups should NOT contain original client contact", !campaign.Lookups.ClientContacts.Contains(clientContact1));

			campaign.HV_OH_ClientAccount = ZGuid.Invalid;
			AssertEquals("Contacts Lookups should be empty", 0, campaign.Lookups.ClientContacts.Count);
		}

		public void TestClientAddresses()
		{
			OrgHeader clientOrg = Factory.New<OrgHeader>();
			OrgAddress clientAddress = clientOrg.MainAddress;

			OrgHeader otherOrg = Factory.New<OrgHeader>();
			OrgAddress otherOrgAddress = otherOrg.MainAddress;

			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			AssertEquals("Client account not set, Client addresses lookups should not contain any addresses", 0, campaign.Lookups.ClientAddresses.Count);

			campaign.HV_OH_ClientAccount = clientOrg.PK;
			AssertEquals("Client account is set, should be able to select addresses", 1, campaign.Lookups.ClientAddresses.Count);
			Assert("Client account is set, client address lookups should contain client address", campaign.Lookups.ClientAddresses.Contains(clientAddress));
			Assert("Client account is set, client address lookups should NOT contain addresses of other clients", !campaign.Lookups.ClientAddresses.Contains(otherOrgAddress));

			campaign.HV_OH_ClientAccount = otherOrg.PK;
			Assert("Client account is set to OtherOrg, client address lookups should contain other addresses", campaign.Lookups.ClientAddresses.Contains(otherOrgAddress));
			Assert("Client account is set to OtherOrg, client address lookups should NOT contain original client address", !campaign.Lookups.ClientAddresses.Contains(clientAddress));

			campaign.HV_OH_ClientAccount = ZGuid.Invalid;
			AssertEquals("Addresses Lookups should be empty", 0, campaign.Lookups.ClientAddresses.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Campaign = Factory.New<HRRecruitmentJobCampaign>();
		}

		HRRecruitmentJobCampaign Campaign;

		#endregion
	}
}
