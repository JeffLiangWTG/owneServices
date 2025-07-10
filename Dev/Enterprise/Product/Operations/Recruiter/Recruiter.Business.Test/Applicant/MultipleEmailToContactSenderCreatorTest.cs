using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class MultipleEmailToContactSenderCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			NotificationEmailTemplate template = new NotificationEmailTemplate(typeof(HRJobApplication), "Some Subject", "Some Body");
			HRRecruitmentJobCampaign campaign = Factory.New<HRRecruitmentJobCampaign>();
			HRJobApplication application1 = campaign.Applications.AddNew();
			application1.HP_HA = applicant.PK;
			HRJobApplication application2 = campaign.Applications.AddNew();
			application2.HP_HA = applicant.PK;
			HRJobApplication application3 = campaign.Applications.AddNew();
			application3.HP_HA = applicant.PK;
			HRJobApplication[] applications = new[] { application2, application3 };

			MultipleEmailToContactSender sender = new MultipleEmailToContactSenderCreator().Create(campaign, applications, template);
			AssertEquals(2, sender.EmailsToContacts.Count);
			AssertEquals("Some Subject", sender.EmailsToContacts[0].Subject);
			AssertEquals("Some Subject", sender.EmailsToContacts[1].Subject);
			AssertEquals("Some Body", sender.EmailsToContacts[0].Body);
			AssertEquals("Some Body", sender.EmailsToContacts[1].Body);
			sender.EmailsToContacts[0].FromDisplayName = "new Display";
			sender.EmailsToContacts[1].FromEmailAddress = "newFrom@cargowise.com";
			sender.EmailsToContacts[0].Body = "new body";
			sender.EmailsToContacts[1].Subject = "new subject";

			AssertEquals("new Display", sender.EmailsToContacts[0].FromDisplayName);
			AssertEquals("new Display", sender.EmailsToContacts[1].FromDisplayName);
			AssertEquals("newFrom@cargowise.com", sender.EmailsToContacts[0].FromEmailAddress);
			AssertEquals("newFrom@cargowise.com", sender.EmailsToContacts[1].FromEmailAddress);
			AssertEquals("new subject", sender.EmailsToContacts[0].Subject);
			AssertEquals("new subject", sender.EmailsToContacts[1].Subject);
			AssertEquals("new body", sender.EmailsToContacts[0].Body);
			AssertEquals("new body", sender.EmailsToContacts[1].Body);
		}
	}
}
