using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI
{
	[TestedType(typeof(GlbCompanyCampaignItemForm))]
	public class GlbCompanyCampaignItemFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Paul";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			Factory.Save();

			using (GlbCompanyCampaignItemForm form = new GlbCompanyCampaignItemForm(item))
			{
				form.Show();
				AssertEquals("Form caption", "Campaign Sent To Paul", form.FormCaption);
			}
		}

		public void TestDisableNewAction()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			Factory.Save();

			using (GlbCompanyCampaignItemForm form = new GlbCompanyCampaignItemForm(item))
			{
				form.Show();
				Assert("Save button should exist", ZFormPostingButtonsStrategy.ApplyButtonText(form).Text.IndexOf("Save") != -1);
				Assert("Form should NOT have new button", ZFormPostingButtonsStrategy.ApplyButtonText(form).Text.IndexOf("New") == -1);
				AssertEquals("Display mode should be ", ODisplayMode.NewSaved, form.DisplayMode);
			}

			item.G8_FollowedUp = ZDateTime.Now;

			using (GlbCompanyCampaignItemForm form = new GlbCompanyCampaignItemForm(item))
			{
				form.Show();
				Assert("Form should NOT have new button", ZFormPostingButtonsStrategy.ApplyButtonText(form).Text.IndexOf("New") == -1);
				AssertEquals("Display mode should be ", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			return new GlbCompanyCampaignItemForm(item);
		}

		#endregion

	}
}
