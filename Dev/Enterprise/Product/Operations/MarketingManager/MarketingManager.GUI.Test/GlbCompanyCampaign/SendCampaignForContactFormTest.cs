using System.Reflection;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SendCampaignForContactForm))]
	public class SendCampaignForContactFormTest : ZFormBasherTest
	{
		public void TestContactInfo()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "nobody@nowhere.com";
			contact.OC_OH = org.PK;

			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);

			using (SendCampaignForContactForm form = new SendCampaignForContactForm(sendCampaignBizO))
			{
				form.Show();

				FieldInfo orgNameTextBoxInfo = typeof(SendCampaignForContactForm).GetField("OrganisationNameTextBox", BindingFlags.NonPublic | BindingFlags.Instance);
				ZTextBox orgNameTextBox = (ZTextBox)orgNameTextBoxInfo.GetValue(form);
				AssertEquals("CargoWise", orgNameTextBox.Text);

				FieldInfo contactNameTextBoxInfo = typeof(SendCampaignForContactForm).GetField("ContactNameTextBox", BindingFlags.NonPublic | BindingFlags.Instance);
				ZTextBox contactNameTextBox = (ZTextBox)contactNameTextBoxInfo.GetValue(form);
				AssertEquals("Samuel", contactNameTextBox.Text);

				FieldInfo contactEmailTextBoxInfo = typeof(SendCampaignForContactForm).GetField("ContactEmailTextBox", BindingFlags.NonPublic | BindingFlags.Instance);
				ZTextBox contactEmailTextBox = (ZTextBox)contactEmailTextBoxInfo.GetValue(form);
				AssertEquals("nobody@nowhere.com", contactEmailTextBox.Text);
			}
		}

		public void TestSelectCampaign()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "nobody@nowhere.com";
			contact.OC_OH = org.PK;
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			campaign.G0_CampaignID = "TST00001000";

			Factory.Save();

			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);
			using (SendCampaignForContactForm form = new SendCampaignForContactForm(sendCampaignBizO))
			{
				form.Show();

				FieldInfo campaignFindBoxInfo = typeof(SendCampaignForContactForm).GetField("CampaignFindBox", BindingFlags.NonPublic | BindingFlags.Instance);
				ZGuidFindBox campaignFindBox = (ZGuidFindBox)campaignFindBoxInfo.GetValue(form);

				campaignFindBox.PopupButton.PerformClick();

				AssertEquals("Module form should popup", typeof(EmbeddedModulePopup), ZFormModaliser.LastFormShownForTest.GetType());

				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;

				FieldInfo filterControlPanelInfo = typeof(EmbeddedModulePopup).GetField("FilterControlPanel", BindingFlags.NonPublic | BindingFlags.Instance);
				ZPanel filterControlPanel = (ZPanel)filterControlPanelInfo.GetValue(popup);

				ZFilterStripControl embeddedControl = (ZFilterStripControl)filterControlPanel.Controls[0];
				embeddedControl.FirePerformSearch();
				embeddedControl.FilteredGrid.Select(0);

				FieldInfo okButtonInfo = typeof(EmbeddedModulePopup).GetField("OK_Button", BindingFlags.NonPublic | BindingFlags.Instance);
				ZButton okButton = (ZButton)okButtonInfo.GetValue(popup);
				okButton.PerformClick();

				AssertEquals("Campaign should be selected", campaign.G0_CampaignID, campaignFindBox.CodeBox.Text);

				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestSendCampaign()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "nobody@nowhere.com";
			contact.OC_OH = org.PK;
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();

			Factory.Save();

			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);
			sendCampaignBizO.CampaignPK = campaign.PK;

			using (SendCampaignForContactForm form = new SendCampaignForContactForm(sendCampaignBizO))
			{
				form.Show();

				FieldInfo sendCampaignButtonInfo = typeof(SendCampaignForContactForm).GetField("SendCampaignButton", BindingFlags.NonPublic | BindingFlags.Instance);
				ZButton sendCampaignButton = (ZButton)sendCampaignButtonInfo.GetValue(form);
				sendCampaignButton.PerformClick();
				AssertEquals("Campaign should be sent", 1, campaign.CampaignsItemsSent.Count);
			}
		}

		#region Implementation

		protected sealed override Form GetFormToBashCore()
		{
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			SendCampaignForContactBizO sendCampaignBizO = new SendCampaignForContactBizO(contact);
			SendCampaignForContactForm form = GetNewSendCampaignForm(sendCampaignBizO);
			return form;
		}

		protected virtual SendCampaignForContactForm GetNewSendCampaignForm(SendCampaignForContactBizO sendCampaignBizO)
		{
			return new SendCampaignForContactForm(sendCampaignBizO);
		}

		GlbCompanyCampaignTestHelper Helper
		{
			get { return helper ?? (helper = new GlbCompanyCampaignTestHelper(Factory)); }
		}
		GlbCompanyCampaignTestHelper helper;

		#endregion
	}
}
