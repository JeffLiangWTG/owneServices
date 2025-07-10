using System;
using System.Web.UI.HtmlControls;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	class UnsubscribePageTest : ZPageTestCase
	{
		UnsubscribeForTest UnsubscribePage => (UnsubscribeForTest)Page;

		OrgContact Contact;

		public void TestUnsubscribeSender()
		{
			AssertUnsubscribe(UnsubscribeType.Sender);
		}

		public void TestUnsubscribeMediaCategory()
		{
			AssertUnsubscribe(UnsubscribeType.MediaCategory);
		}

		public void TestUnsubscribeMediaType()
		{
			AssertUnsubscribe(UnsubscribeType.MediaType);
		}

		public void TestUnsubscribeMediaCategoryAndType()
		{
			AssertUnsubscribe(UnsubscribeType.MediaCategoryAndType);
		}

		void AssertUnsubscribe(UnsubscribeType unsubscribeType)
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithValidData(unsubscribeType).ToString());
			UnsubscribePage.ForceLoad();

			AssertEquals("You have successfully unsubscribed.", UnsubscribePage.ResultMessageLabelText);
			AssertEquals("UnsubscribeImages/success.png", UnsubscribePage.ResultImageSrc);
			AssertEquals(true, UnsubscribePage.ResubscribeIsVisible);
			var emailAddress = GlbEmailAddress.LoadOrNew(Factory, Contact.Email);
			AssertEquals("Contact should be verified.", EmailDeliveryReportStatus.Codes.ValidReport, emailAddress.GI_DeliveryStatus);
		}

		SecureQueryString NewQueryStringWithValidData(UnsubscribeType unsubscribeType)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			Contact = Factory.NewWithValidTestData<OrgContact>();
			Contact.OC_Email = $"{nameof(Contact)}@ema.il";
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString(),
				[UnsubscribeUrlHelper.UnsubscribeTypeKey] = unsubscribeType.ToString("G")
			};
			Factory.Save();
			return queryString;
		}

		public void TestResubscribe()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithValidDataForResubscribe().ToString());
			UnsubscribePage.ForceLoad();

			AssertEquals("You have successfully re-subscribed.", UnsubscribePage.ResultMessageLabelText);
			AssertEquals("UnsubscribeImages/success.png", UnsubscribePage.ResultImageSrc);
			AssertEquals(false, UnsubscribePage.ResubscribeIsVisible);
		}

		SecureQueryString NewQueryStringWithValidDataForResubscribe()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = $"{nameof(contact)}@ema.il";
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString(),
				[UnsubscribeUrlHelper.UnsubscribeTypeKey] = UnsubscribeType.MediaCategory.ToString("G"),
				[UnsubscribeUrlHelper.ResubscribeKey] = true.ToString()
			};
			Factory.Save();
			return queryString;
		}

		public void TestErrorResult()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithInvalidData().ToString());
			UnsubscribePage.ForceLoad();

			AssertEquals("Error loading campaign.", UnsubscribePage.ResultMessageLabelText);
			AssertEquals("UnsubscribeImages/error.png", UnsubscribePage.ResultImageSrc);
			AssertEquals(false, UnsubscribePage.ResubscribeIsVisible);
			AssertEquals("Uri path", "#", UnsubscribePage.ResubscribeHref);
		}

		SecureQueryString NewQueryStringWithInvalidData()
		{
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = Guid.NewGuid().ToString(),
				[UnsubscribeUrlHelper.UnsubscribeTypeKey] = UnsubscribeType.MediaCategoryAndType.ToString("G")
			};
			Factory.Save();
			return queryString;
		}

		protected override ZPage GetNewZPage() => new UnsubscribeForTest();

		protected override void SetUp()
		{
			base.SetUp();
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmehserver.webvoting.com.au/");
		}

		sealed class UnsubscribeForTest : Unsubscribe
		{
			public UnsubscribeForTest()
			{
				ResultMessageLabel = new ZTextLabel { BindTo = "ResultMessage" };
				ResultImage = new HtmlImage();
				Resubscribe = new HtmlAnchor();

				Controls.Add(ResultMessageLabel);
				Controls.Add(ResultImage);
				Controls.Add(Resubscribe);
			}

			public string ResultMessageLabelText => ResultMessageLabel.Text;
			public string ResultImageSrc => ResultImage.Src;
			public bool ResubscribeIsVisible => Resubscribe.Visible;
			public string ResubscribeHref => Resubscribe.HRef;

			internal void ForceLoad()
			{
				OnLoad(EventArgs.Empty);
			}
		}
	}
}
