using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ContactCampaignSenderGUIManagerTest : TestCaseWithFactory
	{
		public void TestSendCampaign()
		{
			IOrgContact contact = Factory.NewWithValidTestData<OrgContact>();

			ContactCampaignSenderGUIManager manager = new ContactCampaignSenderGUIManager();
			manager.SendCampaign(contact);

			AssertEquals("SendCampaignForContactForm should popup", typeof(SendCampaignForContactForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			ZFormModaliser.LastFormShownDialogForTest = null;
		}

		public void TestResendCampaign()
		{
			GlbStaff campaignStaff = Factory.NewWithValidTestData<GlbStaff>();
			campaignStaff.GS_Code = "CMS";
			campaignStaff.GS_EmailAddress = "cms@test.org";

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact@gmail.com";
			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "contact1@gmail.com";

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_Category = "PRINT";
			campaign.G0_EmailSubject = "Email Subject";
			campaign.HtmlDocumentBlob = new ZBlob(Encoding.ASCII.GetBytes("(*CampaignID*)"));
			campaign.G0_EstimatedStartedDate = ZDateTime.Now.AddDays(-1);
			campaign.G0_GS_NKCampaignCoordinator = campaignStaff.GS_Code;
			campaign.G0_GS_NKCampaignManager = campaignStaff.GS_Code;
			campaign.G0_Type = "EXIST";

			GlbCompanyCampaignItem campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			GlbCompanyCampaignItem campaignItem2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem2.G8_G0 = campaign.PK;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact1.PK;

			Factory.Save();

			ContactCampaignSenderGUIManager manager = new ContactCampaignSenderGUIManager();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			manager.ResendCampaign(campaignItem);
			AssertEquals("Message emails sent", "1 campaigns were successfully sent", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestResendCampaign_WithTargetList()
		{
			GlbStaff campaignStaff = Factory.NewWithValidTestData<GlbStaff>();
			campaignStaff.GS_Code = "CMS";
			campaignStaff.GS_EmailAddress = "cms@test.org";

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_Category = "PRINT";
			campaign.G0_EmailSubject = "Email Subject";
			campaign.HtmlDocumentBlob = new ZBlob(Encoding.ASCII.GetBytes("(*CampaignURL*)"));
			campaign.G0_EstimatedStartedDate = ZDateTime.Now.AddDays(-1);
			campaign.G0_GS_NKCampaignCoordinator = campaignStaff.GS_Code;
			campaign.G0_GS_NKCampaignManager = campaignStaff.GS_Code;
			campaign.G0_Type = "EXIST";
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;

			GlbCompanyCampaignItem campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();

			ContactCampaignSenderGUIManager manager = new ContactCampaignSenderGUIManager();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manager.ResendCampaign(campaignItem);
			AssertNull("Message should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
