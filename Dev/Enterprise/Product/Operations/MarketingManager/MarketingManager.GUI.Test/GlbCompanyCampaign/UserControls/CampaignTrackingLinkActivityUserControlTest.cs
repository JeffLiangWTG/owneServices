using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	public class CampaignTrackingLinkActivityUserControlTest : ZFormBasherTest
	{
		#region Test Objects

		public class DummyCampaignForm : ZChildForm
		{
			public DummyCampaignForm(GlbCompanyCampaign campaign)
				: base(campaign)
			{
				this.CaptionRenderingEnabled = true;
			}

			public CampaignTrackingControl TrackingControl;

			public CampaignTrackingLinkActivityUserControl LinkActivityControl;

			protected override void InitializeComponent()
			{
				TrackingControl = new CampaignTrackingControl();
				LinkActivityControl = new CampaignTrackingLinkActivityUserControl();
				Controls.Add(TrackingControl);
				TrackingControl.Controls.Add(LinkActivityControl);
			}

			public ZButton ViewDetailsButtonExposed
			{
				get { return LinkActivityControl.ViewDetailsButton; }
			}

			public ZArchitecture.ZLabel TrackingStatusLabelExposed
			{
				get { return LinkActivityControl.TrackingStatusLabel; }
			}
		}

		Business.Testing.GlbCompanyCampaignTestHelper Helper
		{
			get { return helper ?? (helper = new Business.Testing.GlbCompanyCampaignTestHelper(Factory)); }
		}
		Business.Testing.GlbCompanyCampaignTestHelper helper;

		#endregion

		public void TestViewDetailsButton()
		{
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.LinkActivityControl.SetDataBinding(campaignItem.StatModel, null);

				form.Show();
				form.LinkActivityControl.ViewDetailsButton_Click(null, EventArgs.Empty);
				AssertEquals("Form Shown", typeof(LinkActivityDetailsForm), ZFormModaliser.LastFormShownForTest.GetType());
				if (ZFormModaliser.LastFormShownForTest != null)
				{
					ZFormModaliser.LastFormShownForTest.Close();
					ZFormModaliser.LastFormShownForTest.Dispose();
				}
			}
		}

		public void TestSetTrackingStatusLabelBackColor()
		{
			GlbCompanyCampaign campaign = Helper.GetCampaignWithoutErrors();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = ZGuid.NewZGuid();
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = ZGuid.NewZGuid();

			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webtest.com";
			link.GCL_Context = "Test";

			var click = Factory.New<GlbCompanyCampaignClick>();
			click.GCC_G8_Recipient = campaignItem.PK;
			click.GCC_GCL = link.PK;
			click.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			using (DummyCampaignForm form = new DummyCampaignForm(campaign))
			{
				form.LinkActivityControl.SetDataBinding(campaignItem.StatModel, null);

				form.Show();
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(System.Drawing.Color.Orange, form.TrackingStatusLabelExposed.BackColor);

				form.LinkActivityControl.SetDataBinding(campaignItem2.StatModel, null);
				AssertEquals(System.Drawing.Color.LightSkyBlue, form.TrackingStatusLabelExposed.BackColor);

				form.LinkActivityControl.SetDataBinding(campaignItem3.StatModel, null);
				AssertEquals(System.Drawing.Color.Silver, form.TrackingStatusLabelExposed.BackColor);
			}
		}

		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			var userControl = new CampaignTrackingLinkActivityUserControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaignItem.StatModel, null);
			result.Size = ControlDpiScalingHelper.NewScaledSize(1000, 700, true);
			return result;
		}
	}
}
